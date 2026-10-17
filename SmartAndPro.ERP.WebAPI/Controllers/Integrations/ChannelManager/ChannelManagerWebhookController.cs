using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Services.ChannelManager;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.ChannelManager;

[ApiController]
[Route("api/integrations/channel-manager/webhook")]
public class ChannelManagerWebhookController : ControllerBase
{
    private readonly IChannelManagerService _channelService;
    private readonly IConfiguration _config;
    private readonly ILogger<ChannelManagerWebhookController> _logger;

    public ChannelManagerWebhookController(
        IChannelManagerService channelService,
        IConfiguration config,
        ILogger<ChannelManagerWebhookController> logger)
    {
        _channelService = channelService;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/webhook
    /// Receives real-time reservations updates from the Channel Manager.
    /// Secured using HMAC-SHA256 signature and replay protection timestamp checks.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        // 1. Replay Attack Protection: verify timestamp header
        if (!Request.Headers.TryGetValue("X-Channel-Timestamp", out var timestampHeader) ||
            !long.TryParse(timestampHeader.FirstOrDefault(), out long requestTimestampUnix))
        {
            return BadRequest("Missing or invalid timestamp header.");
        }

        var timeDifference = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - requestTimestampUnix;
        if (Math.Abs(timeDifference) > 300) // 5 minutes threshold
        {
            _logger.LogWarning("Rejecting webhook due to replay attack suspicion: timestamp age is {Diff} seconds", timeDifference);
            return BadRequest("Timestamp is too old (replay protection).");
        }

        // 2. Validate HMAC Signature
        if (!Request.Headers.TryGetValue("X-Channel-Signature", out var signatureHeader))
        {
            return Unauthorized("Missing signature header.");
        }

        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync();

        string signature = signatureHeader.FirstOrDefault() ?? string.Empty;
        string secret = _config["ChannelManager:WebhookSecret"] ?? "default_secret";

        if (!VerifyWebhookSignature(rawPayload, signature, secret))
        {
            _logger.LogWarning("Webhook signature verification failed.");
            return Unauthorized("Invalid webhook signature.");
        }

        // 3. Queue processing job in database outbox (Outbox Pattern)
        await _channelService.QueueWebhookProcessingJob(rawPayload);

        return Ok(new { status = "received" });
    }

    private bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(secret) || secret == "default_secret") return true; // Bypass signature validation in local/dev if unset

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }
}
