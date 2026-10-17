using System.Text;
using System.Text.Json;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.WebAPI.Services.Cashier;

public interface ICashierAuditService
{
    // ── Write ──────────────────────────────────────────────────────
    Task LogAsync(
        Guid userGuid,
        string userName,
        string actionType,
        string entityType,
        string? entityReference,
        int? sessionId,
        Guid? storeDetailsId,
        object? payload = null,
        CancellationToken cancellationToken = default);

    // ── Read ───────────────────────────────────────────────────────
    Task<object> GetAllAsync(CashierAuditFilter filter);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> GetStatsAsync(Guid? storeDetailsId, int? sessionId, DateTime? from, DateTime? to);
    Task<IEnumerable<object>> GetEventTypesAsync();
    Task<byte[]> ExportCsvAsync(CashierAuditFilter filter);
}

public class CashierAuditFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public int? SessionId { get; set; }
    public string? UserId { get; set; }
    public string? EventType { get; set; }
    public string? InvoiceNo { get; set; }
    public string? Keyword { get; set; }
}

public class CashierAuditService : ICashierAuditService
{
    private readonly ERPContext _context;

    public CashierAuditService(ERPContext context)
    {
        _context = context;
    }

    // ── Write ──────────────────────────────────────────────────────
    public async Task LogAsync(
        Guid userGuid,
        string userName,
        string actionType,
        string entityType,
        string? entityReference,
        int? sessionId,
        Guid? storeDetailsId,
        object? payload = null,
        CancellationToken cancellationToken = default)
    {
        var audit = new CashierAuditLog
        {
            Id = Guid.NewGuid(),
            UserGuid = userGuid,
            UserName = string.IsNullOrWhiteSpace(userName) ? "Cashier" : userName.Trim(),
            ActionType = string.IsNullOrWhiteSpace(actionType) ? "UNKNOWN" : actionType.Trim(),
            EntityType = string.IsNullOrWhiteSpace(entityType) ? "UNKNOWN" : entityType.Trim(),
            EntityReference = entityReference,
            SessionId = sessionId,
            StoreDetailsId = storeDetailsId,
            PayloadJson = payload is null ? null : JsonSerializer.Serialize(payload),
            CreatedBy = userGuid,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.CashierAuditLogs.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ── Read: List ─────────────────────────────────────────────────
    public async Task<object> GetAllAsync(CashierAuditFilter filter)
    {
        var q = _context.CashierAuditLogs.AsNoTracking().AsQueryable();

        if (filter.StoreDetailsId.HasValue)
            q = q.Where(x => x.StoreDetailsId == filter.StoreDetailsId);

        if (filter.SessionId.HasValue)
            q = q.Where(x => x.SessionId == filter.SessionId);

        if (!string.IsNullOrWhiteSpace(filter.EventType))
            q = q.Where(x => x.ActionType == filter.EventType);

        if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
            q = q.Where(x => x.EntityReference == filter.InvoiceNo);

        if (!string.IsNullOrWhiteSpace(filter.UserId) && Guid.TryParse(filter.UserId, out var uid))
            q = q.Where(x => x.UserGuid == uid);

        if (filter.From.HasValue)
            q = q.Where(x => x.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            q = q.Where(x => x.CreatedAt <= filter.To.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            q = q.Where(x => x.UserName.Contains(filter.Keyword)
                           || x.ActionType.Contains(filter.Keyword)
                           || (x.EntityReference != null && x.EntityReference.Contains(filter.Keyword)));

        var total = await q.CountAsync();
        var data = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new
            {
                x.Id,
                eventDate     = x.CreatedAt,
                eventType     = x.ActionType,
                action        = x.ActionType,
                entityType    = x.EntityType,
                entityRef     = x.EntityReference,
                invoiceNo     = x.EntityType == "SaleInvoice" ? x.EntityReference : null,
                userId        = x.UserGuid,
                userName      = x.UserName,
                sessionId     = x.SessionId,
                storeDetailsId = x.StoreDetailsId,
                severity      = ResolveSeverity(x.ActionType),
                payloadJson   = x.PayloadJson
            })
            .ToListAsync();

        return new { total, page = filter.Page, pageSize = filter.PageSize, data };
    }

    // ── Read: Single ───────────────────────────────────────────────
    public async Task<object?> GetByIdAsync(Guid id)
    {
        var x = await _context.CashierAuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (x is null) return null;

        return new
        {
            x.Id,
            eventDate      = x.CreatedAt,
            eventType      = x.ActionType,
            action         = x.ActionType,
            entityType     = x.EntityType,
            entityRef      = x.EntityReference,
            invoiceNo      = x.EntityType == "SaleInvoice" ? x.EntityReference : null,
            userId         = x.UserGuid,
            userName       = x.UserName,
            sessionId      = x.SessionId,
            storeDetailsId = x.StoreDetailsId,
            severity       = ResolveSeverity(x.ActionType),
            payloadJson    = x.PayloadJson
        };
    }

    // ── Read: Stats ────────────────────────────────────────────────
    public async Task<object> GetStatsAsync(Guid? storeDetailsId, int? sessionId, DateTime? from, DateTime? to)
    {
        var q = _context.CashierAuditLogs.AsNoTracking().AsQueryable();

        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
        if (sessionId.HasValue)      q = q.Where(x => x.SessionId == sessionId);
        if (from.HasValue)           q = q.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue)             q = q.Where(x => x.CreatedAt <= to.Value.AddDays(1));

        var total = await q.CountAsync();
        var byType = await q
            .GroupBy(x => x.ActionType)
            .Select(g => new { eventType = g.Key, count = g.Count() })
            .ToListAsync();

        var critical = byType
            .Where(t => ResolveSeverity(t.eventType) == "critical")
            .Sum(t => t.count);

        return new { total, critical, byEventType = byType };
    }

    // ── Read: Event types ──────────────────────────────────────────
    public Task<IEnumerable<object>> GetEventTypesAsync()
    {
        var types = new[]
        {
            new { code = "SALE_COMPLETED",           label = "اكتمال البيع",             severity = "info" },
            new { code = "INVOICE_CREATED",          label = "إنشاء فاتورة",             severity = "info" },
            new { code = "INVOICE_CANCELLED",        label = "إلغاء فاتورة",             severity = "warning" },
            new { code = "INVOICE_VOIDED",           label = "إبطال فاتورة",             severity = "critical" },
            new { code = "PAYMENT_CREATED",          label = "إنشاء دفعة",              severity = "info" },
            new { code = "PAYMENT_CONFIRMED",        label = "تأكيد دفعة",              severity = "info" },
            new { code = "PAYMENT_FAILED",           label = "فشل دفعة",               severity = "warning" },
            new { code = "REFUND_CREATED",           label = "إنشاء استرجاع",           severity = "warning" },
            new { code = "RECEIPT_REPRINTED",        label = "إعادة طباعة إيصال",       severity = "info" },
            new { code = "SESSION_OPENED",           label = "فتح جلسة",               severity = "info" },
            new { code = "SESSION_CLOSED",           label = "إغلاق جلسة",             severity = "info" },
            new { code = "DRAWER_OPENED",            label = "فتح الدرج",              severity = "info" },
            new { code = "MANUAL_DISCOUNT",          label = "خصم يدوي",               severity = "warning" },
            new { code = "PRICE_OVERRIDE",           label = "تجاوز سعر",              severity = "critical" },
            new { code = "RESTRICTED_DRUG_DISPENSE", label = "صرف دواء مقيّد",         severity = "critical" },
            new { code = "PRESCRIPTION_DISPENSE",    label = "صرف وصفة طبية",          severity = "info" },
            new { code = "INSURANCE_CLAIM_SUBMITTED",label = "إرسال مطالبة تأمين",     severity = "info" },
            new { code = "UNKNOWN",                  label = "غير معروف",              severity = "info" }
        };

        return Task.FromResult<IEnumerable<object>>(types);
    }

    // ── Export CSV ─────────────────────────────────────────────────
    public async Task<byte[]> ExportCsvAsync(CashierAuditFilter filter)
    {
        filter.Page = 1;
        filter.PageSize = 5000;
        var result = (dynamic)(await GetAllAsync(filter));

        var sb = new StringBuilder();
        sb.AppendLine("Id,Date,EventType,EntityType,EntityRef,UserName,SessionId,Severity");

        foreach (var row in result.data)
        {
            sb.AppendLine(
                $"{row.Id},{row.eventDate:yyyy-MM-dd HH:mm:ss}," +
                $"{Csv(row.eventType)},{Csv(row.entityType)}," +
                $"{Csv(row.entityRef)},{Csv(row.userName)}," +
                $"{row.sessionId},{row.severity}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // ── Helpers ────────────────────────────────────────────────────
    private static string ResolveSeverity(string? actionType) => actionType switch
    {
        "INVOICE_VOIDED"          => "critical",
        "PRICE_OVERRIDE"          => "critical",
        "RESTRICTED_DRUG_DISPENSE"=> "critical",
        "INVOICE_CANCELLED"       => "warning",
        "PAYMENT_FAILED"          => "warning",
        "REFUND_CREATED"          => "warning",
        "MANUAL_DISCOUNT"         => "warning",
        _                         => "info"
    };

    private static string Csv(object? val)
    {
        var s = val?.ToString() ?? "";
        return s.Contains(',') || s.Contains('"') || s.Contains('\n')
            ? $"\"{s.Replace("\"", "\"\"")}\""
            : s;
    }
}

