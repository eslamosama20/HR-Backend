using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Domains.Enums.Salla;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Salla.Dtos;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Salla;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Salla;

/// <summary>
/// Admin API for managing Salla integration (orders, product mappings, sync, settings)
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/integrations/salla")]
[PermissionAuthorize("System.Read")]
public class SallaController : ControllerBase
{
    private readonly ISallaIntegrationService _sallaService;
    private readonly ILogger<SallaController> _logger;

    public SallaController(
        ISallaIntegrationService sallaService,
        ILogger<SallaController> logger)
    {
        _sallaService = sallaService;
        _logger = logger;
    }

    // ═══════════════════════════ Settings / Connection ═══════════════════════════

    /// <summary>
    /// GET /api/integrations/salla/settings
    /// Dashboard summary: connection status, orders count, mapped products, etc.
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _sallaService.GetSettingsAsync();
        return Ok(result);
    }

    /// <summary>
    /// GET /api/integrations/salla/oauth-url
    /// Returns the URL to redirect the user to for Salla OAuth authorization
    /// </summary>
    [AllowAnonymous]
    [HttpGet("oauth-url")]
    public IActionResult GetOAuthUrl()
    {
        var url = _sallaService.GetOAuthUrl();
        return Ok(new { url });
    }

    /// <summary>
    /// POST /api/integrations/salla/connect
    /// Exchange authorization code for access token (alternative to callback redirect)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] SallaConnectRequest request)
    {
        try
        {
            await _sallaService.ExchangeCodeForTokenAsync(request.Code);
            return Ok(new { status = "connected", message = "Salla account connected successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to connect", detail = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/integrations/salla/oauth/callback?code=XXX
    /// Salla redirects here after user authorizes. Exchanges code for token automatically.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing authorization code");

        try
        {
            await _sallaService.ExchangeCodeForTokenAsync(code);
            return Ok(new { status = "connected", message = "Salla account connected successfully! You can close this page." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to connect", detail = ex.Message });
        }
    }

    // ═══════════════════════════ Orders ═══════════════════════════

    /// <summary>
    /// POST /api/integrations/salla/set-token
    /// Manually set Salla access token (for development — paste token from Salla Webhooks Log)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("set-token")]
    public async Task<IActionResult> SetToken([FromBody] SetTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
            return BadRequest(new { error = "accessToken is required" });

        await _sallaService.SetTokenManuallyAsync(request.AccessToken, request.RefreshToken, request.Expires);
        return Ok(new { status = "connected", message = "Token saved successfully" });
    }

    /// <summary>
    /// GET /api/integrations/salla/orders?page=1&pageSize=20&status=0&syncStatus=0
    /// List all Salla orders with pagination and filters
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SallaOrderStatus? status = null,
        [FromQuery] SallaSyncStatus? syncStatus = null)
    {
        var result = await _sallaService.GetOrdersAsync(page, pageSize, status, syncStatus);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/integrations/salla/orders/{id}
    /// Get detailed Salla order with items
    /// </summary>
    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var result = await _sallaService.GetOrderByIdAsync(id);
        if (result == null) return NotFound(new { error = "Order not found" });
        return Ok(result);
    }

    /// <summary>
    /// POST /api/integrations/salla/orders/{id}/sync-to-invoice
    /// Convert a Salla order to an ERP sale invoice
    /// </summary>
    [HttpPost("orders/{id:guid}/sync-to-invoice")]
    public async Task<IActionResult> SyncOrderToInvoice(Guid id, [FromBody] SyncOrderToInvoiceRequest? request)
    {
        try
        {
            await _sallaService.SyncOrderToInvoiceAsync(id, request?.StoreDetailsId);
            return Ok(new { status = "synced", message = "Order synced to invoice successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/integrations/salla/orders/{id}/status
    /// Update order status on Salla (in_progress, completed, cancelled)
    /// </summary>
    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateSallaOrderStatusRequest request)
    {
        try
        {
            await _sallaService.UpdateOrderStatusOnSallaAsync(id, request.Status);
            return Ok(new { status = "updated", newStatus = request.Status.ToString() });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ═══════════════════════════ Product Mappings ═══════════════════════════

    /// <summary>
    /// GET /api/integrations/salla/products?page=1&pageSize=20&mappedOnly=true
    /// List Salla products with their ERP mapping status
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProductMappings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? mappedOnly = null)
    {
        var result = await _sallaService.GetProductMappingsAsync(page, pageSize, mappedOnly);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/integrations/salla/products/map
    /// Map a single Salla product to an ERP ProductUnit
    /// </summary>
    [HttpPost("products/map")]
    public async Task<IActionResult> MapProduct([FromBody] MapProductRequest request)
    {
        try
        {
            await _sallaService.MapProductAsync(request.SallaProductId, request.MainPurchaseClassId);
            return Ok(new { status = "mapped" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/integrations/salla/products/bulk-map
    /// Map multiple Salla products to ERP ProductUnits in one call
    /// </summary>
    [HttpPost("products/bulk-map")]
    public async Task<IActionResult> BulkMapProducts([FromBody] BulkMapProductsRequest request)
    {
        await _sallaService.BulkMapProductsAsync(request.Mappings);
        return Ok(new { status = "mapped", count = request.Mappings.Count });
    }

    /// <summary>
    /// POST /api/integrations/salla/products/sync
    /// Pull all products from Salla and upsert into product mappings table
    /// </summary>
    [HttpPost("products/sync")]
    public async Task<IActionResult> SyncProductsFromSalla()
    {
        try
        {
            await _sallaService.SyncProductsFromSallaAsync();
            return Ok(new { status = "synced", message = "Products synced from Salla" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/integrations/salla/products/sync-stock
    /// Push ERP stock quantity to Salla for a specific product
    /// </summary>
    [HttpPost("products/sync-stock")]
    public async Task<IActionResult> SyncStockToSalla([FromBody] SyncStockToSallaRequest request)
    {
        try
        {
            await _sallaService.SyncStockToSallaAsync(request.SallaProductId, request.Quantity);
            return Ok(new { status = "synced", sallaProductId = request.SallaProductId, quantity = request.Quantity });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/integrations/salla/products/sync-all-stock
    /// Sync ALL mapped products' stock from a specific ERP store to Salla.
    /// Called automatically when cashier session opens, or manually from admin.
    /// </summary>
    [HttpPost("products/sync-all-stock")]
    public async Task<IActionResult> SyncAllStockToSalla([FromBody] SyncAllStockRequest request)
    {
        try
        {
            var result = await _sallaService.SyncAllStockToSallaAsync(request.StoreDetailsId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/integrations/salla/orders/cashier/{storeDetailsId}
    /// Get pending Salla orders for the cashier screen (filtered by store).
    /// Shows orders that need to be converted to invoices.
    /// </summary>
    [HttpGet("orders/cashier/{storeDetailsId:guid}")]
    public async Task<IActionResult> GetPendingOrdersForCashier(Guid storeDetailsId)
    {
        var result = await _sallaService.GetPendingOrdersForCashierAsync(storeDetailsId);
        return Ok(new
        {
            storeDetailsId,
            pendingCount = result.Count,
            orders = result
        });
    }

    // ═══════════════════════════ Sync Logs ═══════════════════════════

    /// <summary>
    /// GET /api/integrations/salla/sync-logs?page=1&pageSize=50&eventType=order.created
    /// View sync/webhook processing logs
    /// </summary>
    [HttpGet("sync-logs")]
    public async Task<IActionResult> GetSyncLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? eventType = null)
    {
        var result = await _sallaService.GetSyncLogsAsync(page, pageSize, eventType);
        return Ok(result);
    }
}
