using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Salla.Dtos;
using SmartAndPro.ERP.WebAPI.Services.Salla;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Salla;

/// <summary>
/// Receives Salla webhooks (order.created, order.updated, product.*, etc.)
/// This endpoint is PUBLIC — no JWT auth. Secured by HMAC signature verification.
/// </summary>
[ApiController]
[Route("api/integrations/salla")]
public class SallaWebhookController : ControllerBase
{
    private readonly ISallaIntegrationService _sallaService;
    private readonly ILogger<SallaWebhookController> _logger;

    public SallaWebhookController(
        ISallaIntegrationService sallaService,
        ILogger<SallaWebhookController> logger)
    {
        _sallaService = sallaService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/integrations/salla/webhook
    /// Salla sends events here (order.created, product.updated, etc.)
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        // Read raw body for signature verification
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync();

        // Verify HMAC signature
        var signature = Request.Headers["X-Salla-Signature"].FirstOrDefault() ?? "";
        if (!_sallaService.VerifyWebhookSignature(rawPayload, signature))
        {
            _logger.LogWarning("Salla webhook signature verification failed");
            return Unauthorized(new { error = "Invalid webhook signature" });
        }

        // Parse payload
        SallaWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SallaWebhookPayload>(rawPayload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Salla webhook payload");
            return BadRequest(new { error = "Invalid payload format" });
        }

        if (payload == null || string.IsNullOrEmpty(payload.Event))
            return BadRequest(new { error = "Missing event type" });

        try
        {
            await _sallaService.HandleWebhookAsync(payload.Event, payload.Data, rawPayload);
            return Ok(new { status = "received", @event = payload.Event });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Salla webhook {Event}", payload.Event);
            // Return 200 to prevent Salla from retrying endlessly — error is logged internally
            return Ok(new { status = "error_logged", @event = payload.Event });
        }
    }

    /// <summary>
    /// GET /api/integrations/salla/callback?code={code}
    /// OAuth2 callback — Salla redirects here after merchant authorizes the app
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest(new { error = "Authorization code is required" });

        try
        {
            var token = await _sallaService.ExchangeCodeForTokenAsync(code);
            // In production, redirect to frontend with success indicator
            return Ok(new { status = "connected", message = "Salla account connected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange Salla OAuth code");
            return BadRequest(new { error = "Failed to connect Salla account", detail = ex.Message });
        }
    }
}
