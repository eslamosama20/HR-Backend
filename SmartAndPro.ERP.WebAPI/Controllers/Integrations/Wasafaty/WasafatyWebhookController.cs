using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty.Dtos;
using SmartAndPro.ERP.WebAPI.Services.Wasafaty;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty;

/// <summary>
/// Receives Wasafaty (وصفتي) webhooks — prescription notifications from NHIC.
/// This endpoint is PUBLIC (no JWT). Secured by HMAC signature verification.
/// </summary>
[ApiController]
[Route("api/integrations/wasafaty")]
public class WasafatyWebhookController : ControllerBase
{
    private readonly IWasafatyIntegrationService _wasafatyService;
    private readonly ILogger<WasafatyWebhookController> _logger;

    public WasafatyWebhookController(
        IWasafatyIntegrationService wasafatyService,
        ILogger<WasafatyWebhookController> logger)
    {
        _wasafatyService = wasafatyService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/integrations/wasafaty/webhook
    /// Wasafaty pushes prescription events here (prescription.created, prescription.cancelled).
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync();

        var signature = Request.Headers["X-Wasafaty-Signature"].FirstOrDefault() ?? "";
        if (!_wasafatyService.VerifyWebhookSignature(rawPayload, signature))
        {
            _logger.LogWarning("Wasafaty webhook signature verification failed");
            return Unauthorized(new { error = "Invalid webhook signature" });
        }

        WasafatyNotificationPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<WasafatyNotificationPayload>(rawPayload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Wasafaty webhook payload");
            return BadRequest(new { error = "Invalid payload format" });
        }

        if (payload?.Data == null || string.IsNullOrEmpty(payload.Event))
            return BadRequest(new { error = "Missing event type" });

        try
        {
            await _wasafatyService.HandleWebhookAsync(payload.Event, payload.Data, rawPayload);
            return Ok(new { status = "received", @event = payload.Event });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Wasafaty webhook {Event}", payload.Event);
            return Ok(new { status = "error_logged", @event = payload.Event });
        }
    }
}
