using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty.Dtos;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Wasafaty;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty;

/// <summary>
/// Wasafaty (وصفتي) integration controller — authenticated endpoints for cashier/pharmacy.
/// Routes: /api/integrations/wasafaty/*
/// </summary>
[ApiController]
[Route("api/integrations/wasafaty")]
[PermissionAuthorize("System.Read")]
public class WasafatyController : ControllerBase
{
    private readonly IWasafatyIntegrationService _wasafatyService;
    private readonly ILogger<WasafatyController> _logger;

    public WasafatyController(
        IWasafatyIntegrationService wasafatyService,
        ILogger<WasafatyController> logger)
    {
        _wasafatyService = wasafatyService;
        _logger = logger;
    }

    // ═══════════════════════════ Prescriptions ═══════════════════════════

    /// <summary>
    /// GET /api/integrations/wasafaty/prescriptions/pending
    /// Get all pending Wasafaty prescriptions ready for dispensing.
    /// Called by pharmacy cashier to view incoming e-prescriptions.
    /// </summary>
    [HttpGet("prescriptions/pending")]
    public async Task<IActionResult> GetPendingPrescriptions([FromQuery] Guid? storeDetailsId)
    {
        try
        {
            var prescriptions = await _wasafatyService.GetPendingPrescriptionsAsync(storeDetailsId);
            return Ok(new { data = prescriptions, count = prescriptions.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending Wasafaty prescriptions");
            return StatusCode(500, new { message = "Error retrieving prescriptions", messageAr = "خطأ في استرجاع الوصفات" });
        }
    }

    /// <summary>
    /// GET /api/integrations/wasafaty/prescriptions/{id}
    /// Get a specific Wasafaty prescription by internal ID.
    /// </summary>
    [HttpGet("prescriptions/{id:guid}")]
    public async Task<IActionResult> GetPrescriptionById(Guid id)
    {
        try
        {
            var prescription = await _wasafatyService.GetPrescriptionByIdAsync(id);
            if (prescription == null)
                return NotFound(new { message = "Prescription not found", messageAr = "الوصفة غير موجودة" });

            return Ok(prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Wasafaty prescription {Id}", id);
            return StatusCode(500, new { message = "Error retrieving prescription", messageAr = "خطأ في استرجاع الوصفة" });
        }
    }

    /// <summary>
    /// POST /api/integrations/wasafaty/prescriptions/fetch
    /// Fetch a prescription from Wasafaty API by NationalId+OTP or by FHIR ID.
    /// Cashier calls this when patient presents at the counter.
    /// </summary>
    [HttpPost("prescriptions/fetch")]
    public async Task<IActionResult> FetchPrescription(
        [FromBody] FetchPrescriptionRequest request,
        [FromQuery] Guid? storeDetailsId)
    {
        try
        {
            if (string.IsNullOrEmpty(request.NationalId) && string.IsNullOrEmpty(request.FhirResourceId))
                return BadRequest(new { message = "NationalId or FhirResourceId is required", messageAr = "رقم الهوية أو رقم الوصفة مطلوب" });

            var prescription = await _wasafatyService.FetchFromWasafatyAsync(request, storeDetailsId);

            if (prescription == null)
                return NotFound(new { message = "No active prescription found", messageAr = "لا توجد وصفة نشطة" });

            return Ok(prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prescription from Wasafaty");
            return StatusCode(500, new { message = "Error fetching from Wasafaty", messageAr = "خطأ في جلب الوصفة من وصفتي" });
        }
    }

    /// <summary>
    /// POST /api/integrations/wasafaty/verify-otp
    /// Verify patient identity via NID + OTP from Wasafaty mobile app.
    /// Required before dispensing controlled substances.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.NationalId) || string.IsNullOrEmpty(request.Otp))
                return BadRequest(new { message = "NationalId and OTP are required", messageAr = "رقم الهوية ورمز OTP مطلوبان" });

            var result = await _wasafatyService.VerifyPatientOtpAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying patient OTP");
            return StatusCode(500, new { message = "Error verifying OTP", messageAr = "خطأ في التحقق من رمز OTP" });
        }
    }

    // ═══════════════════════════ Dispensing ═══════════════════════════

    /// <summary>
    /// POST /api/integrations/wasafaty/dispense
    /// Mark prescription items as dispensed with batch/qty details.
    /// Called after cashier confirms items in the cart.
    /// </summary>
    [HttpPost("dispense")]
    public async Task<IActionResult> Dispense([FromBody] DispenseRequest request)
    {
        try
        {
            var success = await _wasafatyService.MarkDispensedAsync(request);

            // Report dispense to Wasafaty synchronously (reliable, logged)
            try
            {
                await _wasafatyService.ReportDispenseToWasafatyAsync(request.WasafatyPrescriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dispense report to Wasafaty failed for {Id} — will retry via sync logs", request.WasafatyPrescriptionId);
            }

            return Ok(new
            {
                success,
                message = "Prescription dispensed successfully",
                messageAr = "تم صرف الوصفة بنجاح"
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message, messageAr = "الوصفة غير موجودة" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispensing prescription");
            return StatusCode(500, new { message = "Error dispensing prescription", messageAr = "خطأ في صرف الوصفة" });
        }
    }

    // ═══════════════════════════ Insurance Claims ═══════════════════════════

    /// <summary>
    /// POST /api/integrations/wasafaty/claims/submit
    /// Submit insurance claim to NPHIES for a dispensed prescription.
    /// </summary>
    [HttpPost("claims/submit")]
    public async Task<IActionResult> SubmitClaim([FromBody] SubmitClaimRequest request)
    {
        try
        {
            var result = await _wasafatyService.SubmitInsuranceClaimAsync(request.WasafatyPrescriptionId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting insurance claim");
            return StatusCode(500, new { message = "Error submitting claim", messageAr = "خطأ في تقديم المطالبة" });
        }
    }

    // ═══════════════════════════ Settings / Dashboard ═══════════════════════════

    /// <summary>
    /// GET /api/integrations/wasafaty/settings
    /// Get Wasafaty integration status and statistics.
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var settings = await _wasafatyService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Wasafaty settings");
            return StatusCode(500, new { message = "Error retrieving settings", messageAr = "خطأ في استرجاع الإعدادات" });
        }
    }

    /// <summary>
    /// GET /api/integrations/wasafaty/sync-logs
    /// Get sync/audit logs for Wasafaty API interactions.
    /// </summary>
    [HttpGet("sync-logs")]
    public async Task<IActionResult> GetSyncLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? eventType = null)
    {
        try
        {
            var logs = await _wasafatyService.GetSyncLogsAsync(page, pageSize, eventType);
            return Ok(new { data = logs, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Wasafaty sync logs");
            return StatusCode(500, new { message = "Error retrieving logs", messageAr = "خطأ في استرجاع السجلات" });
        }
    }

    /// <summary>
    /// POST /api/integrations/wasafaty/token
    /// Manually set Wasafaty API token (for development/testing).
    /// </summary>
    [HttpPost("token")]
    public async Task<IActionResult> SetToken([FromBody] SetTokenRequest request)
    {
        try
        {
            await _wasafatyService.SetTokenManuallyAsync(request.AccessToken, request.RefreshToken, request.ExpiresAtUnix);
            return Ok(new { message = "Token set successfully", messageAr = "تم تعيين الرمز بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting Wasafaty token");
            return StatusCode(500, new { message = "Error setting token" });
        }
    }
}
