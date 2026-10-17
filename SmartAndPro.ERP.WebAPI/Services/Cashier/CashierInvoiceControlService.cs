using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Cashier;

// ═══════════════════════════════════════════════════════════════════
// Cashier Invoice Control Service
// ═══════════════════════════════════════════════════════════════════
public interface ICashierInvoiceControlService
{
    Task<object> GetInvoicesAsync(CashierInvoiceFilter filter);
    Task<object?> GetInvoiceByIdAsync(int id);
    Task<object> GetInvoiceStatsAsync(Guid? storeDetailsId, int? sessionId, DateTime? from, DateTime? to);
    Task<object> CancelInvoiceAsync(int id, string reason, string reasonCode, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId);
    Task<object> VoidInvoiceAsync(int id, string reason, string reasonCode, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId);
    Task<object> RefundInvoiceAsync(int id, CashierRefundRequest req, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId);
    Task<object> ReprintInvoiceAsync(int id, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId);
    Task<object?> GetReceiptAsync(int id);
    Task<IEnumerable<object>> GetActionReasonsAsync(string action);
}

public class CashierInvoiceFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public int? SessionId { get; set; }
    public string? CashierUserId { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Keyword { get; set; }
}

public class CashierRefundRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ReasonText { get; set; } = string.Empty;
    public string RefundMethod { get; set; } = "original_method";
    public string? Notes { get; set; }
    public List<CashierRefundItemRequest>? Items { get; set; }
}

public class CashierRefundItemRequest
{
    public int SaleItemId { get; set; }           // SaleItem.Id from the original invoice line
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal RefundAmount { get; set; }
}

// ─── Implementation ────────────────────────────────────────────────
public class CashierInvoiceControlService : ICashierInvoiceControlService
{
    private readonly ERPContext _db;
    private readonly ICashierAuditService _audit;
    private readonly ILogger<CashierInvoiceControlService> _logger;
    private readonly ICashierReservationService _reservationService;

    private static readonly IReadOnlyDictionary<string, IEnumerable<object>> _actionReasons =
        new Dictionary<string, IEnumerable<object>>(StringComparer.OrdinalIgnoreCase)
        {
            ["cancel"] = new[]
            {
                new { code = "CUSTOMER_CHANGED_MIND", label = "العميل غيّر رأيه", labelEn = "Customer changed mind" },
                new { code = "WRONG_ITEM",            label = "صنف خاطئ",           labelEn = "Wrong item" },
                new { code = "PRICING_ERROR",         label = "خطأ في السعر",        labelEn = "Pricing error" },
                new { code = "DUPLICATE_INVOICE",     label = "فاتورة مكررة",        labelEn = "Duplicate invoice" },
                new { code = "OTHER",                 label = "أخرى",               labelEn = "Other" }
            },
            ["void"] = new[]
            {
                new { code = "ACCOUNTING_ERROR",  label = "خطأ محاسبي",          labelEn = "Accounting error" },
                new { code = "SYSTEM_ERROR",      label = "خطأ في النظام",       labelEn = "System error" },
                new { code = "SUPERVISOR_OVERRIDE", label = "تجاوز مشرف",        labelEn = "Supervisor override" },
                new { code = "FRAUD_SUSPECTED",   label = "اشتباه بالاحتيال",   labelEn = "Fraud suspected" },
                new { code = "OTHER",             label = "أخرى",               labelEn = "Other" }
            },
            ["refund"] = new[]
            {
                new { code = "DEFECTIVE",         label = "بضاعة معيبة",         labelEn = "Defective product" },
                new { code = "WRONG_ITEM",        label = "صنف خاطئ",            labelEn = "Wrong item" },
                new { code = "CUSTOMER_DISSATISFIED", label = "عدم رضا العميل", labelEn = "Customer dissatisfied" },
                new { code = "EXPIRED",           label = "منتهي الصلاحية",     labelEn = "Expired product" },
                new { code = "OTHER",             label = "أخرى",               labelEn = "Other" }
            }
        };

