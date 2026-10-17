using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Payment;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.IO;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Payment;

/// <summary>
/// Payment Orchestration Controller
/// Routes:
///   POST  /api/payments
///   GET   /api/payments/methods
///   POST  /api/webhooks/{provider}
/// </summary>
[ApiController]
[Authorize]
[PermissionAuthorize("Account.Read")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _svc;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService svc, ILogger<PaymentController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    // ── 1. Create payment ────────────────────────────────────────────
    /// <summary>
    /// POST /api/payments
    /// Initiates a payment and returns a redirect URL or session.
    /// Supported methods: card | mada | apple_pay | tabby | tamara | stc_pay
    /// </summary>
    [Authorize]
    [HttpPost("api/payments")]
    public async Task<IActionResult> CreatePayment([FromBody] OrderDto order)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (order.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        if (string.IsNullOrWhiteSpace(order.PaymentMethod))
            return BadRequest(new { message = "paymentMethod is required." });

        var result = await _svc.InitiatePaymentAsync(order);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            paymentUrl  = result.PaymentUrl,
            externalId  = result.ExternalId,
            provider    = result.Provider,
            status      = result.Status,
            internalId  = result.RawData.TryGetValue("internalPaymentId", out var id) ? id : null,
            rawData     = result.RawData
        });
    }

    // ── 2. Available payment methods ─────────────────────────────────
    /// <summary>
    /// GET /api/payments/methods?country=SA
    /// Returns the list of available payment methods for a given country.
    /// </summary>
    [HttpGet("api/payments/methods")]
    public async Task<IActionResult> GetMethods([FromQuery] string country = "SA")
    {
        var methods = await _svc.GetAvailableMethodsAsync(country);
        return Ok(methods);
    }

    // ── 3. Webhook handler ───────────────────────────────────────────
    /// <summary>
    /// POST /api/webhooks/{provider}
    /// Receives webhook notifications from payment providers.
    /// provider: paymob | tabby | tamara | hyperpay
    /// ⚠ This endpoint must be publicly accessible (no [Authorize]) and registered in the provider dashboard.
    /// </summary>
    [HttpPost("api/webhooks/{provider}")]
    public async Task<IActionResult> Webhook(string provider)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawPayload = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        _logger.LogInformation("[Webhook] Provider={Provider}, IP={IP}, Length={Len}",
            provider,
            HttpContext.Connection.RemoteIpAddress,
            rawPayload.Length);

        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            // Some providers send form data or query string — combine into JSON
            rawPayload = System.Text.Json.JsonSerializer.Serialize(
                Request.Query.ToDictionary(q => q.Key, q => (object)q.Value.ToString()));
        }

        string? hmac = Request.Query["hmac"].ToString();
        if (string.IsNullOrWhiteSpace(hmac))
        {
            hmac = Request.Headers["x-hmac"].ToString();
            if (string.IsNullOrWhiteSpace(hmac))
            {
                hmac = null;
            }
        }

        var (ok, error) = await _svc.HandleWebhookAsync(provider, rawPayload, hmac);

        if (!ok)
        {
            return Unauthorized(new { received = false, error = error });
        }

        return Ok(new { received = true });
    }
}
