using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.Application.Services.Invoice.Purchase;
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
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

namespace SmartAndPro.ERP.Application.Services.PurchaseQuotations
{
    public class PurchaseQuotationService : IPurchaseQuotationService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;

        public PurchaseQuotationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory)

        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
        }
        #region OldGetAllFrontal
        //public async Task<OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    try
        //    {
        //        var filterOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
        //            .Filter(i => i.IsActive == true && i.IsFrontal == true);

        //        if (filterOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(filterOperation.ErrorCode, null);
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
        //            return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(
        //                ErrorCodes.Success,
        //                new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, new List<PurchaseQuotationDto>())
        //            );
        //        }

        //        var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
        //            .Filter(i => invoiceIds.Contains(i.Id));

        //        if (invoicesOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(invoicesOperation.ErrorCode, null);
        //        }

        //        var invoicesEntities = await invoicesOperation.Data
        //            .AsNoTracking()
        //            .ToListAsync();

        //        var invoicesDto = _mapper.Map<List<PurchaseQuotationDto>>(invoicesEntities);

        //        var result = new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, invoicesDto);

        //        return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(ErrorCodes.Success, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(ErrorCodes.GeneralFail, null, message: ex.Message);
        //    }
        //}
        #endregion
        public async Task<OperationResponse<object>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var filterOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
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

                    var invoicesDto = _mapper.Map<List<PurchaseQuotationDto>>(allInvoices);

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
                        new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, new List<PurchaseQuotationDto>())
                    );
                }

                var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
                    .Filter(i => invoiceIds.Contains(i.Id));

                if (invoicesOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(invoicesOperation.ErrorCode, null);

                var invoicesEntities = await invoicesOperation.Data
                    .AsNoTracking()
                    .ToListAsync();

                var invoicesDtoPaged = _mapper.Map<List<PurchaseQuotationDto>>(invoicesEntities);

                var paginatedResult = new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, invoicesDtoPaged);

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

        public async Task<PurchaseQuotation> MakeFrontalAsync(int purchaseId)
        {
            var PurchaseQuotationRepository = _repositoriesFactory.GetRepository<PurchaseQuotation>();
            var PurchaseQuotationQuery = await PurchaseQuotationRepository.Filter(x => x.Id == purchaseId);
            var PurchaseQuotation = PurchaseQuotationQuery.Data?
                .IgnoreAutoIncludes()
                .FirstOrDefault();
            if (PurchaseQuotation == null)
            {
                return null;
            }
            
            PurchaseQuotation.IsFrontal = true;
            var updateResult = await PurchaseQuotationRepository.Update(PurchaseQuotation);

            return updateResult.Data;
        }


        public async Task<string> GetLastInvoiceNo()
        {
            var invoicesQuery = await _repositoriesFactory.GetRepository<PurchaseQuotation>().GetAll();
            var invoices = await invoicesQuery.Data.ToListAsync();
            if (!invoices.Any()) return await Task.FromResult(string.Empty);

            var invoice = invoices.OrderBy(i => i.Id).Last();
            return invoice.InvoiceNo;
        }

        #region OldGetAll
        //public async Task<IEnumerable<PurchaseQuotation>> GetAllAsync()
        //{

        //    var invoicesQuery = await _repositoriesFactory.GetRepository<PurchaseQuotation>().GetAll();
        //    return await invoicesQuery.Data.ToListAsync();
        //}

        //public async Task<OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    try
        //    {
        //        var filterOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
        //            .Filter(i => i.IsActive == true);

        //        if (filterOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(filterOperation.ErrorCode, null);
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
        //            return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(
        //                ErrorCodes.Success,
        //                new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, new List<PurchaseQuotationDto>())
        //            );
        //        }

        //        var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
        //            .Filter(i => invoiceIds.Contains(i.Id));

        //        if (invoicesOperation.ErrorCode != ErrorCodes.Success)
        //        {
        //            return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(invoicesOperation.ErrorCode, null);
        //        }

        //        var invoicesEntities = await invoicesOperation.Data
        //            .AsNoTracking()
        //            .ToListAsync();

        //        var invoicesDto = _mapper.Map<List<PurchaseQuotationDto>>(invoicesEntities);

        //        var result = new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, invoicesDto);

        //        return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(ErrorCodes.Success, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return new OperationResponse<PaginatedResult<List<PurchaseQuotationDto>>>(ErrorCodes.GeneralFail, null, message: ex.Message);
        //    }
        //}

        #endregion


        public async Task<OperationResponse<object>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var filterOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
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

                    var invoicesDto = _mapper.Map<List<PurchaseQuotationDto>>(allInvoices);

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
                        new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, new List<PurchaseQuotationDto>())
                    );
                }

                var invoicesOperation = await _repositoriesFactory.GetRepository<PurchaseQuotation>()
                    .Filter(i => invoiceIds.Contains(i.Id));

                if (invoicesOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(invoicesOperation.ErrorCode, null);

                var invoicesEntities = await invoicesOperation.Data
                    .AsNoTracking()
                    .ToListAsync();

                var invoicesDtoPaged = _mapper.Map<List<PurchaseQuotationDto>>(invoicesEntities);

                var paginatedResult = new PaginatedResult<List<PurchaseQuotationDto>>(totalRecords, invoicesDtoPaged);

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
   
        public async Task<PurchaseQuotation> GetFirstAsync()
        {
            var invoicesQuery = await _repositoriesFactory.GetRepository<PurchaseQuotation>().GetAll();
            var invoices = await invoicesQuery.Data.FirstOrDefaultAsync();
            return invoices;
        }
        public async Task<PurchaseQuotation> GetByIdAsync(int id)
        {
            var invoicesQuery = await _repositoriesFactory
                .GetRepository<PurchaseQuotation>().GetAll();
            return await invoicesQuery.Data.FirstOrDefaultAsync(i => i.Id == id);
        }
        //public async Task<int> AddAsync(PurchaseQuotation purchaseQuotation)
        //{
        //    var resultQuery = await _repositoriesFactory.GetRepository<PurchaseQuotation>().Add(purchaseQuotation);
        //    var result = resultQuery.Data;
        //    return result != null ? 1 : 0;
        //}

        public async Task<int> AddAsync(PurchaseQuotation purchaseQuotation)
        {
            try
            {
                LogService.LogInformation(
                    "Starting AddAsync for PurchaseQuotation",
                    parameters: new object[] { "New PurchaseQuotation" } 
                );

                if (string.IsNullOrEmpty(purchaseQuotation.InvoiceNo))
                {
                    var lastInvoiceNo = await GetLastInvoiceNo();
                    purchaseQuotation.InvoiceNo = GenerateNewInvoiceNo(lastInvoiceNo);
                }

                // --- 1. Validate Supplier ---
                if (purchaseQuotation.SupplierDetailsId != null) // If nullable
                {
                   var supplierCheck = await _repositoriesFactory.GetRepository<SupplierDetails>()
                       .Filter(x => x.Id == purchaseQuotation.SupplierDetailsId);
                   if (supplierCheck.ErrorCode == ErrorCodes.Success && !await supplierCheck.Data.AnyAsync())
                   {
                        throw new Exception($"Invalid SupplierDetailsId: {purchaseQuotation.SupplierDetailsId} does not exist in database.");
                   }
                }

                // --- 2. Validate Items (Store & Class) & Calculate Price ---
                if (purchaseQuotation.PurchaseQuotationItems != null && purchaseQuotation.PurchaseQuotationItems.Any())
                {
                    foreach (var item in purchaseQuotation.PurchaseQuotationItems.Select((value, index)=> new { value, index}))
                    {
                        //// Auto-generate ItemNo if missing
                        //if (string.IsNullOrEmpty(item.value.ItemNo))
                        //{
                        //     item.value.ItemNo = $"{purchaseQuotation.InvoiceNo}-{item.index + 1}";
                        //}

                        // Auto-Calculate Price if missing (0)
                        if (item.value.Price <= 0)
                        {
                            item.value.Price = await CalculateItemPriceAsync(item.value.MainPurchaseClassId);
                        }

                        // Recalculate totals based on the (potentially new) price
                        // Assuming simple calculation: Total = Price * Qty (Tax logic might need review if TaxAmount depends on it)
                        // If TaxAmount is passed as absolute value from FE, we might need to recalculate it if Price changed.
                        // For now, let's assume valid Price is enough or Tax is percentage based in future logic.
                        // Ideally: item.value.TotalPrice = (item.value.Price * item.value.Quantity) + item.value.TaxAmount - item.value.DiscountAmount;


                        // Check Store - REMOVED per user request (Store not needed in Quotation)


                        // Check MainPurchaseClass
                        var classCheck = await _repositoriesFactory.GetRepository<MainPurchaseClass>()
                             .Filter(x => x.Id == item.value.MainPurchaseClassId);
                         if (classCheck.ErrorCode == ErrorCodes.Success && !await classCheck.Data.AnyAsync())
                        {
                            throw new Exception($"Item #{item.index + 1} has invalid MainPurchaseClassId: {item.value.MainPurchaseClassId}");
                        }
                    }
                }


                var resultQuery = await _repositoriesFactory
                    .GetRepository<PurchaseQuotation>()
                    .Add(purchaseQuotation);

                // DEBUGGING: Throw exception if repository fails
                if (resultQuery.ErrorCode != ErrorCodes.Success)
                {
                    var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
                    throw new Exception($"Repository Failed: {resultQuery.ErrorCode}. Data: {JsonSerializer.Serialize(resultQuery.Data, options)}");
                }

                LogService.LogError(
                    "Repository Add finished",
                    parameters: new object[]
                    {
                resultQuery.ErrorCode,
                resultQuery.Data?.Id
                    }
                );

                return resultQuery.ErrorCode == ErrorCodes.Success ? 1 : 0;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                throw; // 👈 مهم جدًا
            }
        }

        private async Task<decimal> CalculateItemPriceAsync(Guid mainPurchaseClassId)
        {
            // 1. Try Average Price (Last 30 Days)
            var avgPrice = await GetAverageItemPriceAsync(mainPurchaseClassId, 30);
            if (avgPrice > 0) return avgPrice;

            // 2. Fallback: Last Purchase Price
            return await GetLastItemPriceAsync(mainPurchaseClassId);
        }

        public async Task<decimal> GetAverageItemPriceAsync(Guid mainPurchaseClassId, int days)
        {
            try
            {
                var purchaseItemRepo = _repositoriesFactory.GetRepository<PurchaseItem>();
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-days);

                var result = await purchaseItemRepo.Filter(x => 
                    x.MainPurchaseClassId == mainPurchaseClassId && 
                    x.PurchaseInvoice != null && 
                    x.PurchaseInvoice.IssueDate >= thirtyDaysAgo
                );

                if (result.ErrorCode != ErrorCodes.Success || !result.Data.Any())
                    return 0;

                var avgPrice = await result.Data.AverageAsync(x => x.Price);
                return Math.Round(avgPrice, 2);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return 0;
            }
        }

        public async Task<decimal> GetLastItemPriceAsync(Guid mainPurchaseClassId)
        {
            try
            {
                var purchaseItemRepo = _repositoriesFactory.GetRepository<PurchaseItem>();
                
                var result = await purchaseItemRepo.Filter(x => x.MainPurchaseClassId == mainPurchaseClassId);
                
                if (result.ErrorCode != ErrorCodes.Success) return 0;

                var lastItem = await result.Data
                    .Include(x => x.PurchaseInvoice)
                    .Where(x => x.PurchaseInvoice != null)
                    .OrderByDescending(x => x.PurchaseInvoice.IssueDate)
                    .FirstOrDefaultAsync();

                return lastItem?.Price ?? 0;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return 0;
            }
        }


        public async Task<OperationResponse<object>> ConvertToInvoice(int quotationId, Guid storeDetailsId)
        {
            try
            {
                var invoiceService = _serviceBaseFactory.GetService<PurchaseInvoiceService>();
                var quotationRepo = _repositoriesFactory.GetRepository<PurchaseQuotation>();

                var filterResult = await quotationRepo.Filter(q => q.Id == quotationId);
                //var quotation = await filterResult.Data
                //    .Include(q => q.PurchaseQuotationItems)
                //    //.ThenInclude(qi => qi.StoreDetails) // Removed as StoreDetails is no longer in Item
                //    .FirstOrDefaultAsync();

                // Use AsNoTracking to avoid tracking conflicts when we add the new invoice
                var quotation = await filterResult.Data
                    .Include(q => q.PurchaseQuotationItems)
                    .Include(q => q.Attachments)
                    .Include(q => q.SupplierDetails)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


                if (quotation == null)
                {
                    return new OperationResponse<object>(ErrorCodes.NotFound, 0, message: "Quotation not found");
                }

                if (quotation.Status == PurchaseQuotationStatus.ConvertedToInvoice)
                {
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, 0, message: "Quotation already converted");
                }

                // Map Quotation to Invoice using AutoMapper
                var invoice = _mapper.Map<PurchaseInvoice>(quotation);

                invoice.PurchaseQuotationId = quotation.Id;


                // Handle Fields that are not mapped or need reset
                invoice.CreatedBy = quotation.CreatedBy;
                invoice.CreatedAt = DateTime.UtcNow;
                invoice.IssueDate = DateTime.UtcNow;
                
                // Fix: Ensure IsFrontal is false (default for standard invoice)
                invoice.IsFrontal = false;

                // Fix: Generate correct InvoiceNo (Inv-XX)
                var lastInvoiceNo = await invoiceService.GetLastInvoiceNo();
                invoice.InvoiceNo = GenerateNewPurchaseInvoiceNo(lastInvoiceNo);

                // IMPORTANT: Nullify navigation properties to prevent EF from trying to add/update them
                // We rely on the Foreign Keys (SupplierDetailsId, etc.) which are already mapped
                invoice.SupplierDetails = null; 

                // Reset IDs for Items and Assign Store
                foreach (var item in invoice.PurchaseItems)
                {
                    item.Id = 0; // Ensure ID is 0 so EF treats it as a new entity
                    item.PurchaseInvoiceId = 0;
                    item.PurchaseInvoice = null;
                    item.StoreDetailsId = storeDetailsId; // Assign the selected store
                    
                    // Nullify item navigation properties
                    item.StoreDetails = null;
                    item.MainPurchaseClass = null;
                }

                // Add Invoice
                // This will automatically handle the Quotation Status Update in PurchaseInvoiceService.AddAsync
                var result = await invoiceService.AddAsync(invoice);

                if (result == 1)
                {
                    return new OperationResponse<object>(ErrorCodes.Success, invoice.Id);
                }

                return new OperationResponse<object>(ErrorCodes.GeneralFail, 0, message: "Failed to create invoice");
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, 0, message: ex.Message);
            }
        }
        private string GenerateNewInvoiceNo(string lastInvoiceNo)
        {
            const string prefix = "Q-";
            if (string.IsNullOrEmpty(lastInvoiceNo)) return prefix + "1";

            var digits = new string(lastInvoiceNo.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int oldNum))
            {
                return prefix + (oldNum + 1);
            }
            return prefix + DateTime.UtcNow.Ticks;
        }

        private string GenerateNewPurchaseInvoiceNo(string lastInvoiceNo)
        {
            const string inv = "Inv-";
            if (string.IsNullOrEmpty(lastInvoiceNo)) return inv + "1";

            var digits = new string(lastInvoiceNo.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out int oldNum);

            return inv + (oldNum + 1);
        }

        public async Task<int> RemoveAsync(PurchaseQuotation quotation)
        {
            //var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
            //var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
            //// query all operation entries that are part of this invoice then loop and delete the entries
            //var inventoryOperationQuery = await inventoryOperationRepository.Filter(x => x.PurchaseInvoiceId == invoice.Id);
            //var inventoryOperation = inventoryOperationQuery.Data
            //    .Include(x => x.OperationEntries)
            //    .IgnoreAutoIncludes()
            //    .FirstOrDefault();
            //if (inventoryOperation != null)
            //{
            //    var operationEntriesToDelete = inventoryOperation.OperationEntries.ToList();
            //    foreach (var operationEntry in operationEntriesToDelete)
            //    {
            //        await operationEntryRepository.Delete(operationEntry);
            //    }
            //    inventoryOperation.OperationEntries = null;
            //    inventoryOperation.PurchaseInvoice = null;
            //    var operationDeleteResult = await inventoryOperationRepository.Delete(inventoryOperation);
            //    if (operationDeleteResult.ErrorCode != ErrorCodes.Success)
            //    {
            //        throw new Exception("Inventory Operations Deletion Failed");
            //    }
            //}

            //if (invoice.TransactionId != null)
            //{
            //    // Delete the transaction
            //    var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
            //    var transactionQuery = await transactionRepository.Filter(x => x.Id == invoice.TransactionId);
            //    var transaction = transactionQuery.Data.FirstOrDefault();
            //    if (transaction != null)
            //    {
            //        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
            //        var transactionEntries = transaction.TransactionEntries;
            //        transaction.TransactionEntries = null;
            //        foreach (var entry in transactionEntries)
            //        {
            //            entry.Transaction = null;
            //            entry.MainAccount = null;
            //            var deletionResult = await transactionEntryRepository.Delete(entry);
            //        }
            //        await transactionRepository.Delete(transaction);
            //    }
            //}
            var purchaseItemRepository = _repositoriesFactory.GetRepository<PurchaseQuotationItem>();
            var purchaseQuotationItemsToDelete = quotation.PurchaseQuotationItems.ToList();
            foreach (var purchaseQuotationItem in purchaseQuotationItemsToDelete)
            {
                //purchaseQuotationItem.StoreDetails = null;
                purchaseQuotationItem.MainPurchaseClass = null;
                //purchaseQuotationItem.PurchaseInvoice = null;
                await purchaseItemRepository.Delete(purchaseQuotationItem);
            }
            quotation.PurchaseQuotationItems = null;
            quotation.SupplierDetails = null;
            var resultQuery = await _repositoriesFactory.GetRepository<PurchaseQuotation>().Delete(quotation);
            var result = resultQuery.Data;
            return result != null ? 1 : 0;
        }

        public async Task<OperationResponse<object>> UpdateAsync(int id, PurchaseQuotationPutDto dto)
        {
            try
            {
                if(id != dto.Id) return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message : "Id Mismatch");

                var quotationRepo = _repositoriesFactory.GetRepository<PurchaseQuotation>();

                // 1. Get Existing Header (No Include)
                var existingWrapper = await quotationRepo.Filter(q => q.Id == id);
                if (existingWrapper.ErrorCode != ErrorCodes.Success || !existingWrapper.Data.Any())
                    return new OperationResponse<object>(ErrorCodes.NotFound, null,message : "Quotation not found");

                var existingQuotation = await existingWrapper.Data.FirstOrDefaultAsync();
                
                if (existingQuotation == null) return new OperationResponse<object>(ErrorCodes.NotFound, null);

                // 2. Update Header
                var newItemsDtos = dto.PurchaseQuotationItems;
                dto.PurchaseQuotationItems = null; 
                _mapper.Map(dto, existingQuotation);
                dto.PurchaseQuotationItems = newItemsDtos;

                existingQuotation.SupplierDetails = null; // Prevent tracking issues
                var updateResult = await quotationRepo.Update(existingQuotation);

                if (updateResult.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Header Update failed");

                // 3. Handle Items
                var itemRepo = _repositoriesFactory.GetRepository<PurchaseQuotationItem>();
                
                // We must fetch items separately because we didn't include them in the first query
                var existingItemsWrapper = await itemRepo.Filter(x => x.PurchaseQuotationId == id);
                var existingItems = new List<PurchaseQuotationItem>();
                
                if (existingItemsWrapper.ErrorCode == ErrorCodes.Success)
                     existingItems = await existingItemsWrapper.Data.ToListAsync();

                // A. Delete Removed Items
                var newItemIds = newItemsDtos.Where(x => x.Id.HasValue && x.Id.Value > 0).Select(x => x.Id.Value).ToList();
                var itemsToDelete = existingItems.Where(x => !newItemIds.Contains(x.Id)).ToList();

                foreach (var itemToDelete in itemsToDelete)
                {
                    itemToDelete.MainPurchaseClass = null;
                    await itemRepo.Delete(itemToDelete);
                }

                // B. Update/Add Items
                foreach (var itemDto in newItemsDtos)
                {
                    if (itemDto.Id.HasValue && itemDto.Id.Value > 0)
                    {
                        var existingItem = existingItems.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                        if (existingItem != null)
                        {
                            _mapper.Map(itemDto, existingItem);
                            if (existingItem.Price <= 0)
                                existingItem.Price = await CalculateItemPriceAsync(existingItem.MainPurchaseClassId);
                            
                            existingItem.MainPurchaseClass = null; 
                            await itemRepo.Update(existingItem);
                        }
                    }
                    else
                    {
                        var newItem = _mapper.Map<PurchaseQuotationItem>(itemDto);
                        newItem.PurchaseQuotationId = id;
                        if (newItem.Price <= 0)
                            newItem.Price = await CalculateItemPriceAsync(newItem.MainPurchaseClassId);
                        
                        newItem.MainPurchaseClass = null;
                        await itemRepo.Add(newItem);
                    }
                }

                return new OperationResponse<object>(ErrorCodes.Success, updateResult.Data);

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }

        public async Task<int> EditAsync(PurchaseQuotation quotation, List<PurchaseQuotationItem> oldpurchasequotationItems)
        {
            try
            {

                var purchaseQuotationRepository = _repositoriesFactory.GetRepository<PurchaseQuotation>();
                var purchaseQuotationItemRepository = _repositoriesFactory.GetRepository<PurchaseQuotationItem>();
                //var inventoryOperationRepository = _repositoriesFactory.GetRepository<InventoryOperation>();
                //var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();

                var purchaseQuotationItems = quotation.PurchaseQuotationItems;
                //quotation.PurchaseQuotationItems = null;

                // Delete items that exist in oldpurchaseItems but not in new purchaseItems
                var itemsToDelete = oldpurchasequotationItems
                    .Where(oldItem => !purchaseQuotationItems.Any(newItem => newItem.Id == oldItem.Id))
                    .ToList();

                foreach (var itemToDelete in itemsToDelete)
                {
                    //itemToDelete.StoreDetails = null;
                    itemToDelete.MainPurchaseClass = null;
                    await purchaseQuotationItemRepository.Delete(itemToDelete);
                }

                // Update or add new purchase items
                foreach (var purchaseQuotationItem in purchaseQuotationItems)
                {
                    if (purchaseQuotationItem.Id != 0)
                    {
                        //purchaseQuotationItem.StoreDetails = null;
                        purchaseQuotationItem.MainPurchaseClass = null;
                        await purchaseQuotationItemRepository.Update(purchaseQuotationItem);
                    }
                    else
                    {
                        await purchaseQuotationItemRepository.Add(purchaseQuotationItem);
                    }
                }

                // Delete the existing inventory operations
                //var inventoryOperationQuery = await inventoryOperationRepository.Filter(x => x.PurchaseInvoiceId == invoice.Id);
                //var inventoryOperation = inventoryOperationQuery.Data
                //    .Include(x => x.OperationEntries)
                //    .IgnoreAutoIncludes()
                //    .FirstOrDefault();
                //if (inventoryOperation != null)
                //{
                //    var operationEntriesToDelete = inventoryOperation.OperationEntries.ToList();
                //    foreach (var operationEntry in operationEntriesToDelete)
                //    {
                //        await operationEntryRepository.Delete(operationEntry);
                //    }
                //    var updatedOperationEntries = purchaseItems.Select(pi => new OperationEntry
                //    {
                //        MainPurchaseClassId = pi.MainPurchaseClassId,
                //        Quantity = pi.Quantity,
                //        UnitPrice = pi.Price,
                //        StoreDetailsId = pi.StoreDetailsId,
                //        InventoryOperationId = inventoryOperation.Id
                //    }).ToList();

                //    await operationEntryRepository.AddRange(updatedOperationEntries);
                //}



                // Nullify unrelated properties for the invoice before updating
                quotation.SupplierDetails = null;
                var response = await purchaseQuotationRepository.Update(quotation);

                //// Update the transaction if present
                //if (invoice.TransactionId != null)
                //{
                //    if (invoice.IsFrontal)
                //    {
                //        var transactionService = _serviceBaseFactory.GetService<TransactionService>();
                //        await transactionService.MakeFrontalAsync(invoice.TransactionId.Value);
                //    }
                //    var transactionUpdateResult = await UpdateTransaction(invoice.Id, invoice.TransactionId);
                //}

                var result = response.Data;
                return result != null ? 1 : 0;
            }
            catch (Exception e)
            {
                LogService.LogError(e);
            }

            return 0;
        }

    }
}
