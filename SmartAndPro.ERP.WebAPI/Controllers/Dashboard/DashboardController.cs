using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Dashboard;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Dashboard;

[PermissionAuthorize("Dashboard.Read")]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "User identity not found." });

            var data = await _dashboardService.GetKpisAsync(userId.Value);
            return Ok(data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to load dashboard KPIs." });
        }
    }

    /// <summary>
    /// Aggregated dashboard payload (KPIs + all charts) in a single round-trip.
    /// Backward-compatible: individual endpoints above remain available.
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "User identity not found." });

            var data = await _dashboardService.GetOverviewAsync(userId.Value);
            return Ok(data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to load dashboard overview." });
        }
    }

    [HttpGet("charts/sales-purchase")]
    public async Task<IActionResult> GetSalesPurchaseChart()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "User identity not found." });

            var data = await _dashboardService.GetSalesPurchaseChartAsync(userId.Value);
            return Ok(data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to load sales and purchase chart." });
        }
    }

    [HttpGet("charts/reservations")]
    public async Task<IActionResult> GetReservationsChart()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "User identity not found." });

            var data = await _dashboardService.GetReservationsChartAsync(userId.Value);
            return Ok(data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to load reservations chart." });
        }
    }

    [HttpGet("charts/treasury")]
    public async Task<IActionResult> GetTreasuryChart()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "User identity not found." });

            var data = await _dashboardService.GetTreasuryChartAsync(userId.Value);
            return Ok(data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to load treasury chart." });
        }
    }

    [HttpGet("charts/inventory")]
    public async Task<IActionResult> GetInventoryChart()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { message = "User identity not found." });

            var data = await _dashboardService.GetInventoryChartAsync(userId.Value);
            return Ok(data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Failed to load inventory chart." });
        }
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.Name)?.Value;
        return string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId) ? null : userId;
    }
}
