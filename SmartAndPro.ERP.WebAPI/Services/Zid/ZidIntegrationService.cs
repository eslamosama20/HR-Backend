using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Domains.Entities.Zid;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Enums.Zid;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Zid.Dtos;

namespace SmartAndPro.ERP.WebAPI.Services.Zid;

public interface IZidIntegrationService
{
    // OAuth
    string GetOAuthUrl();
    Task<string> ExchangeCodeForTokenAsync(string code);

    // Webhook
    bool VerifyWebhookSignature(string payload, string signature);
    Task HandleWebhookAsync(string eventType, ZidWebhookData data, string rawPayload);

    // Orders
    Task<ZidPaginatedResponse<ZidOrderListResponse>> GetOrdersAsync(int page, int pageSize, ZidOrderStatus? status, ZidSyncStatus? syncStatus);
    Task<ZidOrderDetailResponse?> GetOrderByIdAsync(Guid orderId);
    Task SyncOrderToInvoiceAsync(Guid orderId, Guid? storeDetailsId);
    Task UpdateOrderStatusOnZidAsync(Guid orderId, ZidOrderStatus newStatus);

    // Product Mapping
    Task<ZidPaginatedResponse<ZidProductMappingResponse>> GetProductMappingsAsync(int page, int pageSize, bool? mappedOnly);
    Task MapProductAsync(long zidProductId, Guid mainPurchaseClassId);
    Task BulkMapProductsAsync(List<MapZidProductRequest> mappings);
    Task SyncProductsFromZidAsync();
    Task SyncStockToZidAsync(long zidProductId, int quantity);
    Task<ZidStockSyncResult> SyncAllStockToZidAsync(Guid storeDetailsId);

    // Cashier
    Task<List<ZidCashierOrderResponse>> GetPendingOrdersForCashierAsync(Guid storeDetailsId);

    // Sync Logs
    Task<ZidPaginatedResponse<ZidSyncLogResponse>> GetSyncLogsAsync(int page, int pageSize, string? eventType);

    // Settings
    Task<ZidSettingsResponse> GetSettingsAsync();

    // Manual token (dev/testing)
    Task SetTokenManuallyAsync(string accessToken, string? refreshToken, long? expiresAt);
}

public class ZidIntegrationService : IZidIntegrationService
{
    private readonly ERPContext _db;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZidIntegrationService> _logger;

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _webhookSecret;
    private readonly string _baseUrl;
    private readonly string _oauthUrl;
    private readonly string _redirectUri;
    private readonly string? _defaultStoreDetailsId;

    // In-memory token cache (persisted to DB)
    private static string? _accessToken;
    private static string? _refreshToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;

    public ZidIntegrationService(
        ERPContext db,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ZidIntegrationService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _clientId = configuration["Zid:ClientId"] ?? "";
        _clientSecret = configuration["Zid:ClientSecret"] ?? "";
        _webhookSecret = configuration["Zid:WebhookSecret"] ?? "";
        _baseUrl = configuration["Zid:BaseUrl"] ?? "https://api.zid.sa/v1";
        _oauthUrl = configuration["Zid:OAuthUrl"] ?? "https://oauth.zid.sa/oauth";
        _redirectUri = configuration["Zid:RedirectUri"] ?? "";
        _defaultStoreDetailsId = configuration["Zid:DefaultStoreDetailsId"];
    }

    // ═══════════════════════════ OAuth ═══════════════════════════

    public string GetOAuthUrl()
    {
        return $"{_oauthUrl}/authorize?client_id={_clientId}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&response_type=code&scope=offline_access";
    }