    public CashierInvoiceControlService(
        ERPContext db, 
        ICashierAuditService audit, 
        ILogger<CashierInvoiceControlService> logger,
        ICashierReservationService reservationService)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
        _reservationService = reservationService;
    }

    // ── List invoices ──────────────────────────────────────────────
    public async Task<object> GetInvoicesAsync(CashierInvoiceFilter filter)
    {
        // Get cancelled/voided invoice IDs from audit log
        var cancelledIds = await _db.CashierAuditLogs
            .AsNoTracking()
            .Where(a => a.ActionType == "INVOICE_CANCELLED" || a.ActionType == "INVOICE_VOIDED")
            .Select(a => a.EntityReference)
            .ToListAsync();

        // Base invoice query
        var q = _db.SaleInvoices.AsNoTracking()
            .Include(x => x.SaleItems).ThenInclude(i => i.MainSalesClass)
            .AsQueryable();

        if (filter.StoreDetailsId.HasValue)
            q = q.Where(x => x.StoreDetailsId == filter.StoreDetailsId);

        if (filter.From.HasValue)
            q = q.Where(x => x.IssueDate >= filter.From.Value);

        if (filter.To.HasValue)
            q = q.Where(x => x.IssueDate <= filter.To.Value.AddDays(1));

        if (filter.SessionId.HasValue)
            q = q.Where(x => _db.CashierPayments.Any(p => p.InvoiceId == x.Id && p.SessionId == filter.SessionId.Value));

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            q = q.Where(x => x.InvoiceNo.Contains(filter.Keyword)
                           || (x.Notes != null && x.Notes.Contains(filter.Keyword)));

        var total = await q.CountAsync();
        var invoices = await q
            .OrderByDescending(x => x.IssueDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var invoiceIds = invoices.Select(x => x.Id).ToList();
        var invoiceNos = invoices.Select(x => x.InvoiceNo).ToList();

        // Load payments
        var payments = await _db.CashierPayments.AsNoTracking()
            .Where(p => invoiceIds.Contains(p.InvoiceId))
            .ToListAsync();

        // Load sessions
        var sessionIds = payments.Select(p => p.SessionId).Distinct().ToList();
        var sessions = await _db.CashierSessions.AsNoTracking()
            .Where(s => sessionIds.Contains(s.SessionId))
            .ToListAsync();

        // Load refunds
        var refunds = await _db.CashierRefunds.AsNoTracking()
            .Where(r => invoiceIds.Contains(r.InvoiceId))
            .ToListAsync();

        // Load all payments for the relevant sessions to calculate exact shift sequence number
        var sessionAllPayments = await _db.CashierPayments.AsNoTracking()
            .Where(p => sessionIds.Contains(p.SessionId))
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { p.SessionId, p.InvoiceId, p.CreatedAt })
            .ToListAsync();

        var sessionInvoiceSequenceMap = sessionAllPayments
            .GroupBy(p => p.SessionId)
            .SelectMany(g => g.Select((p, idx) => new { Key = $"{p.SessionId}_{p.InvoiceId}", ShiftInvoiceNo = idx + 1 }))
            .ToDictionary(x => x.Key, x => x.ShiftInvoiceNo);

        var data = invoices.Select(inv =>
        {
            var payment = payments.FirstOrDefault(p => p.InvoiceId == inv.Id);
            var session = payment is not null
                ? sessions.FirstOrDefault(s => s.SessionId == payment.SessionId)
                : null;
            var invoiceRefunds = refunds.Where(r => r.InvoiceId == inv.Id).ToList();
            var refundedAmount = invoiceRefunds.Sum(r => r.RefundTotalAmount);

            var isCancelled = cancelledIds.Contains(inv.InvoiceNo + ":cancelled");
            var isVoided    = cancelledIds.Contains(inv.InvoiceNo + ":voided");
            var isRefunded  = invoiceRefunds.Any(r => r.Status == "completed");
            var isPaid      = payment is not null;

            var status = isVoided    ? "voided"
                       : isCancelled ? "cancelled"
                       : isRefunded  ? (refundedAmount >= inv.NetPrice ? "refunded" : "partial_refund")
                       : isPaid      ? "paid"
                       : "pending";

            var paymentMethod = payment?.PaymentMethod switch
            {
                0 => "cash",
                1 => "card",
                2 => "credit",
                _ => "unknown"
            };

            var shiftInvoiceNo = (payment is not null && sessionInvoiceSequenceMap.TryGetValue($"{payment.SessionId}_{inv.Id}", out var seq))
                ? seq
                : (int?)null;

            return (object)new
            {
                id           = inv.Id,
                invoiceNo    = inv.InvoiceNo,
                shiftInvoiceNo,
                invoiceDate  = inv.IssueDate,
                sessionId    = session?.SessionId,
                sessionNo    = session?.SessionNumber,
                cashierId    = session?.CashierUserGuid,
                cashierName  = session?.CashierUserName,
                storeDetailsId = inv.StoreDetailsId,
                customerName = inv.CustomerProfile?.ArabicName,
                paymentType  = paymentMethod,
                paymentStatus= isPaid ? "paid" : "unpaid",
                invoiceStatus= status,
                subtotal     = inv.NetPrice + inv.DiscountAmount - inv.TaxAmount,
                discount     = inv.DiscountAmount,
                tax          = inv.TaxAmount,
                totalAmount  = inv.NetPrice,
                paidAmount   = payment?.PaidAmount ?? 0m,
                refundedAmount,
                canCancel  = !isPaid && !isCancelled && !isVoided,
                canVoid    = isPaid && !isVoided,
                canRefund  = isPaid && !isVoided && refundedAmount < inv.NetPrice,
                canReprint = isPaid && !isVoided,
                lastActionAt  = inv.CreatedAt,
                items = inv.SaleItems.Select(si => new
                {
                    id        = si.Id,
                    itemId    = si.MainSalesClassId,
                    itemCode  = si.ItemNo,
                    itemName  = si.Name ?? si.MainSalesClass?.ArabicName,
                    quantity  = si.Quantity,
                    unitPrice = si.Price,
                    discount  = si.DiscountAmount,
                    tax       = si.TaxAmount,
                    total     = si.TotalPrice
                })
            };
        }).ToList();



        return new { total, page = filter.Page, pageSize = filter.PageSize, data };
    }

    // ── Single invoice ─────────────────────────────────────────────
    public async Task<object?> GetInvoiceByIdAsync(int id)
    {
        var inv = await _db.SaleInvoices.AsNoTracking()
            .Include(x => x.SaleItems).ThenInclude(i => i.MainSalesClass)
            .Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv is null) return null;

        var payment = await _db.CashierPayments.AsNoTracking()
            .FirstOrDefaultAsync(p => p.InvoiceId == id);

        var session = payment is not null
            ? await _db.CashierSessions.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SessionId == payment.SessionId)
            : null;

        var refunds = await _db.CashierRefunds.AsNoTracking()
            .Include(r => r.Items)
            .Where(r => r.InvoiceId == id)
            .ToListAsync();

        var auditHistory = await _db.CashierAuditLogs.AsNoTracking()
            .Where(a => a.EntityReference == inv.InvoiceNo)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new { a.ActionType, a.UserName, a.CreatedAt, a.PayloadJson })
            .ToListAsync();

        var isCancelled = auditHistory.Any(a => a.ActionType == "INVOICE_CANCELLED");
        var isVoided    = auditHistory.Any(a => a.ActionType == "INVOICE_VOIDED");
        var refundedQtyBySaleItemId = refunds
            .Where(r => r.Status == "completed")
            .SelectMany(r => r.Items ?? new List<CashierRefundItem>())
            .GroupBy(ri => ri.SaleItemId)
            .ToDictionary(g => g.Key, g => g.Sum(ri => ri.ReturnQty));

        var refundedAmount = refunds.Where(r => r.Status == "completed").Sum(r => r.RefundTotalAmount);
        var isPaid = payment is not null;

        var status = isVoided    ? "voided"
                   : isCancelled ? "cancelled"
                   : refundedAmount >= inv.NetPrice && isPaid ? "refunded"
                   : refundedAmount > 0 && isPaid            ? "partial_refund"
                   : isPaid      ? "paid"
                   : "pending";

        var shiftInvoiceNo = payment is not null
            ? await _db.CashierPayments.AsNoTracking()
                .Where(p => p.SessionId == payment.SessionId && p.CreatedAt <= payment.CreatedAt)
                .CountAsync()
            : (int?)null;

        return new
        {
            id          = inv.Id,
            invoiceNo   = inv.InvoiceNo,
            shiftInvoiceNo,
            invoiceDate = inv.IssueDate,
            notes       = inv.Notes,
            currency    = inv.Currency,
            sessionId   = session?.SessionId,
            sessionNo   = session?.SessionNumber,
            cashierId   = session?.CashierUserGuid,
            cashierName = session?.CashierUserName,
            storeDetailsId   = inv.StoreDetailsId,
            customerId  = inv.CustomerProfileId,
            customerName = inv.CustomerProfile?.ArabicName,
            paymentType = payment?.PaymentMethod switch { 0 => "cash", 1 => "card", 2 => "credit", _ => "unknown" },
            paymentStatus = isPaid ? "paid" : "unpaid",
            invoiceStatus = status,
            subtotal    = inv.NetPrice + inv.DiscountAmount - inv.TaxAmount,
            discount    = inv.DiscountAmount,
            tax         = inv.TaxAmount,
            totalAmount = inv.NetPrice,
            paidAmount  = payment?.PaidAmount ?? 0m,
            cashAmount  = payment?.CashAmount ?? 0m,
            cardAmount  = payment?.CardAmount ?? 0m,
            creditAmount = payment?.CreditAmount ?? 0m,
            changeAmount = payment?.ChangeAmount ?? 0m,
            refundedAmount,
            canCancel  = !isPaid && !isCancelled && !isVoided,
            canVoid    = isPaid && !isVoided,
            canRefund  = isPaid && !isVoided && refundedAmount < inv.NetPrice,
            canReprint = isPaid && !isVoided,
            items = inv.SaleItems.Select(si => {
                var alreadyRefunded = refundedQtyBySaleItemId.TryGetValue(si.Id ?? 0, out var rq) ? rq : 0m;
                return new
                {
                    id        = si.Id,
                    itemId    = si.MainSalesClassId,
                    itemCode  = si.ItemNo,
                    itemName  = si.Name ?? si.MainSalesClass?.ArabicName,
                    quantity  = si.Quantity,
                    unitPrice = si.Price,
                    discount  = si.DiscountAmount,
                    tax       = si.TaxAmount,
                    total     = si.TotalPrice,
                    returnedQty = alreadyRefunded,
                    maxReturnQty = Math.Max(0, si.Quantity - alreadyRefunded)
                };
            }).ToList(),
            refunds = refunds.Select(r => new
            {
                r.RefundNo,
                r.RefundTotalAmount,
                r.Status,
                r.Reason,
                r.RefundDateUtc
            }),
            auditHistory
        };
    }

    // ── Stats ──────────────────────────────────────────────────────
    public async Task<object> GetInvoiceStatsAsync(Guid? storeDetailsId, int? sessionId, DateTime? from, DateTime? to)
    {
        var q = _db.SaleInvoices.AsNoTracking().AsQueryable();
        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
        if (from.HasValue) q = q.Where(x => x.IssueDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.IssueDate <= to.Value.AddDays(1));

        var invoiceList = await q.Select(x => new { x.Id, x.InvoiceNo, x.NetPrice }).ToListAsync();
        var ids = invoiceList.Select(x => x.Id).ToList();

        var payments = await _db.CashierPayments.AsNoTracking()
            .Where(p => ids.Contains(p.InvoiceId))
            .ToListAsync();

        if (sessionId.HasValue)
        {
            var filteredPaymentIds = payments.Where(p => p.SessionId == sessionId.Value).Select(p => p.InvoiceId).ToHashSet();
            invoiceList = invoiceList.Where(x => filteredPaymentIds.Contains(x.Id)).ToList();
            ids = invoiceList.Select(x => x.Id).ToList();
            payments = payments.Where(p => filteredPaymentIds.Contains(p.InvoiceId)).ToList();
        }

        var refunds = await _db.CashierRefunds.AsNoTracking()
            .Where(r => ids.Contains(r.InvoiceId) && r.Status == "completed")
            .SumAsync(r => r.RefundTotalAmount);

        var cancelledCount = await _db.CashierAuditLogs.AsNoTracking()
            .Where(a => a.ActionType == "INVOICE_CANCELLED" && a.StoreDetailsId == storeDetailsId)
            .CountAsync();

        var voidedCount = await _db.CashierAuditLogs.AsNoTracking()
            .Where(a => a.ActionType == "INVOICE_VOIDED" && a.StoreDetailsId == storeDetailsId)
            .CountAsync();

        return new
        {
            totalInvoices = invoiceList.Count,
            totalAmount   = invoiceList.Sum(x => x.NetPrice),
            totalPaid     = payments.Sum(p => p.PaidAmount),
            totalCash     = payments.Sum(p => p.CashAmount),
            totalCard     = payments.Sum(p => p.CardAmount),
            totalCredit   = payments.Sum(p => p.CreditAmount),
            totalRefunded = refunds,
            cancelledCount,
            voidedCount,
            netRevenue    = payments.Sum(p => p.PaidAmount) - refunds
        };
    }

    // ── Cancel ─────────────────────────────────────────────────────
    public async Task<object> CancelInvoiceAsync(int id, string reason, string reasonCode, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId)
    {
        var inv = await _db.SaleInvoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (inv is null)
            throw new KeyNotFoundException($"Invoice {id} not found.");

        var hasPayment = await _db.CashierPayments.AnyAsync(p => p.InvoiceId == id);
        if (hasPayment)
            throw new InvalidOperationException("Cannot cancel a paid invoice. Use Void or Refund instead.");

        var alreadyCancelled = await _db.CashierAuditLogs.AnyAsync(
            a => a.EntityReference == inv.InvoiceNo && a.ActionType == "INVOICE_CANCELLED");
        if (alreadyCancelled)
            throw new InvalidOperationException("Invoice is already cancelled.");

        await _audit.LogAsync(userGuid, userName, "INVOICE_CANCELLED", "SaleInvoice", inv.InvoiceNo,
            sessionId, storeDetailsId,
            new { reason, reasonCode, invoiceId = id, invoiceNo = inv.InvoiceNo, totalAmount = inv.NetPrice });

        _logger.LogInformation("[CashierInvoice] Invoice {InvoiceNo} cancelled by {User}, reason: {Reason}", inv.InvoiceNo, userName, reason);

        return new
        {
            success = true,
            invoiceNo = inv.InvoiceNo,
            status = "cancelled",
            message = "تم إلغاء الفاتورة بنجاح",
            messageEn = "Invoice cancelled successfully"
        };
    }

    // ── Void ───────────────────────────────────────────────────────
    public async Task<object> VoidInvoiceAsync(int id, string reason, string reasonCode, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId)
    {
        var inv = await _db.SaleInvoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (inv is null)
            throw new KeyNotFoundException($"Invoice {id} not found.");

        var alreadyVoided = await _db.CashierAuditLogs.AnyAsync(
            a => a.EntityReference == inv.InvoiceNo && a.ActionType == "INVOICE_VOIDED");
        if (alreadyVoided)
            throw new InvalidOperationException("Invoice is already voided.");

        await _audit.LogAsync(userGuid, userName, "INVOICE_VOIDED", "SaleInvoice", inv.InvoiceNo,
            sessionId, storeDetailsId,
            new { reason, reasonCode, invoiceId = id, invoiceNo = inv.InvoiceNo, totalAmount = inv.NetPrice });

        _logger.LogInformation("[CashierInvoice] Invoice {InvoiceNo} voided by {User}, reason: {Reason}", inv.InvoiceNo, userName, reason);

        return new
        {
            success = true,
            invoiceNo = inv.InvoiceNo,
            status = "voided",
            message = "تم إبطال الفاتورة بنجاح",
            messageEn = "Invoice voided successfully"
        };
    }

    // ── Refund ─────────────────────────────────────────────────────
    public async Task<object> RefundInvoiceAsync(int id, CashierRefundRequest req, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId)
    {
        var inv = await _db.SaleInvoices.AsNoTracking()
            .Include(x => x.SaleItems)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (inv is null)
            throw new KeyNotFoundException($"Invoice {id} not found.");

        var hasPayment = await _db.CashierPayments.AnyAsync(p => p.InvoiceId == id);
        if (!hasPayment)
            throw new InvalidOperationException("Cannot refund an unpaid invoice.");

        var existingRefundTotal = await _db.CashierRefunds
            .Where(r => r.InvoiceId == id && r.Status == "completed")
            .SumAsync(r => r.RefundTotalAmount);

        var refundItems = req.Items?.Select(i => new CashierRefundItem
        {
            Id = Guid.NewGuid(),
            SaleItemId   = i.SaleItemId,
            ItemCode     = i.ItemCode,
            ItemName     = i.ItemName,
            UnitPrice    = i.UnitPrice,
            ReturnQty    = i.Quantity,
            RefundAmount = i.RefundAmount,
            IsActive = true,
            CreatedBy = userGuid,
            CreatedAt = DateTime.UtcNow
        }).ToList() ?? new List<CashierRefundItem>();

        decimal refundSubtotal = refundItems.Sum(i => i.RefundAmount);
        decimal refundTotal    = refundSubtotal;

        if (existingRefundTotal + refundTotal > inv.NetPrice)
            throw new InvalidOperationException("Refund amount exceeds the original invoice total.");

        var refundNo = $"RFN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var refund = new CashierRefund
        {
            Id              = Guid.NewGuid(),
            RefundNo        = refundNo,
            InvoiceId       = id,
            InvoiceNo       = inv.InvoiceNo,
            SessionId       = sessionId ?? 0,
            CustomerName    = req.CustomerName,
            CashierName     = userName,
            Reason          = req.Reason,
            ReasonText      = req.ReasonText,
            RefundMethod    = req.RefundMethod,
            Status          = "completed",
            ItemsCount      = refundItems.Count,
            RefundSubtotal  = refundSubtotal,
            RefundTaxAmount = 0,
            RefundTotalAmount = refundTotal,
            Notes           = req.Notes,
            RefundDateUtc   = DateTime.UtcNow,
            StoreDetailsId  = inv.StoreDetailsId ?? storeDetailsId,
            Items           = refundItems,
            IsActive        = true,
            CreatedBy       = userGuid,
            CreatedAt       = DateTime.UtcNow
        };

        _db.CashierRefunds.Add(refund);
        await _db.SaveChangesAsync();

        try
        {
            await _reservationService.CreateInventoryEntryPermitForRefundAsync(refund.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to automatically create inventory entry permit for refund {RefundId}", refund.Id);
        }

        await _audit.LogAsync(userGuid, userName, "REFUND_CREATED", "SaleInvoice", inv.InvoiceNo,
            sessionId, storeDetailsId,
            new { refundNo, refundTotal, reason = req.Reason, invoiceId = id });

        _logger.LogInformation("[CashierInvoice] Refund {RefundNo} created for invoice {InvoiceNo}", refundNo, inv.InvoiceNo);

        return new
        {
            success = true,
            refundNo,
            invoiceNo = inv.InvoiceNo,
            refundTotal,
            status = "completed",
            message = "تم إنشاء الاسترجاع بنجاح",
            messageEn = "Refund created successfully"
        };
    }

    // ── Reprint ────────────────────────────────────────────────────
    public async Task<object> ReprintInvoiceAsync(int id, Guid userGuid, string userName, int? sessionId, Guid? storeDetailsId)
    {
        var inv = await _db.SaleInvoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (inv is null)
            throw new KeyNotFoundException($"Invoice {id} not found.");

        await _audit.LogAsync(userGuid, userName, "RECEIPT_REPRINTED", "SaleInvoice", inv.InvoiceNo,
            sessionId, storeDetailsId,
            new { invoiceId = id, invoiceNo = inv.InvoiceNo, reprintedAt = DateTime.UtcNow });

        return new
        {
            success = true,
            invoiceNo = inv.InvoiceNo,
            message = "تم تسجيل إعادة الطباعة",
            messageEn = "Reprint logged successfully"
        };
    }

    // ── Receipt ────────────────────────────────────────────────────
    public async Task<object?> GetReceiptAsync(int id)
    {
        var detail = await GetInvoiceByIdAsync(id);
        return detail; // full invoice data serves as receipt
    }

    // ── Action reasons ─────────────────────────────────────────────
    public Task<IEnumerable<object>> GetActionReasonsAsync(string action)
    {
        if (_actionReasons.TryGetValue(action, out var reasons))
            return Task.FromResult(reasons);
        return Task.FromResult(Enumerable.Empty<object>());
    }
}
