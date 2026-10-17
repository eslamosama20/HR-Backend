using System.Text.Json.Serialization;
using SmartAndPro.ERP.Domains.Enums.Zid;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Zid.Dtos;

// ────────────────────── OAuth ──────────────────────

public record ZidOAuthTokenResponse(
    [property: JsonPropertyName("access_token")]  string AccessToken,
    [property: JsonPropertyName("token_type")]    string TokenType,
    [property: JsonPropertyName("expires_in")]    int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("scope")]         string Scope
);

// ──────────────────── Webhook Payload ────────────────────

public class ZidWebhookPayload
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("store_id")]
    public long StoreId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("data")]
    public ZidWebhookData Data { get; set; } = new();
}

public class ZidWebhookData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("reference_id")]
    public string? ReferenceId { get; set; }

    [JsonPropertyName("status")]
    public ZidWebhookStatus? Status { get; set; }

    [JsonPropertyName("payment")]
    public ZidWebhookPaymentInfo? Payment { get; set; }

    [JsonPropertyName("customer")]
    public ZidWebhookCustomer? Customer { get; set; }

    [JsonPropertyName("amounts")]
    public ZidWebhookAmounts? Amounts { get; set; }

    [JsonPropertyName("products")]
    public List<ZidWebhookItem> Products { get; set; } = new();

    [JsonPropertyName("shipping_address")]
    public ZidWebhookAddress? ShippingAddress { get; set; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    // Product-specific fields
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }
}

public class ZidWebhookStatus
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ZidWebhookPaymentInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class ZidWebhookCustomer
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }
}

public class ZidWebhookAmounts
{
    [JsonPropertyName("subtotal")]
    public decimal SubTotal { get; set; }

    [JsonPropertyName("shipping")]
    public decimal Shipping { get; set; }

    [JsonPropertyName("tax")]
    public decimal Tax { get; set; }

    [JsonPropertyName("discount")]
    public decimal Discount { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";
}

public class ZidWebhookItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("tax")]
    public decimal Tax { get; set; }

    [JsonPropertyName("discount")]
    public decimal Discount { get; set; }

    [JsonPropertyName("options")]
    public List<ZidWebhookItemOption>? Options { get; set; }
}

public class ZidWebhookItemOption
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class ZidWebhookAddress
{
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("district")]
    public string? District { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("zip_code")]
    public string? ZipCode { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lng")]
    public double? Lng { get; set; }
}

// ──────────────────── API Responses (from Zid REST API) ────────────────────

public class ZidOrderListResponse
{
    public Guid Id { get; set; }
    public long ZidOrderId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public ZidOrderStatus Status { get; set; }
    public ZidPaymentStatus PaymentStatus { get; set; }
    public ZidFulfillmentStatus FulfillmentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
    public ZidSyncStatus SyncStatus { get; set; }
    public int? SaleInvoiceId { get; set; }
    public DateTime ZidCreatedAt { get; set; }
    public int ItemsCount { get; set; }
}

public class ZidOrderDetailResponse
{
    public Guid Id { get; set; }
    public long ZidOrderId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public ZidOrderStatus Status { get; set; }
    public ZidPaymentStatus PaymentStatus { get; set; }
    public ZidFulfillmentStatus FulfillmentStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? PaymentMethod { get; set; }
    public ZidSyncStatus SyncStatus { get; set; }
    public string? SyncError { get; set; }
    public DateTime? SyncedAt { get; set; }
    public int? SaleInvoiceId { get; set; }
    public DateTime ZidCreatedAt { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? ShippingStreet { get; set; }
    public int ItemsCount { get; set; }
    public List<ZidOrderItemResponse> Items { get; set; } = new();
}

public class ZidOrderItemResponse
{
    public Guid Id { get; set; }
    public long ZidProductId { get; set; }
    public string? ZidSku { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? OptionsSummary { get; set; }
    public Guid? MainPurchaseClassId { get; set; }
}

// ──────────────────── Product Mapping ────────────────────

public class ZidProductMappingResponse
{
    public Guid Id { get; set; }
    public long ZidProductId { get; set; }
    public string? ZidSku { get; set; }
    public string ZidProductName { get; set; } = string.Empty;
    public decimal? ZidPrice { get; set; }
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }
    public bool IsMapped { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

// ──────────────────── Sync Log ────────────────────

public class ZidSyncLogResponse
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public ZidSyncDirection Direction { get; set; }
    public ZidSyncStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public long? ZidResourceId { get; set; }
    public int DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ──────────────────── Settings ────────────────────

public class ZidSettingsResponse
{
    public bool IsConnected { get; set; }
    public string? StoreId { get; set; }
    public string? StoreName { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int SyncedOrders { get; set; }
    public int FailedOrders { get; set; }
    public int TotalProducts { get; set; }
    public int MappedProducts { get; set; }
    public DateTime? LastSyncAt { get; set; }
}

// ──────────────────── Cashier ────────────────────

public class ZidCashierOrderResponse
{
    public Guid Id { get; set; }
    public long ZidOrderId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? PaymentMethod { get; set; }
    public DateTime ZidCreatedAt { get; set; }
    public List<ZidOrderItemResponse> Items { get; set; } = new();
}

// ──────────────────── Stock Sync Result ────────────────────

public class ZidStockSyncResult
{
    public int TotalProducts { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
    public long DurationMs { get; set; }
}

// ──────────────────── Request DTOs ────────────────────

public record ZidConnectRequest(string Code);

public record ZidSetTokenRequest(string AccessToken, string? RefreshToken, long? ExpiresAt);

public record ZidUpdateOrderStatusRequest(ZidOrderStatus Status);

public record ZidSyncOrderRequest(Guid? StoreDetailsId);

public record MapZidProductRequest(long ZidProductId, Guid MainPurchaseClassId);

// ──────────────────── Pagination ────────────────────

public class ZidPaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
