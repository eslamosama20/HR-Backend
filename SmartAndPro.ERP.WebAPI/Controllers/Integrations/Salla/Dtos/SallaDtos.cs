using System.Text.Json.Serialization;
using SmartAndPro.ERP.Domains.Enums.Salla;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Salla.Dtos;

// ────────────────────── OAuth ──────────────────────

public record SallaOAuthTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string RefreshToken,
    string Scope
);

// ──────────────────── Webhook Payload ────────────────────

public class SallaWebhookPayload
{
    public string Event { get; set; } = string.Empty;
    [JsonPropertyName("merchant")]
    public long MerchantId { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    public SallaWebhookData Data { get; set; } = new();
}

public class SallaWebhookData
{
    public long Id { get; set; }
    [JsonPropertyName("reference_id")]
    public string? ReferenceId { get; set; }
    public SallaWebhookStatus? Status { get; set; }
    public SallaWebhookPaymentInfo? Payment { get; set; }
    public SallaWebhookCustomer? Customer { get; set; }
    public SallaWebhookAmounts? Amounts { get; set; }
    public List<SallaWebhookItem> Items { get; set; } = new();
    public SallaWebhookShipping? Shipping { get; set; }
    public DateTime? Date { get; set; }

    // Product fields (for product.* events)
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public SallaWebhookPrice? Price { get; set; }
    public SallaWebhookQuantity? Quantity { get; set; }

    // OAuth fields (for app.store.authorize event — Easy Mode)
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    public int? Expires { get; set; }
    public string? Scope { get; set; }
}

public class SallaWebhookStatus
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}

public class SallaWebhookPaymentInfo
{
    public string? Id { get; set; }
    public string? Method { get; set; }
    public string? Status { get; set; }
}

public class SallaWebhookCustomer
{
    public long Id { get; set; }
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
}

public class SallaWebhookAmounts
{
    [JsonPropertyName("sub_total")]
    public SallaWebhookMoneyField? SubTotal { get; set; }
    public SallaWebhookMoneyField? Shipping { get; set; }
    public SallaWebhookMoneyField? Tax { get; set; }
    public SallaWebhookMoneyField? Discounts { get; set; }
    public SallaWebhookMoneyField? Total { get; set; }
}

public class SallaWebhookMoneyField
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
}

public class SallaWebhookItem
{
    public long Id { get; set; }
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public SallaWebhookMoneyField? Price { get; set; }
    public SallaWebhookMoneyField? Total { get; set; }
    public SallaWebhookMoneyField? Tax { get; set; }
    public SallaWebhookMoneyField? Discount { get; set; }
    public List<SallaWebhookItemOption>? Options { get; set; }
}

public class SallaWebhookItemOption
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class SallaWebhookShipping
{
    public SallaWebhookAddress? Address { get; set; }
}

public class SallaWebhookAddress
{
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Street { get; set; }
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}

public class SallaWebhookPrice
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
}

public class SallaWebhookQuantity
{
    public int Amount { get; set; }
}

// ──────────────────── API Responses ────────────────────

public class SallaOrderListResponse
{
    public Guid Id { get; set; }
    public long SallaOrderId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public SallaOrderStatus Status { get; set; }
    public SallaPaymentStatus PaymentStatus { get; set; }
    public SallaFulfillmentStatus FulfillmentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
    public SallaSyncStatus SyncStatus { get; set; }
    public int? SaleInvoiceId { get; set; }
    public DateTime SallaCreatedAt { get; set; }
    public int ItemsCount { get; set; }
}

public class SallaOrderDetailResponse : SallaOrderListResponse
{
    public decimal SubTotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CustomerEmail { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? ShippingStreet { get; set; }
    public string? PaymentMethod { get; set; }
    public string? SyncError { get; set; }
    public DateTime? SyncedAt { get; set; }
    public List<SallaOrderItemResponse> Items { get; set; } = new();
}

public class SallaOrderItemResponse
{
    public Guid Id { get; set; }
    public long SallaProductId { get; set; }
    public string? SallaSku { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? OptionsSummary { get; set; }
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }
}

public class SallaProductMappingResponse
{
    public Guid Id { get; set; }
    public long SallaProductId { get; set; }
    public string? SallaSku { get; set; }
    public string SallaProductName { get; set; } = string.Empty;
    public decimal? SallaPrice { get; set; }
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }
    public bool IsMapped { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

public class SallaSyncLogResponse
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public SallaSyncDirection Direction { get; set; }
    public SallaSyncStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public long? SallaResourceId { get; set; }
    public int DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ──────────────────── Requests ────────────────────

public class MapProductRequest
{
    public long SallaProductId { get; set; }
    public Guid MainPurchaseClassId { get; set; }
}

public class BulkMapProductsRequest
{
    public List<MapProductRequest> Mappings { get; set; } = new();
}

public class SyncOrderToInvoiceRequest
{
    public Guid? StoreDetailsId { get; set; }
}

public class UpdateSallaOrderStatusRequest
{
    public SallaOrderStatus Status { get; set; }
}

public class SallaSettingsResponse
{
    public bool IsConnected { get; set; }
    public string? StoreName { get; set; }
    public int TotalOrders { get; set; }
    public int PendingSyncOrders { get; set; }
    public int MappedProducts { get; set; }
    public int UnmappedProducts { get; set; }
    public DateTime? LastSyncAt { get; set; }
}

public class SallaConnectRequest
{
    public string Code { get; set; } = string.Empty;
}

public class SetTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public long? Expires { get; set; }
}

public class SallaPaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>Push stock quantity to Salla</summary>
public class SyncStockToSallaRequest
{
    public long SallaProductId { get; set; }
    public int Quantity { get; set; }
}

// ──────────────────── Store-Aware Stock Sync ────────────────────

/// <summary>Request to sync all stock from a specific store to Salla</summary>
public class SyncAllStockRequest
{
    public Guid StoreDetailsId { get; set; }
}

/// <summary>Result of syncing all mapped products' stock to Salla</summary>
public class SallaStockSyncResult
{
    public Guid StoreDetailsId { get; set; }
    public int TotalProducts { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

// ──────────────────── Cashier Salla Orders ────────────────────

/// <summary>Salla order view for cashier screen</summary>
public class SallaCashierOrderResponse
{
    public Guid Id { get; set; }
    public long SallaOrderId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
    public SallaPaymentStatus PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    /// <summary>true = paid online (can create invoice immediately), false = COD (wait for payment)</summary>
    public bool IsPaidOnline { get; set; }
    public int ItemsCount { get; set; }
    public DateTime SallaCreatedAt { get; set; }
    public List<SallaCashierOrderItemResponse> Items { get; set; } = new();
}

public class SallaCashierOrderItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid? MainPurchaseClassId { get; set; }
    public bool IsMapped { get; set; }
}