    public async Task<string> ExchangeCodeForTokenAsync(string code)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["code"] = code,
                ["redirect_uri"] = _redirectUri
            };

            var response = await _httpClient.PostAsync(
                $"{_oauthUrl}/token",
                new FormUrlEncodedContent(payload));

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

            _accessToken = tokenData.GetProperty("access_token").GetString();
            _refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = tokenData.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 14400;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            await PersistTokenAsync(_accessToken!, _refreshToken, _tokenExpiry);

            await LogSyncAsync("oauth.token_exchange", ZidSyncDirection.ZidToErp, ZidSyncStatus.Synced,
                null, null, null, null, sw.ElapsedMilliseconds);

            return _accessToken!;
        }
        catch (Exception ex)
        {
            await LogSyncAsync("oauth.token_exchange", ZidSyncDirection.ZidToErp, ZidSyncStatus.Failed,
                null, null, ex.Message, null, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        // Restore from DB if in-memory token is missing
        if (_accessToken == null)
        {
            var stored = await _db.Set<ZidAuthToken>()
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (stored != null)
            {
                _accessToken = stored.AccessToken;
                _refreshToken = stored.RefreshToken;
                _tokenExpiry = stored.ExpiresAt;
                _logger.LogInformation("Zid token restored from DB. Expires at {Expiry}", _tokenExpiry);
            }
        }

        if (_accessToken == null)
            throw new InvalidOperationException("Zid is not connected. Please complete OAuth flow first.");

        if (DateTime.UtcNow >= _tokenExpiry && _refreshToken != null)
        {
            var payload = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["refresh_token"] = _refreshToken
            };

            var response = await _httpClient.PostAsync(
                $"{_oauthUrl}/token",
                new FormUrlEncodedContent(payload));

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

            _accessToken = tokenData.GetProperty("access_token").GetString();
            if (tokenData.TryGetProperty("refresh_token", out var newRt))
                _refreshToken = newRt.GetString();

            var expiresIn = tokenData.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 14400;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            await PersistTokenAsync(_accessToken!, _refreshToken, _tokenExpiry);
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Manager-Token", _accessToken);
    }

    private async Task PersistTokenAsync(string accessToken, string? refreshToken, DateTime expiresAt)
    {
        var old = await _db.Set<ZidAuthToken>().ToListAsync();
        _db.Set<ZidAuthToken>().RemoveRange(old);

        _db.Set<ZidAuthToken>().Add(new ZidAuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IsActive = true
        });

        await _db.SaveChangesAsync();
    }

    public async Task SetTokenManuallyAsync(string accessToken, string? refreshToken, long? expiresAt)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _tokenExpiry = expiresAt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(expiresAt.Value).UtcDateTime
            : DateTime.UtcNow.AddHours(4);

        await PersistTokenAsync(_accessToken, _refreshToken, _tokenExpiry);
        _logger.LogInformation("Zid token set manually. Expires at {Expiry}", _tokenExpiry);
    }

    // ═══════════════════════════ Webhook ═══════════════════════════

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_webhookSecret)) return true;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public async Task HandleWebhookAsync(string eventType, ZidWebhookData data, string rawPayload)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Zid webhook received: {Event}, ResourceId: {Id}", eventType, data.Id);

        try
        {
            switch (eventType)
            {
                case "order.created":
                    await HandleOrderCreatedAsync(data);
                    break;
                case "order.updated":
                    await HandleOrderUpdatedAsync(data);
                    break;
                case "order.cancelled":
                    await HandleOrderCancelledAsync(data.Id);
                    break;
                case "order.refunded":
                    await HandleOrderRefundedAsync(data.Id);
                    break;
                case "product.created":
                case "product.updated":
                    await HandleProductUpsertAsync(data);
                    break;
                case "product.deleted":
                    await HandleProductDeletedAsync(data.Id);
                    break;
                case "product.quantity.updated":
                    _logger.LogInformation("Zid product quantity updated for product {Id}", data.Id);
                    break;
                default:
                    _logger.LogWarning("Unhandled Zid event: {Event}", eventType);
                    break;
            }

            await LogSyncAsync(eventType, ZidSyncDirection.ZidToErp, ZidSyncStatus.Synced,
                rawPayload, null, null, data.Id, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Zid webhook {Event}", eventType);
            await LogSyncAsync(eventType, ZidSyncDirection.ZidToErp, ZidSyncStatus.Failed,
                rawPayload, null, ex.Message, data.Id, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task HandleOrderCreatedAsync(ZidWebhookData data)
    {
        if (await _db.Set<ZidOrder>().AnyAsync(o => o.ZidOrderId == data.Id))
        {
            _logger.LogInformation("Zid order {Id} already exists, skipping", data.Id);
            return;
        }

        var order = new ZidOrder
        {
            ZidOrderId = data.Id,
            ReferenceNumber = data.ReferenceId ?? $"#{data.Id}",
            Status = MapZidStatus(data.Status?.Code),
            PaymentStatus = MapPaymentStatus(data.Payment?.Status),
            CustomerName = data.Customer?.Name ?? "Unknown",
            CustomerEmail = data.Customer?.Email,
            CustomerPhone = data.Customer?.Mobile,
            ZidCustomerId = data.Customer?.Id,
            SubTotal = data.Amounts?.SubTotal ?? 0,
            ShippingAmount = data.Amounts?.Shipping ?? 0,
            TaxAmount = data.Amounts?.Tax ?? 0,
            DiscountAmount = data.Amounts?.Discount ?? 0,
            TotalAmount = data.Amounts?.Total ?? 0,
            Currency = data.Amounts?.Currency ?? "SAR",
            PaymentMethod = data.Payment?.Method,
            ZidPaymentId = data.Payment?.Id,
            ShippingCity = data.ShippingAddress?.City,
            ShippingDistrict = data.ShippingAddress?.District,
            ShippingStreet = data.ShippingAddress?.Street,
            ShippingPostalCode = data.ShippingAddress?.ZipCode,
            ShippingLat = data.ShippingAddress?.Lat,
            ShippingLng = data.ShippingAddress?.Lng,
            ZidCreatedAt = data.Date ?? DateTime.UtcNow,
            SyncStatus = ZidSyncStatus.Pending
        };

        foreach (var item in data.Products)
        {
            var mapping = await _db.Set<ZidProductMapping>()
                .FirstOrDefaultAsync(m => m.ZidProductId == item.Id);

            var options = item.Options != null
                ? string.Join(", ", item.Options.Select(o => $"{o.Name}: {o.Value}"))
                : null;

            order.Items.Add(new ZidOrderItem
            {
                ZidProductId = item.Id,
                ZidSku = item.Sku,
                ProductName = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                TotalPrice = item.Total,
                TaxAmount = item.Tax,
                DiscountAmount = item.Discount,
                OptionsSummary = options,
                MainPurchaseClassId = mapping?.MainPurchaseClassId
            });
        }

        _db.Set<ZidOrder>().Add(order);
        await _db.SaveChangesAsync();
    }

    private async Task HandleOrderUpdatedAsync(ZidWebhookData data)
    {
        var order = await _db.Set<ZidOrder>()
            .FirstOrDefaultAsync(o => o.ZidOrderId == data.Id);

        if (order == null)
        {
            await HandleOrderCreatedAsync(data);
            return;
        }

        order.Status = MapZidStatus(data.Status?.Code);
        order.PaymentStatus = MapPaymentStatus(data.Payment?.Status);
        order.TotalAmount = data.Amounts?.Total ?? order.TotalAmount;
        await _db.SaveChangesAsync();
    }

    private async Task HandleOrderCancelledAsync(long zidOrderId)
    {
        var order = await _db.Set<ZidOrder>()
            .FirstOrDefaultAsync(o => o.ZidOrderId == zidOrderId);

        if (order == null) return;
        order.Status = ZidOrderStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    private async Task HandleOrderRefundedAsync(long zidOrderId)
    {
        var order = await _db.Set<ZidOrder>()
            .FirstOrDefaultAsync(o => o.ZidOrderId == zidOrderId);

        if (order == null) return;
        order.Status = ZidOrderStatus.Refunded;
        order.PaymentStatus = ZidPaymentStatus.Refunded;
        await _db.SaveChangesAsync();
    }

    private async Task HandleProductUpsertAsync(ZidWebhookData data)
    {
        var existing = await _db.Set<ZidProductMapping>()
            .FirstOrDefaultAsync(m => m.ZidProductId == data.Id);

        if (existing != null)
        {
            existing.ZidProductName = data.Name ?? existing.ZidProductName;
            existing.ZidSku = data.Sku ?? existing.ZidSku;
            existing.ZidPrice = data.Price ?? existing.ZidPrice;
            existing.LastSyncedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Set<ZidProductMapping>().Add(new ZidProductMapping
            {
                ZidProductId = data.Id,
                ZidSku = data.Sku,
                ZidProductName = data.Name ?? $"Zid Product #{data.Id}",
                ZidPrice = data.Price,
                LastSyncedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task HandleProductDeletedAsync(long zidProductId)
    {
        var mapping = await _db.Set<ZidProductMapping>()
            .FirstOrDefaultAsync(m => m.ZidProductId == zidProductId);

        if (mapping != null)
        {
            mapping.SoftDelete();
            await _db.SaveChangesAsync();
        }
    }

    // ═══════════════════════════ Orders ═══════════════════════════

    public async Task<ZidPaginatedResponse<ZidOrderListResponse>> GetOrdersAsync(
        int page, int pageSize, ZidOrderStatus? status, ZidSyncStatus? syncStatus)
    {
        var query = _db.Set<ZidOrder>()
            .Include(o => o.Items)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (syncStatus.HasValue) query = query.Where(o => o.SyncStatus == syncStatus.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.ZidCreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new ZidOrderListResponse
            {
                Id = o.Id,
                ZidOrderId = o.ZidOrderId,
                ReferenceNumber = o.ReferenceNumber,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                FulfillmentStatus = o.FulfillmentStatus,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                SyncStatus = o.SyncStatus,
                SaleInvoiceId = o.SaleInvoiceId,
                ZidCreatedAt = o.ZidCreatedAt,
                ItemsCount = o.Items.Count
            })
            .ToListAsync();

        return new ZidPaginatedResponse<ZidOrderListResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ZidOrderDetailResponse?> GetOrderByIdAsync(Guid orderId)
    {
        return await _db.Set<ZidOrder>()
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new ZidOrderDetailResponse
            {
                Id = o.Id,
                ZidOrderId = o.ZidOrderId,
                ReferenceNumber = o.ReferenceNumber,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                CustomerEmail = o.CustomerEmail,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                FulfillmentStatus = o.FulfillmentStatus,
                SubTotal = o.SubTotal,
                ShippingAmount = o.ShippingAmount,
                TaxAmount = o.TaxAmount,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                PaymentMethod = o.PaymentMethod,
                SyncStatus = o.SyncStatus,
                SyncError = o.SyncError,
                SyncedAt = o.SyncedAt,
                SaleInvoiceId = o.SaleInvoiceId,
                ZidCreatedAt = o.ZidCreatedAt,
                ShippingCity = o.ShippingCity,
                ShippingDistrict = o.ShippingDistrict,
                ShippingStreet = o.ShippingStreet,
                ItemsCount = o.Items.Count,
                Items = o.Items.Select(i => new ZidOrderItemResponse
                {
                    Id = i.Id,
                    ZidProductId = i.ZidProductId,
                    ZidSku = i.ZidSku,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    TaxAmount = i.TaxAmount,
                    DiscountAmount = i.DiscountAmount,
                    OptionsSummary = i.OptionsSummary,
                    MainPurchaseClassId = i.MainPurchaseClassId
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task SyncOrderToInvoiceAsync(Guid orderId, Guid? storeDetailsId)
    {
        var order = await _db.Set<ZidOrder>()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Zid order {orderId} not found");

        if (order.SaleInvoiceId.HasValue)
            throw new InvalidOperationException("Order already synced to invoice");

        var unmapped = order.Items.Where(i => !i.MainPurchaseClassId.HasValue).ToList();
        if (unmapped.Any())
        {
            var names = string.Join(", ", unmapped.Select(u => u.ProductName));
            throw new InvalidOperationException(
                $"Cannot sync: unmapped products: {names}. Map them first via product mapping.");
        }

        var store = storeDetailsId
            ?? (Guid.TryParse(_defaultStoreDetailsId, out var parsed) ? parsed : (Guid?)null);

        if (!store.HasValue)
            throw new InvalidOperationException("StoreDetailsId is required.");

        order.StoreDetailsId = store;
        order.SyncStatus = ZidSyncStatus.Synced;
        order.SyncedAt = DateTime.UtcNow;
        order.SyncError = null;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Zid order {OrderId} synced for invoice creation, store={StoreId}",
            orderId, store);
    }

    public async Task UpdateOrderStatusOnZidAsync(Guid orderId, ZidOrderStatus newStatus)
    {
        var order = await _db.Set<ZidOrder>().FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Zid order {orderId} not found");

        await EnsureAuthenticatedAsync();

        var zidStatus = newStatus switch
        {
            ZidOrderStatus.InProgress => "in_progress",
            ZidOrderStatus.Completed => "completed",
            ZidOrderStatus.Cancelled => "cancelled",
            ZidOrderStatus.Shipped => "shipped",
            ZidOrderStatus.Delivered => "delivered",
            _ => throw new ArgumentException($"Cannot push status {newStatus} to Zid")
        };

        var body = new { status = new { code = zidStatus } };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync(
            $"{_baseUrl}/orders/{order.ZidOrderId}", content);

        response.EnsureSuccessStatusCode();
        order.Status = newStatus;
        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════ Product Mapping ═══════════════════════════

    public async Task<ZidPaginatedResponse<ZidProductMappingResponse>> GetProductMappingsAsync(
        int page, int pageSize, bool? mappedOnly)
    {
        var query = _db.Set<ZidProductMapping>().AsNoTracking().AsQueryable();

        if (mappedOnly == true) query = query.Where(m => m.MainPurchaseClassId != null);
        if (mappedOnly == false) query = query.Where(m => m.MainPurchaseClassId == null);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ZidProductMappingResponse
            {
                Id = m.Id,
                ZidProductId = m.ZidProductId,
                ZidSku = m.ZidSku,
                ZidProductName = m.ZidProductName,
                ZidPrice = m.ZidPrice,
                MainPurchaseClassId = m.MainPurchaseClassId,
                MainPurchaseClassName = m.MainPurchaseClassName,
                IsMapped = m.MainPurchaseClassId != null,
                LastSyncedAt = m.LastSyncedAt
            })
            .ToListAsync();

        return new ZidPaginatedResponse<ZidProductMappingResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task MapProductAsync(long zidProductId, Guid mainPurchaseClassId)
    {
        var mapping = await _db.Set<ZidProductMapping>()
            .FirstOrDefaultAsync(m => m.ZidProductId == zidProductId)
            ?? throw new KeyNotFoundException($"Zid product {zidProductId} not found. Sync products first.");

        mapping.MainPurchaseClassId = mainPurchaseClassId;
        mapping.LastSyncedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task BulkMapProductsAsync(List<MapZidProductRequest> mappings)
    {
        var zidIds = mappings.Select(m => m.ZidProductId).ToList();
        var existing = await _db.Set<ZidProductMapping>()
            .Where(m => zidIds.Contains(m.ZidProductId))
            .ToListAsync();

        foreach (var req in mappings)
        {
            var mapping = existing.FirstOrDefault(m => m.ZidProductId == req.ZidProductId);
            if (mapping != null)
            {
                mapping.MainPurchaseClassId = req.MainPurchaseClassId;
                mapping.LastSyncedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task SyncProductsFromZidAsync()
    {
        var sw = Stopwatch.StartNew();
        await EnsureAuthenticatedAsync();

        int page = 1;
        int synced = 0;

        try
        {
            while (true)
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/products?page={page}&per_page=50");

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Zid products API returned {StatusCode}: {Body}",
                        (int)response.StatusCode, errorBody);
                    response.EnsureSuccessStatusCode();
                }

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Zid wraps products in "products" or "data" key
                JsonElement dataArray;
                if (!root.TryGetProperty("products", out dataArray) &&
                    !root.TryGetProperty("data", out dataArray))
                    break;

                if (dataArray.GetArrayLength() == 0) break;

                foreach (var product in dataArray.EnumerateArray())
                {
                    var productId = product.GetProperty("id").GetInt64();
                    var name = product.TryGetProperty("name", out var n) ? n.GetString() : $"Product #{productId}";
                    var sku = product.TryGetProperty("sku", out var s) ? s.GetString() : null;
                    decimal? price = null;
                    if (product.TryGetProperty("price", out var p))
                    {
                        if (p.ValueKind == JsonValueKind.Number)
                            price = p.GetDecimal();
                        else if (p.ValueKind == JsonValueKind.Object &&
                                 p.TryGetProperty("amount", out var amt))
                            price = amt.GetDecimal();
                    }

                    var existing = await _db.Set<ZidProductMapping>()
                        .FirstOrDefaultAsync(m => m.ZidProductId == productId);

                    if (existing != null)
                    {
                        existing.ZidProductName = name ?? existing.ZidProductName;
                        existing.ZidSku = sku ?? existing.ZidSku;
                        existing.ZidPrice = price ?? existing.ZidPrice;
                        existing.LastSyncedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _db.Set<ZidProductMapping>().Add(new ZidProductMapping
                        {
                            ZidProductId = productId,
                            ZidSku = sku,
                            ZidProductName = name ?? $"Product #{productId}",
                            ZidPrice = price,
                            LastSyncedAt = DateTime.UtcNow
                        });
                    }
                    synced++;
                }

                await _db.SaveChangesAsync();

                // Pagination: check if more pages exist
                bool hasMore = false;
                if (root.TryGetProperty("pagination", out var pag))
                {
                    if (pag.TryGetProperty("current_page", out var cp) &&
                        pag.TryGetProperty("last_page", out var lp))
                        hasMore = cp.GetInt32() < lp.GetInt32();
                }
                else if (root.TryGetProperty("meta", out var meta) &&
                         meta.TryGetProperty("current_page", out var mcp) &&
                         meta.TryGetProperty("last_page", out var mlp))
                {
                    hasMore = mcp.GetInt32() < mlp.GetInt32();
                }

                if (!hasMore) break;
                page++;
            }

            await LogSyncAsync("products.sync", ZidSyncDirection.ZidToErp, ZidSyncStatus.Synced,
                null, $"Synced {synced} products", null, null, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            await LogSyncAsync("products.sync", ZidSyncDirection.ZidToErp, ZidSyncStatus.Failed,
                null, null, ex.Message, null, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task SyncStockToZidAsync(long zidProductId, int quantity)
    {
        await EnsureAuthenticatedAsync();

        var body = new { quantity };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync(
            $"{_baseUrl}/products/{zidProductId}/quantity", content);

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Pushed stock {Qty} to Zid product {Id}", quantity, zidProductId);
    }

    public async Task<ZidStockSyncResult> SyncAllStockToZidAsync(Guid storeDetailsId)
    {
        var sw = Stopwatch.StartNew();
        var result = new ZidStockSyncResult();

        var mappings = await _db.Set<ZidProductMapping>()
            .Where(m => m.MainPurchaseClassId != null && m.StoreDetailsId == storeDetailsId)
            .AsNoTracking()
            .ToListAsync();

        result.TotalProducts = mappings.Count;

        var mainPurchaseClassIds = mappings.Select(m => m.MainPurchaseClassId!.Value).ToList();
        var storeQuantities = await _db.Set<ProductUnit>()
            .Where(pu => pu.StoreDetailsId == storeDetailsId &&
                         mainPurchaseClassIds.Contains(pu.MainPurchaseClassId!.Value))
            .ToDictionaryAsync(pu => pu.MainPurchaseClassId!.Value, pu => (int)(pu.Quantity ?? 0));

        foreach (var mapping in mappings)
        {
            try
            {
                var stock = storeQuantities.TryGetValue(mapping.MainPurchaseClassId!.Value, out var q) ? q : 0;

                await SyncStockToZidAsync(mapping.ZidProductId, stock);

                mapping.StoreDetailsId = storeDetailsId;
                mapping.LastSyncedAt = DateTime.UtcNow;
                result.Succeeded++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Product {mapping.ZidProductId} ({mapping.ZidProductName}): {ex.Message}");
                _logger.LogError(ex, "Failed to sync stock for Zid product {Id}", mapping.ZidProductId);
            }
        }

        await _db.SaveChangesAsync();
        result.DurationMs = sw.ElapsedMilliseconds;

        await LogSyncAsync("stock.sync_all", ZidSyncDirection.ErpToZid,
            result.Failed == 0 ? ZidSyncStatus.Synced : ZidSyncStatus.Failed,
            null, $"Synced {result.Succeeded}/{result.TotalProducts}", null, null, sw.ElapsedMilliseconds);

        return result;
    }

    // ═══════════════════════════ Cashier ═══════════════════════════

    public async Task<List<ZidCashierOrderResponse>> GetPendingOrdersForCashierAsync(Guid storeDetailsId)
    {
        return await _db.Set<ZidOrder>()
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o =>
                o.SyncStatus == ZidSyncStatus.Pending &&
                o.Status == ZidOrderStatus.New &&
                (o.StoreDetailsId == storeDetailsId || o.StoreDetailsId == null))
            .OrderByDescending(o => o.ZidCreatedAt)
            .Select(o => new ZidCashierOrderResponse
            {
                Id = o.Id,
                ZidOrderId = o.ZidOrderId,
                ReferenceNumber = o.ReferenceNumber,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                PaymentMethod = o.PaymentMethod,
                ZidCreatedAt = o.ZidCreatedAt,
                Items = o.Items.Select(i => new ZidOrderItemResponse
                {
                    Id = i.Id,
                    ZidProductId = i.ZidProductId,
                    ZidSku = i.ZidSku,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();
    }

    // ═══════════════════════════ Sync Logs ═══════════════════════════

    public async Task<ZidPaginatedResponse<ZidSyncLogResponse>> GetSyncLogsAsync(
        int page, int pageSize, string? eventType)
    {
        var query = _db.Set<ZidSyncLog>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType == eventType);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ZidSyncLogResponse
            {
                Id = l.Id,
                EventType = l.EventType,
                Direction = l.Direction,
                Status = l.Status,
                ErrorMessage = l.ErrorMessage,
                ZidResourceId = l.ZidResourceId,
                DurationMs = l.DurationMs,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new ZidPaginatedResponse<ZidSyncLogResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // ═══════════════════════════ Settings ═══════════════════════════

    public async Task<ZidSettingsResponse> GetSettingsAsync()
    {
        var isConnected = _accessToken != null || await _db.Set<ZidAuthToken>().AnyAsync();

        ZidAuthToken? token = null;
        if (isConnected)
            token = await _db.Set<ZidAuthToken>()
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

        var totalOrders = await _db.Set<ZidOrder>().CountAsync();
        var pendingOrders = await _db.Set<ZidOrder>()
            .CountAsync(o => o.SyncStatus == ZidSyncStatus.Pending);
        var syncedOrders = await _db.Set<ZidOrder>()
            .CountAsync(o => o.SyncStatus == ZidSyncStatus.Synced);
        var failedOrders = await _db.Set<ZidOrder>()
            .CountAsync(o => o.SyncStatus == ZidSyncStatus.Failed);
        var totalProducts = await _db.Set<ZidProductMapping>().CountAsync();
        var mappedProducts = await _db.Set<ZidProductMapping>()
            .CountAsync(m => m.MainPurchaseClassId != null);
        var lastSync = await _db.Set<ZidSyncLog>()
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => (DateTime?)l.CreatedAt)
            .FirstOrDefaultAsync();

        return new ZidSettingsResponse
        {
            IsConnected = isConnected,
            TokenExpiresAt = token?.ExpiresAt,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            SyncedOrders = syncedOrders,
            FailedOrders = failedOrders,
            TotalProducts = totalProducts,
            MappedProducts = mappedProducts,
            LastSyncAt = lastSync
        };
    }

    // ═══════════════════════════ Helpers ═══════════════════════════

    private static ZidOrderStatus MapZidStatus(string? statusCode) => statusCode switch
    {
        "new" => ZidOrderStatus.New,
        "in_progress" => ZidOrderStatus.InProgress,
        "completed" => ZidOrderStatus.Completed,
        "cancelled" => ZidOrderStatus.Cancelled,
        "refunded" => ZidOrderStatus.Refunded,
        "ready_for_shipping" => ZidOrderStatus.ReadyForShipping,
        "shipped" => ZidOrderStatus.Shipped,
        "delivered" => ZidOrderStatus.Delivered,
        _ => ZidOrderStatus.New
    };

    private static ZidPaymentStatus MapPaymentStatus(string? status) => status switch
    {
        "paid" => ZidPaymentStatus.Paid,
        "failed" => ZidPaymentStatus.Failed,
        "refunded" => ZidPaymentStatus.Refunded,
        "partially_refunded" => ZidPaymentStatus.PartiallyRefunded,
        _ => ZidPaymentStatus.Pending
    };

    private async Task LogSyncAsync(
        string eventType,
        ZidSyncDirection direction,
        ZidSyncStatus status,
        string? requestPayload,
        string? responsePayload,
        string? errorMessage,
        long? resourceId,
        long durationMs)
    {
        try
        {
            _db.Set<ZidSyncLog>().Add(new ZidSyncLog
            {
                EventType = eventType,
                Direction = direction,
                Status = status,
                RequestPayload = requestPayload?.Length > 4000 ? requestPayload[..4000] : requestPayload,
                ResponsePayload = responsePayload?.Length > 2000 ? responsePayload[..2000] : responsePayload,
                ErrorMessage = errorMessage?.Length > 2000 ? errorMessage[..2000] : errorMessage,
                ZidResourceId = resourceId,
                DurationMs = (int)Math.Min(durationMs, int.MaxValue),
                IsActive = true
            });

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write Zid sync log for event {Event}", eventType);
        }
    }
}
