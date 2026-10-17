using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using System.Security.Claims;
using SmartAndPro.ERP.Domains.Entities.Cashier;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier
{
    [Authorize]
    [ApiController]
    [Route("api/cashier")]
    public class CashierController : ControllerBase
    {
        private readonly ILogger<CashierController> _logger;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ICashierRuntimeStore _cashierStore;
        private readonly ICashierReservationService _reservationService;
        private readonly ICashierAuditService _auditService;

        public CashierController(
            ILogger<CashierController> logger,
            RepositoriesFactory repositoriesFactory,
            ISalesInvoicesService salesInvoicesService,
            ICashierRuntimeStore cashierStore,
            ICashierReservationService reservationService,
            ICashierAuditService auditService)
        {
            _logger = logger;
            _repositoriesFactory = repositoriesFactory;
            _salesInvoicesService = salesInvoicesService;
            _cashierStore = cashierStore;
            _reservationService = reservationService;
            _auditService = auditService;
        }

        [HttpGet("products")]
        public async Task<IActionResult> SearchProducts([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] string? barcode, [FromQuery] bool? inStock, [FromQuery] Guid? storeDetailsId, [FromQuery] int? cashierSessionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var (resolvedStoreDetailsId, resolutionError) = await ResolveStoreDetailsIdAsync(storeDetailsId, cashierSessionId);
                if (!string.IsNullOrWhiteSpace(resolutionError))
                {
                    return BadRequest(new { message = resolutionError });
                }

                if (cashierSessionId.HasValue && !resolvedStoreDetailsId.HasValue)
                {
                    return BadRequest(new { message = "The active cashier session is not linked to any store. Please open a store-linked session first." });
                }

                // CRITICAL FIX: Use MainPurchaseClass instead of MainSalesClass!
                // Inventory is stored using MainPurchaseClassId in PermitEntry
                var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var saleItemRepo = _repositoriesFactory.GetRepository<SaleItem>();

                // Get all purchase classes (ignore query filters to bypass soft delete)
                var classesQuery = (await purchaseClassRepo.GetAll()).Data;
                if (classesQuery == null)
                {
                    _logger.LogWarning("[CashierController] Purchase class query is null");
                    return Ok(new List<object>());
                }
                
                var classes = classesQuery
                    .IgnoreQueryFilters()
                    .Where(x => x.IsActive)
                    .ToList();
                    
                _logger.LogInformation("[CashierController] Found {ClassCount} active purchase classes (total in DB before filter: {TotalCount})", 
                    classes.Count, 
                    classesQuery.IgnoreQueryFilters().Count());
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim();
                    classes = classes.Where(x =>
                        (!string.IsNullOrWhiteSpace(x.ArabicName) && x.ArabicName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(x.EnglishName) && x.EnglishName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(x.ReferenceCode) && x.ReferenceCode.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    var b = barcode.Trim();
                    classes = classes.Where(x => x.ReferenceCode.Equals(b, StringComparison.OrdinalIgnoreCase) || x.Id.ToString().Equals(b, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var qtyByClass = await GetAvailableQuantityByPurchaseClassAsync(classes, resolvedStoreDetailsId);
                
                _logger.LogInformation("[CashierController] GetAvailableQuantityByPurchaseClassAsync returned {QtyCount} products with quantities", qtyByClass.Count);
                
                var displayQtyByClass = qtyByClass;
                if (resolvedStoreDetailsId.HasValue)
                {
                    var classIds = qtyByClass.Keys.ToList();
                    if (classIds.Count > 0)
                    {
                        var reservedByClass = await _reservationService.GetReservedQuantitiesByClassAsync(resolvedStoreDetailsId.Value, classIds);
                        displayQtyByClass = qtyByClass.ToDictionary(
                            x => x.Key,
                            x => Math.Max(0m, x.Value - (reservedByClass.TryGetValue(x.Key, out var reservedQty) ? reservedQty : 0m)));
                    }
                }

                // CRITICAL FIX: If inStock filter is on, we need to include ALL products with quantities
                // not just the ones in the original 'classes' list
                if (inStock == true)
                {
                    var productsWithStock = displayQtyByClass.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToHashSet();
                    _logger.LogInformation("[CashierController] Found {StockCount} products with available stock", productsWithStock.Count);
                    
                    // Get ALL purchase classes that have stock, not just the filtered ones
                    var allPurchaseClassesQuery = (await purchaseClassRepo.GetAll()).Data;
                    if (allPurchaseClassesQuery != null)
                    {
                        var classesWithStock = allPurchaseClassesQuery
                            .IgnoreQueryFilters()
                            .Where(x => x.IsActive && productsWithStock.Contains(x.Id))
                            .ToList();
                        
                        // Re-apply search filter if present
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            var keyword = search.Trim();
                            classesWithStock = classesWithStock.Where(x =>
                                (!string.IsNullOrWhiteSpace(x.ArabicName) && x.ArabicName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(x.EnglishName) && x.EnglishName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(x.ReferenceCode) && x.ReferenceCode.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                            ).ToList();
                        }
                        
                        classes = classesWithStock;
                        _logger.LogInformation("[CashierController] Returning {ClassCount} classes with stock", classes.Count);
                    }
                    else
                    {
                        classes = classes.Where(x => displayQtyByClass.TryGetValue(x.Id, out var q) && q > 0).ToList();
                    }
                }

                var safePage = Math.Max(pageNumber, 1);
                var safeSize = Math.Clamp(pageSize, 1, 300);
                var paged = classes.OrderBy(x => x.ArabicName).ThenBy(x => x.EnglishName).Skip((safePage - 1) * safeSize).Take(safeSize).ToList();
                var pagedIds = paged.Select(x => x.Id).ToHashSet();
                
                // Map purchase classes to sales classes to get prices and tax info
                var salesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
                var allSalesClasses = (await salesClassRepo.GetAll()).Data?.IgnoreQueryFilters().Where(x => x.IsActive).ToList() ?? new List<MainSalesClass>();
                var purchaseToSalesMap = new Dictionary<Guid, Guid>();
                var salesClassById = new Dictionary<Guid, MainSalesClass>();
                foreach (var pc in paged)
                {
                    var matchingSales = allSalesClasses.FirstOrDefault(sc =>
                        (!string.IsNullOrWhiteSpace(pc.ReferenceCode) && sc.ReferenceCode == pc.ReferenceCode) ||
                        (!string.IsNullOrWhiteSpace(pc.ArabicName) && sc.ArabicName == pc.ArabicName) ||
                        (!string.IsNullOrWhiteSpace(pc.EnglishName) && sc.EnglishName == pc.EnglishName));
                    if (matchingSales != null)
                    {
                        purchaseToSalesMap[pc.Id] = matchingSales.Id;
                        salesClassById[matchingSales.Id] = matchingSales;
                    }
                }
                var salesClassIds = purchaseToSalesMap.Values.ToHashSet();
                var sales = (await saleItemRepo.GetAll()).Data?.Where(x => salesClassIds.Contains(x.MainSalesClassId)).ToList() ?? new List<SaleItem>();
                var lastPriceBySalesId = sales.GroupBy(x => x.MainSalesClassId).ToDictionary(x => x.Key, x => x.OrderByDescending(i => i.CreatedAt).First().Price);
                var lastPrice = purchaseToSalesMap.ToDictionary(kvp => kvp.Key, kvp => lastPriceBySalesId.TryGetValue(kvp.Value, out var p) ? p : 1m);

                // قراءة أسعار البيع الثلاثة من آخر فاتورة شراء لكل صنف
                var purchaseItemRepo = _repositoriesFactory.GetRepository<PurchaseItem>();
                var allPurchaseItems = (await purchaseItemRepo.GetAll()).Data?
                    .IgnoreQueryFilters()
                    .Where(pi => pi.IsActive && pagedIds.Contains(pi.MainPurchaseClassId))
                    .ToList() ?? new List<PurchaseItem>();
                var latestPricesByClass = allPurchaseItems
                    .GroupBy(pi => pi.MainPurchaseClassId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(pi => pi.RetailPrice > 0 || pi.WholesalePrice > 0 || pi.BulkPrice > 0)
                              .OrderByDescending(pi => pi.CreatedAt)
                              .FirstOrDefault() ?? g.OrderByDescending(pi => pi.CreatedAt).First()
                    );

                var storeRepo = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.StoreManagement.StoreDetails>();
                var currentStore = resolvedStoreDetailsId.HasValue 
                    ? (await storeRepo.GetAll()).Data?.IgnoreQueryFilters().FirstOrDefault(s => s.Id == resolvedStoreDetailsId.Value) 
                    : null;
                var isPharmacyStore = currentStore != null && (
                    (currentStore.StoreCategory != null && currentStore.StoreCategory.Equals("pharmacy", StringComparison.OrdinalIgnoreCase)) ||
                    currentStore.ArabicName.Contains("صيدلية") ||
                    (currentStore.EnglishName != null && currentStore.EnglishName.Contains("pharmacy", StringComparison.OrdinalIgnoreCase)));

                // أيضاً قراءة أسعار البيع من إذون الدخول (PermitEntry) واختيار الأحدث
                var permitEntryRepo = _repositoriesFactory.GetRepository<PermitEntry>();
                var allPermitEntries = (await permitEntryRepo.GetAll()).Data?
                    .IgnoreQueryFilters()
                    .Where(pe => pe.IsActive == true && pagedIds.Contains(pe.MainPurchaseClassId))
                    .ToList() ?? new List<PermitEntry>();
                var latestPermitByClass = allPermitEntries
                    .GroupBy(pe => pe.MainPurchaseClassId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(pe => pe.RetailPrice > 0 || pe.WholesalePrice > 0 || pe.BulkPrice > 0)
                              .OrderByDescending(pe => pe.CreatedAt)
                              .FirstOrDefault() ?? g.OrderByDescending(pe => pe.CreatedAt).First()
                    );

                // For pharmacy store: compute distinct batches per item
                var permitRepo = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();
                var allPermitsQuery = !resolvedStoreDetailsId.HasValue 
                    ? await permitRepo.GetAll()
                    : await permitRepo.Filter(p => p.StoreDetailsId == resolvedStoreDetailsId.Value);

                var allPermits = allPermitsQuery.Data?
                    .IgnoreQueryFilters()
                    .Where(p => p.IsActive)
                    .Include(p => p.PermitEntries)
                    .ToList() ?? new List<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();

                var batchQtyMap = new Dictionary<(Guid classId, string batchNo), decimal>();
                var batchEntryMap = new Dictionary<(Guid classId, string batchNo), PermitEntry>();

                foreach (var permit in allPermits)
                {
                    if (permit.PermitEntries == null) continue;
                    foreach (var pe in permit.PermitEntries.Where(e => e.IsActive && pagedIds.Contains(e.MainPurchaseClassId)))
                    {
                        var bNo = string.IsNullOrWhiteSpace(pe.BatchNo) ? "1" : pe.BatchNo.Trim();
                        var key = (pe.MainPurchaseClassId, bNo);
                        if (!batchQtyMap.ContainsKey(key)) batchQtyMap[key] = 0m;

                        if (permit.PermitType == Domains.Enums.PermitType.Entry)
                            batchQtyMap[key] += pe.Quantity;
                        else if (permit.PermitType == Domains.Enums.PermitType.Exit)
                            batchQtyMap[key] -= pe.Quantity;

                        if (!batchEntryMap.TryGetValue(key, out var existingPe))
                        {
                            batchEntryMap[key] = pe;
                        }
                        else
                        {
                            bool hasPrices = pe.RetailPrice > 0 || pe.WholesalePrice > 0 || pe.BulkPrice > 0;
                            bool existingHasPrices = existingPe.RetailPrice > 0 || existingPe.WholesalePrice > 0 || existingPe.BulkPrice > 0;

                            if (hasPrices && (!existingHasPrices || pe.CreatedAt > existingPe.CreatedAt))
                            {
                                batchEntryMap[key] = pe;
                            }
                            else if (!existingHasPrices && pe.CreatedAt > existingPe.CreatedAt)
                            {
                                batchEntryMap[key] = pe;
                            }
                        }
                    }
                }

                var productResults = new List<object>();

                foreach (var x in paged)
                {
                    var price = lastPrice.TryGetValue(x.Id, out var p) ? decimal.Round(p, 2) : 1m;
                    
                    // Get tax info from mapped sales class
                    var isTaxExempted = false;
                    if (purchaseToSalesMap.TryGetValue(x.Id, out var salesClassId) && 
                        salesClassById.TryGetValue(salesClassId, out var salesClass))
                    {
                        isTaxExempted = salesClass.IsTaxExempted;
                    }
                    
                    var latestPi = latestPricesByClass.TryGetValue(x.Id, out var lpi) ? lpi : null;
                    var latestPe = latestPermitByClass.TryGetValue(x.Id, out var lpe) ? lpe : null;

                    var classBatches = batchQtyMap.Where(kvp => kvp.Key.classId == x.Id && kvp.Value > 0).ToList();

                    if (isPharmacyStore && classBatches.Count > 0)
                    {
                        foreach (var cb in classBatches.OrderBy(b => b.Key.batchNo))
                        {
                            var bNo = cb.Key.batchNo;
                            var batchQty = cb.Value;
                            var batchPe = batchEntryMap.TryGetValue(cb.Key, out var bpe) ? bpe : null;
                            var id = _cashierStore.GetOrCreateProductId(x.Id, bNo);

                            decimal resolvedRetail = (batchPe?.RetailPrice > 0)
                                ? batchPe.RetailPrice
                                : ((latestPe?.RetailPrice > 0) ? latestPe.RetailPrice : ((latestPi?.RetailPrice > 0) ? latestPi.RetailPrice : price));

                            decimal resolvedWholesale = (batchPe?.WholesalePrice > 0)
                                ? batchPe.WholesalePrice
                                : ((latestPe?.WholesalePrice > 0) ? latestPe.WholesalePrice : ((latestPi?.WholesalePrice > 0) ? latestPi.WholesalePrice : resolvedRetail));

                            decimal resolvedBulk = (batchPe?.BulkPrice > 0)
                                ? batchPe.BulkPrice
                                : ((latestPe?.BulkPrice > 0) ? latestPe.BulkPrice : ((latestPi?.BulkPrice > 0) ? latestPi.BulkPrice : resolvedWholesale));

                            decimal resolvedCost = (batchPe?.UnitPrice > 0)
                                ? batchPe.UnitPrice
                                : ((latestPe?.UnitPrice > 0) ? latestPe.UnitPrice : ((latestPi?.Price > 0) ? latestPi.Price : (price * 0.75m)));

                            if (resolvedCost <= 0m) resolvedCost = price * 0.75m;

                            productResults.Add(new
                            {
                                id,
                                code = x.Id.ToString(),
                                barcode = x.ReferenceCode,
                                nameAr = x.ArabicName,
                                nameEn = x.EnglishName,
                                sellingPrice = price,
                                costPrice = decimal.Round(resolvedCost, 2),
                                availableQuantity = decimal.Round(batchQty, 2),
                                taxPercentage = isTaxExempted ? 0m : 15m,
                                includesTax = false,
                                categoryId,
                                categoryNameAr = "POS",
                                categoryNameEn = "POS",
                                isActive = x.IsActive,
                                unitName = x.MeasurementUnit?.ArabicName ?? x.MeasurementUnit?.EnglishName ?? "Unit",
                                storeDetailsId = resolvedStoreDetailsId,
                                retailPrice = decimal.Round(resolvedRetail, 2),
                                wholesalePrice = decimal.Round(resolvedWholesale, 2),
                                bulkPrice = decimal.Round(resolvedBulk, 2),
                                image = x.ImageUrl,
                                shelfNumber = batchPe?.ShelfNumber,
                                rowNumber = batchPe?.RowNumber,
                                batchNo = bNo,
                                expiryDate = batchPe?.ExpiryDate
                            });
                        }
                    }
                    else
                    {
                        var id = _cashierStore.GetOrCreateProductId(x.Id);
                        var qty = displayQtyByClass.TryGetValue(x.Id, out var q) ? q : 0m;

                        decimal resolvedRetail = (latestPe?.RetailPrice > 0)
                            ? latestPe.RetailPrice
                            : ((latestPi?.RetailPrice > 0) ? latestPi.RetailPrice : price);

                        decimal resolvedWholesale = (latestPe?.WholesalePrice > 0)
                            ? latestPe.WholesalePrice
                            : ((latestPi?.WholesalePrice > 0) ? latestPi.WholesalePrice : resolvedRetail);

                        decimal resolvedBulk = (latestPe?.BulkPrice > 0)
                            ? latestPe.BulkPrice
                            : ((latestPi?.BulkPrice > 0) ? latestPi.BulkPrice : resolvedWholesale);

                        decimal resolvedCost = (latestPe?.UnitPrice > 0)
                            ? latestPe.UnitPrice
                            : ((latestPi?.Price > 0) ? latestPi.Price : (price * 0.75m));

                        if (resolvedCost <= 0m) resolvedCost = price * 0.75m;

                        productResults.Add(new
                        {
                            id,
                            code = x.Id.ToString(),
                            barcode = x.ReferenceCode,
                            nameAr = x.ArabicName,
                            nameEn = x.EnglishName,
                            sellingPrice = price,
                            costPrice = decimal.Round(resolvedCost, 2),
                            availableQuantity = decimal.Round(qty, 2),
                            taxPercentage = isTaxExempted ? 0m : 15m,
                            includesTax = false,
                            categoryId,
                            categoryNameAr = "POS",
                            categoryNameEn = "POS",
                            isActive = x.IsActive,
                            unitName = x.MeasurementUnit?.ArabicName ?? x.MeasurementUnit?.EnglishName ?? "Unit",
                            storeDetailsId = resolvedStoreDetailsId,
                            retailPrice = decimal.Round(resolvedRetail, 2),
                            wholesalePrice = decimal.Round(resolvedWholesale, 2),
                            bulkPrice = decimal.Round(resolvedBulk, 2),
                            image = x.ImageUrl,
                            shelfNumber = latestPe?.ShelfNumber,
                            rowNumber = latestPe?.RowNumber,
                            batchNo = latestPe?.BatchNo,
                            expiryDate = latestPe?.ExpiryDate
                        });
                    }
                }

                return Ok(productResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return StatusCode(500, new { message = ex.ToString() });
            }
        }

        [HttpGet("products/by-session/{sessionId}")]
        public async Task<IActionResult> SearchProductsBySession(
            int sessionId,
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] string? barcode,
            [FromQuery] bool? inStock,
            [FromQuery] Guid? storeDetailsId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var session = _cashierStore.GetSession(sessionId);
            
            // If session not found in memory, try loading from database
            if (session is null)
            {
                var sessionRepo = _repositoriesFactory.GetRepository<Domains.Entities.Cashier.CashierSession>();
                var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();
                
                if (dbSession is null)
                {
                    return StatusCode(403, new { message = "Session not found", messageAr = "لم يتم العثور على الجلسة" });
                }
                
                var finalStoreId = storeDetailsId.HasValue && storeDetailsId.Value != Guid.Empty
                    ? storeDetailsId.Value
                    : (dbSession.StoreDetailsId ?? Guid.Empty);

                // Validate store
                if (finalStoreId == Guid.Empty)
                {
                    return StatusCode(403, new
                    {
                        errorCode = "SESSION_NO_STORE",
                        message = "Session is not bound to a store",
                        messageAr = "الجلسة غير مرتبطة بمخزن"
                    });
                }
                
                return await SearchProducts(search, categoryId, barcode, inStock, finalStoreId, sessionId, pageNumber, pageSize);
            }

            if (session.Status != 0)
            {
                return StatusCode(403, new { message = "Session is not active", messageAr = "الجلسة غير نشطة" });
            }

            var activeStoreId = storeDetailsId.HasValue && storeDetailsId.Value != Guid.Empty
                ? storeDetailsId.Value
                : (session.StoreDetailsId ?? Guid.Empty);

            if (activeStoreId == Guid.Empty)
            {
                return StatusCode(403, new
                {
                    errorCode = "SESSION_NO_STORE",
                    message = "Session is not bound to a store",
                    messageAr = "الجلسة غير مرتبطة بمخزن"
                });
            }

            return await SearchProducts(search, categoryId, barcode, inStock, activeStoreId, sessionId, pageNumber, pageSize);
        }

        [HttpGet("products/barcode/{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode, [FromQuery] Guid? storeDetailsId, [FromQuery] int? cashierSessionId)
        {
            try
            {
                var (resolvedStoreDetailsId, resolutionError) = await ResolveStoreDetailsIdAsync(storeDetailsId, cashierSessionId);
                if (!string.IsNullOrWhiteSpace(resolutionError))
                {
                    return BadRequest(new { message = resolutionError });
                }

                if (cashierSessionId.HasValue && !resolvedStoreDetailsId.HasValue)
                {
                    return BadRequest(new { message = "The active cashier session is not linked to any store. Please open a store-linked session first." });
                }

                var salesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
                var saleItemRepo = _repositoriesFactory.GetRepository<SaleItem>();
                var normalizedBarcode = barcode.Trim().ToLower();
                Guid? parsedGuid = null;
                if (Guid.TryParse(normalizedBarcode, out var tempGuid))
                {
                    parsedGuid = tempGuid;
                }

                var item = (await salesClassRepo.GetAll()).Data?
                    .FirstOrDefault(x =>
                        (x.ReferenceCode != null && x.ReferenceCode.ToLower() == normalizedBarcode) ||
                        (parsedGuid.HasValue && x.Id == parsedGuid.Value));
                if (item is null) return NotFound(new { message = "Product not found" });

                var id = _cashierStore.GetOrCreateProductId(item.Id);
                var qtyByClass = await GetAvailableQuantityBySalesClassAsync(new[] { item }, resolvedStoreDetailsId);
                var qty = qtyByClass.TryGetValue(item.Id, out var availableQty) ? availableQty : 0m;
                if (resolvedStoreDetailsId.HasValue)
                {
                    var reservedByClass = await _reservationService.GetReservedQuantitiesByClassAsync(resolvedStoreDetailsId.Value, new[] { item.Id });
                    var reservedQty = reservedByClass.TryGetValue(item.Id, out var r) ? r : 0m;
                    qty = Math.Max(0m, qty - reservedQty);
                }
                var price = (await saleItemRepo.Filter(x => x.MainSalesClassId == item.Id)).Data?.OrderByDescending(x => x.CreatedAt).FirstOrDefault()?.Price ?? 1m;

                var permitEntryRepo = _repositoriesFactory.GetRepository<PermitEntry>();
                var latestPe = (await permitEntryRepo.GetAll()).Data?
                    .Where(pe => (pe.MainPurchaseClassId == item.Id || pe.MainSalesClassId == item.Id) && (pe.RetailPrice > 0 || pe.WholesalePrice > 0 || pe.BulkPrice > 0))
                    .OrderByDescending(pe => pe.CreatedAt)
                    .FirstOrDefault();

                decimal resolvedRetail = (latestPe?.RetailPrice > 0) ? latestPe.RetailPrice : price;
                decimal resolvedWholesale = (latestPe?.WholesalePrice > 0) ? latestPe.WholesalePrice : resolvedRetail;
                decimal resolvedBulk = (latestPe?.BulkPrice > 0) ? latestPe.BulkPrice : resolvedWholesale;
                decimal resolvedCost = (latestPe?.UnitPrice > 0) ? latestPe.UnitPrice : (price * 0.75m);

                return Ok(new
                {
                    id,
                    code = item.Id.ToString(),
                    barcode = item.ReferenceCode,
                    nameAr = item.ArabicName,
                    nameEn = item.EnglishName,
                    sellingPrice = decimal.Round(resolvedRetail, 2),
                    retailPrice = decimal.Round(resolvedRetail, 2),
                    wholesalePrice = decimal.Round(resolvedWholesale, 2),
                    bulkPrice = decimal.Round(resolvedBulk, 2),
                    costPrice = decimal.Round(resolvedCost, 2),
                    availableQuantity = decimal.Round(qty, 2),
                    taxPercentage = item.IsTaxExempted ? 0m : 15m,
                    unitName = item.MeasurementUnit?.ArabicName ?? item.MeasurementUnit?.EnglishName ?? "Unit",
                    storeDetailsId = resolvedStoreDetailsId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by barcode");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("products/quick")]
        public async Task<IActionResult> GetQuickProducts()
        {
            try
            {
                var saleItemRepo = _repositoriesFactory.GetRepository<SaleItem>();
                var salesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
                var topIds = (await saleItemRepo.GetAll()).Data?
                    .GroupBy(x => x.MainSalesClassId)
                    .OrderByDescending(x => x.Sum(i => i.Quantity))
                    .Take(12)
                    .Select(x => x.Key)
                    .ToHashSet() ?? new HashSet<Guid>();
                if (topIds.Count == 0) return Ok(new List<object>());

                var classes = (await salesClassRepo.GetAll()).Data?.Where(x => topIds.Contains(x.Id)).ToList() ?? new List<MainSalesClass>();
                return Ok(classes.Select((x, i) => new
                {
                    productId = _cashierStore.GetOrCreateProductId(x.Id),
                    productName = x.ArabicName,
                    price = 0m,
                    barcode = x.ReferenceCode,
                    position = i + 1
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick products");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("legacy-invoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreatePOSInvoiceRequest request)
        {
            try
            {
                if (!HasCashierAccess())
                {
                    return StatusCode(403, new { message = "Cashier or Admin role is required" });
                }

                if (request.Items is null || request.Items.Count == 0) return BadRequest(new { message = "Invoice must contain at least one item" });

                var (resolvedStoreDetailsId, resolutionError) = await ResolveStoreDetailsIdAsync(request.StoreDetailsId, request.CashierSessionId);
                if (!string.IsNullOrWhiteSpace(resolutionError))
                {
                    return BadRequest(new { message = resolutionError });
                }

                if (!resolvedStoreDetailsId.HasValue)
                {
                    return BadRequest(new { message = "Invoice must be linked to a store. Please open a cashier session with a store first." });
                }

                var saleItems = new List<SaleItemPostDto>();
                var requestedByClass = new Dictionary<Guid, decimal>();
                var requestedNameByClass = new Dictionary<Guid, string>();
                foreach (var item in request.Items)
                {
                    var mainSalesClassId = await ResolveMainSalesClassIdAsync(item);
                    if (!mainSalesClassId.HasValue) return BadRequest(new { message = $"Unable to resolve sales class for item '{item.ItemName ?? item.ItemCode ?? item.ItemId.ToString()}'" });

                    var q = item.Quantity <= 0 ? 1m : item.Quantity;
                    var unitPrice = item.UnitPrice < 0 ? 0m : item.UnitPrice;
                    var sub = q * unitPrice;
                    var discount = item.DiscountAmount < 0 ? 0m : item.DiscountAmount;
                    var afterDiscount = Math.Max(0m, sub - discount);
                    var tax = item.TaxAmount > 0 ? item.TaxAmount : decimal.Round(afterDiscount * (Math.Max(0m, item.TaxPercentage) / 100m), 2);
                    var total = item.TotalAmount > 0 ? item.TotalAmount : afterDiscount + tax;

                    saleItems.Add(new SaleItemPostDto
                    {
                        ItemNo = item.ItemCode ?? item.ItemId.ToString(),
                        MainSalesClassId = mainSalesClassId.Value,
                        Price = decimal.Round(unitPrice, 2),
                        Quantity = q,
                        Tax = Math.Max(0m, item.TaxPercentage),
                        TaxAmount = decimal.Round(tax, 2),
                        Discount = Math.Max(0m, item.DiscountPercentage),
                        DiscountAmount = decimal.Round(discount, 2),
                        NetPrice = decimal.Round(afterDiscount, 2),
                        TotalPrice = decimal.Round(total, 2),
                        NoOfNights = 1
                    });

                    if (!requestedByClass.ContainsKey(mainSalesClassId.Value))
                    {
                        requestedByClass[mainSalesClassId.Value] = 0m;
                    }

                    requestedByClass[mainSalesClassId.Value] += q;

                    if (!requestedNameByClass.ContainsKey(mainSalesClassId.Value))
                    {
                        requestedNameByClass[mainSalesClassId.Value] = item.ItemName?.Trim() ?? item.ItemCode ?? item.ItemId.ToString();
                    }
                }

                var stockValidationError = await ValidateStoreStockAsync(resolvedStoreDetailsId.Value, requestedByClass, requestedNameByClass);
                if (!string.IsNullOrWhiteSpace(stockValidationError))
                {
                    return BadRequest(new { message = stockValidationError });
                }

                var subtotal = request.Subtotal > 0 ? request.Subtotal : saleItems.Sum(x => x.Price * x.Quantity);
                var discountAmount = Math.Max(0m, request.DiscountAmount);
                var net = Math.Max(0m, subtotal - discountAmount);
                var taxAmount = request.TaxAmount > 0 ? request.TaxAmount : saleItems.Sum(x => x.TaxAmount);
                var totalAmount = request.TotalAmount > 0 ? request.TotalAmount : net + taxAmount;
                var discountPercent = request.DiscountPercentage > 0 ? request.DiscountPercentage : (subtotal > 0 ? decimal.Round(discountAmount / subtotal * 100m, 2) : 0m);
                var taxPercent = request.TaxPercentage > 0 ? request.TaxPercentage : (net > 0 ? decimal.Round(taxAmount / net * 100m, 2) : 0m);
                var issueDate = request.InvoiceDate?.ToUniversalTime() ?? DateTime.UtcNow;
                var customerName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Walk-in Customer" : request.CustomerName!.Trim();

                var dto = new SaleInvoicePostDto
                {
                    CustomerProfileName = customerName,
                    Currency = "SAR",
                    Description = request.Notes,
                    IssueDate = issueDate,
                    DueDate = issueDate,
                    Tax = taxPercent,
                    TaxAmount = decimal.Round(taxAmount, 2),
                    Discount = discountPercent,
                    DiscountAmount = decimal.Round(discountAmount, 2),
                    TotalPrice = decimal.Round(totalAmount, 2),
                    NetPrice = decimal.Round(net, 2),
                    SaleItems = saleItems,
                    IsQuotation = false,
                    IsFrontal = false,
                    SaleInvoiceType = SaleInvoiceType.SaleInvoice
                };

                if (request.CashierSessionId.HasValue)
                {
                    try
                    {
                        var sessionId = request.CashierSessionId.Value;
                        var sessionRepo = _repositoriesFactory.GetRepository<CashierSession>();
                        var session = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();
                        var sessionNum = session?.SessionNumber ?? sessionId.ToString("D5");

                        var paymentRepo = _repositoriesFactory.GetRepository<CashierPayment>();
                        var sessionPayments = (await paymentRepo.Filter(p => p.SessionId == sessionId)).Data;
                        var existingInvoicesCount = sessionPayments != null ? await sessionPayments.CountAsync() : 0;

                        var shiftSeq = existingInvoicesCount + 1;
                        dto.InvoiceNo = $"Inv-{sessionNum}-{shiftSeq}";
                    }
                    catch (Exception exSeq)
                    {
                        _logger.LogWarning(exSeq, "[CashierController] Could not calculate shift invoice number");
                    }
                }

                var invoice = await _salesInvoicesService.AddAsync(dto);
                if (invoice is null) return StatusCode(500, new { message = "Failed to create invoice" });

                _cashierStore.TrackInvoice(invoice.Id, request.CashierSessionId, ResolveCurrentUserGuid(), ResolveCurrentUserName(), decimal.Round(totalAmount, 2), issueDate, resolvedStoreDetailsId, requestedByClass);

                await _reservationService.ReserveForInvoiceAsync(
                    invoice.Id,
                    request.CashierSessionId,
                    resolvedStoreDetailsId.Value,
                    ResolveCurrentUserGuid(),
                    requestedByClass,
                    "LEGACY_POS");

                await _auditService.LogAsync(
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    "INVOICE_CREATED",
                    "SaleInvoice",
                    invoice.Id.ToString(),
                    request.CashierSessionId,
                    resolvedStoreDetailsId,
                    new { invoice.InvoiceNo, totalAmount, IsLegacy = true });

                return Ok(new
                {
                    id = invoice.Id,
                    invoiceNo = invoice.InvoiceNo,
                    invoiceDate = invoice.IssueDate,
                    customerName,
                    subtotal = decimal.Round(subtotal, 2),
                    discountAmount = decimal.Round(discountAmount, 2),
                    taxAmount = decimal.Round(taxAmount, 2),
                    totalAmount = decimal.Round(totalAmount, 2),
                    paymentMethod = request.PaymentMethod,
                    paidAmount = request.PaidAmount,
                    changeAmount = request.ChangeAmount,
                    storeDetailsId = resolvedStoreDetailsId,
                    status = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("legacy-payment")]
        public async Task<IActionResult> ProcessPayment([FromBody] CashierPaymentRequest request)
        {
            try
            {
                if (!HasCashierAccess())
                {
                    return StatusCode(403, new { message = "Cashier or Admin role is required" });
                }

                if (!request.InvoiceId.HasValue || request.InvoiceId.Value <= 0) return BadRequest(new { message = "InvoiceId is required" });
                var invoice = await _salesInvoicesService.GetByIdAsync(request.InvoiceId.Value);
                if (invoice is null) return NotFound(new { message = "Invoice not found" });

                var normalized = NormalizePaymentAmounts(request);
                var payment = _cashierStore.RecordPayment(request.InvoiceId.Value, request.PaymentMethod, request.TotalAmount, normalized.ChangeAmount, normalized.CashAmount, normalized.CardAmount, normalized.CreditAmount);
                _cashierStore.MarkInvoicePaid(request.InvoiceId.Value);
                await _reservationService.ConfirmInvoiceAsync(request.InvoiceId.Value);

                // ✅ Save payment to database
                try
                {
                    var sessionId = HttpContext.Items["CashierSessionId"] as int?;
                    var (storeId, _) = await ResolveStoreDetailsIdAsync(null, sessionId);
                    
                    var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                    var dbPayment = new SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment
                    {
                        InvoiceId = request.InvoiceId.Value,
                        SessionId = sessionId ?? 0,
                        PaymentMethod = request.PaymentMethod,
                        TotalAmount = request.TotalAmount,
                        PaidAmount = request.TotalAmount,
                        ChangeAmount = normalized.ChangeAmount,
                        CashAmount = normalized.CashAmount,
                        CardAmount = normalized.CardAmount,
                        CreditAmount = normalized.CreditAmount,
                        PaidAtUtc = DateTime.UtcNow,
                        StoreDetailsId = storeId,
                        IsActive = true,
                        CreatedBy = ResolveCurrentUserGuid(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await paymentRepo.Add(dbPayment);
                    
                    // ✅ Update session statistics in database
                    if (sessionId.HasValue)
                    {
                        var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                        var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId.Value)).Data?.FirstOrDefault();
                        if (dbSession != null)
                        {
                            dbSession.TotalSales += invoice.NetPrice;
                            dbSession.TransactionsCount += 1;
                            dbSession.TotalCash += normalized.CashAmount;
                            dbSession.TotalCard += normalized.CardAmount;
                            dbSession.TotalCredit += normalized.CreditAmount;
                            await sessionRepo.Update(dbSession);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save payment to database for invoice {InvoiceId}", request.InvoiceId.Value);
                }

                try { await _salesInvoicesService.CreateTransaction(request.InvoiceId.Value); }
                catch (Exception ex) { _logger.LogWarning(ex, "Transaction creation failed for invoice {InvoiceId}", request.InvoiceId.Value); }

                var sessionId2 = HttpContext.Items["CashierSessionId"] as int?;
                var (storeId2, _) = await ResolveStoreDetailsIdAsync(null, sessionId2);
                await _auditService.LogAsync(
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    "PAYMENT_PROCESSED",
                    "SaleInvoice",
                    request.InvoiceId.Value.ToString(),
                    sessionId2,
                    storeId2,
                    new { request.PaymentMethod, request.TotalAmount, IsLegacy = true });

                return Ok(new
                {
                    success = true,
                    invoiceId = request.InvoiceId.Value,
                    invoiceNo = invoice.InvoiceNo,
                    paidAmount = payment.PaidAmount,
                    changeAmount = payment.ChangeAmount,
                    message = "Payment processed successfully",
                    receiptUrl = $"/api/cashier/invoice/{request.InvoiceId.Value}/receipt"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetInvoice(int invoiceId)
        {
            try
            {
                var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
                if (invoice is null) return NotFound(new { message = "Invoice not found" });

                var payment = _cashierStore.GetPayment(invoiceId);
                var tracked = _cashierStore.GetInvoiceState(invoiceId);
                var totalAmount = invoice.TotalPrice > 0 ? invoice.TotalPrice : invoice.NetPrice + invoice.TaxAmount;
                var paidAmount = payment?.PaidAmount ?? totalAmount;

                return Ok(new
                {
                    id = invoice.Id,
                    invoiceNo = invoice.InvoiceNo,
                    invoiceDate = invoice.IssueDate,
                    cashierSessionId = tracked?.SessionId,
                    storeDetailsId = tracked?.StoreDetailsId,
                    cashierUserId = tracked is null ? (int?)null : _cashierStore.GetOrCreateUserNumericId(tracked.CashierUserGuid),
                    cashierUserName = tracked?.CashierUserName,
                    customerName = invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "Walk-in Customer",
                    subtotal = invoice.NetPrice + invoice.DiscountAmount,
                    discountAmount = invoice.DiscountAmount,
                    taxAmount = invoice.TaxAmount,
                    totalAmount,
                    paymentMethod = payment?.PaymentMethod ?? 0,
                    paidAmount,
                    changeAmount = payment?.ChangeAmount ?? 0m,
                    status = tracked?.IsRefunded == true ? 3 : 1,
                    isRefunded = tracked?.IsRefunded == true,
                    items = invoice.SaleItems.Select(item => new
                    {
                        itemId = _cashierStore.GetOrCreateProductId(item.MainSalesClassId),
                        itemCode = item.MainSalesClassId.ToString(),
                        itemName = item.MainSalesClass?.ArabicName ?? item.MainSalesClass?.EnglishName ?? item.Name ?? item.ItemNo,
                        quantity = item.Quantity,
                        unitPrice = item.Price,
                        discountAmount = item.DiscountAmount,
                        taxAmount = item.TaxAmount,
                        totalAmount = item.TotalPrice
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("invoice/{invoiceId}/refund")]
        public async Task<IActionResult> RefundInvoice(int invoiceId, [FromBody] RefundRequest request)
        {
            try
            {
                var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
                if (invoice is null) return NotFound(new { message = "Invoice not found" });

                var success = _cashierStore.MarkInvoiceRefunded(invoiceId, request.Reason);
                if (!success) return BadRequest(new { message = "Unable to refund invoice" });
                return Ok(new { message = "Invoice refunded successfully", invoiceId, reason = request.Reason });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding invoice");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("invoice/{invoiceId}/receipt")]
        public async Task<IActionResult> PrintReceipt(int invoiceId)
        {
            try
            {
                var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
                if (invoice is null) return NotFound(new { message = "Invoice not found" });

                var payment = _cashierStore.GetPayment(invoiceId);
                var totalAmount = invoice.TotalPrice > 0 ? invoice.TotalPrice : invoice.NetPrice + invoice.TaxAmount;
                var text = string.Join(Environment.NewLine, new[]
                {
                    $"Invoice: {invoice.InvoiceNo}",
                    $"Date: {invoice.IssueDate:yyyy-MM-dd HH:mm:ss}",
                    $"Customer: {invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "Walk-in Customer"}",
                    $"Total: {totalAmount:0.00}",
                    $"Paid: {(payment?.PaidAmount ?? totalAmount):0.00}",
                    $"Change: {(payment?.ChangeAmount ?? 0m):0.00}"
                });

                return File(System.Text.Encoding.UTF8.GetBytes(text), "text/plain", $"receipt-{invoiceId}.txt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing receipt");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reports/daily-sales")]
        public async Task<IActionResult> GetDailySalesReport([FromQuery] DateTime date)
        {
            try
            {
                var report = _cashierStore.BuildDailySalesReport(date);
                return Ok(new
                {
                    date = report.Date,
                    totalSales = report.TotalSales,
                    totalTransactions = report.TotalTransactions,
                    cashAmount = report.CashAmount,
                    cardAmount = report.CardAmount,
                    creditAmount = report.CreditAmount,
                    refundsAmount = report.RefundsAmount,
                    averageTransaction = report.AverageTransaction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily sales report");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("reports/performance")]
        public async Task<IActionResult> GetCashierPerformance([FromQuery] int cashierId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var userGuid = cashierId > 0 ? _cashierStore.TryResolveUserGuid(cashierId) : ResolveCurrentUserGuid();
                if (!userGuid.HasValue || userGuid.Value == Guid.Empty)
                {
                    return Ok(new { cashierId, fromDate, toDate, totalSales = 0m, totalTransactions = 0, totalCash = 0m, totalCard = 0m, totalCredit = 0m, averageTransaction = 0m, sessionsCount = 0 });
                }

                var report = _cashierStore.BuildPerformanceReport(userGuid.Value, fromDate.ToUniversalTime(), toDate.ToUniversalTime());
                return Ok(new
                {
                    cashierId = _cashierStore.GetOrCreateUserNumericId(report.CashierUserGuid),
                    fromDate,
                    toDate,
                    totalSales = report.TotalSales,
                    totalTransactions = report.TotalTransactions,
                    totalCash = report.TotalCash,
                    totalCard = report.TotalCard,
                    totalCredit = report.TotalCredit,
                    averageTransaction = report.AverageTransaction,
                    sessionsCount = report.SessionsCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cashier performance");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private async Task<(Guid? StoreDetailsId, string? ErrorMessage)> ResolveStoreDetailsIdAsync(Guid? requestStoreDetailsId, int? cashierSessionId)
        {
            var normalizedRequestStoreId = requestStoreDetailsId.HasValue && requestStoreDetailsId.Value != Guid.Empty
                ? requestStoreDetailsId
                : null;

            if (normalizedRequestStoreId.HasValue)
            {
                return (normalizedRequestStoreId, null);
            }

            Guid? sessionStoreId = null;
            if (cashierSessionId.HasValue)
            {
                sessionStoreId = _cashierStore.GetSessionStoreDetailsId(cashierSessionId.Value);
                
                // If not found in memory, try loading from database
                if (!sessionStoreId.HasValue)
                {
                    try
                    {
                        var sessionRepo = _repositoriesFactory.GetRepository<Domains.Entities.Cashier.CashierSession>();
                        var dbSessionResult = await sessionRepo.Filter(s => s.SessionId == cashierSessionId.Value);
                        var dbSession = dbSessionResult.Data?.FirstOrDefault();
                        sessionStoreId = dbSession?.StoreDetailsId;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load session {SessionId} from database", cashierSessionId.Value);
                    }
                }
            }
            else
            {
                var userGuid = ResolveCurrentUserGuid();
                if (userGuid != Guid.Empty)
                {
                    var activeSession = _cashierStore.GetCurrentSession(userGuid);
                    if (activeSession != null)
                    {
                        sessionStoreId = activeSession.StoreDetailsId;
                    }
                }
            }

            return (sessionStoreId, null);
        }


        private async Task<string?> ValidateStoreStockAsync(
            Guid storeDetailsId,
            IReadOnlyDictionary<Guid, decimal> requestedByClass,
            IReadOnlyDictionary<Guid, string> requestedNameByClass)
        {
            if (requestedByClass.Count == 0)
            {
                return null;
            }

            var classIds = requestedByClass.Keys.ToList();
            var salesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
            var salesClasses = (await salesClassRepo.Filter(x => classIds.Contains(x.Id))).Data?.ToList() ?? new List<MainSalesClass>();

            var availableByClass = await GetAvailableQuantityBySalesClassAsync(salesClasses, storeDetailsId);
            var reservedByClass = await _reservationService.GetReservedQuantitiesByClassAsync(storeDetailsId, classIds);

            foreach (var requestedItem in requestedByClass)
            {
                var requestedQty = Math.Max(0m, requestedItem.Value);
                var availableQty = availableByClass.TryGetValue(requestedItem.Key, out var available) ? available : 0m;
                var reservedQty = reservedByClass.TryGetValue(requestedItem.Key, out var reserved) ? reserved : 0m;
                var netAvailableQty = Math.Max(0m, availableQty - reservedQty);
                if (netAvailableQty >= requestedQty)
                {
                    continue;
                }

                var itemName = requestedNameByClass.TryGetValue(requestedItem.Key, out var name) && !string.IsNullOrWhiteSpace(name)
                    ? name
                    : requestedItem.Key.ToString();

                return $"Insufficient stock for '{itemName}' in the selected store. Available: {decimal.Round(netAvailableQty, 2)}, requested: {decimal.Round(requestedQty, 2)}.";
            }

            return null;
        }

        private async Task<Dictionary<Guid, decimal>> GetAvailableQuantityByPurchaseClassAsync(
            IReadOnlyCollection<MainPurchaseClass> purchaseClasses,
            Guid? storeDetailsId)
        {
            // IMPORTANT: Don't limit to purchaseClasses parameter only
            // We need to get ALL purchase classes that have quantities in the store
            var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var allPurchaseClassesQuery = (await purchaseClassRepo.GetAll()).Data;
            if (allPurchaseClassesQuery == null)
            {
                _logger.LogWarning("[CashierController] Could not load purchase classes for quantity calculation");
                return new Dictionary<Guid, decimal>();
            }
            
            var allPurchaseClasses = allPurchaseClassesQuery
                .IgnoreQueryFilters()
                .Where(x => x.IsActive)
                .ToList();
            
            _logger.LogInformation("[CashierController] Using {TotalCount} purchase classes for quantity calculation (parameter had {ParamCount})", 
                allPurchaseClasses.Count, purchaseClasses.Count);
            
            var classIds = allPurchaseClasses.Select(x => x.Id).ToHashSet();
            var quantityByClass = new Dictionary<Guid, decimal>();

            // حساب الكميات من PermitEntry مباشرة بناءً على MainPurchaseClassId
            _logger.LogInformation("[CashierController] Calculating quantities from PermitEntry (PurchaseClass) for store: {StoreId}", storeDetailsId);
            
            var permitRepo = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();
            var allPermitsQuery = !storeDetailsId.HasValue 
                ? await permitRepo.GetAll()
                : await permitRepo.Filter(p => p.StoreDetailsId == storeDetailsId.Value);
            
            var allPermits = allPermitsQuery.Data?
                .IgnoreQueryFilters()
                .Where(p => p.IsActive)
                .Include(p => p.PermitEntries)
                .ToList() ?? new List<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();

            _logger.LogInformation("[CashierController] Found {PermitCount} active permits with total {EntryCount} entries", 
                allPermits.Count, 
                allPermits.Sum(p => p.PermitEntries?.Count ?? 0));

            // حساب الكميات: Entry بيزود، Exit بينقص
            var processedEntriesCount = 0;
            var skippedEntriesCount = 0;
            
            foreach (var permit in allPermits)
            {
                if (permit.PermitEntries == null) continue;
                
                foreach (var entry in permit.PermitEntries.Where(e => e.IsActive))
                {
                    // استخدام MainPurchaseClassId مباشرة
                    if (entry.MainPurchaseClassId == Guid.Empty || !classIds.Contains(entry.MainPurchaseClassId))
                    {
                        skippedEntriesCount++;
                        continue;
                    }
                    
                    processedEntriesCount++;
                    
                    // إضافة أو خصم الكمية حسب نوع الإذن
                    var quantity = entry.Quantity;
                    if (permit.PermitType == Domains.Enums.PermitType.Entry)
                    {
                        AddQuantity(quantityByClass, entry.MainPurchaseClassId, quantity);
                    }
                    else if (permit.PermitType == Domains.Enums.PermitType.Exit)
                    {
                        AddQuantity(quantityByClass, entry.MainPurchaseClassId, -quantity);
                    }
                }
            }

            _logger.LogInformation("[CashierController] Processed {ProcessedCount} entries, skipped {SkippedCount} entries. Calculated quantities for {ProductCount} purchase products", 
                processedEntriesCount, skippedEntriesCount, quantityByClass.Count);

            // ──── Apply CashierInventoryMovements Adjustment ────
            try
            {
                var movementsRepo = _repositoriesFactory.GetRepository<CashierInventoryMovement>();
                var allMovementsQuery = !storeDetailsId.HasValue 
                    ? await movementsRepo.GetAll()
                    : await movementsRepo.Filter(m => m.StoreDetailsId == storeDetailsId.Value);
                
                var allMovements = allMovementsQuery.Data?
                    .IgnoreQueryFilters()
                    .Where(m => m.IsActive)
                    .ToList() ?? new List<CashierInventoryMovement>();

                foreach (var mov in allMovements)
                {
                    if (mov.MainPurchaseClassId == Guid.Empty || !classIds.Contains(mov.MainPurchaseClassId))
                    {
                        continue;
                    }

                    if (mov.MovementType == "SALE" || mov.InvoiceId != 0)
                    {
                        continue;
                    }

                    var quantity = mov.Quantity;
                    if (mov.Direction == 0) // Entry
                    {
                        AddQuantity(quantityByClass, mov.MainPurchaseClassId, quantity);
                    }
                    else if (mov.Direction == 1) // Exit
                    {
                        AddQuantity(quantityByClass, mov.MainPurchaseClassId, -quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GetAvailableQuantityByPurchaseClassAsync] Failed to apply CashierInventoryMovements adjustment");
            }

            return quantityByClass;
        }

        private async Task<Dictionary<Guid, decimal>> GetAvailableQuantityBySalesClassAsync(
            IReadOnlyCollection<MainSalesClass> salesClasses,
            Guid? storeDetailsId)
        {
            // IMPORTANT: Don't limit to salesClasses parameter only
            // We need to get ALL sales classes that have quantities in the store
            var salesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
            var allSalesClassesQuery = (await salesClassRepo.GetAll()).Data;
            if (allSalesClassesQuery == null)
            {
                _logger.LogWarning("[CashierController] Could not load sales classes for quantity calculation");
                return new Dictionary<Guid, decimal>();
            }
            
            var allSalesClasses = allSalesClassesQuery
                .IgnoreQueryFilters()
                .Where(x => x.IsActive)
                .ToList();
            
            _logger.LogInformation("[CashierController] Using {TotalCount} sales classes for quantity calculation (parameter had {ParamCount})", 
                allSalesClasses.Count, salesClasses.Count);
            
            var classIds = allSalesClasses.Select(x => x.Id).ToHashSet();
            var classIdByReferenceCode = allSalesClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.ReferenceCode))
                .GroupBy(x => NormalizeLookup(x.ReferenceCode))
                .ToDictionary(group => group.Key, group => group.First().Id);
            var classIdByArabicName = allSalesClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.ArabicName))
                .GroupBy(x => NormalizeLookup(x.ArabicName))
                .ToDictionary(group => group.Key, group => group.First().Id);
            var classIdByEnglishName = allSalesClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.EnglishName))
                .GroupBy(x => NormalizeLookup(x.EnglishName))
                .ToDictionary(group => group.Key, group => group.First().Id);

            var quantityByClass = new Dictionary<Guid, decimal>();

            // CRITICAL FIX: Calculate quantities from InventoryPermit (PermitEntry) 
            // instead of ProductUnit which may be empty
            _logger.LogInformation("[CashierController] Calculating quantities from PermitEntry for store: {StoreId}", storeDetailsId);
            
            var permitRepo = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();
            var allPermitsQueryResult = !storeDetailsId.HasValue 
                ? await permitRepo.GetAll()
                : await permitRepo.Filter(p => p.StoreDetailsId == storeDetailsId.Value);
            
            if (allPermitsQueryResult.Data == null)
            {
                _logger.LogWarning("[CashierController] Permits query returned null");
                return new Dictionary<Guid, decimal>();
            }
            
            var allPermits = allPermitsQueryResult.Data
                .IgnoreQueryFilters()
                .Where(p => p.IsActive)
                .Include(p => p.PermitEntries)
                .ToList();

            _logger.LogInformation("[CashierController] Found {PermitCount} active permits with total {EntryCount} entries", 
                allPermits.Count, 
                allPermits.Sum(p => p.PermitEntries?.Count ?? 0));

            // Calculate quantities: Entry permits add, Exit permits subtract
            var purchaseClassToSalesClass = new Dictionary<Guid, Guid>();
            var skippedEntriesCount = 0;
            var processedEntriesCount = 0;
            
            foreach (var permit in allPermits)
            {
                if (permit.PermitEntries == null) continue;
                
                var activeEntries = permit.PermitEntries.Where(e => e.IsActive).ToList();
                _logger.LogInformation("[CashierController] Permit {PermitId} ({PermitType}) has {ActiveEntryCount} active entries out of {TotalEntryCount}",
                    permit.Id, permit.PermitType, activeEntries.Count, permit.PermitEntries.Count);
                
                foreach (var entry in activeEntries)
                {
                    // Map MainPurchaseClassId to MainSalesClassId
                    Guid? salesClassId = entry.MainSalesClassId;
                    
                    if (!salesClassId.HasValue && entry.MainPurchaseClassId != Guid.Empty)
                    {
                        // Try to map from cache first
                        if (purchaseClassToSalesClass.TryGetValue(entry.MainPurchaseClassId, out var cachedSalesId))
                        {
                            salesClassId = cachedSalesId;
                        }
                        else
                        {
                            // Load purchase class and map to sales class
                            var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                            var purchaseClass = (await purchaseClassRepo.Filter(x => x.Id == entry.MainPurchaseClassId))
                                .Data?.FirstOrDefault();
                            
                            if (purchaseClass != null)
                            {
                                var mappedId = TryMapPurchaseClassToSalesClass(
                                    purchaseClass,
                                    classIdByReferenceCode,
                                    classIdByArabicName,
                                    classIdByEnglishName);
                                
                                if (mappedId.HasValue)
                                {
                                    purchaseClassToSalesClass[entry.MainPurchaseClassId] = mappedId.Value;
                                    salesClassId = mappedId.Value;
                                }
                                else
                                {
                                    _logger.LogWarning("[CashierController] Failed to map PurchaseClass {PurchaseClassId} ({PurchaseClassName}) to SalesClass",
                                        entry.MainPurchaseClassId, purchaseClass.ArabicName ?? purchaseClass.EnglishName);
                                }
                            }
                        }
                    }
                    
                    if (!salesClassId.HasValue || !classIds.Contains(salesClassId.Value))
                    {
                        skippedEntriesCount++;
                        _logger.LogDebug("[CashierController] Skipping entry: SalesClassId={SalesClassId}, InClassIds={InClassIds}",
                            salesClassId, salesClassId.HasValue && classIds.Contains(salesClassId.Value));
                        continue;
                    }
                    
                    processedEntriesCount++;
                    
                    // Add quantity based on permit type
                    var quantity = entry.Quantity;
                    if (permit.PermitType == Domains.Enums.PermitType.Entry)
                    {
                        AddQuantity(quantityByClass, salesClassId.Value, quantity);
                    }
                    else if (permit.PermitType == Domains.Enums.PermitType.Exit)
                    {
                        AddQuantity(quantityByClass, salesClassId.Value, -quantity);
                    }
                }
            }

            _logger.LogInformation("[CashierController] Processed {ProcessedCount} entries, skipped {SkippedCount} entries. Calculated quantities for {ProductCount} products", 
                processedEntriesCount, skippedEntriesCount, quantityByClass.Count);

            // FALLBACK 1: Try ProductUnit only if PermitEntry yielded no results
            var purchaseById = new Dictionary<Guid, MainPurchaseClass>();
            if (quantityByClass.Count == 0)
            {
                _logger.LogInformation("[CashierController] No quantities from PermitEntry, trying ProductUnit fallback");
                var productUnitRepo = _repositoriesFactory.GetRepository<ProductUnit>();
                var units = (await productUnitRepo.GetAll()).Data?
                    .Where(x =>
                        x.Quantity.HasValue &&
                        (!storeDetailsId.HasValue || x.StoreDetailsId == storeDetailsId.Value) &&
                        (x.MainSalesClassId.HasValue || x.MainPurchaseClassId.HasValue))
                    .ToList() ?? new List<ProductUnit>();

                foreach (var unit in units.Where(x => x.MainSalesClassId.HasValue))
                {
                    var classId = unit.MainSalesClassId!.Value;
                    if (!classIds.Contains(classId))
                    {
                        continue;
                    }

                    AddQuantity(quantityByClass, classId, unit.Quantity ?? 0m);
                }

                var fallbackUnits = units
                    .Where(x => !x.MainSalesClassId.HasValue && x.MainPurchaseClassId.HasValue)
                    .ToList();
                
                if (fallbackUnits.Count > 0)
                {
                    var purchaseIds = fallbackUnits.Select(x => x.MainPurchaseClassId!.Value).Distinct().ToList();
                    var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                    var purchaseClasses = (await purchaseClassRepo.Filter(x => purchaseIds.Contains(x.Id))).Data?.ToList() ?? new List<MainPurchaseClass>();
                    purchaseById = purchaseClasses.ToDictionary(x => x.Id, x => x);

                    foreach (var unit in fallbackUnits)
                    {
                        if (!purchaseById.TryGetValue(unit.MainPurchaseClassId!.Value, out var purchaseClass))
                        {
                            continue;
                        }

                        var mappedClassId = TryMapPurchaseClassToSalesClass(
                            purchaseClass,
                            classIdByReferenceCode,
                            classIdByArabicName,
                            classIdByEnglishName);

                        if (!mappedClassId.HasValue || !classIds.Contains(mappedClassId.Value))
                        {
                            continue;
                        }

                        AddQuantity(quantityByClass, mappedClassId.Value, unit.Quantity ?? 0m);
                    }
                }
                
                _logger.LogInformation("[CashierController] ProductUnit fallback found {ProductCount} products", quantityByClass.Count);
            }

            // FALLBACK 2: Try OperationEntry only if still no results
            if (quantityByClass.Count == 0)
            {
                _logger.LogInformation("[CashierController] Still no quantities, trying OperationEntry fallback");
                var operationEntryRepo = _repositoriesFactory.GetRepository<OperationEntry>();
                var operationEntries = (await operationEntryRepo.GetAll()).Data?
                    .Where(x =>
                        (!storeDetailsId.HasValue || x.StoreDetailsId == storeDetailsId.Value) &&
                        x.MainPurchaseClassId != Guid.Empty)
                    .ToList() ?? new List<OperationEntry>();

                if (operationEntries.Count > 0)
                {
                    var operationPurchaseIds = operationEntries.Select(x => x.MainPurchaseClassId).Distinct().ToList();
                    var missingPurchaseIds = operationPurchaseIds.Where(x => !purchaseById.ContainsKey(x)).ToList();
                    if (missingPurchaseIds.Count > 0)
                    {
                        var additionalPurchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                        var extraPurchaseClasses = (await additionalPurchaseClassRepo.Filter(x => missingPurchaseIds.Contains(x.Id))).Data?.ToList() ?? new List<MainPurchaseClass>();
                        foreach (var purchaseClass in extraPurchaseClasses)
                        {
                            purchaseById[purchaseClass.Id] = purchaseClass;
                        }
                    }

                    var quantityByClassFromOperations = new Dictionary<Guid, decimal>();
                    foreach (var entry in operationEntries)
                    {
                        if (!purchaseById.TryGetValue(entry.MainPurchaseClassId, out var purchaseClass))
                        {
                            continue;
                        }

                        var mappedClassId = TryMapPurchaseClassToSalesClass(
                            purchaseClass,
                            classIdByReferenceCode,
                            classIdByArabicName,
                            classIdByEnglishName);

                        if (!mappedClassId.HasValue || !classIds.Contains(mappedClassId.Value))
                        {
                            continue;
                        }

                        AddQuantity(quantityByClassFromOperations, mappedClassId.Value, entry.Quantity);
                    }

                    foreach (var mappedEntry in quantityByClassFromOperations)
                    {
                        if (quantityByClass.ContainsKey(mappedEntry.Key))
                        {
                            continue;
                        }

                        quantityByClass[mappedEntry.Key] = mappedEntry.Value;
                    }
                    
                    _logger.LogInformation("[CashierController] OperationEntry fallback found {ProductCount} products", quantityByClass.Count);
                }
            }

            // ──── Apply CashierInventoryMovements Adjustment ────
            try
            {
                var movementsRepo = _repositoriesFactory.GetRepository<CashierInventoryMovement>();
                var allMovementsQuery = !storeDetailsId.HasValue 
                    ? await movementsRepo.GetAll()
                    : await movementsRepo.Filter(m => m.StoreDetailsId == storeDetailsId.Value);
                
                var allMovements = allMovementsQuery.Data?
                    .IgnoreQueryFilters()
                    .Where(m => m.IsActive)
                    .ToList() ?? new List<CashierInventoryMovement>();

                var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var allPurchaseClasses = (await purchaseClassRepo.GetAll()).Data?.IgnoreQueryFilters().Where(x => x.IsActive).ToList() ?? new List<MainPurchaseClass>();

                foreach (var mov in allMovements)
                {
                    if (mov.MainPurchaseClassId == Guid.Empty)
                    {
                        continue;
                    }

                    if (mov.MovementType == "SALE" || mov.InvoiceId != 0)
                    {
                        continue;
                    }

                    var purchaseClass = allPurchaseClasses.FirstOrDefault(x => x.Id == mov.MainPurchaseClassId);
                    if (purchaseClass == null) continue;

                    var mappedClassId = TryMapPurchaseClassToSalesClass(
                        purchaseClass,
                        classIdByReferenceCode,
                        classIdByArabicName,
                        classIdByEnglishName);

                    if (!mappedClassId.HasValue || !classIds.Contains(mappedClassId.Value))
                    {
                        continue;
                    }

                    var quantity = mov.Quantity;
                    if (mov.Direction == 0) // Entry
                    {
                        AddQuantity(quantityByClass, mappedClassId.Value, quantity);
                    }
                    else if (mov.Direction == 1) // Exit
                    {
                        AddQuantity(quantityByClass, mappedClassId.Value, -quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GetAvailableQuantityBySalesClassAsync] Failed to apply CashierInventoryMovements adjustment");
            }

            return quantityByClass;
        }

        private static Guid? TryMapPurchaseClassToSalesClass(
            MainPurchaseClass purchaseClass,
            IReadOnlyDictionary<string, Guid> classIdByReferenceCode,
            IReadOnlyDictionary<string, Guid> classIdByArabicName,
            IReadOnlyDictionary<string, Guid> classIdByEnglishName)
        {
            var normalizedReferenceCode = NormalizeLookup(purchaseClass.ReferenceCode);
            if (normalizedReferenceCode.Length > 0 && classIdByReferenceCode.TryGetValue(normalizedReferenceCode, out var classIdFromCode))
            {
                return classIdFromCode;
            }

            var normalizedArabicName = NormalizeLookup(purchaseClass.ArabicName);
            if (normalizedArabicName.Length > 0 && classIdByArabicName.TryGetValue(normalizedArabicName, out var classIdFromArabicName))
            {
                return classIdFromArabicName;
            }

            var normalizedEnglishName = NormalizeLookup(purchaseClass.EnglishName);
            if (normalizedEnglishName.Length > 0 && classIdByEnglishName.TryGetValue(normalizedEnglishName, out var classIdFromEnglishName))
            {
                return classIdFromEnglishName;
            }

            return null;
        }

        private static void AddQuantity(Dictionary<Guid, decimal> quantityByClass, Guid classId, decimal quantity)
        {
            if (quantity == 0m)
            {
                return;
            }

            if (quantityByClass.TryGetValue(classId, out var existingQuantity))
            {
                quantityByClass[classId] = existingQuantity + quantity;
                return;
            }

            quantityByClass[classId] = quantity;
        }

        private static string NormalizeLookup(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private async Task<Guid?> ResolveMainSalesClassIdAsync(InvoiceItemRequest item)
        {
            if (!string.IsNullOrWhiteSpace(item.ItemCode) && Guid.TryParse(item.ItemCode, out var parsedGuid)) return parsedGuid;
            if (item.ItemId > 0)
            {
                var mapped = _cashierStore.TryResolveProductGuid(item.ItemId);
                if (mapped.HasValue) return mapped.Value;
            }
            if (!string.IsNullOrWhiteSpace(item.ItemName))
            {
                var fromName = await FindMainSalesClassByNameAsync(item.ItemName!);
                if (fromName.HasValue) return fromName.Value;
                return await EnsureMainSalesClassExistsAsync(item.ItemName!, item.ItemCode);
            }
            return null;
        }

        private async Task<Guid?> FindMainSalesClassByNameAsync(string name)
        {
            var normalized = name.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized)) return null;

            var existing = (await _repositoriesFactory.GetRepository<MainSalesClass>().GetAll()).Data?.FirstOrDefault(x =>
                (!string.IsNullOrWhiteSpace(x.ArabicName) && x.ArabicName.ToLower() == normalized) ||
                (!string.IsNullOrWhiteSpace(x.EnglishName) && x.EnglishName.ToLower() == normalized) ||
                (!string.IsNullOrWhiteSpace(x.ReferenceCode) && x.ReferenceCode.ToLower() == normalized));
            return existing?.Id;
        }

        private async Task<Guid?> EnsureMainSalesClassExistsAsync(string itemName, string? itemCode)
        {
            var repo = _repositoriesFactory.GetRepository<MainSalesClass>();
            var measurementRepo = _repositoriesFactory.GetRepository<MeasurementUnit>();
            var detailRepo = _repositoriesFactory.GetRepository<DetailSalesClass>();

            var measurementData = (await measurementRepo.GetAll()).Data?.ToList() ?? new List<MeasurementUnit>();
            var detailData = (await detailRepo.GetAll()).Data?.ToList() ?? new List<DetailSalesClass>();
            var measurement = measurementData.FirstOrDefault(x => x.EnglishName.Equals("Nos", StringComparison.OrdinalIgnoreCase)) ?? measurementData.FirstOrDefault();
            var detail = detailData.FirstOrDefault(x => x.EnglishName.Equals("Detail Container", StringComparison.OrdinalIgnoreCase)) ?? detailData.FirstOrDefault();
            if (measurement is null || detail is null) return null;

            var allExisting = (await repo.GetAll()).Data?.ToList() ?? new List<MainSalesClass>();
            var refCode = BuildReferenceCode(itemCode ?? itemName);
            while (allExisting.Any(x => x.ReferenceCode.Equals(refCode, StringComparison.OrdinalIgnoreCase)))
            {
                refCode = BuildReferenceCode($"{refCode}-{Guid.NewGuid():N}");
            }

            var created = await repo.Add(new MainSalesClass
            {
                ArabicName = itemName.Trim(),
                EnglishName = itemName.Trim(),
                ReferenceCode = refCode,
                MeasurementUnitId = measurement.Id,
                DetailSalesClassId = detail.Id,
                ClassType = ClassType.Direct
            });

            return created.Data?.Id;
        }

        private static string BuildReferenceCode(string value)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? $"POS-{Guid.NewGuid():N}" : value.Trim();
            var cleaned = new string(normalized.ToUpperInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            if (string.IsNullOrWhiteSpace(cleaned)) cleaned = $"POS-{Guid.NewGuid():N}";
            return cleaned.Length <= 100 ? cleaned : cleaned[..100];
        }

        private (decimal CashAmount, decimal CardAmount, decimal CreditAmount, decimal ChangeAmount) NormalizePaymentAmounts(CashierPaymentRequest request)
        {
            var total = Math.Max(0m, request.TotalAmount);
            decimal cash = 0m, card = 0m, credit = 0m, change = 0m;

            switch (request.PaymentMethod)
            {
                case 0:
                    cash = request.CashAmount ?? total;
                    change = Math.Max(0m, cash - total);
                    break;
                case 1:
                    card = request.CardAmount ?? total;
                    break;
                case 2:
                    credit = total;
                    break;
                case 3:
                    cash = request.CashAmount ?? 0m;
                    card = request.CardAmount ?? 0m;
                    if (cash <= 0m && card <= 0m) cash = total;
                    if ((cash + card) < total) card += total - (cash + card);
                    break;
                default:
                    card = total;
                    break;
            }

            return (cash, card, credit, change);
        }

        private Guid ResolveCurrentUserGuid()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        private string ResolveCurrentUserName()
        {
            var name = User.FindFirstValue(ClaimTypes.GivenName) ?? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Cashier";
            return string.IsNullOrWhiteSpace(name) ? "Cashier" : name.Trim();
        }

        private bool HasCashierAccess()
        {
            return User.IsInRole("Admin") || User.IsInRole("Cashier") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.POS");
        }
    }

    public class CreatePOSInvoiceRequest
    {
        public DateTime? InvoiceDate { get; set; }
        public int? CashierSessionId { get; set; }
        public Guid? StoreDetailsId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public List<InvoiceItemRequest> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class InvoiceItemRequest
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CashierPaymentRequest
    {
        public int? InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaymentMethod { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? CardAmount { get; set; }
        public int? CreditDays { get; set; }
        public string? Notes { get; set; }
    }

    public class RefundRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
