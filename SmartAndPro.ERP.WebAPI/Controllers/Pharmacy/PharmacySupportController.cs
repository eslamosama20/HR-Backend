using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Pharmacy;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy;

/// <summary>
/// Pharmacy Supporting Modules Controller – PDF spec §20
/// Insurance Claims, Alerts, Controlled Drugs, Returns, Drug Interactions
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/pharmacy")]
[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class PharmacySupportController : ControllerBase
{
    private readonly IPharmacySupportService _svc;

    public PharmacySupportController(IPharmacySupportService svc) => _svc = svc;

    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;



    // ═══════════════════════════════════════════════════════════════════
    // §20.4 – Alerts
    // ═══════════════════════════════════════════════════════════════════

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid? storeDetailsId, [FromQuery] string? state,
        [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _svc.GetAlertsAsync(storeDetailsId, state, page, size));

    [PermissionAuthorize("Pharmacy.ExpiryAlerts.Manage")]
    [HttpPost("alerts")]
    public async Task<IActionResult> CreateAlert([FromBody] PharmacyAlert alert)
        => Created("", await _svc.CreateAlertAsync(alert, UserId));

    [HttpPatch("alerts/{id}/assign")]
    public async Task<IActionResult> AssignAlert(Guid id, [FromBody] AlertAssignRequest req)
    {
        var result = await _svc.AssignAlertAsync(id, req.Owner, UserId);
        return result is null ? NotFound(new { message = "Alert not found" }) : Ok(result);
    }

    [HttpPost("alerts/{id}/close")]
    public async Task<IActionResult> CloseAlert(Guid id)
    {
        var result = await _svc.CloseAlertAsync(id, UserId);
        return result is null ? NotFound(new { message = "Alert not found" }) : Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    // §20.5 – Controlled Drugs
    // ═══════════════════════════════════════════════════════════════════

    [HttpGet("controlled-drugs")]
    public async Task<IActionResult> GetControlledDrugs([FromQuery] Guid? storeDetailsId, [FromQuery] string? status,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _svc.GetControlledDrugsAsync(storeDetailsId, status, search, page, size));

    [HttpGet("controlled-drugs/{id}")]
    public async Task<IActionResult> GetControlledDrug(Guid id)
    {
        var r = await _svc.GetControlledDrugByIdAsync(id);
        return r is null ? NotFound(new { message = "Record not found" }) : Ok(r);
    }

    [HttpPost("controlled-drugs")]
    public async Task<IActionResult> CreateControlledDrug([FromBody] PharmacyControlledDrugRecord record)
        => Created("", await _svc.CreateControlledDrugAsync(record, UserId));

    [HttpPut("controlled-drugs/{id}")]
    public async Task<IActionResult> UpdateControlledDrug(Guid id, [FromBody] PharmacyControlledDrugRecord record)
    {
        var result = await _svc.UpdateControlledDrugAsync(id, record, UserId);
        return result is null ? NotFound(new { message = "Record not found" }) : Ok(result);
    }

    [HttpPatch("controlled-drugs/{id}/status")]
    public async Task<IActionResult> PatchControlledDrugStatus(Guid id, [FromBody] StatusPatchRequest req)
    {
        var result = await _svc.PatchControlledDrugStatusAsync(id, req.Status, UserId);
        return result is null ? NotFound(new { message = "Record not found" }) : Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    // §20.2 – Pharmacy Returns
    // ═══════════════════════════════════════════════════════════════════

    [HttpGet("returns")]
    public async Task<IActionResult> GetReturns([FromQuery] Guid? storeDetailsId, [FromQuery] string? status,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _svc.GetReturnsAsync(storeDetailsId, status, search, page, size));

    [PermissionAuthorize("Pharmacy.POS.ReturnDrugs")]
    [HttpPost("returns")]
    public async Task<IActionResult> CreateReturn([FromBody] PharmacyReturn ret)
        => Created("", await _svc.CreateReturnAsync(ret, UserId));

    [HttpPatch("returns/{id}")]
    public async Task<IActionResult> PatchReturn(Guid id, [FromBody] ReturnPatchRequest req)
    {
        var result = await _svc.PatchReturnAsync(id, req.Status, req.ApprovedQty, req.ReceivedQty, req.DecisionReason, req.Notes, UserId);
        return result is null ? NotFound(new { message = "Return not found" }) : Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    // §20.7 – Drug Interactions
    // ═══════════════════════════════════════════════════════════════════

    [HttpPost("prescriptions/check-interactions")]
    public async Task<IActionResult> CheckInteractions([FromBody] CheckInteractionsRequest req)
        => Ok(await _svc.CheckInteractionsAsync(req.ItemIds, req.PatientId));

    [HttpGet("interactions")]
    public async Task<IActionResult> GetInteractions([FromQuery] Guid? storeDetailsId,
        [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _svc.GetInteractionsAsync(storeDetailsId, page, size));
}

#region Request DTOs

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class ClaimStatusPatchRequest
{
    public string Status { get; set; } = string.Empty;
    public decimal? ApprovedAmount { get; set; }
    public string? Notes { get; set; }
}

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class AlertAssignRequest
{
    public string Owner { get; set; } = string.Empty;
}

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class StatusPatchRequest
{
    public string Status { get; set; } = string.Empty;
}

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class ReturnPatchRequest
{
    public string? Status { get; set; }
    public decimal? ApprovedQty { get; set; }
    public decimal? ReceivedQty { get; set; }
    public string? DecisionReason { get; set; }
    public string? Notes { get; set; }
}

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class CheckInteractionsRequest
{
    public List<int> ItemIds { get; set; } = new();
    public Guid? PatientId { get; set; }
}

#endregion
