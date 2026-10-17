using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier;

/// <summary>
/// Cashier Audit Log — سجل مراقبة الكاشير
/// Read-only endpoints for the audit log page.
/// </summary>
[ApiController]
[Route("api/cashier")]
[Authorize]
public class CashierAuditLogController : ControllerBase
{
    private readonly ICashierAuditService _svc;

    public CashierAuditLogController(ICashierAuditService svc) => _svc = svc;

    private Guid? CashierStoreId =>
        HttpContext.Items["CashierStoreId"] as Guid?;

    private int? CashierSessionId =>
        HttpContext.Items["CashierSessionId"] as int?;

    // ── GET /api/cashier/audit-logs ───────────────────────────────
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? storeDetailsId = null,
        [FromQuery] int? sessionId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? eventType = null,
        [FromQuery] string? invoiceNo = null,
        [FromQuery] string? keyword = null)
    {
        var filter = new CashierAuditFilter
        {
            Page       = page,
            PageSize   = Math.Min(pageSize, 200),
            From       = from,
            To         = to,
            StoreDetailsId = storeDetailsId ?? CashierStoreId,
            SessionId  = sessionId ?? CashierSessionId,
            UserId     = userId,
            EventType  = eventType,
            InvoiceNo  = invoiceNo,
            Keyword    = keyword
        };

        var result = await _svc.GetAllAsync(filter);
        return Ok(result);
    }

    // ── GET /api/cashier/audit-logs/stats ─────────────────────────
    [HttpGet("audit-logs/stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] Guid? storeDetailsId = null,
        [FromQuery] int? sessionId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _svc.GetStatsAsync(
            storeDetailsId ?? CashierStoreId,
            sessionId ?? CashierSessionId,
            from, to);
        return Ok(result);
    }

    // ── GET /api/cashier/audit-logs/{id} ──────────────────────────
    [HttpGet("audit-logs/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _svc.GetByIdAsync(id);
        return result is null
            ? NotFound(new { message = "Audit log entry not found", messageAr = "السجل غير موجود" })
            : Ok(result);
    }

    // ── GET /api/cashier/audit-log-types ──────────────────────────
    [HttpGet("audit-log-types")]
    public async Task<IActionResult> GetEventTypes()
    {
        var result = await _svc.GetEventTypesAsync();
        return Ok(result);
    }

    // ── POST /api/cashier/audit-logs/export ───────────────────────
    [HttpPost("audit-logs/export")]
    public async Task<IActionResult> Export([FromBody] CashierAuditFilter filter)
    {
        var csvBytes = await _svc.ExportCsvAsync(filter);
        var fileName = $"audit-log-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(csvBytes, "text/csv", fileName);
    }
}
