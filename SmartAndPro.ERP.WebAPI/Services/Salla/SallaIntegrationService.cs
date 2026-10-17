using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Domains.Entities.Salla;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Enums.Salla;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Salla.Dtos;

namespace SmartAndPro.ERP.WebAPI.Services.Salla;

public interface ISallaIntegrationService
{
    // OAuth
    string GetOAuthUrl();
    Task<string> ExchangeCodeForTokenAsync(string code);

    // Webhook
    bool VerifyWebhookSignature(string payload, string signature);
    Task HandleWebhookAsync(string eventType, SallaWebhookData data, string rawPayload);

    // Orders
    Task<SallaPaginatedResponse<SallaOrderListResponse>> GetOrdersAsync(int page, int pageSize, SallaOrderStatus? status, SallaSyncStatus? syncStatus);
    Task<SallaOrderDetailResponse?> GetOrderByIdAsync(Guid orderId);
    Task SyncOrderToInvoiceAsync(Guid orderId, Guid? storeDetailsId);
    Task UpdateOrderStatusOnSallaAsync(Guid orderId, SallaOrderStatus newStatus);

    // Product Mapping
    Task<SallaPaginatedResponse<SallaProductMappingResponse>> GetProductMappingsAsync(int page, int pageSize, bool? mappedOnly);
    Task MapProductAsync(long sallaProductId, Guid mainPurchaseClassId);
    Task BulkMapProductsAsync(List<MapProductRequest> mappings);
    Task SyncProductsFromSallaAsync();
    Task SyncStockToSallaAsync(long sallaProductId, int quantity);

    /// <summary>
    /// Sync ALL mapped products' stock from a specific ERP store to Salla.
    /// Called when a cashier session opens or manually from admin.
    /// </summary>
    Task<SallaStockSyncResult> SyncAllStockToSallaAsync(Guid storeDetailsId);

    /// <summary>
    /// Get pending Salla orders for the cashier screen (filtered by store).
    /// </summary>
    Task<List<SallaCashierOrderResponse>> GetPendingOrdersForCashierAsync(Guid storeDetailsId);

    // Sync Logs
    Task<SallaPaginatedResponse<SallaSyncLogResponse>> GetSyncLogsAsync(int page, int pageSize, string? eventType);

    // Settings / Dashboard
    Task<SallaSettingsResponse> GetSettingsAsync();

    // Manual token (dev)
    Task SetTokenManuallyAsync(string accessToken, string? refreshToken, long? expires);
}

public class SallaIntegrationService : ISallaIntegrationService
{
    private readonly ERPContext _db;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SallaIntegrationService> _logger;

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _webhookSecret;
    private readonly string _baseUrl;
    private readonly string _oauthUrl;
    private readonly string _redirectUri;
    private readonly string? _defaultStoreDetailsId;

    // In production, store tokens in DB or secure vault
    private static string? _accessToken;
    private static string? _refreshToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;

    public SallaIntegrationService(
        ERPContext db,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SallaIntegrationService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _clientId = configuration["Salla:ClientId"] ?? "";
        _clientSecret = configuration["Salla:ClientSecret"] ?? "";
        _webhookSecret = configuration["Salla:WebhookSecret"] ?? "";
        _baseUrl = configuration["Salla:BaseUrl"] ?? "https://api.salla.dev/admin/v2";
        _oauthUrl = configuration["Salla:OAuthUrl"] ?? "https://accounts.salla.sa/oauth2";
        _redirectUri = configuration["Salla:RedirectUri"] ?? "";
        _defaultStoreDetailsId = configuration["Salla:DefaultStoreDetailsId"];
    }

    // ═══════════════════════════ OAuth ═══════════════════════════

