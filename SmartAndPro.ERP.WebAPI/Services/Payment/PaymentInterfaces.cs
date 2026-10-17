namespace SmartAndPro.ERP.WebAPI.Services.Payment;

// ═══════════════════════════════════════════════════════════
// Shared DTOs
// ═══════════════════════════════════════════════════════════

public class OrderDto
{
    public string OrderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";

    /// <summary>card | mada | apple_pay | tabby | tamara | stc_pay</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Description { get; set; }

    /// <summary>Where to redirect after success</summary>
    public string? CallbackUrl { get; set; }
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? ExternalId { get; set; }
    public string? Provider { get; set; }
    public string? ErrorMessage { get; set; }
    public string Status { get; set; } = "Pending";
    public Dictionary<string, object> RawData { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════
// Provider Interface
// ═══════════════════════════════════════════════════════════

public interface IPaymentProvider
{
    string ProviderName { get; }
    Task<PaymentResponse> CreatePaymentAsync(OrderDto order);
    Task HandleWebhookAsync(string payload, string? hmac = null);
    Task<PaymentResponse> RefundAsync(string externalId, decimal amount);
}

// ═══════════════════════════════════════════════════════════
// Payment Service Interface
// ═══════════════════════════════════════════════════════════

public interface IPaymentService
{
    Task<PaymentResponse> InitiatePaymentAsync(OrderDto order);
    Task<List<PaymentMethodDto>> GetAvailableMethodsAsync(string country);
    Task<(bool ok, string? error)> HandleWebhookAsync(string provider, string rawPayload, string? hmac = null);
}

// ═══════════════════════════════════════════════════════════
// Payment Method DTO (for GET /methods endpoint)
// ═══════════════════════════════════════════════════════════

public class PaymentMethodDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool Available { get; set; } = true;
    public string? IconUrl { get; set; }
}

// ═══════════════════════════════════════════════════════════
// Configuration Options
// ═══════════════════════════════════════════════════════════

public class PaymentProvidersOptions
{
    public PaymobOptions Paymob { get; set; } = new();
    public TabbyOptions Tabby { get; set; } = new();
    public TamaraOptions Tamara { get; set; } = new();
    public HyperPayOptions HyperPay { get; set; } = new();
}

public class PaymobOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
    public string HmacSecret { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string SandboxBaseUrl { get; set; } = "https://accept.paymob.com/api";
    public string ProductionBaseUrl { get; set; } = "https://accept.paymob.com/api";
}

public class TabbyOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string SandboxBaseUrl { get; set; } = "https://api.tabby.ai/api/v2";
    public string ProductionBaseUrl { get; set; } = "https://api.tabby.ai/api/v2";
}

public class TamaraOptions
{
    public string ApiToken { get; set; } = string.Empty;
    public string NotificationToken { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string SandboxBaseUrl { get; set; } = "https://api-sandbox.tamara.co";
    public string ProductionBaseUrl { get; set; } = "https://api.tamara.co";
}

public class HyperPayOptions
{
    public string EntityId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string SandboxBaseUrl { get; set; } = "https://eu-test.oppwa.com/v1";
    public string ProductionBaseUrl { get; set; } = "https://eu-prod.oppwa.com/v1";
}
