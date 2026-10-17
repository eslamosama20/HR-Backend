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

[Authorize]
[ApiController]
[Route("api/v1/pharmacy")]
public class PharmacyInsuranceClaimsController : ControllerBase
{
    private readonly IPharmacyInsuranceClaimsService _svc;
    private readonly IPharmacyCchiClaimService _cchi;

    public PharmacyInsuranceClaimsController(
        IPharmacyInsuranceClaimsService svc,
        IPharmacyCchiClaimService cchi)
    {
        _svc = svc;
        _cchi = cchi;
    }

    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;

    // ─── Basic CRUD ────────────────────────────────────────────────

    [PermissionAuthorize("Pharmacy.Insurance.EligibilityCheck")]
    [HttpGet("insurance-claims")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeDetailsId, [FromQuery] string? status,
        [FromQuery] string? insurer, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _svc.GetAllAsync(storeDetailsId, status, insurer, search, page, size));

    [PermissionAuthorize("Pharmacy.Insurance.EligibilityCheck")]
    [HttpGet("insurance-claims/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var claim = await _svc.GetByIdAsync(id);
        return claim is null ? NotFound(new { message = "Claim not found" }) : Ok(claim);
    }

    [PermissionAuthorize("Pharmacy.Insurance.SubmitClaim")]
    [HttpPost("insurance-claims")]
    public async Task<IActionResult> Create([FromBody] CreateInsuranceClaimRequest req)
    {
        var claim = new PharmacyInsuranceClaim
        {
            ClaimNo = req.ClaimNo ?? "",
            RxNo = req.RxNo,
            PatientName = req.PatientName,
            Insurer = req.Insurer,
            TotalAmount = req.TotalAmount,
            ApprovedAmount = req.ApprovedAmount,
            Status = req.Status ?? "Submitted",
            ReferenceNo = req.ReferenceNo,
            Notes = req.Notes,
            StoreDetailsId = req.StoreDetailsId
        };
        return Created("", await _svc.CreateAsync(claim, UserId));
    }

    [PermissionAuthorize("Pharmacy.Insurance.SettleClaims")]
    [HttpPatch("insurance-claims/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ClaimStatusUpdateRequest req)
    {
        var ok = await _svc.UpdateStatusAsync(id, req.Status, req.ApprovedAmount, req.ReferenceNo, req.Notes, UserId);
        return ok ? Ok(new { message = "Claim status updated" }) : NotFound(new { message = "Claim not found" });
    }

    // ─── CCHI / NPHIES Endpoints ───────────────────────────────────

    /// <summary>
    /// GET /api/v1/pharmacy/nphies-claims
    /// List insurance claims with NPHIES status info
    /// </summary>
    [HttpGet("nphies-claims")]
    public async Task<IActionResult> GetNphiesClaims(
        [FromQuery] Guid? storeDetailsId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
        => Ok(await _cchi.GetClaimsAsync(storeDetailsId, status, page, size));

    /// <summary>
    /// POST /api/v1/pharmacy/prescriptions/{prescriptionId}/submit-claim
    /// Manually submit a prescription's insurance claim to NPHIES
    /// </summary>
    [HttpPost("prescriptions/{prescriptionId}/submit-claim")]
    public async Task<IActionResult> SubmitClaimForPrescription(
        Guid prescriptionId,
        [FromBody] SubmitClaimManualRequest? req)
    {
        var result = await _cchi.SubmitClaimAsync(
            prescriptionId,
            req?.SaleInvoiceId,
            req?.CashierPaymentId,
            UserId);

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// GET /api/v1/pharmacy/prescriptions/{prescriptionId}/claim
    /// Get the NPHIES insurance claim linked to a prescription
    /// </summary>
    [HttpGet("prescriptions/{prescriptionId}/claim")]
    public async Task<IActionResult> GetClaimByPrescription(Guid prescriptionId)
    {
        var claim = await _cchi.GetClaimByPrescriptionAsync(prescriptionId);
        return claim is null
            ? NotFound(new { message = "No claim found for this prescription", messageAr = "لا توجد مطالبة لهذه الوصفة" })
            : Ok(claim);
    }

    /// <summary>
    /// POST /api/v1/pharmacy/insurance-claims/{id}/poll-nphies
    /// Refresh the claim's adjudication status from NPHIES
    /// </summary>
    [HttpPost("insurance-claims/{id}/poll-nphies")]
    public async Task<IActionResult> PollNphiesStatus(Guid id)
    {
        var result = await _cchi.PollClaimStatusAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// POST /api/v1/pharmacy/prescriptions/{prescriptionId}/poll-nphies
    /// Forward request to refresh the associated claim's adjudication status from NPHIES
    /// </summary>
    [HttpPost("prescriptions/{prescriptionId}/poll-nphies")]
    public async Task<IActionResult> PollNphiesStatusByPrescription(Guid prescriptionId)
    {
        var claim = await _cchi.GetClaimByPrescriptionAsync(prescriptionId);
        if (claim == null)
        {
            return NotFound(new { message = "No claim found for this prescription", messageAr = "لا توجد مطالبة لهذه الوصفة" });
        }

        var result = await _cchi.PollClaimStatusAsync(claim.Id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public class CreateInsuranceClaimRequest
{
    public string? ClaimNo { get; set; }
    public string? RxNo { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Insurer { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string? Status { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public Guid? StoreDetailsId { get; set; }
}

public class ClaimStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
    public decimal? ApprovedAmount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}

public class SubmitClaimManualRequest
{
    public int? SaleInvoiceId { get; set; }
    public Guid? CashierPaymentId { get; set; }
}

