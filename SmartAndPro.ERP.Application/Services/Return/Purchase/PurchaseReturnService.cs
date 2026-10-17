using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.Application.Services.Return.Purchase.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SmartAndPro.ERP.Application.Services.Return.Purchase
{
    public class PurchaseReturnService
    {
        #region Failds
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        #endregion

        #region Constructors
        public PurchaseReturnService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
        }

        #endregion

        #region Methods
        public async Task<string> GetLastReturnNo()
        {
            var purchaseReturnsQuery = await _repositoriesFactory.GetRepository<PurchaseReturn>().GetAll();
            var purchaseReturns = await purchaseReturnsQuery.Data.ToListAsync();
            if (!purchaseReturns.Any()) return await Task.FromResult(string.Empty);

            var purchaseReturn = purchaseReturns.OrderBy(i => i.Id).Last();
            return purchaseReturn.ReturnNo;
        }
        public async Task<IEnumerable<PurchaseReturn>> GetAllAsync()
        {
            var purchaseReturnsQuery = await _repositoriesFactory.GetRepository<PurchaseReturn>().GetAll();
            return await purchaseReturnsQuery.Data
                .Include(pr => pr.PurchaseReturnItems)
                .Include(pr => pr.SupplierDetails)
                .ToListAsync();
        }
        public async Task<PurchaseReturn> GetFirstAsync()
        {
            var purchaseReturnsQuery = await _repositoriesFactory.GetRepository<PurchaseReturn>().GetAll();
            var purchaseReturns = await purchaseReturnsQuery.Data.FirstOrDefaultAsync();
            return purchaseReturns;
        }
        public async Task<PurchaseReturn> GetByIdAsync(int id)
        {
            var purchaseReturnsQuery = await _repositoriesFactory
                .GetRepository<PurchaseReturn>().GetAll();
            return await purchaseReturnsQuery.Data
                .Include(pr => pr.PurchaseReturnItems)
                .Include(pr => pr.SupplierDetails)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<int> AddAsync(PurchaseReturn purchaseReturn)
        {
            var resultQuery = await _repositoriesFactory.GetRepository<PurchaseReturn>().Add(purchaseReturn);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }
        public async Task<int> UpdateTransaction(int? purchaseReturnId, Guid? transactionId)
        {
            try
            {
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
                var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();

                
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var purchaseReturnRepository = _repositoriesFactory.GetRepository<PurchaseReturn>();
                var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();

                var purchaseReturnQuery = await purchaseReturnRepository.Filter(x => x.Id == purchaseReturnId);
                var purchaseReturn = purchaseReturnQuery.Data.FirstOrDefault();
                if (purchaseReturn == null) return 0;
                
                var transactionQuery = await transactionRepository.Filter(x=>x.Id == transactionId);
                var transaction = transactionQuery.Data.FirstOrDefault();
                if (transaction == null) return 0;

                var oldTransactionEntries = transaction.TransactionEntries;
                foreach (var entry in oldTransactionEntries)
                {
                    entry.Transaction = null;
                    entry.MainAccount = null;
                    var deletionResult = await transactionEntryRepository.Delete(entry);
                }
                
                // Retrieve the "Main Vault" MainAccount
                var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية");
                var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
                if (mainVaultAccount == null)
                {
                    return 0; // Main Vault account not found
                }

                
                
                // Retrieve Transaction Type for purchaseReturn transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد مرتجع"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on purchaseReturn items
                var transactionEntries = new List<TransactionEntry>();

                var returnTotalAmount = purchaseReturn.PurchaseReturnItems.Sum(item => item.Price * item.Quantity);
                var purchaseTaxAmount = returnTotalAmount * (decimal)0.15;
                returnTotalAmount += purchaseTaxAmount;
                if (purchaseReturn.TaxAmount > 0)
                {
                    // Retrieve the "Purchase Tax" MainAccount
                    var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
                    var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
                    if (purchaseTaxAccount == null)
                    {
                        return 0; // Purchase Tax account not found
                    }
                    var taxEntry = new TransactionEntry
                    {
                        TransactionId = transaction.Id,
                        MainAccountId = purchaseTaxAccount.Id,
                        CreditAmount = purchaseTaxAmount,
                        DebitAmount = 0,
                        Description = $"Return #{purchaseReturn.ReturnNo} - Purchase Tax",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(taxEntry);
                }
                

                var supplier = purchaseReturn.SupplierDetails;
                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,
                    MainAccountId = supplier.MainAccountId.Value,
                    DebitAmount = returnTotalAmount,
                    CreditAmount =  0,
                    Description = $"Return #{purchaseReturn.ReturnNo} (Supplier)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);

                // 3. Group entries by store
                var storeGroups = purchaseReturn.PurchaseReturnItems
                    .GroupBy(i => i.StoreDetailsId)
                    .ToList();

                foreach (var storeGroup in storeGroups)
                {
                    var storeId = storeGroup.Key;

                    // Retrieve the store
                    var storeQuery = await storeRepository.Filter(s => s.Id == storeId);
                    var store = storeQuery.Data.FirstOrDefault();
                    if (store == null || store.MainAccountId == null)
                    {
                        return 0; // Store or MainAccount not found
                    }

                    // Calculate the total for this store group
                    var storeTotal = storeGroup.Sum(item => item.Price * item.Quantity);

                    // Create a transaction entry for the store's account (Debit)
                    var storeEntry = new TransactionEntry
                    {
                        TransactionId = transaction.Id,
                        MainAccountId = store.MainAccountId.Value,
                        DebitAmount = 0,
                        CreditAmount = storeTotal,
                        Description = $"Return #{purchaseReturn.ReturnNo} (Store)",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(storeEntry);
                }
                
                var additionResult = await transactionEntryRepository.AddRange(transactionEntries);
                if (additionResult.Data.Count()== transactionEntries.Count)
                {
                    return 1;
                }
                
            }
            catch (Exception e)
            {
                LogService.LogError(e);
            }

            return 0;
        }
        public async Task<int> CreateTransaction(int id)
        {
            try
            {
                var purchaseReturnRepository = _repositoriesFactory.GetRepository<PurchaseReturn>();
                var supplierRepository = _repositoriesFactory.GetRepository<SupplierDetails>();
                var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

                var purchaseReturnQuery = await purchaseReturnRepository.Filter(i => i.Id == id);
                var purchaseReturn = purchaseReturnQuery.Data.FirstOrDefault();
                if (purchaseReturn == null)
                {
                    return 0; // Return not found
                }

                var supplierQuery = await supplierRepository.Filter(s => s.Id == purchaseReturn.SupplierDetailsId);
                var supplier = supplierQuery.Data.FirstOrDefault();
                if (supplier == null || supplier.MainAccountId == null)
                {
                    return 0; // Supplier or MainAccount not found
                }

                // Retrieve the "Main Vault" MainAccount
                var mainVaultAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "الخزينة الرئيسية");
                var mainVaultAccount = mainVaultAccountQuery.Data.FirstOrDefault();
                if (mainVaultAccount == null)
                {
                    return 0; // Main Vault account not found
                }
                

                // Retrieve Transaction Type for purchaseReturn transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد مرتجع"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on purchaseReturn items
                var transactionEntries = new List<TransactionEntry>();

                var returnTotalAmount = purchaseReturn.PurchaseReturnItems.Sum(item => item.Price * item.Quantity);
                var purchaseTaxAmount = returnTotalAmount * (decimal)0.15;
                returnTotalAmount += purchaseTaxAmount;
                if (purchaseReturn.TaxAmount > 0)
                {
                    // Retrieve the "Purchase Tax" MainAccount
                    var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
                    var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
                    if (purchaseTaxAccount == null)
                    {
                        return 0; // Purchase Tax account not found
                    }
                    
                    var taxEntry = new TransactionEntry
                    {
                        MainAccountId = purchaseTaxAccount.Id,
                        CreditAmount = purchaseTaxAmount,
                        DebitAmount = 0,
                        Description = $"Return #{purchaseReturn.ReturnNo} - Purchase Tax",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(taxEntry);
                }


                

                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    MainAccountId = supplier.MainAccountId.Value,
                    DebitAmount = returnTotalAmount,
                    CreditAmount = 0,
                    Description = $"Return #{purchaseReturn.ReturnNo} (Supplier)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);

                // 3. Group entries by store
                var storeGroups = purchaseReturn.PurchaseReturnItems
                    .GroupBy(i => i.StoreDetailsId)
                    .ToList();

                foreach (var storeGroup in storeGroups)
                {
                    var storeId = storeGroup.Key;

                    // Retrieve the store
                    var storeQuery = await storeRepository.Filter(s => s.Id == storeId);
                    var store = storeQuery.Data.FirstOrDefault();
                    if (store == null || store.MainAccountId == null)
                    {
                        return 0; // Store or MainAccount not found
                    }

                    // Calculate the total for this store group
                    var storeTotal = storeGroup.Sum(item => item.Price * item.Quantity);

                    // Create a transaction entry for the store's account (Debit)
                    var storeEntry = new TransactionEntry
                    {
                        MainAccountId = store.MainAccountId.Value,
                        DebitAmount = 0,
                        CreditAmount = storeTotal,
                        Description = $"Return #{purchaseReturn.ReturnNo} (Store)",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(storeEntry);
                }

                // Create the transaction
                var transaction = new Transaction
                {
                    Description = $"قيد مرتجع مشتريات رقم {purchaseReturn.ReturnNo}",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TransactionTypeId = transactionTypeId.Value,
                    TransactionEntries = transactionEntries,
                    PurchaseReturnId = purchaseReturn.Id
                };

                // Enforce rules and save transaction
                var transactionEnforcer = new TransactionEnforcer();
                var enforcerResult = transactionEnforcer.EnforceRules(transaction);
                if (enforcerResult.IsSuccess)
                {
                    var result = await transactionRepository.Add(transaction);
                    if (result.Data != null)
                    {
                        var inventoryOperationService = _serviceBaseFactory.GetService<InventoryOperationService>();
                        var inventoryOperation = new InventoryOperationPostDto
                        {
                            Description = $"Purchase Return #{purchaseReturn.ReturnNo}",
                            SupplierDetailsId = purchaseReturn.SupplierDetailsId,
                            OperationDate = purchaseReturn.IssueDate,
                            OperationSource = OperationSource.PurchaseReturn.ToString(),
                            OperationEntries = purchaseReturn.PurchaseReturnItems.Select(pr => new OperationEntryPostDto
                            {
                                MainPurchaseClassId = pr.MainPurchaseClassId,
                                Quantity = -pr.Quantity,
                                UnitPrice = pr.Price,
                                StoreDetailsId = pr.StoreDetailsId
                            }).ToList(),
                            TransactionId = result.Data.Id,
                            PurchaseReturnId = purchaseReturn.Id
                        };
                        var response = await inventoryOperationService.AddAsync(inventoryOperation);
                        if (response.ErrorCode != ErrorCodes.Success)
                        {
                            return 0;
                        }
                        purchaseReturn.PurchaseReturnItems = null;
                        purchaseReturn.TransactionId = result.Data.Id;
                        var r = await purchaseReturnRepository.Update(purchaseReturn);
                        return r.ErrorCode == ErrorCodes.Success ? 1 : 0;
                    }
                }

                return 0; // If rules enforcement failed
            }
            catch (Exception e)
            {
                LogService.LogError(e);
                return 0; // Handle error
            }
        }

        
        public async Task<int> EditAsync(PurchaseReturn purchaseReturn, List<PurchaseReturnItem> oldPurchaseReturnItems)
        {
            try
            {

                var purchaseReturnRepository = _repositoriesFactory.GetRepository<PurchaseReturn>();
                var purchaseItemRepository = _repositoriesFactory.GetRepository<PurchaseReturnItem>();
                var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
                var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
                
                var purchaseItems = purchaseReturn.PurchaseReturnItems;
                purchaseReturn.PurchaseReturnItems = null;

                // Delete items that exist in oldPurchaseReturnItems but not in new purchaseItems
                var itemsToDelete = oldPurchaseReturnItems
                    .Where(oldItem => !purchaseItems.Any(newItem => newItem.Id == oldItem.Id))
                    .ToList();

                foreach (var itemToDelete in itemsToDelete)
                {
                    itemToDelete.StoreDetails = null;
                    itemToDelete.MainPurchaseClass = null;
                    await purchaseItemRepository.Delete(itemToDelete);
                }

                // Update or add new purchase items
                foreach (var purchaseItem in purchaseItems)
                {
                    if (purchaseItem.Id != null)
                    {
                        purchaseItem.StoreDetails = null;
                        purchaseItem.MainPurchaseClass = null;
                        await purchaseItemRepository.Update(purchaseItem);
                    }
                    else
                    {
                        await purchaseItemRepository.Add(purchaseItem);
                    }
                }
                
                // Delete the existing inventory operations
                var inventoryOperationQuery = await inventoryOperationRepository.Filter(x => x.PurchaseReturnId == purchaseReturn.Id);
                var inventoryOperation = inventoryOperationQuery.Data
                    .Include(x => x.OperationEntries)
                    .IgnoreAutoIncludes()
                    .FirstOrDefault();
                if (inventoryOperation != null)
                {
                    var operationEntriesToDelete = inventoryOperation.OperationEntries.ToList();
                    foreach (var operationEntry in operationEntriesToDelete)
                    {
                        await operationEntryRepository.Delete(operationEntry);
                    }
                    var updatedOperationEntries = purchaseItems.Select(pi => new OperationEntry
                    {
                        MainPurchaseClassId = pi.MainPurchaseClassId,
                        Quantity = -pi.Quantity,
                        UnitPrice = pi.Price,
                        StoreDetailsId = pi.StoreDetailsId,
                        InventoryOperationId = inventoryOperation.Id
                    }).ToList();
                
                    await operationEntryRepository.AddRange(updatedOperationEntries);
                }
                
                

                // Nullify unrelated properties for the purchaseReturn before updating
                purchaseReturn.SupplierDetails = null;
                var response = await purchaseReturnRepository.Update(purchaseReturn);

                // Update the transaction if present
                if (purchaseReturn.TransactionId != null)
                {
                    var transactionUpdateResult = await UpdateTransaction(purchaseReturn.Id, purchaseReturn.TransactionId);
                }

                var result = response.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
            }

            return 0;
        }
        public async Task<int> RemoveAsync(PurchaseReturn purchasseReturn)
        {
            var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
            var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
            // query all operation entries that are part of this purchasseReturn then loop and delete the entries
            var inventoryOperationQuery = await inventoryOperationRepository.Filter(x => x.PurchaseReturnId == purchasseReturn.Id);
            var inventoryOperation = inventoryOperationQuery.Data
                .Include(x => x.OperationEntries)
                .IgnoreAutoIncludes()
                .FirstOrDefault();
            if (inventoryOperation != null)
            {
                var operationEntriesToDelete = inventoryOperation.OperationEntries.ToList();
                foreach (var operationEntry in operationEntriesToDelete)
                {
                    await operationEntryRepository.Delete(operationEntry);
                }
                inventoryOperation.OperationEntries = null;
                inventoryOperation.PurchaseReturn = null;
                var operationDeleteResult = await inventoryOperationRepository.Delete(inventoryOperation);
                if (operationDeleteResult.ErrorCode != ErrorCodes.Success)
                {
                    throw new Exception("Inventory Operations Deletion Failed");
                }
            }
            
            if (purchasseReturn.TransactionId != null)
            {
                // Delete the transaction
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var transactionQuery = await transactionRepository.Filter(x => x.Id == purchasseReturn.TransactionId);
                var transaction = transactionQuery.Data.FirstOrDefault();
                if (transaction != null)
                {
                    var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
                    var transactionEntries = transaction.TransactionEntries;
                    transaction.TransactionEntries = null;
                    foreach (var entry in transactionEntries)
                    {
                        entry.Transaction = null;
                        entry.MainAccount = null;
                        var deletionResult = await transactionEntryRepository.Delete(entry);
                    }
                    await transactionRepository.Delete(transaction);
                }
            }
            var purchaseReturnItemRepository = _repositoriesFactory.GetRepository<PurchaseReturnItem>();
            var purchaseReturnItemsToDelete = purchasseReturn.PurchaseReturnItems.ToList();
            foreach (var purchaseReturnItem in purchaseReturnItemsToDelete)
            {
                purchaseReturnItem.StoreDetails = null;
                purchaseReturnItem.MainPurchaseClass = null;
                purchaseReturnItem.PurchaseReturn = null;
                await purchaseReturnItemRepository.Delete(purchaseReturnItem);
            }
            purchasseReturn.PurchaseReturnItems = null;
            purchasseReturn.SupplierDetails = null;
            var resultQuery = await _repositoriesFactory.GetRepository<PurchaseReturn>().Delete(purchasseReturn);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }

        public async Task<IEnumerable<object>> GetPurchaseReturnVATReport(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // OPTIMIZED: Simple query with only required data - no unnecessary joins
                var purchaseReturnsQueryResponse = await _repositoriesFactory.GetRepository<PurchaseReturn>().GetAll();
                
                if (purchaseReturnsQueryResponse.ErrorCode != ErrorCodes.Success || purchaseReturnsQueryResponse.Data == null)
                    return new List<object>();

                var returns = await purchaseReturnsQueryResponse.Data
                    .Where(pr => pr.IssueDate >= startDate.ToDateTime(TimeOnly.MinValue) 
                              && pr.IssueDate <= endDate.ToDateTime(TimeOnly.MaxValue)
                              && pr.IsActive)
                    .Select(pr => new
                    {
                        pr.Id,
                        pr.ReturnNo,
                        pr.IssueDate,
                        SupplierId = pr.SupplierDetailsId,
                        Items = pr.PurchaseReturnItems
                            .Where(item => item.IsActive)
                            .Select(item => new
                            {
                                item.ItemNo,
                                item.Name,
                                item.Quantity,
                                item.Price,
                                item.Tax,
                                item.TaxAmount,
                                item.TotalPrice,
                                item.Discount,
                                item.DiscountAmount,
                                item.NetPrice
                            }).ToList(),
                        SupplierName = pr.SupplierDetails != null 
                            ? (pr.SupplierDetails.ArabicName ?? pr.SupplierDetails.EnglishName ?? "N/A")
                            : "N/A"
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!returns.Any())
                    return new List<object>();

                // Flatten to report rows
                var reportData = returns
                    .SelectMany(pr => pr.Items.Select(item => new
                    {
                        returnId = pr.Id,
                        returnNo = pr.ReturnNo,
                        supplierName = pr.SupplierName,
                        supplierId = pr.SupplierId,
                        issueDate = pr.IssueDate,
                        itemName = item.Name,
                        itemNo = item.ItemNo,
                        quantity = item.Quantity,
                        price = item.Price,
                        totalBeforeTax = item.Quantity * item.Price,
                        taxRate = item.Tax,
                        taxAmount = item.TaxAmount,
                        totalAfterTax = item.TotalPrice,
                        discount = item.Discount,
                        discountAmount = item.DiscountAmount,
                        netPrice = item.NetPrice
                    }))
                    .ToList<object>();

                LogService.LogInformation($"Purchase Return VAT Report generated with {reportData.Count} items");
                return reportData;
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error in GetPurchaseReturnVATReport: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GetPurchaseReturnVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar")
        {
            try
            {
                LogService.LogInformation($"GetPurchaseReturnVATDocument called with startDate: {startDate}, endDate: {endDate}, language: {language}");

                // Get VAT report data
                var reportData = await GetPurchaseReturnVATReport(startDate, endDate);
                
                if (reportData == null || !reportData.Any())
                {
                    LogService.LogError($"Purchase Return VAT details data is null or empty for startDate: {startDate}, endDate: {endDate}");
                    throw new InvalidOperationException("Purchase Return VAT details data not found");
                }

                LogService.LogInformation("Fetching company details...");
                var companyDetailsQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>().GetAll();
                var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
                if (companyDetails == null)
                {
                    LogService.LogError("CompanyDetails not found, using default values");
                    companyDetails = new SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails
                    {
                        ArabicName = "الشركة",
                        EnglishName = "Company",
                        TaxNumber = "",
                        Email = "",
                        Website = ""
                    };
                }
                LogService.LogInformation($"Company details found: {companyDetails.ArabicName}");

                LogService.LogInformation("Fetching PDF template config...");
                var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().GetAll();
                var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();
                LogService.LogInformation($"PDF template config found: {pdfTemplateConfig != null}");

                LogService.LogInformation("Creating Purchase Return VAT Report PDF Document...");
                var document = new PurchaseReturnVATReportDocument(
                    reportData.ToList(),
                    companyDetails,
                    pdfTemplateConfig,
                    startDate,
                    endDate,
                    language
                );

                LogService.LogInformation("Generating PDF bytes...");
                var pdfBytes = document.GeneratePdf();
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    LogService.LogError("Generated Purchase Return VAT PDF is null or empty");
                    throw new InvalidOperationException("Failed to generate Purchase Return VAT PDF document");
                }

                LogService.LogInformation($"PDF generated successfully. Size: {pdfBytes.Length} bytes");
                return pdfBytes;
            }
            catch (Exception e)
            {
                LogService.LogError($"GetPurchaseReturnVATDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
                if (e.InnerException != null)
                {
                    LogService.LogError($"Inner exception: {e.InnerException.Message}\nInner StackTrace: {e.InnerException.StackTrace}");
                }
                throw;
            }
        }

        #endregion
    }
}
