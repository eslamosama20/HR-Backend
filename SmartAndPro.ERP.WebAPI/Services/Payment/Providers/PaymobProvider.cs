using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartAndPro.ERP.WebAPI.Services.Payment.Providers;

/// <summary>
/// Paymob provider — handles: card, mada, apple_pay
/// Docs: https://docs.paymob.com
/// </summary>
public class PaymobProvider : IPaymentProvider
{
    public string ProviderName => "paymob";

    private readonly PaymobOptions _opts;
    private readonly HttpClient _http;
    private readonly ILogger<PaymobProvider> _logger;

    public PaymobProvider(IOptions<PaymentProvidersOptions> opts, HttpClient http, ILogger<PaymobProvider> logger)
    {
        _opts = opts.Value.Paymob;
        _http = http;
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(OrderDto order)
    {
        try
        {
            var baseUrl = _opts.IsSandbox ? _opts.SandboxBaseUrl : _opts.ProductionBaseUrl;

            // ── Step 1: Auth token ─────────────────────────────────
            var authResp = await _http.PostAsync(
                $"{baseUrl}/auth/tokens",
                JsonContent("{ \"api_key\": \"" + _opts.ApiKey + "\" }"));

            if (!authResp.IsSuccessStatusCode)
                return Fail("Paymob auth failed: " + authResp.StatusCode);

            var authBody = await authResp.Content.ReadAsStringAsync();
            var authDoc = JsonDocument.Parse(authBody);
            var authToken = authDoc.RootElement.GetProperty("token").GetString();

            // ── Step 2: Order registration ────────────────────────
            var amountCents = (long)(order.Amount * 100);
            var orderPayload = $$"""
            {
              "auth_token": "{{authToken}}",
              "delivery_needed": false,
              "amount_cents": {{amountCents}},
              "currency": "{{order.Currency}}",
              "merchant_order_id": "{{order.OrderReference}}"
            }
            """;

            var orderResp = await _http.PostAsync($"{baseUrl}/ecommerce/orders", JsonContent(orderPayload));
            if (!orderResp.IsSuccessStatusCode)
                return Fail("Paymob order creation failed: " + orderResp.StatusCode);

            var orderBody = await orderResp.Content.ReadAsStringAsync();
            var orderDoc = JsonDocument.Parse(orderBody);
            var orderId = orderDoc.RootElement.GetProperty("id").GetInt64();

            // ── Step 3: Payment key ────────────────────────────────
            var keyPayload = $$"""
            {
              "auth_token": "{{authToken}}",
              "amount_cents": {{amountCents}},
              "expiration": 3600,
              "order_id": {{orderId}},
              "billing_data": {
                "first_name": "{{order.CustomerName ?? "N/A"}}",
                "last_name": "N/A",
                "email": "{{order.CustomerEmail ?? "na@na.com"}}",
                "phone_number": "{{order.CustomerPhone ?? "N/A"}}",
                "apartment": "NA", "floor": "NA", "street": "NA",
                "building": "NA", "shipping_method": "NA",
                "postal_code": "NA", "city": "NA",
                "country": "SA", "state": "NA"
              },
              "currency": "{{order.Currency}}",
              "integration_id": {{_opts.IntegrationId}},
              "lock_order_when_paid": false
            }
            """;

            var keyResp = await _http.PostAsync($"{baseUrl}/acceptance/payment_keys", JsonContent(keyPayload));
            if (!keyResp.IsSuccessStatusCode)
                return Fail("Paymob payment key failed: " + keyResp.StatusCode);

            var keyBody = await keyResp.Content.ReadAsStringAsync();
            var keyDoc = JsonDocument.Parse(keyBody);
            var paymentKey = keyDoc.RootElement.GetProperty("token").GetString();

            var iframeUrl = _opts.IsSandbox
                ? $"https://accept.paymob.com/api/acceptance/iframes/{_opts.IntegrationId}?payment_token={paymentKey}"
                : $"https://accept.paymob.com/api/acceptance/iframes/{_opts.IntegrationId}?payment_token={paymentKey}";

            return new PaymentResponse
            {
                Success = true,
                PaymentUrl = iframeUrl,
                ExternalId = orderId.ToString(),
                Provider = ProviderName,
                Status = "Pending",
                RawData = new Dictionary<string, object> { { "orderId", orderId }, { "paymentKey", paymentKey! } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob CreatePayment error for order {Ref}", order.OrderReference);
            return Fail($"Paymob error: {ex.Message}");
        }
    }

    public async Task HandleWebhookAsync(string payload, string? hmac = null)
    {
        _logger.LogInformation("[Paymob] Webhook received with HMAC: {HMAC}. Payload: {Payload}", hmac, payload[..Math.Min(500, payload.Length)]);
        await Task.CompletedTask;
    }

    public async Task<PaymentResponse> RefundAsync(string externalId, decimal amount)
    {
        try
        {
            var baseUrl = _opts.IsSandbox ? _opts.SandboxBaseUrl : _opts.ProductionBaseUrl;

            var authResp = await _http.PostAsync(
                $"{baseUrl}/auth/tokens",
                JsonContent("{ \"api_key\": \"" + _opts.ApiKey + "\" }"));

            var authBody = await authResp.Content.ReadAsStringAsync();
            var authDoc = JsonDocument.Parse(authBody);
            var authToken = authDoc.RootElement.GetProperty("token").GetString();

            var refundPayload = $$"""
            {
              "auth_token": "{{authToken}}",
              "transaction_id": {{externalId}},
              "amount_cents": {{(long)(amount * 100)}}
            }
            """;

            var refundResp = await _http.PostAsync($"{baseUrl}/acceptance/void_refund/refund", JsonContent(refundPayload));

            return new PaymentResponse
            {
                Success = refundResp.IsSuccessStatusCode,
                Provider = ProviderName,
                ExternalId = externalId,
                Status = refundResp.IsSuccessStatusCode ? "Refunded" : "Failed",
                ErrorMessage = refundResp.IsSuccessStatusCode ? null : await refundResp.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob Refund error for {ExternalId}", externalId);
            return Fail($"Refund error: {ex.Message}");
        }
    }

    private static StringContent JsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");

    private static PaymentResponse Fail(string msg) =>
        new() { Success = false, ErrorMessage = msg, Status = "Failed" };
}
