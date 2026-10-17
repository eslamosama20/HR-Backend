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
public class PharmacyControlledDrugsController : ControllerBase
{
    private readonly IPharmacyControlledDrugsService _svc;
    public PharmacyControlledDrugsController(IPharmacyControlledDrugsService svc) => _svc = svc;
    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;

    [PermissionAuthorize("Pharmacy.ControlledDrugs.View")]
    [HttpGet("controlled-log")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeDetailsId, [FromQuery] string? schedule,
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int size = 20)
        => Ok(await _svc.GetAllAsync(storeDetailsId, schedule, status, search, page, size));

    [PermissionAuthorize("Pharmacy.ControlledDrugs.View")]
    [HttpGet("controlled-log/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rec = await _svc.GetByIdAsync(id);
        return rec is null ? NotFound(new { message = "Record not found" }) : Ok(rec);
    }

    [PermissionAuthorize("Pharmacy.ControlledDrugs.Dispense")]
    [HttpPost("controlled-log")]
    public async Task<IActionResult> Create([FromBody] CreateControlledDrugRequest req)
    {
        var record = new PharmacyControlledDrugRecord
        {
            LogNo = req.LogNo ?? "",
            PatientName = req.PatientName,
            DrugName = req.DrugName,
            Schedule = req.Schedule,
            Quantity = req.Quantity,
            Prescriber = req.Prescriber,
            Pharmacist = req.Pharmacist,
            PermitNo = req.PermitNo,
            Status = req.Status ?? "Active",
            Reason = req.Reason,
            Notes = req.Notes,
            DispenseDate = req.DispenseDate ?? DateTime.UtcNow,
            StoreDetailsId = req.StoreDetailsId
        };
        return Created("", await _svc.CreateAsync(record, UserId));
    }

    [PermissionAuthorize("Pharmacy.ControlledDrugs.ApproveReject")]
    [HttpPost("controlled-log/{id}/approve-reject")]
    public IActionResult ApproveRejectControlledDrug(Guid id) => Ok(new { success = true });

    [PermissionAuthorize("Pharmacy.ControlledDrugs.ExportReports")]
    [HttpGet("controlled-log/export")]
    public IActionResult ExportControlledDrugs() => Ok(new { success = true });
}

public class CreateControlledDrugRequest
{
    public string? LogNo { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string Schedule { get; set; } = "II";
    public decimal Quantity { get; set; }
    public string Prescriber { get; set; } = string.Empty;
    public string Pharmacist { get; set; } = string.Empty;
    public string? PermitNo { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime? DispenseDate { get; set; }
    public Guid? StoreDetailsId { get; set; }
}