    public string GetOAuthUrl()
    {
        return $"{_oauthUrl}/auth?client_id={_clientId}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&response_type=code&scope=offline_access";
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
            _refreshToken = tokenData.GetProperty("refresh_token").GetString();
            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            await PersistTokenAsync(_accessToken!, _refreshToken, _tokenExpiry);

            await LogSyncAsync("oauth.token_exchange", SallaSyncDirection.SallaToErp, SallaSyncStatus.Synced,
                null, null, null, null, sw.ElapsedMilliseconds);

            return _accessToken!;
        }
        catch (Exception ex)
        {
            await LogSyncAsync("oauth.token_exchange", SallaSyncDirection.SallaToErp, SallaSyncStatus.Failed,
                null, null, ex.Message, null, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        // If in-memory token is missing, try to restore from database
        if (_accessToken == null)
        {
            var stored = await _db.Set<SallaAuthToken>()
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (stored != null)
            {
                _accessToken = stored.AccessToken;
                _refreshToken = stored.RefreshToken;
                _tokenExpiry = stored.ExpiresAt;
                _logger.LogInformation("Salla token restored from DB. Expires at {Expiry}, Token starts with: {TokenStart}",
                    _tokenExpiry, _accessToken?[..Math.Min(20, _accessToken.Length)]);
            }
        }

        if (_accessToken == null)
            throw new InvalidOperationException("Salla is not connected. Please complete OAuth flow first.");

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
            if (tokenData.TryGetProperty("refresh_token", out var rt))
                _refreshToken = rt.GetString();

            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            await PersistTokenAsync(_accessToken!, _refreshToken, _tokenExpiry);
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private async Task PersistTokenAsync(string accessToken, string? refreshToken, DateTime expiresAt)
    {
        // Keep only the latest token record — remove old ones first
        var old = await _db.Set<SallaAuthToken>().ToListAsync();
        _db.Set<SallaAuthToken>().RemoveRange(old);

        _db.Set<SallaAuthToken>().Add(new SallaAuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IsActive = true
        });

        await _db.SaveChangesAsync();
    }

    public async Task SetTokenManuallyAsync(string accessToken, string? refreshToken, long? expires)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _tokenExpiry = expires.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(expires.Value).UtcDateTime
            : DateTime.UtcNow.AddHours(4);

        await PersistTokenAsync(_accessToken, _refreshToken, _tokenExpiry);
        _logger.LogInformation("Salla token set manually. Expires at {Expiry}", _tokenExpiry);
    }

