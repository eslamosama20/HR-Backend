using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Zid.Dtos;
using SmartAndPro.ERP.WebAPI.Services.Zid;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Zid;

/// <summary>
/// Receives Zid webhooks (order.created, order.updated, product.*, etc.)
/// This endpoint is PUBLIC — no JWT. Secured by HMAC-SHA256 signature.
/// </summary>
[ApiController]
[Route("api/integrations/zid")]
public class ZidWebhookController : ControllerBase
{
    private readonly IZidIntegrationService _zidService;
    private readonly ILogger<ZidWebhookController> _logger;

    public ZidWebhookController(IZidIntegrationService zidService, ILogger<ZidWebhookController> logger)
    {
        _zidService = zidService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/integrations/zid/webhook
    /// Zid sends events here. Header: X-Zid-Signature
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync();

        // Verify HMAC-SHA256 signature
        var signature = Request.Headers["X-Zid-Signature"].FirstOrDefault() ?? "";
        if (!_zidService.VerifyWebhookSignature(rawPayload, signature))
        {
            _logger.LogWarning("Zid webhook signature verification failed");
            return Unauthorized(new { error = "Invalid webhook signature" });
        }

        ZidWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ZidWebhookPayload>(rawPayload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Zid webhook payload");
            return BadRequest(new { error = "Invalid payload format" });
        }

        if (payload == null || string.IsNullOrEmpty(payload.Event))
            return BadRequest(new { error = "Missing event type" });

        try
        {
            await _zidService.HandleWebhookAsync(payload.Event, payload.Data, rawPayload);
            return Ok(new { status = "received", @event = payload.Event });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Zid webhook {Event}", payload.Event);
            // Return 200 to prevent Zid from retrying — error is logged internally
            return Ok(new { status = "error_logged", @event = payload.Event });
        }
    }

    /// <summary>
    /// GET /api/integrations/zid/callback?code=XXX
    /// OAuth2 redirect — exchange code for token
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest(new { error = "Authorization code is required" });

        try
        {
            await _zidService.ExchangeCodeForTokenAsync(code);
            return Ok(new { status = "connected", message = "Zid account connected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange Zid OAuth code");
            return BadRequest(new { error = "Failed to connect Zid account", detail = ex.Message });
        }
    }
}
