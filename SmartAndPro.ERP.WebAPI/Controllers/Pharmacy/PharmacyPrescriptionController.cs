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
public class PharmacyPrescriptionController : ControllerBase
{
    private readonly IPharmacyPrescriptionService _svc;

    public PharmacyPrescriptionController(IPharmacyPrescriptionService svc) => _svc = svc;

    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;

    // ── Prescriptions ──────────────────────────────────────────────

    [PermissionAuthorize("Pharmacy.Prescriptions.ViewHistory")]
    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeDetailsId, [FromQuery] string? status,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _svc.GetAllAsync(storeDetailsId, status, search, page, pageSize));

    [PermissionAuthorize("Pharmacy.Read")]
    [HttpGet("prescriptions/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rx = await _svc.GetByIdAsync(id);
        return rx is null ? NotFound(new { message = "Prescription not found" }) : Ok(rx);
    }

    [PermissionAuthorize("Pharmacy.Prescriptions.Create")]
    [HttpPost("prescriptions/manual")]
    public async Task<IActionResult> CreateManual([FromBody] ManualCreatePrescriptionRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var isInsurance = req.Insurance != null || (!string.IsNullOrEmpty(req.PaymentType) && req.PaymentType.Equals("insurance", StringComparison.OrdinalIgnoreCase));

        var rx = new PharmacyPrescription
        {
            RxNo = req.PrescriptionNo ?? "",
            IssueDate = DateTime.UtcNow,
            PatientName = req.PatientName ?? "",
            PatientId = req.PatientId,
            DoctorName = req.DoctorName,
            PaymentType = isInsurance ? "insurance" : (req.PaymentType ?? "cash"),
            Priority = "Normal",
            Status = "new",
            Notes = req.Notes,
            StoreDetailsId = req.StoreDetailsId,
            InsuranceEnabled = isInsurance,
            InsuranceCompanyId = req.Insurance?.InsuranceCompanyId,
            InsuranceName = req.Insurance?.InsuranceName,
            InsuranceMemberNo = req.Insurance?.MemberNo,
            InsuranceApprovalNo = req.Insurance?.ApprovalNo,
            InsuranceCoveragePercentage = req.Insurance?.CoveragePercent ?? 0,
            InsurancePatientSharePercentage = isInsurance ? (100m - (req.Insurance?.CoveragePercent ?? 0)) : 0
        };

        var items = req.Items?.Select(i => new PharmacyPrescriptionItem
        {
            ItemId          = i.ItemId,
            ItemCode        = i.ItemCode ?? i.ItemId?.ToString() ?? "",
            ItemName        = i.ItemName ?? "",
            RequestedQty    = i.Quantity > 0 ? i.Quantity : 1,
            DispensedQty    = i.DispensedQty,
            StoreDetailsId  = i.StoreDetailsId ?? req.StoreDetailsId,
            StoreId         = i.StoreId,
            BatchId         = i.BatchId,
            BatchNo         = i.BatchNo,
            ExpiryDate      = i.ExpiryDate,
            UnitPrice       = i.UnitPrice,
            LineTotal       = i.UnitPrice * (i.Quantity > 0 ? i.Quantity : 1),
            Notes           = i.Instructions,
            IsControlled    = i.IsControlled
        }).ToList();

        var result = await _svc.CreateAsync(rx, items, UserId);
        return Created("", result);
    }

    [PermissionAuthorize("Pharmacy.Prescriptions.Create")]
    [HttpPost("prescriptions")]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest req)
    {
        var rx = new PharmacyPrescription
        {
            RxNo = req.RxNo ?? "",
            IssueDate = req.IssueDate ?? DateTime.UtcNow,
            PatientName = req.PatientName,
            PatientId = req.PatientId,
            DoctorName = req.DoctorName,
            DoctorId = req.DoctorId,
            BranchId = req.BranchId,
            BranchName = req.BranchName,
            StoreId = req.StoreId,
            StoreName = req.StoreName,
            PaymentType = req.PaymentType ?? "Cash",
            Priority = req.Priority ?? "Normal",
            Status = "Pending",
            InsuranceEnabled = req.InsuranceEnabled,
            InsuranceCompanyId = req.InsuranceCompanyId,
            InsuranceName = req.InsuranceName,
            InsuranceMemberNo = req.InsuranceMemberNo,
            InsuranceApprovalNo = req.InsuranceApprovalNo,
            InsuranceCoveragePercentage = req.InsuranceCoveragePercentage,
            InsurancePatientSharePercentage = req.InsurancePatientSharePercentage,
            StoreDetailsId = req.StoreDetailsId,
            Notes = req.Notes
        };

        var items = req.Items?.Select(i => new PharmacyPrescriptionItem
        {
            ItemId = i.ItemId,
            ItemCode = i.ItemCode ?? string.Empty,
            ItemName = !string.IsNullOrWhiteSpace(i.ItemName) ? i.ItemName : (i.Name ?? string.Empty),
            RequestedQty = i.RequestedQty ?? i.Quantity ?? 1m,
            DispensedQty = i.DispensedQty,
            UnitPrice = i.UnitPrice,
            DiscountAmount = i.DiscountAmount,
            TaxAmount = i.TaxAmount,
            StoreId = i.StoreId,
            BatchId = i.BatchId,
            BatchNo = i.BatchNo,
            ExpiryDate = i.ExpiryDate,
            AllowSubstitute = i.AllowSubstitute,
            IsControlled = i.IsControlled,
            StoreDetailsId = req.StoreDetailsId,
            Notes = i.Notes
        }).ToList();

        var result = await _svc.CreateAsync(rx, items, UserId);
        return Created("", result);
    }

    [PermissionAuthorize("Pharmacy.Update")]
    [HttpPut("prescriptions/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PharmacyPrescription rx)
    {
        var result = await _svc.UpdateAsync(id, rx, UserId);
        return result is null ? NotFound(new { message = "Prescription not found" }) : Ok(result);
    }

    [HttpPatch("prescriptions/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusUpdateRequest req)
    {
        var ok = await _svc.UpdateStatusAsync(id, req.Status, req.CancelReason, UserId);
        return ok ? Ok(new { message = "Status updated" }) : NotFound(new { message = "Prescription not found" });
    }

    [PermissionAuthorize("Pharmacy.Delete")]
    [HttpDelete("prescriptions/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var ok = await _svc.DeletePrescriptionAsync(id, UserId);
            return ok ? Ok(new { message = "Prescription deleted" }) : NotFound(new { message = "Prescription not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [PermissionAuthorize("Pharmacy.Prescriptions.Cancel")]
    [HttpPost("prescriptions/{id}/cancel")]
    public async Task<IActionResult> Cancel(string id, [FromBody] CancelPrescriptionRequest? req)
    {
        try
        {
            var reason = req?.Reason ?? "Cancelled by user";
            var ok = await _svc.UpdateStatusAsync(id, "cancelled", reason, UserId);
            return ok ? Ok(new { message = "Prescription cancelled", status = "cancelled" }) : NotFound(new { message = "Prescription not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [PermissionAuthorize("Pharmacy.Prescriptions.Dispense")]
    [HttpPost("prescriptions/{id}/ready")]
    public async Task<IActionResult> MarkPrescriptionReady(Guid id)
    {
        try
        {
            var success = await _svc.MarkPrescriptionReadyAsync(id, UserId);
            return success ? Ok(new { success = true, status = "ready" }) : BadRequest(new { message = "Failed to mark prescription as ready" });
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

    // ── Prescription Items CRUD ──────────────────────────────────────

    [HttpPost("prescriptions/{id}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] PrescriptionItemRequest item)
    {
        try
        {
            var entity = new SmartAndPro.ERP.Domains.Entities.Pharmacy.PharmacyPrescriptionItem
            {
                ItemId = item.ItemId,
                ItemCode = item.ItemCode ?? string.Empty,
                ItemName = !string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName : (item.Name ?? string.Empty),
                RequestedQty = item.RequestedQty ?? item.Quantity ?? 1m,
                DispensedQty = item.DispensedQty,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,
                StoreId = item.StoreId,
                BatchId = item.BatchId,
                BatchNo = item.BatchNo,
                ExpiryDate = item.ExpiryDate,
                AllowSubstitute = item.AllowSubstitute,
                IsControlled = item.IsControlled,
                Notes = item.Notes
            };
            var result = await _svc.AddItemAsync(id, entity, UserId);
            return Created("", result);
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Prescription not found" }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("prescriptions/{id}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] PrescriptionItemRequest item)
    {
        try
        {
            var entity = new SmartAndPro.ERP.Domains.Entities.Pharmacy.PharmacyPrescriptionItem
            {
                ItemId = item.ItemId,
                ItemCode = item.ItemCode ?? string.Empty,
                ItemName = !string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName : (item.Name ?? string.Empty),
                RequestedQty = item.RequestedQty ?? item.Quantity ?? 1m,
                DispensedQty = item.DispensedQty,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,
                StoreId = item.StoreId,
                BatchId = item.BatchId,
                BatchNo = item.BatchNo,
                ExpiryDate = item.ExpiryDate,
                AllowSubstitute = item.AllowSubstitute,
                IsControlled = item.IsControlled,
                Notes = item.Notes
            };
            var result = await _svc.UpdateItemAsync(id, itemId, entity, UserId);
            return result is null ? NotFound(new { message = "Item not found" }) : Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("prescriptions/{id}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId)
    {
        try
        {
            var ok = await _svc.DeleteItemAsync(id, itemId, UserId);
            return ok ? Ok(new { message = "Item removed" }) : NotFound(new { message = "Item not found" });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ── Patients ───────────────────────────────────────────────────

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients([FromQuery] Guid? storeDetailsId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _svc.GetPatientsAsync(storeDetailsId, search, page, pageSize));

    [HttpGet("patients/{id}")]
    public async Task<IActionResult> GetPatient(Guid id)
    {
        var p = await _svc.GetPatientByIdAsync(id);
        return p is null ? NotFound(new { message = "Patient not found" }) : Ok(p);
    }

    [HttpGet("patients/clinical-alerts")]
    [PermissionAuthorize("Pharmacy.Patients.ManageClinicalAlerts")]
    public IActionResult GetPatientClinicalAlerts() => Ok(new List<object>());

    [HttpPost("prescriptions/wasfaty-sync")]
    [PermissionAuthorize("Pharmacy.Prescriptions.WasfatySync")]
    public IActionResult SyncWasfaty() => Ok(new { success = true });

    [HttpPost("prescriptions/create")]
    [PermissionAuthorize("Pharmacy.Create")]
    public IActionResult CreatePharmacyGeneral() => Ok(new { success = true });

    [HttpGet("patients/{id}/prescriptions")]
    public async Task<IActionResult> GetPatientPrescriptions(Guid id)
        => Ok(await _svc.GetPatientPrescriptionsAsync(id));

    [PermissionAuthorize("Pharmacy.Patients.CreateEdit")]
    [HttpPost("patients")]
    public async Task<IActionResult> CreatePatient([FromBody] PharmacyPatient p)
        => Created("", await _svc.CreatePatientAsync(p, UserId));

    [PermissionAuthorize("Pharmacy.Patients.CreateEdit")]
    [HttpPut("patients/{id}")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] PharmacyPatient p)
    {
        var result = await _svc.UpdatePatientAsync(id, p, UserId);
        return result is null ? NotFound(new { message = "Patient not found" }) : Ok(result);
    }

    [PermissionAuthorize("Pharmacy.Patients.Delete")]
    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var result = await _svc.DeletePatientAsync(id, UserId);
        return result ? Ok(new { success = true, message = "Patient deleted successfully" }) : NotFound(new { message = "Patient not found" });
    }

    // ── Doctors ────────────────────────────────────────────────────

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors([FromQuery] Guid? storeDetailsId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _svc.GetDoctorsAsync(storeDetailsId, search, page, pageSize));

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] PharmacyDoctor d)
        => Created("", await _svc.CreateDoctorAsync(d, UserId));

    // ── Insurance Companies ────────────────────────────────────────

    [PermissionAuthorize("Pharmacy.Insurance.ManagePolicies")]
    [HttpGet("insurance-companies")]
    public async Task<IActionResult> GetInsuranceCompanies([FromQuery] Guid? storeDetailsId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _svc.GetInsuranceCompaniesAsync(storeDetailsId, search, page, pageSize));

    [PermissionAuthorize("Pharmacy.Insurance.ManagePolicies")]
    [HttpPost("insurance-companies")]
    public async Task<IActionResult> CreateInsuranceCompany([FromBody] PharmacyInsuranceCompany ic)
        => Created("", await _svc.CreateInsuranceCompanyAsync(ic, UserId));
}

// ── DTOs ───────────────────────────────────────────────────────────
public class CreatePrescriptionRequest
{
    public string? RxNo { get; set; }
    public DateTime? IssueDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public string? DoctorName { get; set; }
    public Guid? DoctorId { get; set; }
    public string? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? StoreId { get; set; }
    public string? StoreName { get; set; }
    public string? PaymentType { get; set; }
    public string? Priority { get; set; }
    public bool InsuranceEnabled { get; set; }
    public Guid? InsuranceCompanyId { get; set; }
    public string? InsuranceName { get; set; }
    public string? InsuranceMemberNo { get; set; }
    public string? InsuranceApprovalNo { get; set; }
    public decimal InsuranceCoveragePercentage { get; set; }
    public decimal InsurancePatientSharePercentage { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public string? Notes { get; set; }
    public List<PrescriptionItemRequest>? Items { get; set; }
}

public class PrescriptionItemRequest
{
    public Guid? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? Name { get; set; }
    public decimal? RequestedQty { get; set; }
    public decimal? Quantity { get; set; }
    public decimal DispensedQty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? StoreId { get; set; }
    public string? BatchId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool AllowSubstitute { get; set; } = true;
    public bool IsControlled { get; set; }
    public string? Notes { get; set; }
}

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class StatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
}

/// <summary>PDF simplified prescription creation contract</summary>
[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class ManualCreatePrescriptionRequest
{
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? PrescriptionNo { get; set; }
    public string? Notes { get; set; }
    public string? PaymentType { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public List<ManualPrescriptionItemRequest>? Items { get; set; }
    public ManualInsuranceRequest? Insurance { get; set; }
}

public class ManualInsuranceRequest
{
    public Guid? InsuranceCompanyId { get; set; }
    public string? InsuranceName { get; set; }
    public string? MemberNo { get; set; }
    public string? ApprovalNo { get; set; }
    public decimal? CoveragePercent { get; set; }
    public bool RequiresApproval { get; set; }
}

[Authorize]
[PermissionAuthorize("Pharmacy.Read")]
public class ManualPrescriptionItemRequest
{
    public Guid? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }

    // Dispense / allocation data (selected from inventory lookup)
    public Guid? StoreDetailsId { get; set; }
    public string? StoreId { get; set; }
    public string? BatchId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DispensedQty { get; set; }
    public bool IsControlled { get; set; }
}

public class CancelPrescriptionRequest
{
    public string? Reason { get; set; }
}
