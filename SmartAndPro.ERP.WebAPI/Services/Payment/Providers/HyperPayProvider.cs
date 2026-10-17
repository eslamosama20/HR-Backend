using Microsoft.Extensions.Options;

namespace SmartAndPro.ERP.WebAPI.Services.Payment.Providers;

/// <summary>
/// HyperPay provider — MOCK implementation for STC Pay.
/// Switch to real integration by replacing CreatePaymentAsync body with actual HyperPay REST calls.
/// Docs: https://wordpressdocs.hyperpay.com
/// </summary>
public class HyperPayProvider : IPaymentProvider
{
    public string ProviderName => "hyperpay";

    private readonly HyperPayOptions _opts;
    private readonly ILogger<HyperPayProvider> _logger;

    public HyperPayProvider(IOptions<PaymentProvidersOptions> opts, ILogger<HyperPayProvider> logger)
    {
        _opts = opts.Value.HyperPay;
        _logger = logger;
    }

    public Task<PaymentResponse> CreatePaymentAsync(OrderDto order)
    {
        _logger.LogInformation("[HyperPay MOCK] Creating STC Pay checkout for order {Ref}, amount {Amount}", order.OrderReference, order.Amount);

        var fakeCheckoutId = Guid.NewGuid().ToString("N");
        var baseUrl = _opts.IsSandbox ? _opts.SandboxBaseUrl : _opts.ProductionBaseUrl;

        // Real HyperPay uses a server-side checkout form — we simulate the redirect
        return Task.FromResult(new PaymentResponse
        {
            Success = true,
            PaymentUrl = $"{baseUrl}/checkouts/{fakeCheckoutId}/payment",
            ExternalId = fakeCheckoutId,
            Provider = ProviderName,
            Status = "Pending",
            RawData = new Dictionary<string, object>
            {
                { "mock", true },
                { "checkoutId", fakeCheckoutId },
                { "brand", "STC_PAY" },
                { "note", "Replace with real HyperPay API when credentials are available." }
            }
        });
    }

    public Task HandleWebhookAsync(string payload, string? hmac = null)
    {
        _logger.LogInformation("[HyperPay MOCK] Webhook: {Payload}", payload[..Math.Min(300, payload.Length)]);
        return Task.CompletedTask;
    }

    public Task<PaymentResponse> RefundAsync(string externalId, decimal amount)
    {
        _logger.LogInformation("[HyperPay MOCK] Refund {ExternalId} amount {Amount}", externalId, amount);
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
