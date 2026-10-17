using Microsoft.Extensions.Options;

namespace SmartAndPro.ERP.WebAPI.Services.Payment.Providers;

/// <summary>
/// Tamara provider — MOCK implementation.
/// Switch to real integration by replacing CreatePaymentAsync body with actual Tamara REST calls.
/// Docs: https://docs.tamara.co
/// </summary>
public class TamaraProvider : IPaymentProvider
{
    public string ProviderName => "tamara";

    private readonly TamaraOptions _opts;
    private readonly ILogger<TamaraProvider> _logger;

    public TamaraProvider(IOptions<PaymentProvidersOptions> opts, ILogger<TamaraProvider> logger)
    {
        _opts = opts.Value.Tamara;
        _logger = logger;
    }

    public Task<PaymentResponse> CreatePaymentAsync(OrderDto order)
    {
        _logger.LogInformation("[Tamara MOCK] Creating payment for order {Ref}, amount {Amount}", order.OrderReference, order.Amount);

        var fakeOrderId = Guid.NewGuid().ToString();
        var baseUrl = _opts.IsSandbox ? _opts.SandboxBaseUrl : _opts.ProductionBaseUrl;

        return Task.FromResult(new PaymentResponse
        {
            Success = true,
            PaymentUrl = $"{baseUrl}/checkout?orderId={fakeOrderId}",
            ExternalId = fakeOrderId,
            Provider = ProviderName,
            Status = "Pending",
            RawData = new Dictionary<string, object>
            {
                { "mock", true },
                { "orderId", fakeOrderId },
                { "note", "Replace with real Tamara API when credentials are available." }
            }
        });
    }

    public Task HandleWebhookAsync(string payload, string? hmac = null)
    {
        _logger.LogInformation("[Tamara MOCK] Webhook: {Payload}", payload[..Math.Min(300, payload.Length)]);
        return Task.CompletedTask;
    }

    public Task<PaymentResponse> RefundAsync(string externalId, decimal amount)
    {
        _logger.LogInformation("[Tamara MOCK] Refund {ExternalId} amount {Amount}", externalId, amount);
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
