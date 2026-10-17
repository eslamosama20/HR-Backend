using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Pharmacy;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy;

/// <summary>
/// Restricted Drugs (الأدوية المقيدة) — 4 endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/pharmacy/restricted-drugs")]
[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class RestrictedDrugsController : ControllerBase
{
    private readonly IRestrictedDrugsService _svc;
    private readonly IAuthService _authService;

    public RestrictedDrugsController(IRestrictedDrugsService svc, IAuthService authService)
    {
        _svc = svc;
        _authService = authService;
    }

    private Guid UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;

    // ── 1. Lookup stock ──────────────────────────────────────────────
    /// <summary>
    /// GET /api/v1/pharmacy/restricted-drugs/lookup-stock
    /// Returns available medication batches in the pharmacy store.
    /// Filters: pharmacy stores only, quantity > 0, non-expired, medication type only.
    /// </summary>
    [HttpGet("lookup-stock")]
    public async Task<IActionResult> LookupStock(
        [FromQuery] Guid? storeDetailsId,
        [FromQuery] string? search)
    {
        var result = await _svc.LookupStockAsync(storeDetailsId, search);
        return Ok(result);
    }

    // ── 2. Dispense ──────────────────────────────────────────────────
    /// <summary>
    /// POST /api/v1/pharmacy/restricted-drugs/dispense
    /// Records a restricted drug dispense event.
    /// Validates: quantity, batch existence, expiry, stock availability.
    /// Deducts stock from PharmacyBatch and creates audit log.
    /// </summary>
    [HttpPost("dispense")]
    public async Task<IActionResult> Dispense([FromBody] DispenseRestrictedDrugRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var hasPermission = User.HasClaim(c => c.Type == "Permission" && c.Value == "Pharmacy.RestrictedDrugs.Dispense")
                            || User.IsInRole("Admin")
                            || User.IsInRole("System.Admin");

        if (!hasPermission)
        {
            var userGuid = UserId;
            if (userGuid != Guid.Empty)
            {
                try
                {
                    hasPermission = await _authService.AuthorizeAsync(userGuid, "Pharmacy.RestrictedDrugs.Dispense");
                }
                catch { }
            }
        }

        if (!hasPermission)
        {
            return StatusCode(403, new { message = "Access denied. Required permission: Pharmacy.RestrictedDrugs.Dispense" });
        }

        var (result, error) = await _svc.DispenseAsync(req, UserId);
        if (error != null)
            return BadRequest(new { message = error });

        return Created(string.Empty, result);
    }

    // ── 3. List dispenses ────────────────────────────────────────────
    /// <summary>
    /// GET /api/v1/pharmacy/restricted-drugs/dispenses
    /// Paginated list of dispense records, filterable by store, status, and search term.
    /// </summary>
    [HttpGet("dispenses")]
    public async Task<IActionResult> GetDispenses(
        [FromQuery] Guid? storeDetailsId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _svc.GetDispensesAsync(storeDetailsId, status, search, page, size);
        return Ok(result);
    }

    // ── 4. Update status ─────────────────────────────────────────────
    /// <summary>
    /// PATCH /api/v1/pharmacy/restricted-drugs/dispenses/{id}/status
    /// Updates the workflow status of a dispense record.
    /// Valid statuses: pending | approved | rejected | dispensed | cancelled
    /// </summary>
    [HttpPatch("dispenses/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateDispenseStatusRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (ok, error) = await _svc.UpdateStatusAsync(id, req, UserId);
        if (error != null)
            return BadRequest(new { message = error });

        return ok
            ? Ok(new { message = "Status updated successfully." })
            : NotFound(new { message = "Dispense record not found." });
    }
}
