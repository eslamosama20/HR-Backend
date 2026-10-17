using Microsoft.Extensions.Options;

namespace SmartAndPro.ERP.WebAPI.Services.Payment.Providers;

/// <summary>
/// Tabby provider — MOCK implementation.
/// Switch to real integration by replacing CreatePaymentAsync body with actual Tabby REST calls.
/// Docs: https://docs.tabby.ai
/// </summary>
public class TabbyProvider : IPaymentProvider
{
    public string ProviderName => "tabby";

    private readonly TabbyOptions _opts;
    private readonly ILogger<TabbyProvider> _logger;

    public TabbyProvider(IOptions<PaymentProvidersOptions> opts, ILogger<TabbyProvider> logger)
    {
        _opts = opts.Value.Tabby;
        _logger = logger;
    }

    public Task<PaymentResponse> CreatePaymentAsync(OrderDto order)
    {
        _logger.LogInformation("[Tabby MOCK] Creating payment for order {Ref}, amount {Amount}", order.OrderReference, order.Amount);

        // ── MOCK: return a fake checkout URL ──────────────────────
        var fakeSessionId = Guid.NewGuid().ToString("N")[..16];
        var baseUrl = _opts.IsSandbox
            ? "https://checkout.tabby.ai/tabby/checkout"
            : "https://checkout.tabby.ai/tabby/checkout";

        return Task.FromResult(new PaymentResponse
        {
            Success = true,
            PaymentUrl = $"{baseUrl}?sessionId={fakeSessionId}&ref={order.OrderReference}",
            ExternalId = fakeSessionId,
            Provider = ProviderName,
            Status = "Pending",
            RawData = new Dictionary<string, object>
            {
                { "mock", true },
                { "sessionId", fakeSessionId },
                { "note", "Replace with real Tabby API when credentials are available." }
            }
        });
    }

    public Task HandleWebhookAsync(string payload, string? hmac = null)
    {
        _logger.LogInformation("[Tabby MOCK] Webhook: {Payload}", payload[..Math.Min(300, payload.Length)]);
        return Task.CompletedTask;
    }

    public Task<PaymentResponse> RefundAsync(string externalId, decimal amount)
    {
        _logger.LogInformation("[Tabby MOCK] Refund {ExternalId} amount {Amount}", externalId, amount);
        return Task.FromResult(new PaymentResponse
        {
            Success = true,
            ExternalId = externalId,
            Provider = ProviderName,
            Status = "Refunded",
            RawData = new Dictionary<string, object> { { "mock", true } }
        });
    }
}
