using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier;

/// <summary>
/// Invoice Control — صفحة التحكم بالفواتير
/// Provides list, detail, stats, cancel, void, refund, and reprint endpoints.
/// </summary>
[ApiController]
[Route("api/cashier")]
[Authorize]
public class CashierInvoiceControlController : ControllerBase
{
    private readonly ICashierInvoiceControlService _svc;
    private readonly IAuthService _authService;
    private readonly ICashierAuditService _auditService;

    public CashierInvoiceControlController(
        ICashierInvoiceControlService svc,
        IAuthService authService,
        ICashierAuditService auditService)
    {
        _svc = svc;
        _authService = authService;
        _auditService = auditService;
    }

    private async Task<bool> HasPermissionAsync(string permission)
    {
        if (User.HasClaim(c => c.Type == "Permission" && c.Value == permission)) return true;
        if (User.IsInRole("Admin") || User.IsInRole("System.Admin")) return true;
        var g = CurrentUserGuid;
        if (g == Guid.Empty) return false;
        try { return await _authService.AuthorizeAsync(g, permission); }
        catch { return false; }
    }

    private Guid CurrentUserGuid
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var g) ? g : Guid.Empty;
        }
    }

    private string CurrentUserName =>
        User.FindFirstValue(ClaimTypes.GivenName)
        ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.Identity?.Name
        ?? "Cashier";

    private int? CashierSessionId =>
        HttpContext.Items["CashierSessionId"] as int?;

    private Guid? CashierStoreId =>
        HttpContext.Items["CashierStoreId"] as Guid?;

    // ── GET /api/cashier/invoices ──────────────────────────────────
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? storeDetailsId = null,
        [FromQuery] int? sessionId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] string? keyword = null)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("System.Admin") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.Invoice.ViewAll");
        var filter = new CashierInvoiceFilter
        {
            Page = page,
            PageSize = Math.Min(pageSize, 100),
            From = from,
            To = to,
            StoreDetailsId = storeDetailsId ?? (isAdmin ? null : CashierStoreId),
            SessionId = sessionId ?? (isAdmin ? null : CashierSessionId),
            Status = status,
            PaymentMethod = paymentMethod,
            Keyword = keyword
        };

        var result = await _svc.GetInvoicesAsync(filter);
        return Ok(result);
    }

    // ── GET /api/cashier/invoices/stats ───────────────────────────
    [HttpGet("invoices/stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] Guid? storeDetailsId = null,
        [FromQuery] int? sessionId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("System.Admin") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.Invoice.ViewAll");
        var result = await _svc.GetInvoiceStatsAsync(
            storeDetailsId ?? (isAdmin ? null : CashierStoreId),
            sessionId ?? (isAdmin ? null : CashierSessionId),
            from, to);
        return Ok(result);
    }

    // ── GET /api/cashier/invoices/{id} ────────────────────────────
    [HttpGet("invoices/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _svc.GetInvoiceByIdAsync(id);
        return result is null
            ? NotFound(new { message = "Invoice not found", messageAr = "الفاتورة غير موجودة" })
            : Ok(result);
    }

    // ── GET /api/cashier/invoices/{id}/receipt ────────────────────
    [HttpGet("invoices/{id:int}/receipt")]
    public async Task<IActionResult> GetReceipt(int id)
    {
        var result = await _svc.GetReceiptAsync(id);
        return result is null
            ? NotFound(new { message = "Invoice not found", messageAr = "الفاتورة غير موجودة" })
            : Ok(result);
    }

    // ── GET /api/cashier/invoice-action-reasons ───────────────────
    [HttpGet("invoice-action-reasons")]
    public async Task<IActionResult> GetActionReasons([FromQuery] string action = "cancel")
    {
        var reasons = await _svc.GetActionReasonsAsync(action);
        return Ok(reasons);
    }

    // ── POST /api/cashier/invoices/{id}/cancel ────────────────────
    [HttpPost("invoices/{id:int}/cancel")]
    public async Task<IActionResult> CancelInvoice(int id, [FromBody] InvoiceActionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Reason))
            return BadRequest(new { message = "Reason is required", messageAr = "سبب الإلغاء مطلوب" });

        try
        {
            var result = await _svc.CancelInvoiceAsync(
                id, req.Reason, req.ReasonCode ?? "OTHER",
                CurrentUserGuid, CurrentUserName,
                CashierSessionId, CashierStoreId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST /api/cashier/invoices/{id}/void ──────────────────────
    [HttpPost("invoices/{id:int}/void")]
    public async Task<IActionResult> VoidInvoice(int id, [FromBody] InvoiceActionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Reason))
            return BadRequest(new { message = "Reason is required", messageAr = "سبب الإبطال مطلوب" });

        try
        {
            var result = await _svc.VoidInvoiceAsync(
                id, req.Reason, req.ReasonCode ?? "OTHER",
                CurrentUserGuid, CurrentUserName,
                CashierSessionId, CashierStoreId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST /api/cashier/invoices/{id}/refund ────────────────────
    [HttpPost("invoices/{id:int}/refund")]
    public async Task<IActionResult> RefundInvoice(int id, [FromBody] CashierRefundRequest req)
    {
        if (!await HasPermissionAsync("Cashier.Invoice.Refund"))
        {
            await _auditService.LogAsync(CurrentUserGuid, CurrentUserName,
                "PERMISSION_DENIED", "Permission", "Cashier.Invoice.Refund", null, CashierStoreId,
                new { required = "Cashier.Invoice.Refund", invoiceId = id });
            return StatusCode(403, new { message = "Access denied. Required permission: Cashier.Invoice.Refund" });
        }
        if (string.IsNullOrWhiteSpace(req.Reason))
            return BadRequest(new { message = "Reason is required", messageAr = "سبب الاسترجاع مطلوب" });

        try
        {
            var result = await _svc.RefundInvoiceAsync(
                id, req,
                CurrentUserGuid, CurrentUserName,
                CashierSessionId, CashierStoreId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST /api/cashier/invoices/{id}/reprint ───────────────────
    [HttpPost("invoices/{id:int}/reprint")]
    public async Task<IActionResult> ReprintInvoice(int id)
    {
        try
        {
            var result = await _svc.ReprintInvoiceAsync(
                id, CurrentUserGuid, CurrentUserName,
                CashierSessionId, CashierStoreId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

// ── Request DTOs ────────────────────────────────────────────────────
public class InvoiceActionRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
}
