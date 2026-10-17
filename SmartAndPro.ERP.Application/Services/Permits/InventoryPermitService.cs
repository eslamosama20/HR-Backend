using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.Application.Services.Reports;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Permits;


public class InventoryPermitService
    : ServiceBase
    <
        InventoryPermit,
        InventoryPermitPostDto,
        InventoryPermitPutDto,
        InventoryPermitResponseDto,
        InventoryPermitFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public InventoryPermitService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }

    public async Task<PermitProductBarcodeResultDto?> GetProductByBarcodeAsync(string barcode)
    {
        var normalizedBarcode = barcode?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(normalizedBarcode))
        {
            return null;
        }

        var mainPurchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
        var purchaseProductQueryResult = await mainPurchaseClassRepository.Filter(x => x.ReferenceCode.ToLower() == normalizedBarcode);
        var purchaseProduct = purchaseProductQueryResult.Data.FirstOrDefault();

        if (purchaseProduct != null)
        {
            return new PermitProductBarcodeResultDto
            {
                Id = purchaseProduct.Id,
                ArabicName = purchaseProduct.ArabicName,
                EnglishName = purchaseProduct.EnglishName,
                ReferenceCode = purchaseProduct.ReferenceCode,
                ProductType = "Purchase"
            };
        }

        var mainSalesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();
        var salesProductQueryResult = await mainSalesClassRepository.Filter(x => x.ReferenceCode.ToLower() == normalizedBarcode);
        var salesProduct = salesProductQueryResult.Data.FirstOrDefault();

        if (salesProduct == null)
        {
            return null;
        }

        return new PermitProductBarcodeResultDto
        {
            Id = salesProduct.Id,
            ArabicName = salesProduct.ArabicName,
            EnglishName = salesProduct.EnglishName,
            ReferenceCode = salesProduct.ReferenceCode,
            ProductType = "Sale"
        };
    }

    public override async Task<OperationResponse<InventoryPermitResponseDto>> AddAsync(InventoryPermitPostDto dto)
    {
        // Log incoming DTO for debugging
        LogService.LogInformation($"[InventoryPermitService] AddAsync called with PermitType: {dto.PermitType}");
        LogService.LogInformation($"[InventoryPermitService] PermitEntries count in DTO: {dto.PermitEntries?.Count ?? 0}");
        if (dto.PermitEntries != null)
        {
            foreach (var entry in dto.PermitEntries)
            {
                LogService.LogInformation($"[InventoryPermitService] DTO Entry: ProductId={entry.MainPurchaseClassId}, Qty={entry.Quantity}, ExpiryDate={entry.ExpiryDate}, ShelfNumber={entry.ShelfNumber}, RowNumber={entry.RowNumber}");
            }
        }
        
        // if it is exit permit, then check if the inventory permit entries exist in stock or not
        if (dto.PermitType == PermitType.Exit.ToString())
        {
            var inventoryPermitEntries = dto.PermitEntries;
            
            // التحقق من المخزون باستخدام أذونات المخزون (Entry - Exit)
            var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
            var allPermitsQuery = await permitRepository.Filter(p => p.StoreDetailsId == dto.StoreDetailsId);
            var allPermits = allPermitsQuery.Data.Include(p => p.PermitEntries).ToList();
            
            var errorMessages = new List<string>();
            
            foreach (var inventoryPermitEntry in inventoryPermitEntries)
            {
                // Skip if MainPurchaseClassId is not provided
                if (!inventoryPermitEntry.MainPurchaseClassId.HasValue)
                {
                    errorMessages.Add("يجب تحديد المنتج لكل صنف في الإذن");
                    continue;
                }
                
                // حساب الكمية المتاحة من أذونات الدخول والخروج
                // التحقق من إجمالي الكمية المتاحة للمنتج في المخزن (بدون اعتبار للموقع/الصلاحية في Validation)
                decimal entryQuantity = 0;
                decimal exitQuantity = 0;
                
                foreach (var permit in allPermits)
                {
                    if (permit.PermitEntries == null) continue;
                    
                    foreach (var entry in permit.PermitEntries)
                    {
                        // مطابقة المنتج فقط (إجمالي الكمية المتاحة في المخزن)
                        bool productMatch = entry.MainPurchaseClassId == inventoryPermitEntry.MainPurchaseClassId.Value;
                        
                        if (productMatch)
                        {
                            if (permit.PermitType == PermitType.Entry)
                            {
                                entryQuantity += entry.Quantity;
                            }
                            else if (permit.PermitType == PermitType.Exit)
                            {
                                exitQuantity += entry.Quantity;
                            }
                        }
                    }
                }
                
                decimal availableQuantity = entryQuantity - exitQuantity;
                
                LogService.LogInformation($"[InventoryPermitService] Stock Check: ProductId={inventoryPermitEntry.MainPurchaseClassId.Value}, Store={dto.StoreDetailsId}");
                LogService.LogInformation($"[InventoryPermitService] Total EntryQty={entryQuantity}, Total ExitQty={exitQuantity}, Available={availableQuantity}, Requested={inventoryPermitEntry.Quantity}");
                
                if (availableQuantity < inventoryPermitEntry.Quantity)
                {
                    // الحصول على اسم المنتج
                    var productId = inventoryPermitEntry.MainPurchaseClassId.Value;
                    var productRepository = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.PurchaseClassesManagment.MainPurchaseClass>();
                    var productQuery = await productRepository.Filter(p => p.Id == productId);
                    var product = productQuery.Data?.FirstOrDefault();
                    var productName = product?.ArabicName ?? product?.EnglishName ?? "Unknown";
                    
                    errorMessages.Add($"{productName}: الكمية المتاحة ({availableQuantity}) أقل من المطلوبة ({inventoryPermitEntry.Quantity})");
                }
            }
            
            if (errorMessages.Count > 0)
            {
                return new OperationResponse<InventoryPermitResponseDto>(ErrorCode: ErrorCodes.UnprocessableEntity,data: null, message : string.Join(Environment.NewLine, errorMessages));
            }
        }
        var result = await base.AddAsync(dto);

        try
        {
            if (result.Data != null)
            {
                // Call the new abstracted method to create and handle the transaction
                await CreateTransactionForInventoryPermitAsync(result.Data.Id);
                var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
                var permitQuery = await permitRepository.Filter(p => p.Id == result.Data.Id);
                var permit = permitQuery.Data.FirstOrDefault();
                if (permit == null)
                {
                    throw new Exception("Permit not found");
                }
                
                // Log permit entries for debugging
                LogService.LogInformation($"[InventoryPermitService] Permit {permit.ReferenceNumber} - PermitType: {permit.PermitType}, StoreId: {permit.StoreDetailsId}");
                LogService.LogInformation($"[InventoryPermitService] PermitEntries count: {permit.PermitEntries?.Count ?? 0}");
                if (permit.PermitEntries != null)
                {
                    foreach (var entry in permit.PermitEntries)
                    {
                        LogService.LogInformation($"[InventoryPermitService] Entry: ProductId={entry.MainPurchaseClassId}, Qty={entry.Quantity}, Price={entry.UnitPrice}");
                    }
                }
                
                var inventoryOperationService = _serviceBaseFactory.GetService<InventoryOperationService>();

                if (permit.PermitType == PermitType.Entry)
                {
                    var inventoryOperation = new InventoryOperationPostDto
                    {
                        Description = $"Inventory Entry Permit #{permit.ReferenceNumber}",
                        OperationDate = permit.PermitDate.ToDateTime(TimeOnly.MinValue),
                        OperationSource = OperationSource.EntryPermit.ToString(),
                        OperationEntries = permit.PermitEntries.Select(pe => new OperationEntryPostDto
                        {
                            MainPurchaseClassId = pe.MainPurchaseClassId,
                            Quantity = pe.Quantity,
                            UnitPrice = pe.UnitPrice,
                            StoreDetailsId = permit.StoreDetailsId
                        }).ToList(),
                        InventoryPermitId = permit.Id,
                        TransactionId = permit.TransactionId
                    };
                    LogService.LogInformation($"[InventoryPermitService] Creating Entry InventoryOperation with {inventoryOperation.OperationEntries.Count} entries");
                    var opResult = await inventoryOperationService.AddAsync(inventoryOperation);
                    LogService.LogInformation($"[InventoryPermitService] Entry InventoryOperation result: ErrorCode={opResult.ErrorCode}, Id={opResult.Data?.Id}");
                }
                else if (permit.PermitType == PermitType.Exit)
                {
                    var inventoryOperation = new InventoryOperationPostDto
                        {
                            Description = $"Inventory Exit Permit #{permit.ReferenceNumber}",
                            OperationDate = permit.PermitDate.ToDateTime(TimeOnly.MinValue),
                            OperationSource = OperationSource.ExitPermit.ToString(),
                            OperationEntries = permit.PermitEntries.Select(pe => new OperationEntryPostDto
                            {
                                MainPurchaseClassId = pe.MainPurchaseClassId,
                                Quantity = -pe.Quantity,
                                UnitPrice = pe.UnitPrice,
                                StoreDetailsId = permit.StoreDetailsId
                            }).ToList(),
                            InventoryPermitId = permit.Id,
                            TransactionId = permit.TransactionId
                        };
                    LogService.LogInformation($"[InventoryPermitService] Creating Exit InventoryOperation with {inventoryOperation.OperationEntries.Count} entries, Quantities: {string.Join(", ", inventoryOperation.OperationEntries.Select(e => e.Quantity))}");
                    var opResult = await inventoryOperationService.AddAsync(inventoryOperation);
                    LogService.LogInformation($"[InventoryPermitService] Exit InventoryOperation result: ErrorCode={opResult.ErrorCode}, Id={opResult.Data?.Id}");
                }
            }
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }

        return result;
    }
    private async Task CreateTransactionForInventoryPermitAsync(Guid inventoryPermitId)
    {
        var inventoryPermitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
        var inventoryPermitQuery = await inventoryPermitRepository.Filter(p => p.Id == inventoryPermitId);
        var inventoryPermit = inventoryPermitQuery.Data.FirstOrDefault();
        if (inventoryPermit == null)
        {
            return;
        }

        var permitEntries = inventoryPermit.PermitEntries;
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var storeDetailsRepository = _repositoriesFactory.GetRepository<StoreDetails>();

        // Fetch the transaction type
        var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
            .Filter(tt => tt.ArabicName.Contains("إذن مخزن"));
        var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
        if (transactionTypeId == null)
        {
            return; // Transaction Type not found
        }

        var mainAccountQuery = await mainAccountRepository.Filter(ma => ma.Id == inventoryPermit.MainAccountId);
        var mainAccount = mainAccountQuery.Data.FirstOrDefault();
        if (mainAccount == null)
        {
            return; // Main account not found
        }

        var storeDetailsQuery = await storeDetailsRepository.Filter(sd => sd.Id == inventoryPermit.StoreDetailsId);
        var storeDetails = storeDetailsQuery.Data.FirstOrDefault();
        if (storeDetails == null)
        {
            return; // Store account not found
        }

        var transaction = new Transaction
        {
            TransactionTypeId = transactionTypeId.Value,
            Description = $"({inventoryPermit.PermitType.ToString()}) {inventoryPermit.Description}",
            TransactionDate = inventoryPermit.PermitDate,
            TransactionEntries = new List<TransactionEntry>(),
            InventoryPermitId = inventoryPermit.Id
        };

        var totalAmount = inventoryPermit.PermitEntries.Sum(e => e.UnitPrice * e.Quantity);

        // Adjust debit and credit entries based on PermitType
        if (inventoryPermit.PermitType == PermitType.Entry)
        {
            // Store Account - Credit
            var storeEntry = new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalAmount,
                MainAccountId = (Guid)storeDetails.MainAccountId,
                Description = inventoryPermit.Description,
            };
            transaction.TransactionEntries.Add(storeEntry);

            // Main Account - Debit
            var mainEntry = new TransactionEntry
            {
                CreditAmount = totalAmount,
                DebitAmount = 0,
                MainAccountId = mainAccount.Id,
                Description = inventoryPermit.Description
            };
            transaction.TransactionEntries.Add(mainEntry);
        }
        else if (inventoryPermit.PermitType == PermitType.Exit)
        {
            // Store Account - Debit
            var storeEntry = new TransactionEntry
            {
                CreditAmount = totalAmount,
                DebitAmount = 0,
                MainAccountId = (Guid)storeDetails.MainAccountId,
                Description = inventoryPermit.Description,
            };
            transaction.TransactionEntries.Add(storeEntry);

            // Main Account - Credit
            var mainEntry = new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalAmount,
                MainAccountId = mainAccount.Id,
                Description = inventoryPermit.Description
            };
            transaction.TransactionEntries.Add(mainEntry);
        }

        // Validate and save the transaction
        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
        if (enforcerResult.IsSuccess)
        {
            var result = await transactionRepository.Add(transaction);
            if (result.ErrorCode == ErrorCodes.Success)
            {
                inventoryPermit.TransactionId = result.Data.Id;
                inventoryPermit.PermitEntries = null;
                await inventoryPermitRepository.Update(inventoryPermit);
            }
        }
    }

    public override async Task<OperationResponse<InventoryPermitResponseDto>> UpdateAsync(InventoryPermitPutDto inventoryPermitPut)
    {
        var inventoryPermit = _mapper.Map<InventoryPermit>(inventoryPermitPut);
        GuidHelper.NormalizeGuids<InventoryPermit>(inventoryPermit);

        var result = new OperationResponse<InventoryPermitResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            var permitEntries = inventoryPermitPut.PermitEntries;
            var existingPermitEntriesQuery = await _repositoriesFactory.GetRepository<PermitEntry>()
                .Filter(x => x.InventoryPermitId == inventoryPermit.Id);
            var existingPermitEntries = existingPermitEntriesQuery.Data!.ToList();

            // Update or Add permit entries
            foreach (var permitEntry in permitEntries)
            {
                var entryEntity = _mapper.Map<PermitEntry>(permitEntry);
                GuidHelper.NormalizeGuids<PermitEntry>(entryEntity);

                var existingEntry = existingPermitEntries.FirstOrDefault(x => x.Id == permitEntry.Id);

                if (existingEntry != null)
                {
                    await _repositoriesFactory.GetRepository<PermitEntry>().Update(entryEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<PermitEntry>().Add(entryEntity);
                }
            }

            // Delete removed permit entries
            var entriesToDelete = existingPermitEntries
                .Where(x => !permitEntries.Any(pe => pe.Id == x.Id)).ToList();

            foreach (var entryToDelete in entriesToDelete)
            {
                await _repositoriesFactory.GetRepository<PermitEntry>().Delete(entryToDelete);
            }

            inventoryPermit.PermitEntries = null;
            var operation = await _repositoriesFactory.GetRepository<InventoryPermit>().Update(inventoryPermit);

            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<InventoryPermitResponseDto>(operation.ErrorCode, null);

            var responseData = _mapper.Map<InventoryPermitResponseDto>(operation.Data);

            // Sync InventoryOperations
            await SyncInventoryOperationsAsync(inventoryPermit);

            await UpdateTransactionForInventoryPermitAsync(inventoryPermit.Id);

            return new OperationResponse<InventoryPermitResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    private async Task SyncInventoryOperationsAsync(InventoryPermit inventoryPermit)
    {
        var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
        var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
        // Fetch and delete existing operations
        var existingOperationsQuery = await inventoryOperationRepository
            .Filter(io => io.InventoryPermitId == inventoryPermit.Id);
        var existingOperations = existingOperationsQuery.Data
            .Include(io => io.OperationEntries)
            .IgnoreAutoIncludes()
            .ToList();

        var existingOperationsCopy = existingOperations.ToList();
        foreach (var operation in existingOperationsCopy)
        {
            var operationEntriesCopy = operation.OperationEntries.ToList();
            foreach (var operationEntry in operationEntriesCopy)
            {
                await operationEntryRepository.Delete(operationEntry);
            }
            operation.OperationEntries = null;
            
            await inventoryOperationRepository.Delete(operation);
        }

        // Recreate inventory operations based on updated permit data
        var inventoryOperation = new InventoryOperationPostDto
        {
            Description = inventoryPermit.PermitType == PermitType.Entry
                ? $"Inventory Entry Permit #{inventoryPermit.ReferenceNumber}"
                : $"Inventory Exit Permit #{inventoryPermit.ReferenceNumber}",
            OperationDate = inventoryPermit.PermitDate.ToDateTime(TimeOnly.MinValue),
            OperationSource = inventoryPermit.PermitType == PermitType.Entry
                ? OperationSource.EntryPermit.ToString()
                : OperationSource.ExitPermit.ToString(),
            OperationEntries = inventoryPermit.PermitEntries.Select(pe => new OperationEntryPostDto
            {
                MainPurchaseClassId = pe.MainPurchaseClassId,
                Quantity = inventoryPermit.PermitType == PermitType.Entry ? pe.Quantity : -pe.Quantity,
                UnitPrice = pe.UnitPrice,
                StoreDetailsId = inventoryPermit.StoreDetailsId
            }).ToList(),
            InventoryPermitId = inventoryPermit.Id,
            TransactionId = inventoryPermit.TransactionId
        };

        await _serviceBaseFactory.GetService<InventoryOperationService>().AddAsync(inventoryOperation);
    }
    
    private async Task UpdateTransactionForInventoryPermitAsync(Guid inventoryPermitId)
    {
        var inventoryPermitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
        var inventoryPermitQuery = await inventoryPermitRepository.Filter((p) => p.Id == inventoryPermitId);
        var inventoryPermit = inventoryPermitQuery.Data.FirstOrDefault();
        if (inventoryPermit ==null || inventoryPermit.TransactionId is null) return;

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();

        // Fetch the existing transaction associated with the InventoryPermit
        var transactionQuery = await transactionRepository.Filter(t => t.InventoryPermitId == inventoryPermit.Id);
        var transaction = transactionQuery.Data.FirstOrDefault();

        if (transaction == null)
            return; // Transaction not found

        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
        var existingTransactionEntriesQuery = await transactionEntryRepository
            .Filter(te => te.TransactionId == transaction.Id);
        var existingTransactionEntries = existingTransactionEntriesQuery.Data.ToList();

        foreach (var existingEntry in existingTransactionEntries)
        {
            existingEntry.MainAccount = null;
            existingEntry.Transaction = null;
            await transactionEntryRepository.Delete(existingEntry);
        }

        // Recalculate and add new transaction entries
        var permitEntries = inventoryPermit.PermitEntries;
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var storeDetailsRepository = _repositoriesFactory.GetRepository<StoreDetails>();

        var mainAccountQuery = await mainAccountRepository.Filter(ma => ma.Id == inventoryPermit.MainAccountId);
        var mainAccount = mainAccountQuery.Data.FirstOrDefault();
        if (mainAccount == null)
        {
            return; // Main account not found
        }

        var storeDetailsQuery = await storeDetailsRepository.Filter(sd => sd.Id == inventoryPermit.StoreDetailsId);
        var storeDetails = storeDetailsQuery.Data.FirstOrDefault();
        if (storeDetails == null)
        {
            return; // Store account not found
        }
        
        var transactionEntries = new System.Collections.Generic.List<TransactionEntry>();
        
        var totalAmount = inventoryPermit.PermitEntries.Sum(e => e.UnitPrice * e.Quantity);

        // Adjust debit and credit entries based on PermitType
        if (inventoryPermit.PermitType == PermitType.Entry)
        {
            // Store Account - Credit
            var storeEntry = new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalAmount,
                MainAccountId = (Guid)storeDetails.MainAccountId,
                Description = inventoryPermit.Description,
                TransactionId = transaction.Id
            };
            transactionEntries.Add(storeEntry);

            // Main Account - Debit
            var mainEntry = new TransactionEntry
            {
                CreditAmount = totalAmount,
                DebitAmount = 0,
                MainAccountId = mainAccount.Id,
                Description = inventoryPermit.Description,
                TransactionId = transaction.Id
            };
            transactionEntries.Add(mainEntry);
        }
        else if (inventoryPermit.PermitType == PermitType.Exit)
        {
            // Store Account - Debit
            var storeEntry = new TransactionEntry
            {
                CreditAmount = totalAmount,
                DebitAmount = 0,
                MainAccountId = (Guid)storeDetails.MainAccountId,
                Description = inventoryPermit.Description,
                TransactionId = transaction.Id
            };
            transactionEntries.Add(storeEntry);

            // Main Account - Credit
            var mainEntry = new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = totalAmount,
                MainAccountId = mainAccount.Id,
                Description = inventoryPermit.Description,
                TransactionId = transaction.Id
            };
            transactionEntries.Add(mainEntry);
        }
        await transactionEntryRepository.AddRange(transactionEntries);
    }

    public override async Task<OperationResponse<InventoryPermitResponseDto>> DeleteAsync(Guid permitId)
    {
        var result = new OperationResponse<InventoryPermitResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
            var permitEntryRepository = _repositoriesFactory.GetRepository<PermitEntry>();
            var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();

            // Fetch the InventoryPermit
            var permitQuery = await permitRepository.Filter(p => p.Id == permitId);
            var permit = permitQuery.Data.FirstOrDefault();
            if (permit == null)
            {
                return new OperationResponse<InventoryPermitResponseDto>(ErrorCodes.NotFound, null);
            }

            // Undo related InventoryOperations
            var inventoryOperationQuery = await inventoryOperationRepository.Filter(io => io.InventoryPermitId == permitId);
            var inventoryOperations = inventoryOperationQuery.Data.ToList();

            foreach (var inventoryOperation in inventoryOperations)
            {
                // Delete the operation's entries (if any)
                var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
                var operationEntriesQuery = await operationEntryRepository.Filter(oe => oe.InventoryOperationId == inventoryOperation.Id);
                var operationEntries = operationEntriesQuery.Data.ToList();

                foreach (var operationEntry in operationEntries)
                {
                    await operationEntryRepository.Delete(operationEntry);
                }

                // Delete the operation itself
                await inventoryOperationRepository.Delete(inventoryOperation);
            }

            // Delete PermitEntries
            var permitEntries = permit.PermitEntries;
            foreach (var permitEntry in permitEntries)
            {
                permitEntry.MainSalesClass = null;
                permitEntry.MainPurchaseClass = null;
                await permitEntryRepository.Delete(permitEntry);
            }

            // Delete the Transaction
            var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
            var transactionQuery = await transactionRepository.Filter(t => t.InventoryPermitId == permitId);
            var transaction = transactionQuery.Data.FirstOrDefault();
            if (transaction != null)
            {
                await _serviceBaseFactory.GetService<TransactionService>().DeleteAsync(transaction.Id);
            }

            // Clear relationships in the permit
            permit.StoreDetails = null;
            permit.MainAccount = null;
            permit.PermitEntries = null;

            // Delete the InventoryPermit
            await permitRepository.Delete(permit);

            return new OperationResponse<InventoryPermitResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    public async Task<byte[]?> GetInventoryReportDocument(Guid? storeId, DateTime? startDate = null, DateTime? endDate = null, string language = "ar", string? printedBy = null)
    {
        try
        {
            // Get company details with related data
            // Note: Address and ArabicAddress are Value Objects, not navigation properties, so they don't need Include
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?
                .Include(c => c.Phones)
                .FirstOrDefault();
            if (companyDetails == null)
            {
                LogService.LogError("Failed to retrieve company details for inventory report");
                return null;
            }

            var isRtl = language?.ToLower() == "ar";

            // Get inventory stocks using InventoryStocksService
            var inventoryStocksService = _serviceBaseFactory.GetService<Reports.InventoryStocksService>();
            var stocks = await inventoryStocksService.GetAllStocks(storeId);

            var reportItems = new List<DocumentUtils.InventoryReportItem>();
            string? storeName = null;

            foreach (var stock in stocks)
            {
                if (storeId.HasValue && string.IsNullOrEmpty(storeName))
                {
                    storeName = isRtl 
                        ? (stock.StoreDetailsArabicName ?? stock.StoreDetailsEnglishName)
                        : (stock.StoreDetailsEnglishName ?? stock.StoreDetailsArabicName);
                }

                foreach (var item in stock.StoredItems)
                {
                    reportItems.Add(new DocumentUtils.InventoryReportItem
                    {
                        ProductName = isRtl 
                            ? (item.MainPurchaseClassArabicName ?? "غير محدد")
                            : (item.MainPurchaseClassEnglishName ?? item.MainPurchaseClassArabicName ?? "Unknown"),
                        ProductCode = item.MainPurchaseClassReferenceCode ?? "-",
                        Quantity = item.Quantity,
                        UnitName = isRtl 
                            ? (item.UnitArabicName ?? "-")
                            : (item.UnitEnglishName ?? item.UnitArabicName ?? "-"),
                        StoreName = isRtl 
                            ? (stock.StoreDetailsArabicName ?? stock.StoreDetailsEnglishName ?? "-")
                            : (stock.StoreDetailsEnglishName ?? stock.StoreDetailsArabicName ?? "-"),
                        AverageCost = item.LastPrice, // Use LastPrice as average cost
                        TotalValue = item.Quantity * item.LastPrice
                    });
                }
            }

            // Generate PDF
            var document = new DocumentUtils.InventoryReportDocument(
                reportItems,
                companyDetails,
                storeName,
                language ?? "ar",
                printedBy
            );

            var pdfBytes = document.GeneratePDF();
            return pdfBytes;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return null;
        }
    }

    public async Task<byte[]?> GetStoreTransferReportDocument(DateTime? startDate = null, DateTime? endDate = null, string language = "ar", string? printedBy = null)
    {
        try
        {
            // Get company details with related data
            // Note: Address and ArabicAddress are Value Objects, not navigation properties, so they don't need Include
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?
                .Include(c => c.Phones)
                .FirstOrDefault();
            if (companyDetails == null)
            {
                LogService.LogError("Failed to retrieve company details for store transfer report");
                return null;
            }

            // Get all permits
            var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
            var permitsResult = await permitRepository.GetAll();
            
            if (permitsResult.Data == null)
            {
                return null;
            }

            var permits = await permitsResult.Data
                .Include(p => p.StoreDetails)
                .Include(p => p.PermitEntries)
                    .ThenInclude(e => e.MainPurchaseClass)
                .Include(p => p.PermitEntries)
                    .ThenInclude(e => e.MainSalesClass)
                .ToListAsync();

            // Group permits by reference number to find transfer pairs
            var transferMap = new Dictionary<string, List<InventoryPermit>>();
            
            foreach (var permit in permits)
            {
                if (!string.IsNullOrEmpty(permit.ReferenceNumber) && 
                    (permit.ReferenceNumber.StartsWith("EXIT-") || permit.ReferenceNumber.StartsWith("ENTRY-")))
                {
                    var baseRef = System.Text.RegularExpressions.Regex.Replace(permit.ReferenceNumber, @"^(EXIT|ENTRY)-", "");
                    
                    if (!transferMap.ContainsKey(baseRef))
                    {
                        transferMap[baseRef] = new List<InventoryPermit>();
                    }
                    transferMap[baseRef].Add(permit);
                }
            }

            var reportItems = new List<DocumentUtils.StoreTransferReportItem>();

            foreach (var kvp in transferMap)
            {
                var exitPermit = kvp.Value.FirstOrDefault(p => p.PermitType == PermitType.Exit);
                var entryPermit = kvp.Value.FirstOrDefault(p => p.PermitType == PermitType.Entry);

                if (exitPermit != null && entryPermit != null)
                {
                    // Apply date filters - convert DateOnly to DateTime for comparison
                    var permitDateTime = exitPermit.PermitDate.ToDateTime(TimeOnly.MinValue);
                    if (startDate.HasValue && permitDateTime < startDate.Value)
                        continue;
                    if (endDate.HasValue && permitDateTime > endDate.Value)
                        continue;

                    // Calculate total quantity
                    var totalQuantity = exitPermit.PermitEntries?.Sum(e => e.Quantity) ?? 0;

                    // Get product names
                    var productNames = exitPermit.PermitEntries?
                        .Select(e => e.MainPurchaseClass?.ArabicName ?? e.MainSalesClass?.ArabicName ?? "صنف")
                        .Where(n => !string.IsNullOrEmpty(n))
                        .Take(3)
                        .ToList();
                    
                    var products = productNames != null && productNames.Any() 
                        ? string.Join(", ", productNames) + (exitPermit.PermitEntries?.Count > 3 ? $" (+{exitPermit.PermitEntries.Count - 3} آخرين)" : "")
                        : "-";

                    reportItems.Add(new DocumentUtils.StoreTransferReportItem
                    {
                        ReferenceNumber = kvp.Key,
                        TransferDate = permitDateTime,
                        FromStore = exitPermit.StoreDetails?.ArabicName ?? exitPermit.StoreDetails?.EnglishName ?? "-",
                        ToStore = entryPermit.StoreDetails?.ArabicName ?? entryPermit.StoreDetails?.EnglishName ?? "-",
                        Products = products,
                        TotalQuantity = totalQuantity,
                        ShippingCost = exitPermit.ShippingCost,
                        Status = "مكتمل"
                    });
                }
            }

            // Sort by date descending
            reportItems = reportItems.OrderByDescending(x => x.TransferDate).ToList();

            // Generate PDF
            var document = new DocumentUtils.StoreTransferReportDocument(reportItems, companyDetails, language ?? "ar", printedBy);
            var pdfBytes = document.GeneratePDF();
            return pdfBytes;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return null;
        }
    }

    /// <summary>
    /// Get inventory permits report as PDF document
    /// </summary>
    public async Task<byte[]?> GetPermitsReportDocument(DateTime? startDate = null, DateTime? endDate = null, string language = "ar", string? printedBy = null)
    {
        try
        {
            // Get company details
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?
                .Include(c => c.Phones)
                .FirstOrDefault();
            if (companyDetails == null)
            {
                LogService.LogError("Failed to retrieve company details for permits report");
                return null;
            }

            // Get all inventory permits
            var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
            var permitsQuery = await permitRepository.GetAll();
            var permits = permitsQuery.Data
                .Include(p => p.StoreDetails)
                .Include(p => p.PermitEntries)
                .AsQueryable();

            // Apply date filters
            if (startDate.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                permits = permits.Where(p => p.PermitDate >= startDateOnly);
            }
            if (endDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);
                permits = permits.Where(p => p.PermitDate <= endDateOnly);
            }

            // Order by date descending
            var permitsList = permits.OrderByDescending(p => p.PermitDate).ToList();

            // Map to DTOs
            var permitDtos = _mapper.Map<List<InventoryPermitResponseDto>>(permitsList);

            // Generate PDF
            var document = new DocumentUtils.PermitsReportDocument(
                permitDtos,
                companyDetails,
                startDate,
                endDate,
                language ?? "ar",
                printedBy
            );

            var pdfBytes = document.GeneratePDF();
            return pdfBytes;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return null;
        }
    }
}
