using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.SharedKernel.Exceptions;
using SmartAndPro.ERP.SharedKernel.BusinessRule;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;

namespace SmartAndPro.ERP.Application.Services.Invoice.Purchase
{
    public class PurchaseInvoiceService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;

        public PurchaseInvoiceService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory)

        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
        }
        #region OldGetAllFrontal
        //public async Task<OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    try
        //    {
        //        var filterOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
        //            .Filter(i => i.IsActive == true && i.IsFrontal == true);

        //        if (filterOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(filterOperation.ErrorCode, null);
        //        }

        //        var query = filterOperation.Data.AsNoTracking();

        //        var totalRecords = await query.CountAsync();

        //        pageNumber ??= 1;
        //        pageSize ??= 10;

        //        var skip = (pageNumber.Value - 1) * pageSize.Value;

        //        var invoiceIds = await query
        //            .OrderByDescending(i => i.CreatedAt)
        //            .Skip(skip)
        //            .Take(pageSize.Value)
        //            .Select(i => i.Id)
        //            .ToListAsync();

        //        if (!invoiceIds.Any())
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(
        //                ErrorCodes.Success,
        //                new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, new List<PurchaseInvoiceDto>())
        //            );
        //        }

        //        var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
        //            .Filter(i => invoiceIds.Contains(i.Id));

        //        if (invoicesOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(invoicesOperation.ErrorCode, null);
        //        }

        //        var invoicesEntities = await invoicesOperation.Data
        //            .AsNoTracking()
        //            .ToListAsync();

        //        var invoicesDto = _mapper.Map<List<PurchaseInvoiceDto>>(invoicesEntities);

        //        var result = new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, invoicesDto);

        //        return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(ErrorCodes.Success, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(ErrorCodes.GeneralFail, null, message: ex.Message);
        //    }
        //}
        #endregion
        public async Task<OperationResponse<object>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var filterOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
                    .Filter(i => i.IsActive == true && i.IsFrontal == true);

                if (filterOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(filterOperation.ErrorCode, null);

                var query = filterOperation.Data.AsNoTracking();
                var totalRecords = await query.CountAsync();

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    var allInvoices = await query
                        .OrderByDescending(i => i.CreatedAt)
                        .ToListAsync();

                    var invoicesDto = _mapper.Map<List<PurchaseInvoiceDto>>(allInvoices);

                    return new OperationResponse<object>(
                        ErrorCodes.Success,
                        invoicesDto
                    );
                }

                pageNumber ??= 1;
                pageSize ??= 10;
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                var invoiceIds = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize.Value)
                    .Select(i => i.Id)
                    .ToListAsync();

                if (!invoiceIds.Any())
                {
                    return new OperationResponse<object>(
                        ErrorCodes.Success,
                        new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, new List<PurchaseInvoiceDto>())
                    );
                }

                var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
                    .Filter(i => invoiceIds.Contains(i.Id));

                if (invoicesOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(invoicesOperation.ErrorCode, null);

                var invoicesEntities = await invoicesOperation.Data
                    .AsNoTracking()
                    .ToListAsync();

                var invoicesDtoPaged = _mapper.Map<List<PurchaseInvoiceDto>>(invoicesEntities);

                var paginatedResult = new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, invoicesDtoPaged);

                return new OperationResponse<object>(
                    ErrorCodes.Success,
                    paginatedResult
                );
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }

        public async Task<PurchaseInvoice> MakeFrontalAsync(int purchaseId)
        {
            var purchaseInvoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();
            var purchaseInvoiceQuery = await purchaseInvoiceRepository.Filter(x => x.Id == purchaseId);
            var purchaseInvoice = purchaseInvoiceQuery.Data?
                .IgnoreAutoIncludes()
                .FirstOrDefault();
            if (purchaseInvoice == null)
            {
                return null;
            }
            // if the invoice has a transaction use transaction service to update the transaction 
            if (purchaseInvoice.TransactionId != null)
            {
                var transactionService = _serviceBaseFactory.GetService<TransactionService>();
                var updateTransactionResult = await transactionService.MakeFrontalAsync(purchaseInvoice.TransactionId.Value);
                if (updateTransactionResult.ErrorCode != ErrorCodes.Success)
                {
                    return null; // Transaction update failed
                }
            }
            purchaseInvoice.IsFrontal = true;
            var updateResult = await purchaseInvoiceRepository.Update(purchaseInvoice);

            return updateResult.Data;
        }


        public async Task<string> GetLastInvoiceNo()
        {
            var invoicesQuery = await _repositoriesFactory.GetRepository<PurchaseInvoice>().GetAll();
            var invoices = await invoicesQuery.Data.ToListAsync();
            if (!invoices.Any()) return await Task.FromResult(string.Empty);

            var invoice = invoices.OrderBy(i => i.Id).Last();
            return invoice.InvoiceNo;
        }

        #region OldGetAll
        //public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
        //{

        //    var invoicesQuery = await _repositoriesFactory.GetRepository<PurchaseInvoice>().GetAll();
        //    return await invoicesQuery.Data.ToListAsync();
        //}

        //public async Task<OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    try
        //    {
        //        var filterOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
        //            .Filter(i => i.IsActive == true);

        //        if (filterOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(filterOperation.ErrorCode, null);
        //        }

        //        var query = filterOperation.Data.AsNoTracking();

        //        var totalRecords = await query.CountAsync();

        //        pageNumber ??= 1;
        //        pageSize ??= 10;

        //        var skip = (pageNumber.Value - 1) * pageSize.Value;

        //        var invoiceIds = await query
        //            .OrderByDescending(i => i.CreatedAt)
        //            .Skip(skip)
        //            .Take(pageSize.Value)
        //            .Select(i => i.Id)
        //            .ToListAsync();

        //        if (!invoiceIds.Any())
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(
        //                ErrorCodes.Success,
        //                new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, new List<PurchaseInvoiceDto>())
        //            );
        //        }

        //        var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
        //            .Filter(i => invoiceIds.Contains(i.Id));

        //        if (invoicesOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(invoicesOperation.ErrorCode, null);
        //        }

        //        var invoicesEntities = await invoicesOperation.Data
        //            .AsNoTracking()
        //            .ToListAsync();

        //        var invoicesDto = _mapper.Map<List<PurchaseInvoiceDto>>(invoicesEntities);

        //        var result = new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, invoicesDto);

        //        return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(ErrorCodes.Success, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return new OperationResponse<PaginatedResult<List<PurchaseInvoiceDto>>>(ErrorCodes.GeneralFail, null, message: ex.Message);
        //    }
        //}

        #endregion


        public async Task<OperationResponse<object>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var filterOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
                    .Filter(i => i.IsActive == true);

                if (filterOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(filterOperation.ErrorCode, null);

                var query = filterOperation.Data.AsNoTracking();
                var totalRecords = await query.CountAsync();

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    var allInvoices = await query
                        .OrderByDescending(i => i.CreatedAt)
                        .ToListAsync();

                    var invoicesDto = _mapper.Map<List<PurchaseInvoiceDto>>(allInvoices);

                    return new OperationResponse<object>(
                        ErrorCodes.Success,
                        invoicesDto
                    );
                }

                pageNumber ??= 1;
                pageSize ??= 10;
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                var invoiceIds = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize.Value)
                    .Select(i => i.Id)
                    .ToListAsync();

                if (!invoiceIds.Any())
                {
                    return new OperationResponse<object>(
                        ErrorCodes.Success,
                        new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, new List<PurchaseInvoiceDto>())
                    );
                }

                var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseInvoice>()
                    .Filter(i => invoiceIds.Contains(i.Id));

                if (invoicesOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(invoicesOperation.ErrorCode, null);

                var invoicesEntities = await invoicesOperation.Data
                    .AsNoTracking()
                    .ToListAsync();

                var invoicesDtoPaged = _mapper.Map<List<PurchaseInvoiceDto>>(invoicesEntities);

                var paginatedResult = new PaginatedResult<List<PurchaseInvoiceDto>>(totalRecords, invoicesDtoPaged);

                return new OperationResponse<object>(
                    ErrorCodes.Success,
                    paginatedResult
                );
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }
   
        public async Task<PurchaseInvoice> GetFirstAsync()
        {
            var invoicesQuery = await _repositoriesFactory.GetRepository<PurchaseInvoice>().GetAll();
            var invoices = await invoicesQuery.Data.FirstOrDefaultAsync();
            return invoices;
        }
        public async Task<PurchaseInvoice> GetByIdAsync(int id)
        {
            var invoicesQuery = await _repositoriesFactory
                .GetRepository<PurchaseInvoice>().GetAll();
            return await invoicesQuery.Data.FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<int> AddAsync(PurchaseInvoice invoice)
        {
            var repository = _repositoriesFactory.GetRepository<PurchaseInvoice>();

            if (!string.IsNullOrWhiteSpace(invoice.ReferenceNumber))
            {
                var referenceExists = await repository.Filter(i => i.ReferenceNumber == invoice.ReferenceNumber);
                if (await referenceExists.Data.AnyAsync())
                {
                    throw new BusinessRuleViolationException(new List<BusinessRuleError> 
                    { 
                        new BusinessRuleError($"رقم المرجع {invoice.ReferenceNumber} موجود بالفعل، الرجاء إدخال رقم مرجع غير مكرر.", "ReferenceNumber") 
                    });
                }
            }

            // Validate batch/expiry requirements based on item master settings
            if (invoice.PurchaseItems != null && invoice.PurchaseItems.Any())
            {
                var itemClassIds = invoice.PurchaseItems
                    .Select(pi => pi.MainPurchaseClassId)
                    .Distinct()
                    .ToList();

                var classRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var classQuery = await classRepo.Filter(c => itemClassIds.Contains(c.Id));
                var itemClasses = await classQuery.Data.ToListAsync();
                var classLookup = itemClasses.ToDictionary(c => c.Id);

                var errors = new List<BusinessRuleError>();
                foreach (var item in invoice.PurchaseItems)
                {
                    if (classLookup.TryGetValue(item.MainPurchaseClassId, out var itemClass))
                    {
                        if (itemClass.NeedsBatch && string.IsNullOrWhiteSpace(item.BatchNo))
                            errors.Add(new BusinessRuleError($"الصنف '{itemClass.ArabicName ?? itemClass.EnglishName}' يتطلب إدخال رقم التشغيلة (Batch No).", "BatchNo"));

                        if (itemClass.NeedsExpiry && !item.ExpiryDate.HasValue)
                            errors.Add(new BusinessRuleError($"الصنف '{itemClass.ArabicName ?? itemClass.EnglishName}' يتطلب إدخال تاريخ الصلاحية (Expiry Date).", "ExpiryDate"));
                    }
                }

                if (errors.Any())
                    throw new BusinessRuleViolationException(errors);
            }

            var resultQuery = await repository.Add(invoice);
            
            if (resultQuery.ErrorCode == ErrorCodes.Success && resultQuery.Data != null)
            {
                // Check if this invoice is converted from a Quotation
                if (invoice.PurchaseQuotationId.HasValue && invoice.PurchaseQuotationId.Value > 0)
                {
                    var quotationRepo = _repositoriesFactory.GetRepository<PurchaseQuotation>();
                    var quotationResult = await quotationRepo.Filter(q => q.Id == invoice.PurchaseQuotationId.Value);
                    
                    if (quotationResult.ErrorCode == ErrorCodes.Success && quotationResult.Data.Any())
                    {
                        var quotation = await quotationResult.Data.FirstOrDefaultAsync();
                        if (quotation != null)
                        {
                            quotation.Status = PurchaseQuotationStatus.ConvertedToInvoice;
                            //quotation.PurchaseInvoiceId = invoice.Id; // Entity property is not active/needed as per relationship direction
                            await quotationRepo.Update(quotation);
                        }
                    }
                }
                
                return 1;
            }
            
            return 0;
        }
        public async Task<int> UpdateTransaction(int? invoiceId, Guid? transactionId)
        {
            try
            {
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
                var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();


                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var invoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();

                var invoiceQuery = await invoiceRepository.Filter(x => x.Id == invoiceId);
                var invoice = invoiceQuery.Data.FirstOrDefault();
                if (invoice == null) return 0;

                var transactionQuery = await transactionRepository.Filter(x => x.Id == transactionId);
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

                // Retrieve the "Purchase Tax" MainAccount
                var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
                var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
                if (purchaseTaxAccount == null)
                {
                    return 0; // Purchase Tax account not found
                }

                // Retrieve Transaction Type for invoice transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد فاتورة"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on invoice items
                var transactionEntries = new List<TransactionEntry>();

                var purchaseTotalAmount = invoice.PurchaseItems.Sum(item => item.Price * item.Quantity);
                if (invoice.TaxAmount > 0)
                {
                    var purchaseTaxAmount = purchaseTotalAmount * (decimal)0.15;
                    purchaseTotalAmount += purchaseTaxAmount;
                    var taxEntry = new TransactionEntry
                    {
                        TransactionId = transaction.Id,
                        MainAccountId = purchaseTaxAccount.Id,
                        CreditAmount = 0, // Assuming TaxAmount is what you want to credit
                        DebitAmount = purchaseTaxAmount,
                        Description = $"Invoice #{invoice.InvoiceNo} - Purchase Tax",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(taxEntry);
                }

                var supplier = invoice.SupplierDetails;
                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    TransactionId = transaction.Id,
                    MainAccountId = supplier.MainAccountId.Value,
                    DebitAmount = 0,
                    CreditAmount = purchaseTotalAmount,
                    Description = $"Invoice #{invoice.InvoiceNo} (Supplier)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);

                // 3. Group entries by store
                var storeGroups = invoice.PurchaseItems
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
                        DebitAmount = storeTotal,
                        CreditAmount = 0,
                        Description = $"Invoice #{invoice.InvoiceNo} (Store)",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(storeEntry);
                }

                var additionResult = await transactionEntryRepository.AddRange(transactionEntries);
                if (additionResult.Data.Count() == transactionEntries.Count)
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
        public async Task<int> CreateTransactionAndAddProductUnits(int id)
        {
            try
            {
                var invoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                var supplierRepository = _repositoriesFactory.GetRepository<SupplierDetails>();
                var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();

                var invoiceQuery = await invoiceRepository.Filter(i => i.Id == id);
                var invoice = invoiceQuery.Data.FirstOrDefault();
                if (invoice == null)
                {
                    return 0; // Invoice not found
                }

                var supplierQuery = await supplierRepository.Filter(s => s.Id == invoice.SupplierDetailsId);
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

                // Retrieve the "Purchase Tax" MainAccount
                var purchaseTaxAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
                var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
                if (purchaseTaxAccount == null)
                {
                    return 0; // Purchase Tax account not found
                }

                // Retrieve Transaction Type for invoice transaction
                var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
                    .Filter(tt => tt.ArabicName.Contains("قيد فاتورة"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null)
                {
                    return 0; // Transaction Type not found
                }

                // Create transaction entries based on invoice items
                var transactionEntries = new List<TransactionEntry>();

                // calculate purchase tax amount and total amount from purchase items
                var purchaseTotalAmount = invoice.PurchaseItems.Sum(item => item.Price * item.Quantity);

                if (invoice.TaxAmount > 0)
                {
                    var purchaseTaxAmount = purchaseTotalAmount * (decimal)0.15;
                    purchaseTotalAmount += purchaseTaxAmount;
                    var taxEntry = new TransactionEntry
                    {
                        MainAccountId = purchaseTaxAccount.Id,
                        CreditAmount = 0, // Assuming TaxAmount is what you want to credit
                        DebitAmount = purchaseTaxAmount,
                        Description = $"Invoice #{invoice.InvoiceNo} - Purchase Tax",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(taxEntry);
                }

                // 2. Entry for supplier MainAccount (Debit for supplier)
                var supplierEntry = new TransactionEntry
                {
                    MainAccountId = supplier.MainAccountId.Value,
                    DebitAmount = 0,
                    CreditAmount = purchaseTotalAmount,
                    Description = $"Invoice #{invoice.InvoiceNo} (Supplier)",
                    CreatedAt = DateTime.UtcNow
                };
                transactionEntries.Add(supplierEntry);

                // 3. Group entries by store
                var storeGroups = invoice.PurchaseItems
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
                        DebitAmount = storeTotal,
                        CreditAmount = 0,
                        Description = $"Invoice #{invoice.InvoiceNo} (Store)",
                        CreatedAt = DateTime.UtcNow
                    };
                    transactionEntries.Add(storeEntry);
                }

                // Create the transaction
                var transaction = new Transaction
                {
                    Description = $"قيد فاتورة مشتريات رقم {invoice.InvoiceNo}",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TransactionTypeId = transactionTypeId.Value,
                    TransactionEntries = transactionEntries,
                    PurchaseInvoiceId = invoice.Id,
                    IsFrontal = invoice.IsFrontal,
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
                            Description = $"Purchase Invoice #{invoice.InvoiceNo}",
                            OperationDate = invoice.IssueDate,
                            SupplierDetailsId = invoice.SupplierDetailsId,
                            OperationSource = OperationSource.PurchaseInvoice.ToString(),
                            OperationEntries = invoice.PurchaseItems.Select(pi => new OperationEntryPostDto
                            {
                                MainPurchaseClassId = pi.MainPurchaseClassId,
                                Quantity = pi.Quantity,
                                UnitPrice = pi.Price,
                                StoreDetailsId = pi.StoreDetailsId,
                            }).ToList(),
                            TransactionId = result.Data.Id,
                            PurchaseInvoiceId = invoice.Id
                        };
                        var response = await inventoryOperationService.AddAsync(inventoryOperation);
                        if (response.ErrorCode != ErrorCodes.Success)
                        {
                            return 0;
                        }

                        // إنشاء إذن دخول تلقائي حتى تظهر الكميات في عرض المخزون
                        var permitRepository = _repositoriesFactory.GetRepository<InventoryPermit>();
                        foreach (var storeGroup in storeGroups)
                        {
                            var spQuery = await storeRepository.Filter(s => s.Id == storeGroup.Key);
                            var sp = spQuery.Data.FirstOrDefault();
                            if (sp == null || sp.MainAccountId == null) continue;

                            var permit = new InventoryPermit
                            {
                                ReferenceNumber = $"PIV-{invoice.InvoiceNo}-{storeGroup.Key.ToString()[..8]}",
                                Description = $"إذن دخول تلقائي - فاتورة مشتريات #{invoice.InvoiceNo}",
                                PermitDate = DateOnly.FromDateTime(DateTime.UtcNow),
                                StoreDetailsId = storeGroup.Key,
                                MainAccountId = sp.MainAccountId.Value,
                                PermitType = PermitType.Entry,
                                PermitEntries = storeGroup.Select(pi => new PermitEntry
                                {
                                    MainPurchaseClassId = pi.MainPurchaseClassId,
                                    Quantity = pi.Quantity,
                                    UnitPrice = pi.Price,
                                    RetailPrice = pi.RetailPrice,
                                    WholesalePrice = pi.WholesalePrice,
                                    BulkPrice = pi.BulkPrice,
                                    BatchNo = pi.BatchNo,
                                    ExpiryDate = pi.ExpiryDate.HasValue
                                        ? DateOnly.FromDateTime(pi.ExpiryDate.Value)
                                        : (DateOnly?)null,
                                }).ToList()
                            };
                            var permitResult = await permitRepository.Add(permit);
                            if (permitResult.ErrorCode != ErrorCodes.Success)
                            {
                                LogService.LogError($"[PurchaseInvoiceService] فشل إنشاء إذن دخول PIV للفاتورة {invoice.InvoiceNo}، المخزن {storeGroup.Key}. ErrorCode={permitResult.ErrorCode}");
                            }
                            else
                            {
                                LogService.LogInformation($"[PurchaseInvoiceService] تم إنشاء إذن دخول PIV بنجاح: {permit.ReferenceNumber}");

                                // Sync PharmacyBatch for pharmacy/medication items
                                var classRepo2 = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                                var pharmacyBatchRepo = _repositoriesFactory.GetRepository<Domains.Entities.Pharmacy.PharmacyBatch>();
                                foreach (var pi in storeGroup)
                                {
                                    if (string.IsNullOrWhiteSpace(pi.BatchNo) || !pi.ExpiryDate.HasValue)
                                        continue;

                                    var classQ = await classRepo2.Filter(c => c.Id == pi.MainPurchaseClassId);
                                    var cls = classQ.Data?.FirstOrDefault();
                                    if (cls == null) continue;

                                    // Only sync items that are pharmacy or need batch tracking
                                    if (!cls.IsPharmacyItem && !cls.NeedsBatch)
                                        continue;

                                    // Upsert: if same item+batch+store already exists, add quantity; otherwise insert
                                    var existingQ = await pharmacyBatchRepo.Filter(b =>
                                        b.ItemCode == cls.ReferenceCode &&
                                        b.BatchNo == pi.BatchNo &&
                                        b.StoreDetailsId == storeGroup.Key);
                                    var existing = existingQ.Data?.FirstOrDefault();
                                    if (existing != null)
                                    {
                                        existing.Quantity += pi.Quantity;
                                        existing.ExpiryDate = pi.ExpiryDate.Value;
                                        await pharmacyBatchRepo.Update(existing);
                                    }
                                    else
                                    {
                                        var storeQ2 = await storeRepository.Filter(s => s.Id == storeGroup.Key);
                                        var storeObj = storeQ2.Data?.FirstOrDefault();
                                        await pharmacyBatchRepo.Add(new Domains.Entities.Pharmacy.PharmacyBatch
                                        {
                                            ItemCode = cls.ReferenceCode ?? string.Empty,
                                            ItemName = cls.ArabicName ?? cls.EnglishName ?? string.Empty,
                                            BatchNo = pi.BatchNo,
                                            ExpiryDate = pi.ExpiryDate.Value,
                                            Quantity = pi.Quantity,
                                            StoreDetailsId = storeGroup.Key,
                                            StoreName = storeObj?.ArabicName ?? storeObj?.EnglishName,
                                            Status = "Active",
                                        });
                                    }
                                }
                            }
                        }

                        invoice.PurchaseItems = null;
                        invoice.TransactionId = result.Data.Id;
                        var r = await invoiceRepository.Update(invoice);

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


        public async Task<int> EditAsync(PurchaseInvoice invoice, List<PurchaseItem> oldpurchaseItems)
        {
            try
            {

                var purchaseInvoiceRepository = _repositoriesFactory.GetRepository<PurchaseInvoice>();

                if (!string.IsNullOrWhiteSpace(invoice.ReferenceNumber))
                {
                    var referenceExists = await purchaseInvoiceRepository.Filter(i => i.ReferenceNumber == invoice.ReferenceNumber && i.Id != invoice.Id);
                    if (await referenceExists.Data.AnyAsync())
                    {
                        throw new BusinessRuleViolationException(new List<BusinessRuleError> 
                        { 
                            new BusinessRuleError($"رقم المرجع {invoice.ReferenceNumber} موجود بالفعل، الرجاء إدخال رقم مرجع غير مكرر.", "ReferenceNumber") 
                        });
                    }
                }

                // Validate batch/expiry requirements based on item master settings
                if (invoice.PurchaseItems != null && invoice.PurchaseItems.Any())
                {
                    var itemClassIds2 = invoice.PurchaseItems
                        .Select(pi => pi.MainPurchaseClassId)
                        .Distinct()
                        .ToList();

                    var classRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                    var classQuery = await classRepo.Filter(c => itemClassIds2.Contains(c.Id));
                    var itemClasses = await classQuery.Data.ToListAsync();
                    var classLookup = itemClasses.ToDictionary(c => c.Id);

                    var errors = new List<BusinessRuleError>();
                    foreach (var item in invoice.PurchaseItems)
                    {
                        if (classLookup.TryGetValue(item.MainPurchaseClassId, out var itemClass))
                        {
                            if (itemClass.NeedsBatch && string.IsNullOrWhiteSpace(item.BatchNo))
                                errors.Add(new BusinessRuleError($"الصنف '{itemClass.ArabicName ?? itemClass.EnglishName}' يتطلب إدخال رقم التشغيلة (Batch No).", "BatchNo"));

                            if (itemClass.NeedsExpiry && !item.ExpiryDate.HasValue)
                                errors.Add(new BusinessRuleError($"الصنف '{itemClass.ArabicName ?? itemClass.EnglishName}' يتطلب إدخال تاريخ الصلاحية (Expiry Date).", "ExpiryDate"));
                        }
                    }

                    if (errors.Any())
                        throw new BusinessRuleViolationException(errors);
                }

                var purchaseItemRepository = _repositoriesFactory.GetRepository<PurchaseItem>();
                var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
                var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();

                var purchaseItems = invoice.PurchaseItems;
                invoice.PurchaseItems = null;

                // Delete items that exist in oldpurchaseItems but not in new purchaseItems
                var itemsToDelete = oldpurchaseItems
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
                var inventoryOperationQuery = await inventoryOperationRepository.Filter(x => x.PurchaseInvoiceId == invoice.Id);
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
                        Quantity = pi.Quantity,
                        UnitPrice = pi.Price,
                        StoreDetailsId = pi.StoreDetailsId,
                        InventoryOperationId = inventoryOperation.Id
                    }).ToList();

                    await operationEntryRepository.AddRange(updatedOperationEntries);
                }



                // Nullify unrelated properties for the invoice before updating
                invoice.SupplierDetails = null;
                var response = await purchaseInvoiceRepository.Update(invoice);

                // Update the transaction if present
                if (invoice.TransactionId != null)
                {
                    if (invoice.IsFrontal)
                    {
                        var transactionService = _serviceBaseFactory.GetService<TransactionService>();
                        await transactionService.MakeFrontalAsync(invoice.TransactionId.Value);
                    }
                    var transactionUpdateResult = await UpdateTransaction(invoice.Id, invoice.TransactionId);
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
        public async Task<int> RemoveAsync(PurchaseInvoice invoice)
        {
            var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
            var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
            // query all operation entries that are part of this invoice then loop and delete the entries
            var inventoryOperationQuery = await inventoryOperationRepository.Filter(x => x.PurchaseInvoiceId == invoice.Id);
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
                inventoryOperation.PurchaseInvoice = null;
                var operationDeleteResult = await inventoryOperationRepository.Delete(inventoryOperation);
                if (operationDeleteResult.ErrorCode != ErrorCodes.Success)
                {
                    throw new Exception("Inventory Operations Deletion Failed");
                }
            }

            if (invoice.TransactionId != null)
            {
                // Delete the transaction
                var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
                var transactionQuery = await transactionRepository.Filter(x => x.Id == invoice.TransactionId);
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
            var purchaseItemRepository = _repositoriesFactory.GetRepository<PurchaseItem>();
            var purchaseItemsToDelete = invoice.PurchaseItems.ToList();
            foreach (var purchaseItem in purchaseItemsToDelete)
            {
                purchaseItem.StoreDetails = null;
                purchaseItem.MainPurchaseClass = null;
                purchaseItem.PurchaseInvoice = null;
                await purchaseItemRepository.Delete(purchaseItem);
            }
            invoice.PurchaseItems = null;
            invoice.SupplierDetails = null;
            var resultQuery = await _repositoriesFactory.GetRepository<PurchaseInvoice>().Delete(invoice);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }
    }
}
