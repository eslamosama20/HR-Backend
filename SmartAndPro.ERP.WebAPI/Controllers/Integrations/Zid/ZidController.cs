using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Domains.Enums.Zid;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Zid.Dtos;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Services.Zid;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.Zid;

/// <summary>
/// Admin API for managing Zid integration (orders, product mappings, sync, settings)
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/integrations/zid")]
[PermissionAuthorize("System.Read")]
public class ZidController : ControllerBase
{
    private readonly IZidIntegrationService _zidService;
    private readonly ILogger<ZidController> _logger;

    public ZidController(IZidIntegrationService zidService, ILogger<ZidController> logger)
    {
        _zidService = zidService;
        _logger = logger;
    }

    // ═══════════════════════════ Settings / Connection ═══════════════════════════

    /// <summary>GET /api/integrations/zid/settings</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _zidService.GetSettingsAsync();
        return Ok(result);
    }

    /// <summary>GET /api/integrations/zid/oauth-url</summary>
    [AllowAnonymous]
    [HttpGet("oauth-url")]
    public IActionResult GetOAuthUrl()
    {
        var url = _zidService.GetOAuthUrl();
        return Ok(new { url });
    }

    /// <summary>POST /api/integrations/zid/connect — exchange code for token</summary>
    [AllowAnonymous]
    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] ZidConnectRequest request)
    {
        try
        {
            await _zidService.ExchangeCodeForTokenAsync(request.Code);
            return Ok(new { status = "connected", message = "Zid account connected successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to connect", detail = ex.Message });
        }
    }

    /// <summary>GET /api/integrations/zid/oauth/callback?code=XXX</summary>
    [AllowAnonymous]
    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing authorization code");

        try
        {
            await _zidService.ExchangeCodeForTokenAsync(code);
            return Ok(new { status = "connected", message = "Zid account connected! You can close this page." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to connect", detail = ex.Message });
        }
    }

    /// <summary>POST /api/integrations/zid/set-token — manually set token (development)</summary>
    [HttpPost("set-token")]
    public async Task<IActionResult> SetToken([FromBody] ZidSetTokenRequest request)
    {
        await _zidService.SetTokenManuallyAsync(
            request.AccessToken,
            request.RefreshToken,
            request.ExpiresAt);

        return Ok(new { status = "ok", message = "Zid token set successfully" });
    }

    // ═══════════════════════════ Orders ═══════════════════════════

    /// <summary>GET /api/integrations/zid/orders</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ZidOrderStatus? status = null,
        [FromQuery] ZidSyncStatus? syncStatus = null)
    {
        var result = await _zidService.GetOrdersAsync(page, pageSize, status, syncStatus);
        return Ok(result);
    }

    /// <summary>GET /api/integrations/zid/orders/{id}</summary>
    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var result = await _zidService.GetOrderByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>POST /api/integrations/zid/orders/{id}/sync — sync order to ERP invoice</summary>
    [HttpPost("orders/{id:guid}/sync")]
    public async Task<IActionResult> SyncOrder(Guid id, [FromBody] ZidSyncOrderRequest request)
    {
        try
        {
            await _zidService.SyncOrderToInvoiceAsync(id, request.StoreDetailsId);
            return Ok(new { status = "synced", message = "Order synced to ERP invoice queue" });
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

    /// <summary>PUT /api/integrations/zid/orders/{id}/status — push status back to Zid</summary>
    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] ZidUpdateOrderStatusRequest request)
    {
        try
        {
            await _zidService.UpdateOrderStatusOnZidAsync(id, request.Status);
            return Ok(new { status = "updated" });
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

    /// <summary>GET /api/integrations/zid/cashier/orders?storeDetailsId=xxx — for cashier screen</summary>
    [HttpGet("cashier/orders")]
    public async Task<IActionResult> GetCashierOrders([FromQuery] Guid storeDetailsId)
    {
        var result = await _zidService.GetPendingOrdersForCashierAsync(storeDetailsId);
        return Ok(result);
    }

    // ═══════════════════════════ Product Mapping ═══════════════════════════

    /// <summary>GET /api/integrations/zid/products</summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProductMappings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? mappedOnly = null)
    {
        var result = await _zidService.GetProductMappingsAsync(page, pageSize, mappedOnly);
        return Ok(result);
    }

    /// <summary>PUT /api/integrations/zid/products/{zidProductId}/map — map to ERP product</summary>
    [HttpPut("products/{zidProductId:long}/map")]
    public async Task<IActionResult> MapProduct(long zidProductId, [FromBody] MapZidProductRequest request)
    {
        try
        {
            await _zidService.MapProductAsync(zidProductId, request.MainPurchaseClassId);
            return Ok(new { status = "mapped" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>POST /api/integrations/zid/products/bulk-map</summary>
    [HttpPost("products/bulk-map")]
    public async Task<IActionResult> BulkMapProducts([FromBody] List<MapZidProductRequest> mappings)
    {
        await _zidService.BulkMapProductsAsync(mappings);
        return Ok(new { status = "ok", count = mappings.Count });
    }

    /// <summary>POST /api/integrations/zid/products/sync — pull products from Zid</summary>
    [HttpPost("products/sync")]
    public async Task<IActionResult> SyncProducts()
    {
        try
        {
            await _zidService.SyncProductsFromZidAsync();
            return Ok(new { status = "synced" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>POST /api/integrations/zid/stock/sync?storeDetailsId=xxx — push all stock to Zid</summary>
    [HttpPost("stock/sync")]
    public async Task<IActionResult> SyncAllStock([FromQuery] Guid storeDetailsId)
    {
        var result = await _zidService.SyncAllStockToZidAsync(storeDetailsId);
        return Ok(result);
    }

    /// <summary>PUT /api/integrations/zid/products/{zidProductId}/stock — push single product stock</summary>
    [HttpPut("products/{zidProductId:long}/stock")]
    public async Task<IActionResult> SyncProductStock(long zidProductId, [FromBody] int quantity)
    {
        try
        {
            await _zidService.SyncStockToZidAsync(zidProductId, quantity);
            return Ok(new { status = "updated", zidProductId, quantity });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ═══════════════════════════ Sync Logs ═══════════════════════════

    /// <summary>GET /api/integrations/zid/logs</summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? eventType = null)
    {
        var result = await _zidService.GetSyncLogsAsync(page, pageSize, eventType);
        return Ok(result);
    }
}
