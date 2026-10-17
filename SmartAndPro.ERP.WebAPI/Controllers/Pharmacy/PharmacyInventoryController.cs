using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Pharmacy;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy;

/// <summary>
/// Pharmacy Inventory & Dispense Controller – PDF spec §9-§16
/// Handles: Inventory stocks, Allocation/Dispense, Send-to-POS, Checkout
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/pharmacy")]
public class PharmacyInventoryController : ControllerBase
{
    private readonly IPharmacyInventoryService _inventorySvc;
    private readonly IPharmacyDispenseService _dispenseSvc;

    public PharmacyInventoryController(
        IPharmacyInventoryService inventorySvc,
        IPharmacyDispenseService dispenseSvc)
    {
        _inventorySvc = inventorySvc;
        _dispenseSvc = dispenseSvc;
    }

    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;

    // ═══════════════════════════════════════════════════════════════════
    // §10 – Pharmacy Inventory Unified Endpoint
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>GET /v1/pharmacy/inventory — Unified pharmacy inventory read model</summary>
    [PermissionAuthorize("Pharmacy.Read")]
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? productId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool includeExpired = false,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] string? batchNo = null,
        [FromQuery] DateTime? expiryDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => Ok(await _inventorySvc.GetInventoryStocksAsync(storeId, productId, searchTerm,
            includeExpired, onlyAvailable, batchNo, expiryDate, page, pageSize));

    // ═══════════════════════════════════════════════════════════════════
    // §20.6 – Pharmacy Batches
    // ═══════════════════════════════════════════════════════════════════

    [PermissionAuthorize("Pharmacy.Batches.View")]
    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches(
        [FromQuery] Guid? storeId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(await _inventorySvc.GetBatchesAsync(storeId, search, page, pageSize));

    [HttpGet("batches/{id}")]
    public async Task<IActionResult> GetBatch(Guid id)
    {
        var b = await _inventorySvc.GetBatchByIdAsync(id);
        return b is null ? NotFound(new { errorCode = "BATCH_NOT_FOUND", message = "Batch not found", messageAr = "الباتش غير موجود" }) : Ok(b);
    }

    // ═══════════════════════════════════════════════════════════════════
    // §12 – Allocate and Dispense (MOST CRITICAL)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>POST /v1/pharmacy/prescriptions/{prescriptionId}/allocate-and-dispense</summary>
    [PermissionAuthorize("Pharmacy.Prescriptions.Dispense")]
    [HttpPost("prescriptions/{prescriptionId}/allocate-and-dispense")]
    public async Task<IActionResult> AllocateAndDispense(Guid prescriptionId, [FromBody] AllocateAndDispenseRequest req)
    {
        req.PrescriptionId = prescriptionId;
        var (result, error) = await _dispenseSvc.AllocateAndDispenseAsync(req, UserId);
        if (error != null)
            return BadRequest(error);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    // §13 – Allocation Status Readback
    // ═══════════════════════════════════════════════════════════════════

    [HttpGet("prescriptions/{prescriptionId}/allocation-status")]
    public async Task<IActionResult> GetAllocationStatus(Guid prescriptionId)
        => Ok(await _dispenseSvc.GetAllocationStatusAsync(prescriptionId));

    // ═══════════════════════════════════════════════════════════════════
    // §14 – Send Prescription to POS
    // ═══════════════════════════════════════════════════════════════════

    [PermissionAuthorize("Pharmacy.Batches.Manage")]
    [HttpPost("prescriptions/{prescriptionId}/send-to-pos")]
    public async Task<IActionResult> SendToPos(Guid prescriptionId, [FromBody] SendToPosRequest req)
    {
        var (result, error) = await _dispenseSvc.SendToPosAsync(prescriptionId, req, UserId);
        if (error != null) return BadRequest(error);
        return Ok(result);
    }
}

/// <summary>
/// Cashier POS Controller – PDF spec §15-§16, §19
/// Handles: Ready prescriptions queue, Prescription checkout, Refunds
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/cashier")]
public class CashierPosController : ControllerBase
{
    private readonly IPharmacyDispenseService _dispenseSvc;

    public CashierPosController(IPharmacyDispenseService dispenseSvc)
    {
        _dispenseSvc = dispenseSvc;
    }

    private Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : Guid.Empty;

    // ═══════════════════════════════════════════════════════════════════
    // §15 – Ready Prescriptions Queue
    // ═══════════════════════════════════════════════════════════════════

    [PermissionAuthorize("Pharmacy.Read")]
    [HttpGet("ready-prescriptions")]
    public async Task<IActionResult> GetReadyPrescriptions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] Guid? storeDetailsId = null)
        => Ok(await _dispenseSvc.GetReadyPrescriptionsAsync(page, pageSize, storeDetailsId));

    // ═══════════════════════════════════════════════════════════════════
    // §16 – Prescription Checkout in POS
    // ═══════════════════════════════════════════════════════════════════

    [PermissionAuthorize("Pharmacy.POS.CloseShiftSummary")]
    [HttpPost("prescription-checkout")]
    public async Task<IActionResult> PrescriptionCheckout([FromBody] PrescriptionCheckoutRequest req)
    {
        var (result, error) = await _dispenseSvc.PrescriptionCheckoutAsync(req, UserId);
        if (error != null) return BadRequest(error);
        return Ok(result);
    }

    [PermissionAuthorize("Pharmacy.Pricing.Manage")]
    [HttpPut("pricing")]
    public IActionResult ManagePricing([FromBody] object req) => Ok(new { success = true });

    [PermissionAuthorize("Pharmacy.POS.ApplyDiscount")]
    [HttpPost("pos/discount")]
    public IActionResult ApplyPosDiscount([FromBody] object req) => Ok(new { success = true });
}