    // ═══════════════════════════ Webhook ═══════════════════════════

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_webhookSecret)) return true; // skip if not configured

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public async Task HandleWebhookAsync(string eventType, SallaWebhookData data, string rawPayload)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Salla webhook received: {Event}, ResourceId: {Id}", eventType, data.Id);

        try
        {
            switch (eventType)
            {
                case "app.store.authorize":
                    await HandleAppStoreAuthorizeAsync(data);
                    break;
                case "app.installed":
                    _logger.LogInformation("Salla app installed by merchant");
                    break;
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
                default:
                    _logger.LogWarning("Unhandled Salla event: {Event}", eventType);
                    break;
            }

            await LogSyncAsync(eventType, SallaSyncDirection.SallaToErp, SallaSyncStatus.Synced,
                rawPayload, null, null, data.Id, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Salla webhook {Event}", eventType);
            await LogSyncAsync(eventType, SallaSyncDirection.SallaToErp, SallaSyncStatus.Failed,
                rawPayload, null, ex.Message, data.Id, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task HandleAppStoreAuthorizeAsync(SallaWebhookData data)
    {
        if (string.IsNullOrEmpty(data.AccessToken))
        {
            _logger.LogWarning("app.store.authorize received but no access_token found");
            return;
        }

        _accessToken = data.AccessToken;
        _refreshToken = data.RefreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds((data.Expires ?? 14400) - 60);

        await PersistTokenAsync(_accessToken, _refreshToken, _tokenExpiry);

        _logger.LogInformation("Salla OAuth token received via Easy Mode (app.store.authorize). Token expires at {Expiry}", _tokenExpiry);
    }

    private async Task HandleOrderCreatedAsync(SallaWebhookData data)
    {
        // Idempotency — skip if already imported
        if (await _db.Set<SallaOrder>().AnyAsync(o => o.SallaOrderId == data.Id))
        {
            _logger.LogInformation("Salla order {Id} already exists, skipping", data.Id);
            return;
        }

        var order = new SallaOrder
        {
            SallaOrderId = data.Id,
            ReferenceNumber = data.ReferenceId ?? $"#{data.Id}",
            Status = MapSallaStatus(data.Status?.Slug),
            PaymentStatus = MapPaymentStatus(data.Payment?.Status),
            CustomerName = $"{data.Customer?.FirstName} {data.Customer?.LastName}".Trim(),
            CustomerEmail = data.Customer?.Email,
            CustomerPhone = data.Customer?.Mobile,
            SallaCustomerId = data.Customer?.Id,
            SubTotal = data.Amounts?.SubTotal?.Amount ?? 0,
            ShippingAmount = data.Amounts?.Shipping?.Amount ?? 0,
            TaxAmount = data.Amounts?.Tax?.Amount ?? 0,
            DiscountAmount = data.Amounts?.Discounts?.Amount ?? 0,
            TotalAmount = data.Amounts?.Total?.Amount ?? 0,
            Currency = data.Amounts?.Total?.Currency ?? "SAR",
            PaymentMethod = data.Payment?.Method,
            SallaPaymentId = data.Payment?.Id,
            ShippingCity = data.Shipping?.Address?.City,
            ShippingDistrict = data.Shipping?.Address?.District,
            ShippingStreet = data.Shipping?.Address?.Street,
            ShippingPostalCode = data.Shipping?.Address?.PostalCode,
            ShippingLat = data.Shipping?.Address?.Lat,
            ShippingLng = data.Shipping?.Address?.Lng,
            SallaCreatedAt = data.Date ?? DateTime.UtcNow,
            SyncStatus = SallaSyncStatus.Pending
        };

        // Map items + auto-link to ERP products
        foreach (var item in data.Items)
        {
            var mapping = await _db.Set<SallaProductMapping>()
                .FirstOrDefaultAsync(m => m.SallaProductId == item.Id);

            var options = item.Options != null
                ? string.Join(", ", item.Options.Select(o => $"{o.Name}: {o.Value}"))
                : null;

            order.Items.Add(new SallaOrderItem
            {
                SallaProductId = item.Id,
                SallaSku = item.Sku,
                ProductName = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.Price?.Amount ?? 0,
                TotalPrice = item.Total?.Amount ?? 0,
                TaxAmount = item.Tax?.Amount ?? 0,
                DiscountAmount = item.Discount?.Amount ?? 0,
                OptionsSummary = options,
                MainPurchaseClassId = mapping?.MainPurchaseClassId
            });
        }

        _db.Set<SallaOrder>().Add(order);
        await _db.SaveChangesAsync();
    }

    private async Task HandleOrderUpdatedAsync(SallaWebhookData data)
    {
        var order = await _db.Set<SallaOrder>()
            .FirstOrDefaultAsync(o => o.SallaOrderId == data.Id);

        if (order == null)
        {
            await HandleOrderCreatedAsync(data);
            return;
        }

        order.Status = MapSallaStatus(data.Status?.Slug);
        order.PaymentStatus = MapPaymentStatus(data.Payment?.Status);
        order.TotalAmount = data.Amounts?.Total?.Amount ?? order.TotalAmount;
        await _db.SaveChangesAsync();
    }

    private async Task HandleOrderCancelledAsync(long sallaOrderId)
    {
        var order = await _db.Set<SallaOrder>()
            .FirstOrDefaultAsync(o => o.SallaOrderId == sallaOrderId);

        if (order == null) return;
        order.Status = SallaOrderStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    private async Task HandleOrderRefundedAsync(long sallaOrderId)
    {
        var order = await _db.Set<SallaOrder>()
            .FirstOrDefaultAsync(o => o.SallaOrderId == sallaOrderId);

        if (order == null) return;
        order.Status = SallaOrderStatus.Refunded;
        order.PaymentStatus = SallaPaymentStatus.Refunded;
        await _db.SaveChangesAsync();
    }

    private async Task HandleProductUpsertAsync(SallaWebhookData data)
    {
        var existing = await _db.Set<SallaProductMapping>()
            .FirstOrDefaultAsync(m => m.SallaProductId == data.Id);

        if (existing != null)
        {
            existing.SallaProductName = data.Name ?? existing.SallaProductName;
            existing.SallaSku = data.Sku ?? existing.SallaSku;
            existing.SallaPrice = data.Price?.Amount ?? existing.SallaPrice;
            existing.LastSyncedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Set<SallaProductMapping>().Add(new SallaProductMapping
            {
                SallaProductId = data.Id,
                SallaSku = data.Sku,
                SallaProductName = data.Name ?? $"Salla Product #{data.Id}",
                SallaPrice = data.Price?.Amount,
                LastSyncedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task HandleProductDeletedAsync(long sallaProductId)
    {
        var mapping = await _db.Set<SallaProductMapping>()
            .FirstOrDefaultAsync(m => m.SallaProductId == sallaProductId);

        if (mapping != null)
        {
            mapping.SoftDelete();
            await _db.SaveChangesAsync();
        }
    }

    // ═══════════════════════════ Orders ═══════════════════════════

    public async Task<SallaPaginatedResponse<SallaOrderListResponse>> GetOrdersAsync(
        int page, int pageSize, SallaOrderStatus? status, SallaSyncStatus? syncStatus)
    {
        var query = _db.Set<SallaOrder>()
            .Include(o => o.Items)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (syncStatus.HasValue) query = query.Where(o => o.SyncStatus == syncStatus.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.SallaCreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new SallaOrderListResponse
            {
                Id = o.Id,
                SallaOrderId = o.SallaOrderId,
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
                SallaCreatedAt = o.SallaCreatedAt,
                ItemsCount = o.Items.Count
            })
            .ToListAsync();

        return new SallaPaginatedResponse<SallaOrderListResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SallaOrderDetailResponse?> GetOrderByIdAsync(Guid orderId)
    {
        return await _db.Set<SallaOrder>()
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new SallaOrderDetailResponse
            {
                Id = o.Id,
                SallaOrderId = o.SallaOrderId,
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
                SallaCreatedAt = o.SallaCreatedAt,
                ShippingCity = o.ShippingCity,
                ShippingDistrict = o.ShippingDistrict,
                ShippingStreet = o.ShippingStreet,
                ItemsCount = o.Items.Count,
                Items = o.Items.Select(i => new SallaOrderItemResponse
                {
                    Id = i.Id,
                    SallaProductId = i.SallaProductId,
                    SallaSku = i.SallaSku,
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
        var order = await _db.Set<SallaOrder>()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new KeyNotFoundException($"Salla order {orderId} not found");

        if (order.SaleInvoiceId.HasValue)
            throw new InvalidOperationException("Order already synced to invoice");

        // Validate all items are mapped
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
            throw new InvalidOperationException("StoreDetailsId is required. Provide it or set DefaultStoreDetailsId in config.");

        // Mark synced — actual SaleInvoice creation should be done by the existing
        // sales invoice service. This sets all data needed for the frontend/service to pick up.
        order.StoreDetailsId = store;
        order.SyncStatus = SallaSyncStatus.Synced;
        order.SyncedAt = DateTime.UtcNow;
        order.SyncError = null;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Salla order {OrderId} synced for invoice creation, store={StoreId}",
            orderId, store);
    }

    public async Task UpdateOrderStatusOnSallaAsync(Guid orderId, SallaOrderStatus newStatus)
    {
        var order = await _db.Set<SallaOrder>().FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Salla order {orderId} not found");

        await EnsureAuthenticatedAsync();

        var sallaStatus = newStatus switch
        {
            SallaOrderStatus.InProgress => "in_progress",
            SallaOrderStatus.Completed => "completed",
            SallaOrderStatus.Cancelled => "cancelled",
            _ => throw new ArgumentException($"Cannot push status {newStatus} to Salla")
        };

        var body = new { status = new { id = sallaStatus } };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync(
            $"{_baseUrl}/orders/{order.SallaOrderId}/status", content);

        response.EnsureSuccessStatusCode();
        order.Status = newStatus;
        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════ Product Mapping ═══════════════════════════

    public async Task<SallaPaginatedResponse<SallaProductMappingResponse>> GetProductMappingsAsync(
        int page, int pageSize, bool? mappedOnly)
    {
        var query = _db.Set<SallaProductMapping>().AsNoTracking().AsQueryable();

        if (mappedOnly == true) query = query.Where(m => m.MainPurchaseClassId != null);
        if (mappedOnly == false) query = query.Where(m => m.MainPurchaseClassId == null);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new SallaProductMappingResponse
            {
                Id = m.Id,
                SallaProductId = m.SallaProductId,
                SallaSku = m.SallaSku,
                SallaProductName = m.SallaProductName,
                SallaPrice = m.SallaPrice,
                MainPurchaseClassId = m.MainPurchaseClassId,
                MainPurchaseClassName = m.MainPurchaseClassName,
                IsMapped = m.MainPurchaseClassId != null,
                LastSyncedAt = m.LastSyncedAt
            })
            .ToListAsync();

        return new SallaPaginatedResponse<SallaProductMappingResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task MapProductAsync(long sallaProductId, Guid mainPurchaseClassId)
    {
        var mapping = await _db.Set<SallaProductMapping>()
            .FirstOrDefaultAsync(m => m.SallaProductId == sallaProductId)
            ?? throw new KeyNotFoundException($"Salla product {sallaProductId} not found in mappings. Sync products first.");

        mapping.MainPurchaseClassId = mainPurchaseClassId;
        mapping.LastSyncedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task BulkMapProductsAsync(List<MapProductRequest> mappings)
    {
        var sallaIds = mappings.Select(m => m.SallaProductId).ToList();
        var existing = await _db.Set<SallaProductMapping>()
            .Where(m => sallaIds.Contains(m.SallaProductId))
            .ToListAsync();

        foreach (var req in mappings)
        {
            var mapping = existing.FirstOrDefault(m => m.SallaProductId == req.SallaProductId);
            if (mapping != null)
            {
                mapping.MainPurchaseClassId = req.MainPurchaseClassId;
                mapping.LastSyncedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task SyncProductsFromSallaAsync()
    {
        var sw = Stopwatch.StartNew();
        await EnsureAuthenticatedAsync();

        int page = 1;
        int synced = 0;

        try
        {
            while (true)
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/products?page={page}&per_page=50");

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Salla products API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
                    response.EnsureSuccessStatusCode();
                }

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("data", out var dataArray) || dataArray.GetArrayLength() == 0)
                    break;

                foreach (var product in dataArray.EnumerateArray())
                {
                    var productId = product.GetProperty("id").GetInt64();
                    var name = product.TryGetProperty("name", out var n) ? n.GetString() : $"Product #{productId}";
                    var sku = product.TryGetProperty("sku", out var s) ? s.GetString() : null;
                    decimal? price = null;
                    if (product.TryGetProperty("price", out var p) && p.TryGetProperty("amount", out var amt))
                        price = amt.GetDecimal();

                    var existing = await _db.Set<SallaProductMapping>()
                        .FirstOrDefaultAsync(m => m.SallaProductId == productId);

                    if (existing != null)
                    {
                        existing.SallaProductName = name ?? existing.SallaProductName;
                        existing.SallaSku = sku ?? existing.SallaSku;
                        existing.SallaPrice = price ?? existing.SallaPrice;
                        existing.LastSyncedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _db.Set<SallaProductMapping>().Add(new SallaProductMapping
                        {
                            SallaProductId = productId,
                            SallaSku = sku,
                            SallaProductName = name ?? $"Product #{productId}",
                            SallaPrice = price,
                            LastSyncedAt = DateTime.UtcNow
                        });
                    }
                    synced++;
                }

                await _db.SaveChangesAsync();

                // Pagination
                if (root.TryGetProperty("pagination", out var pag) &&
                    pag.TryGetProperty("currentPage", out var cp) &&
                    pag.TryGetProperty("totalPages", out var tp) &&
                    cp.GetInt32() >= tp.GetInt32())
                    break;

                page++;
            }

            await LogSyncAsync("products.sync", SallaSyncDirection.SallaToErp, SallaSyncStatus.Synced,
                null, $"Synced {synced} products", null, null, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            await LogSyncAsync("products.sync", SallaSyncDirection.SallaToErp, SallaSyncStatus.Failed,
                null, null, ex.Message, null, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task SyncStockToSallaAsync(long sallaProductId, int quantity)
    {
        await EnsureAuthenticatedAsync();

        var body = new { quantity = new { amount = quantity } };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync(
            $"{_baseUrl}/products/{sallaProductId}/quantity", content);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Pushed stock {Qty} to Salla product {Id}", quantity, sallaProductId);
    }

    // ═══════════════════════════ Sync Logs ═══════════════════════════

    public async Task<SallaPaginatedResponse<SallaSyncLogResponse>> GetSyncLogsAsync(
        int page, int pageSize, string? eventType)
    {
        var query = _db.Set<SallaSyncLog>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType == eventType);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new SallaSyncLogResponse
            {
                Id = l.Id,
                EventType = l.EventType,
                Direction = l.Direction,
                Status = l.Status,
                ErrorMessage = l.ErrorMessage,
                SallaResourceId = l.SallaResourceId,
                DurationMs = l.DurationMs,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new SallaPaginatedResponse<SallaSyncLogResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // ═══════════════════════════ Settings ═══════════════════════════

    public async Task<SallaSettingsResponse> GetSettingsAsync()
    {
        var totalOrders = await _db.Set<SallaOrder>().CountAsync();
        var pendingSync = await _db.Set<SallaOrder>().CountAsync(o => o.SyncStatus == SallaSyncStatus.Pending);
        var mapped = await _db.Set<SallaProductMapping>().CountAsync(m => m.MainPurchaseClassId != null);
        var unmapped = await _db.Set<SallaProductMapping>().CountAsync(m => m.MainPurchaseClassId == null);
        var lastSync = await _db.Set<SallaSyncLog>()
            .Where(l => l.Status == SallaSyncStatus.Synced)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => (DateTime?)l.CreatedAt)
            .FirstOrDefaultAsync();

        return new SallaSettingsResponse
        {
            IsConnected = _accessToken != null || await _db.Set<SallaAuthToken>().AnyAsync(),
            TotalOrders = totalOrders,
            PendingSyncOrders = pendingSync,
            MappedProducts = mapped,
            UnmappedProducts = unmapped,
            LastSyncAt = lastSync
        };
    }

    // ═══════════════════════════ Store-Aware Stock Sync ═══════════════════════════

    public async Task<SallaStockSyncResult> SyncAllStockToSallaAsync(Guid storeDetailsId)
    {
        var sw = Stopwatch.StartNew();
        var result = new SallaStockSyncResult();

        try
        {
            await EnsureAuthenticatedAsync();

            // Get all mapped products (that have a MainPurchaseClassId)
            var mappings = await _db.Set<SallaProductMapping>()
                .Where(m => m.MainPurchaseClassId != null)
                .ToListAsync();

            if (!mappings.Any())
            {
                result.Message = "No mapped products found";
                return result;
            }

            // Get ProductUnit quantities for this specific store via MainPurchaseClassId
            var mainPurchaseClassIds = mappings.Select(m => m.MainPurchaseClassId!.Value).ToList();
            var storeQuantities = await _db.Set<ProductUnit>()
                .Where(pu => pu.StoreDetailsId == storeDetailsId && mainPurchaseClassIds.Contains(pu.MainPurchaseClassId!.Value))
                .ToDictionaryAsync(pu => pu.MainPurchaseClassId!.Value, pu => (int)(pu.Quantity ?? 0));

            foreach (var mapping in mappings)
            {
                try
                {
                    var qty = storeQuantities.TryGetValue(mapping.MainPurchaseClassId!.Value, out var q) ? q : 0;

                    var body = new { quantity = new { amount = qty } };
                    var content = new StringContent(
                        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync(
                        $"{_baseUrl}/products/{mapping.SallaProductId}/quantity", content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Update the mapping's store reference
                        mapping.StoreDetailsId = storeDetailsId;
                        mapping.LastSyncedAt = DateTime.UtcNow;
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Errors.Add($"{mapping.SallaProductName}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"{mapping.SallaProductName}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync();

            result.TotalProducts = mappings.Count;
            result.StoreDetailsId = storeDetailsId;
            result.Message = $"Synced {result.SuccessCount}/{result.TotalProducts} products to Salla";

            await LogSyncAsync("stock.sync_all", SallaSyncDirection.ErpToSalla, SallaSyncStatus.Synced,
                null, result.Message, null, null, sw.ElapsedMilliseconds);

            _logger.LogInformation("Stock sync completed for store {StoreId}: {Result}", storeDetailsId, result.Message);

            return result;
        }
        catch (Exception ex)
        {
            await LogSyncAsync("stock.sync_all", SallaSyncDirection.ErpToSalla, SallaSyncStatus.Failed,
                null, null, ex.Message, null, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<List<SallaCashierOrderResponse>> GetPendingOrdersForCashierAsync(Guid storeDetailsId)
    {
        // Get the default store from config if not set on orders
        var defaultStore = Guid.TryParse(_defaultStoreDetailsId, out var parsed) ? parsed : (Guid?)null;

        return await _db.Set<SallaOrder>()
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.SyncStatus == SallaSyncStatus.Pending
                && o.Status != SallaOrderStatus.Cancelled
                && o.Status != SallaOrderStatus.Refunded)
            // Show orders that match this store or have no store assigned yet
            .Where(o => o.StoreDetailsId == storeDetailsId
                || o.StoreDetailsId == null
                || (defaultStore.HasValue && defaultStore.Value == storeDetailsId))
            .OrderByDescending(o => o.SallaCreatedAt)
            .Select(o => new SallaCashierOrderResponse
            {
                Id = o.Id,
                SallaOrderId = o.SallaOrderId,
                ReferenceNumber = o.ReferenceNumber,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                PaymentStatus = o.PaymentStatus,
                PaymentMethod = o.PaymentMethod,
                IsPaidOnline = o.PaymentStatus == SallaPaymentStatus.Paid,
                ItemsCount = o.Items.Count,
                SallaCreatedAt = o.SallaCreatedAt,
                Items = o.Items.Select(i => new SallaCashierOrderItemResponse
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    MainPurchaseClassId = i.MainPurchaseClassId,
                    IsMapped = i.MainPurchaseClassId.HasValue
                }).ToList()
            })
            .ToListAsync();
    }

    // ═══════════════════════════ Helpers ═══════════════════════════

    private static SallaOrderStatus MapSallaStatus(string? slug) => slug?.ToLowerInvariant() switch
    {
        "new" or "created" => SallaOrderStatus.New,
        "in_progress" or "processing" => SallaOrderStatus.InProgress,
        "completed" or "delivered" => SallaOrderStatus.Completed,
        "cancelled" or "canceled" => SallaOrderStatus.Cancelled,
        "refunded" => SallaOrderStatus.Refunded,
        "restoring" => SallaOrderStatus.Restoring,
        _ => SallaOrderStatus.New
    };

    private static SallaPaymentStatus MapPaymentStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "paid" or "success" => SallaPaymentStatus.Paid,
        "failed" => SallaPaymentStatus.Failed,
        "refunded" => SallaPaymentStatus.Refunded,
        _ => SallaPaymentStatus.Pending
    };

    private async Task LogSyncAsync(string eventType, SallaSyncDirection direction, SallaSyncStatus status,
        string? request, string? response, string? error, long? resourceId, long durationMs)
    {
        _db.Set<SallaSyncLog>().Add(new SallaSyncLog
        {
            EventType = eventType,
            Direction = direction,
            Status = status,
            RequestPayload = request?.Length > 4000 ? request[..4000] : request,
            ResponsePayload = response?.Length > 4000 ? response[..4000] : response,
            ErrorMessage = error?.Length > 2000 ? error[..2000] : error,
            SallaResourceId = resourceId,
            DurationMs = (int)durationMs
        });
        await _db.SaveChangesAsync();
    }
}
