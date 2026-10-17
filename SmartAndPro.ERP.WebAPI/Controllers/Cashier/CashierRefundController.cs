using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using System.Security.Claims;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Recipes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier
{
    [Authorize]
    [ApiController]
    [Route("api/cashier/refund")]
    public class CashierRefundController : ControllerBase
    {
        private readonly ILogger<CashierRefundController> _logger;
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ICashierRuntimeStore _cashierStore;
        private readonly IAuthService _authService;
        private readonly ICashierAuditService _auditService;
        private readonly ICashierReservationService _reservationService;

        public CashierRefundController(
            ILogger<CashierRefundController> logger,
            ISalesInvoicesService salesInvoicesService,
            RepositoriesFactory repositoriesFactory,
            ICashierRuntimeStore cashierStore,
            IAuthService authService,
            ICashierAuditService auditService,
            ICashierReservationService reservationService)
        {
            _logger = logger;
            _salesInvoicesService = salesInvoicesService;
            _repositoriesFactory = repositoriesFactory;
            _cashierStore = cashierStore;
            _authService = authService;
            _auditService = auditService;
            _reservationService = reservationService;
        }

        private async Task<bool> HasPermissionAsync(string permission)
        {
            if (User.HasClaim(c => c.Type == "Permission" && c.Value == permission)) return true;
            if (User.IsInRole("Admin") || User.IsInRole("System.Admin")) return true;
            var userGuid = ResolveCurrentUserGuid();
            if (userGuid == Guid.Empty) return false;
            try { return await _authService.AuthorizeAsync(userGuid, permission); }
            catch { return false; }
        }

        /// <summary>
        /// Search invoice by number or barcode for refund
        /// GET /api/cashier/refund/search-invoice?term=INV-2026-0042
        /// </summary>
        [HttpGet("search-invoice")]
        public async Task<IActionResult> SearchInvoice([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(new { message = "أدخل رقم الفاتورة أو الباركود" });

                var keyword = term.Trim();

                var invoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                var saleItemRepo = _repositoriesFactory.GetRepository<SaleItem>();

                // Search by invoice number
                var invoicesResult = await invoiceRepo.Filter(i =>
                    i.InvoiceNo.Contains(keyword));

                var invoice = invoicesResult.Data?.FirstOrDefault();

                if (invoice == null)
                {
                    // Try searching by ID
                    if (int.TryParse(keyword, out var invoiceId))
                    {
                        invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
                    }
                }

                if (invoice == null)
                    return NotFound(new { message = $"لم يتم العثور على فاتورة برقم \"{keyword}\"" });

                // Get invoice items
                var itemsResult = await saleItemRepo.Filter(si => si.SaleInvoiceId == invoice.Id);
                var items = itemsResult.Data?.ToList() ?? new List<SaleItem>();

                // Check already refunded quantities
                var refundItemRepo = _repositoriesFactory.GetRepository<CashierRefundItem>();
                var refundRepo = _repositoriesFactory.GetRepository<CashierRefund>();
                var existingRefunds = (await refundRepo.Filter(r => r.InvoiceId == invoice.Id && r.Status == "completed")).Data?.ToList()
                    ?? new List<CashierRefund>();
                var existingRefundIds = existingRefunds.Select(r => r.Id).ToHashSet();

                // Get all refund items for completed refunds of this invoice
                var allRefundItems = new List<CashierRefundItem>();
                foreach (var refund in existingRefunds)
                {
                    var refItems = (await refundItemRepo.Filter(ri => ri.CashierRefundId == refund.Id)).Data?.ToList();
                    if (refItems != null) allRefundItems.AddRange(refItems);
                }
                var refundedQtyBySaleItemId = allRefundItems
                    .GroupBy(ri => ri.SaleItemId)
                    .ToDictionary(g => g.Key, g => g.Sum(ri => ri.ReturnQty));

                // Get customer name
                var customerName = "Walk-in Customer";
                try
                {
                    var customerRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Accounting.CustomerProfile>();
                    var customer = (await customerRepo.Filter(c => c.Id == invoice.CustomerProfileId)).Data?.FirstOrDefault();
                    if (customer != null)
                        customerName = customer.ArabicName ?? customer.EnglishName ?? "Walk-in Customer";
                }
                catch { }

                // Determine payment method from CashierPayment
                int paymentMethod = 0;
                try
                {
                    var paymentRepo = _repositoriesFactory.GetRepository<CashierPayment>();
                    var payment = (await paymentRepo.Filter(p => p.InvoiceId == invoice.Id)).Data?.FirstOrDefault();
                    if (payment != null) paymentMethod = payment.PaymentMethod;
                }
                catch { }

                var invoiceItems = items.Select(item =>
                {
                    var alreadyRefunded = refundedQtyBySaleItemId.TryGetValue(item.Id ?? 0, out var rq) ? rq : 0m;
                    var maxReturnQty = Math.Max(0, item.Quantity - alreadyRefunded);
                    return new
                    {
                        id = item.Id,
                        itemId = item.MainSalesClassId,
                        itemCode = item.ItemNo ?? "",
                        itemName = item.Name ?? item.MainSalesClass?.ArabicName ?? item.MainSalesClass?.EnglishName ?? "",
                        itemBarcode = item.MainSalesClass?.ReferenceCode ?? "",
                        quantity = item.Quantity,
                        unitPrice = item.Price,
                        discountAmount = item.DiscountAmount,
                        taxPercentage = item.Tax,
                        taxAmount = item.TaxAmount,
                        totalAmount = item.TotalPrice,
                        unitName = item.MainSalesClass?.MeasurementUnit?.ArabicName ?? "وحدة",
                        maxReturnQty,
                        returnQty = 0,
                        selected = false
                    };
                }).ToList();

                return Ok(new
                {
                    id = invoice.Id,
                    invoiceNo = invoice.InvoiceNo,
                    invoiceDate = invoice.IssueDate,
                    customerName,
                    customerPhone = "",
                    cashierName = invoice.CreatedBy ?? "غير محدد",
                    paymentMethod,
                    items = invoiceItems,
                    subtotal = invoice.TotalPrice,
                    discountAmount = invoice.DiscountAmount,
                    taxAmount = invoice.TaxAmount,
                    totalAmount = invoice.NetPrice,
                    paidAmount = invoice.NetPrice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching invoice for refund: {Term}", term);
                return StatusCode(500, new { message = "خطأ داخلي في البحث" });
            }
        }

        /// <summary>
        /// Create a refund
        /// POST /api/cashier/refund
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRefund([FromBody] CreateRefundRequest request)
        {
            try
            {
                // ── Permission check ──────────────────────────────
                if (!await HasPermissionAsync("Cashier.Invoice.Refund"))
                {
                    var denyUser = ResolveCurrentUserGuid();
                    await _auditService.LogAsync(denyUser, ResolveCurrentUserName(),
                        "PERMISSION_DENIED", "Permission", "Cashier.Invoice.Refund", null, null,
                        new { required = "Cashier.Invoice.Refund" });
                    return StatusCode(403, new { message = "Access denied. Required permission: Cashier.Invoice.Refund" });
                }

                if (request.InvoiceId <= 0)
                    return BadRequest(new { message = "رقم الفاتورة مطلوب" });

                if (request.Items == null || request.Items.Count == 0)
                    return BadRequest(new { message = "اختر عنصرًا واحدًا على الأقل" });

                // Validate invoice exists
                var invoice = await _salesInvoicesService.GetByIdAsync(request.InvoiceId);
                if (invoice == null)
                    return NotFound(new { message = "الفاتورة غير موجودة" });

                // Get session info + ownership validation
                var sessionId = 0;
                var cashierName = "ADMIN";
                var storeDetailsId = (Guid?)null;
                try
                {
                    var userGuid = ResolveCurrentUserGuid();
                    var session = _cashierStore.GetCurrentSession(userGuid);
                    if (session != null)
                    {
                        // Validate ownership: the session must belong to the current user
                        if (session.CashierUserGuid != userGuid)
                        {
                            await _auditService.LogAsync(userGuid, ResolveCurrentUserName(),
                                "PERMISSION_DENIED", "CashierSession", session.Id.ToString(), null, null,
                                new { reason = "Session ownership validation failed for refund" });
                            return StatusCode(403, new { message = "Cannot create refund: session does not belong to you." });
                        }
                        sessionId = session.Id;
                        cashierName = session.CashierUserName;
                        storeDetailsId = session.StoreDetailsId;
                    }
                    else
                    {
                        // Fallback: try DB
                        var sessionRepo = _repositoriesFactory.GetRepository<CashierSession>();
                        var dbSession = (await sessionRepo.Filter(s => s.CashierUserGuid == userGuid && s.Status == 0))
                            .Data?.OrderByDescending(s => s.OpeningDateUtc).FirstOrDefault();
                        if (dbSession != null)
                        {
                            sessionId = dbSession.SessionId;
                            cashierName = dbSession.CashierUserName;
                            storeDetailsId = dbSession.StoreDetailsId;
                        }
                    }
                }
                catch { }

                // Generate refund number
                var refundRepo = _repositoriesFactory.GetRepository<CashierRefund>();
                var allRefunds = (await refundRepo.Filter(r => true)).Data?.ToList() ?? new List<CashierRefund>();
                var nextNum = allRefunds.Count + 1;
                var refundNo = $"RFN-{DateTime.UtcNow:yyyy}-{nextNum:D3}";

                // Ensure unique
                while (allRefunds.Any(r => r.RefundNo == refundNo))
                {
                    nextNum++;
                    refundNo = $"RFN-{DateTime.UtcNow:yyyy}-{nextNum:D3}";
                }

                // Check already refunded quantities
                var refundItemRepo = _repositoriesFactory.GetRepository<CashierRefundItem>();
                var completedRefunds = (await refundRepo.Filter(r => r.InvoiceId == request.InvoiceId && r.Status == "completed")).Data?.ToList()
                    ?? new List<CashierRefund>();
                var allRefundItems = new List<CashierRefundItem>();
                foreach (var rfd in completedRefunds)
                {
                    var refItems = (await refundItemRepo.Filter(ri => ri.CashierRefundId == rfd.Id)).Data?.ToList();
                    if (refItems != null) allRefundItems.AddRange(refItems);
                }
                var refundedQtyBySaleItemId = allRefundItems
                    .GroupBy(ri => ri.SaleItemId)
                    .ToDictionary(g => g.Key, g => g.Sum(ri => ri.ReturnQty));

                // Calculate totals from items
                var saleItemRepo = _repositoriesFactory.GetRepository<SaleItem>();
                decimal totalSubtotal = 0, totalTax = 0, totalAmount = 0;
                decimal totalItemsCount = 0;

                var refundItems = new List<CashierRefundItem>();
                foreach (var ri in request.Items)
                {
                    if (ri.ReturnQty <= 0) continue;

                    var saleItemResult = await saleItemRepo.Filter(si => si.Id == ri.SaleItemId);
                    var saleItem = saleItemResult.Data?.FirstOrDefault();
                    if (saleItem == null)
                        return BadRequest(new { message = $"الصنف رقم {ri.SaleItemId} غير موجود بالفاتورة" });

                    // Over-return protection:
                    var alreadyRefunded = refundedQtyBySaleItemId.TryGetValue(saleItem.Id ?? 0, out var rq) ? rq : 0m;
                    var maxReturnQty = Math.Max(0, saleItem.Quantity - alreadyRefunded);
                    if (ri.ReturnQty > maxReturnQty)
                    {
                        return BadRequest(new
                        {
                            message = $"Cannot return quantity {ri.ReturnQty} for item '{ri.ItemName}'. Maximum returnable quantity is {maxReturnQty}.",
                            messageAr = $"لا يمكن إرجاع الكمية {ri.ReturnQty} للصنف '{ri.ItemName}'. الحد الأقصى المسموح به هو {maxReturnQty}."
                        });
                    }

                    var unitPrice = ri.UnitPrice;
                    var perUnitDiscount = ri.OriginalQty > 0 ? ri.DiscountAmount / ri.OriginalQty : 0;
                    var subtotal = (unitPrice - perUnitDiscount) * ri.ReturnQty;
                    var taxAmt = subtotal * ri.TaxPercentage / 100m;
                    var refundAmt = subtotal + taxAmt;

                    totalSubtotal += subtotal;
                    totalTax += taxAmt;
                    totalAmount += refundAmt;
                    totalItemsCount += ri.ReturnQty;

                    refundItems.Add(new CashierRefundItem
                    {
                        SaleItemId = ri.SaleItemId,
                        ItemCode = ri.ItemCode ?? "",
                        ItemName = ri.ItemName ?? "",
                        ItemBarcode = ri.ItemBarcode,
                        UnitPrice = unitPrice,
                        ReturnQty = ri.ReturnQty,
                        DiscountAmount = perUnitDiscount * ri.ReturnQty,
                        TaxPercentage = ri.TaxPercentage,
                        TaxAmount = taxAmt,
                        RefundAmount = refundAmt,
                        UnitName = ri.UnitName,
                        IsActive = true,
                        CreatedBy = ResolveCurrentUserGuid(),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (refundItems.Count == 0)
                    return BadRequest(new { message = "لا توجد أصناف صالحة للمرتجع" });

                // Get customer name
                var customerName = request.CustomerName ?? "Walk-in Customer";

                var refund = new CashierRefund
                {
                    RefundNo = refundNo,
                    InvoiceId = request.InvoiceId,
                    InvoiceNo = invoice.InvoiceNo,
                    SessionId = sessionId,
                    CustomerName = customerName,
                    CashierName = cashierName,
                    Reason = request.Reason ?? "other",
                    ReasonText = request.ReasonText ?? "",
                    RefundMethod = request.RefundMethod ?? "cash",
                    Status = "completed",
                    ItemsCount = totalItemsCount,
                    RefundSubtotal = Math.Round(totalSubtotal, 2),
                    RefundTaxAmount = Math.Round(totalTax, 2),
                    RefundTotalAmount = Math.Round(totalAmount, 2),
                    Notes = request.Notes,
                    RefundDateUtc = DateTime.UtcNow,
                    StoreDetailsId = storeDetailsId,
                    IsActive = true,
                    CreatedBy = ResolveCurrentUserGuid(),
                    CreatedAt = DateTime.UtcNow
                };

                await refundRepo.Add(refund);
                if (refund.Id == Guid.Empty)
                {
                    _logger.LogError("Failed to save CashierRefund for invoice {InvoiceId}", request.InvoiceId);
                    return StatusCode(500, new { message = "فشل في حفظ المرتجع" });
                }

                // Save refund items
                foreach (var item in refundItems)
                {
                    item.CashierRefundId = refund.Id;
                    await refundItemRepo.Add(item);
                }

                // Automatically create an entry permit to restore the inventory stock for the refunded items
                try
                {
                    await _reservationService.CreateInventoryEntryPermitForRefundAsync(refund.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to automatically create inventory entry permit for refund {RefundId}", refund.Id);
                }

                // ──── Record Inventory Movements (ledger) for Refund ────
                try
                {
                    var movRepo = _repositoriesFactory.GetRepository<CashierInventoryMovement>();
                    var recipeRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Recipes.Recipe>();

                    foreach (var item in refundItems)
                    {
                        var saleItemResult = await saleItemRepo.Filter(si => si.Id == item.SaleItemId);
                        var saleItem = saleItemResult.Data?.FirstOrDefault();

                        if (saleItem != null)
                        {
                            var recipe = (await recipeRepo.Filter(r => r.MainSalesClassId == saleItem.MainSalesClassId)).Data
                                ?.FirstOrDefault();

                            if (recipe != null && recipe.RecipeEntries != null && recipe.RecipeEntries.Any())
                            {
                                foreach (var entry in recipe.RecipeEntries)
                                {
                                    var numPersons = recipe.NumberOfPersons > 0 ? recipe.NumberOfPersons : 1;
                                    var returnedQty = item.ReturnQty * (entry.Quantity / numPersons);
                                    var ingName = entry.MainPurchaseClass?.ArabicName ?? entry.MainPurchaseClass?.EnglishName ?? $"Ingredient {entry.MainPurchaseClassId}";

                                    await movRepo.Add(new CashierInventoryMovement
                                    {
                                        InvoiceId = request.InvoiceId,
                                        InvoiceNo = invoice.InvoiceNo,
                                        SessionId = sessionId,
                                        StoreDetailsId = storeDetailsId ?? Guid.Empty,
                                        MainPurchaseClassId = entry.MainPurchaseClassId,
                                        ProductName = ingName,
                                        Direction = -1,    // -1 = Entry (refund)
                                        Quantity = returnedQty,
                                        UnitCost = 0m,
                                        UnitPrice = 0m,
                                        InventoryPermitId = null,
                                        CashierUserGuid = ResolveCurrentUserGuid(),
                                        MovementType = "REFUND",
                                        MovedAtUtc = DateTime.UtcNow,
                                        IsActive = true,
                                        CreatedBy = ResolveCurrentUserGuid(),
                                        CreatedAt = DateTime.UtcNow
                                    });
                                }
                            }
                            else
                            {
                                var purchaseClassId = await MapSalesToPurchaseClassAsync(saleItem.MainSalesClassId);
                                await movRepo.Add(new CashierInventoryMovement
                                {
                                    InvoiceId = request.InvoiceId,
                                    InvoiceNo = invoice.InvoiceNo,
                                    SessionId = sessionId,
                                    StoreDetailsId = storeDetailsId ?? Guid.Empty,
                                    MainPurchaseClassId = purchaseClassId,
                                    ProductName = item.ItemName ?? string.Empty,
                                    Direction = -1,    // -1 = Entry (refund)
                                    Quantity = item.ReturnQty,
                                    UnitCost = 0m,
                                    UnitPrice = item.UnitPrice,
                                    InventoryPermitId = null,
                                    CashierUserGuid = ResolveCurrentUserGuid(),
                                    MovementType = "REFUND",
                                    MovedAtUtc = DateTime.UtcNow,
                                    IsActive = true,
                                    CreatedBy = ResolveCurrentUserGuid(),
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                catch (Exception movEx)
                {
                    _logger.LogWarning(movEx, "Failed to record inventory movements for refund of invoice {InvoiceNo}", invoice.InvoiceNo);
                }

                // Update session TotalRefunds in DB
                if (sessionId > 0)
                {
                    try
                    {
                        var sessionRepo = _repositoriesFactory.GetRepository<CashierSession>();
                        var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();
                        if (dbSession != null)
                        {
                            dbSession.TotalRefunds += totalAmount;
                            await sessionRepo.Update(dbSession);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update session TotalRefunds");
                    }
                }

                _logger.LogInformation("Refund {RefundNo} created for invoice {InvoiceNo}, amount={Amount}",
                    refundNo, invoice.InvoiceNo, totalAmount);

                // Audit log
                var auditUserGuid = ResolveCurrentUserGuid();
                await _auditService.LogAsync(auditUserGuid, cashierName, "REFUND",
                    "CashierRefund", refundNo, sessionId > 0 ? sessionId : null, storeDetailsId,
                    new { refundNo, invoiceNo = invoice.InvoiceNo, amount = totalAmount, method = request.RefundMethod, reason = request.Reason });

                return Ok(new
                {
                    id = refund.Id,
                    refundNo,
                    refundDate = refund.RefundDateUtc,
                    originalInvoiceNo = invoice.InvoiceNo,
                    customerName,
                    reason = refund.Reason,
                    reasonText = refund.ReasonText,
                    refundMethod = refund.RefundMethod,
                    status = refund.Status,
                    itemsCount = totalItemsCount,
                    refundSubtotal = refund.RefundSubtotal,
                    refundTaxAmount = refund.RefundTaxAmount,
                    refundTotalAmount = refund.RefundTotalAmount,
                    cashierName,
                    notes = refund.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund for invoice {InvoiceId}", request.InvoiceId);
                return StatusCode(500, new { message = "خطأ داخلي أثناء إنشاء المرتجع" });
            }
        }

        /// <summary>
        /// Get refund history with optional filters
        /// GET /api/cashier/refund/history?status=completed&search=RFN
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetRefundHistory(
            [FromQuery] string? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var refundRepo = _repositoriesFactory.GetRepository<CashierRefund>();
                var allRefundsResult = await refundRepo.Filter(r => true);
                var allRefunds = allRefundsResult.Data?.ToList() ?? new List<CashierRefund>();

                // Filter by status
                if (!string.IsNullOrWhiteSpace(status) && status != "all")
                {
                    allRefunds = allRefunds.Where(r => r.Status == status).ToList();
                }

                // Filter by search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    allRefunds = allRefunds.Where(r =>
                        r.RefundNo.ToLower().Contains(keyword) ||
                        r.InvoiceNo.ToLower().Contains(keyword) ||
                        (r.CustomerName ?? "").ToLower().Contains(keyword)
                    ).ToList();
                }

                var total = allRefunds.Count;
                var paged = allRefunds
                    .OrderByDescending(r => r.RefundDateUtc)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        id = r.Id,
                        refundNo = r.RefundNo,
                        refundDate = r.RefundDateUtc,
                        originalInvoiceNo = r.InvoiceNo,
                        customerName = r.CustomerName,
                        reason = r.Reason,
                        reasonText = r.ReasonText,
                        refundMethod = r.RefundMethod,
                        status = r.Status,
                        itemsCount = r.ItemsCount,
                        refundSubtotal = r.RefundSubtotal,
                        refundTaxAmount = r.RefundTaxAmount,
                        refundTotalAmount = r.RefundTotalAmount,
                        cashierName = r.CashierName,
                        notes = r.Notes
                    })
                    .ToList();

                // Stats
                var completedRefunds = allRefundsResult.Data?.Where(r => r.Status == "completed").ToList() ?? new List<CashierRefund>();
                var pendingCount = allRefundsResult.Data?.Count(r => r.Status == "pending") ?? 0;
                var rejectedCount = allRefundsResult.Data?.Count(r => r.Status == "rejected") ?? 0;
                var totalRefunded = completedRefunds.Sum(r => r.RefundTotalAmount);

                return Ok(new
                {
                    items = paged,
                    total,
                    pageNumber,
                    pageSize,
                    stats = new
                    {
                        completedCount = completedRefunds.Count,
                        pendingCount,
                        rejectedCount,
                        totalRefundedAmount = totalRefunded
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading refund history");
                return StatusCode(500, new { message = "خطأ في تحميل سجل المرتجعات" });
            }
        }

        private Guid ResolveCurrentUserGuid()
        {
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        private string ResolveCurrentUserName()
        {
            return User.FindFirstValue(ClaimTypes.GivenName)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.Identity?.Name
                ?? "Cashier";
        }

        private async Task<Guid> MapSalesToPurchaseClassAsync(Guid salesClassId)
        {
            var purchaseRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var exists = (await purchaseRepo.Filter(p => p.Id == salesClassId)).Data?.Any();
            if (exists == true)
            {
                return salesClassId;
            }

            var salesRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
            var salesClass = (await salesRepo.Filter(s => s.Id == salesClassId)).Data?.FirstOrDefault();
            if (salesClass != null)
            {
                var match = (await purchaseRepo.Filter(p => p.ReferenceCode == salesClass.ReferenceCode)).Data?.FirstOrDefault();
                if (match != null)
                {
                    return match.Id;
                }
            }

            return salesClassId;
        }
    }

    // ─── Request DTOs ────────────────────────────────────────────
    public class CreateRefundRequest
    {
        public int InvoiceId { get; set; }
        public string? CustomerName { get; set; }
        public string? Reason { get; set; }
        public string? ReasonText { get; set; }
        public string? RefundMethod { get; set; }
        public string? Notes { get; set; }
        public List<RefundItemRequest> Items { get; set; } = new();
    }

    public class RefundItemRequest
    {
        public int SaleItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemBarcode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal OriginalQty { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public string? UnitName { get; set; }
    }
}
