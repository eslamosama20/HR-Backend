namespace SmartAndPro.ERP.Application.IServices;

public interface IPaymentService
{
    /// <summary>
    /// Process a payment through the configured gateway (Stripe/PayTabs)
    /// </summary>
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);

    /// <summary>
    /// Process a refund for a completed payment
    /// </summary>
    Task<PaymentResult> ProcessRefundAsync(Guid transactionId, decimal amount, string reason);

    /// <summary>
    /// Verify webhook signature from payment gateway
    /// </summary>
    Task<WebhookVerificationResult> VerifyWebhookAsync(string payload, string signature);

    /// <summary>
    /// Handle payment webhook from gateway
    /// </summary>
    Task<bool> HandlePaymentWebhookAsync(string payload);
}

public class PaymentRequest
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string CardNumber { get; set; } = string.Empty;
    public string CardExpMonth { get; set; } = string.Empty;
    public string CardExpYear { get; set; } = string.Empty;
    public string CardCvc { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerIpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
    public string? GatewayResponse { get; set; }
}

public class WebhookVerificationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EventType { get; set; }
    public string? TransactionId { get; set; }
}
