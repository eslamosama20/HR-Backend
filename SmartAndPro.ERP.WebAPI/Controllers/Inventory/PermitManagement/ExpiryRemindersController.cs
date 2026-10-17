using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Inventory.PermitManagement;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.WebAPI.Attributes;
using System.Security.Claims;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.PermitManagement;

/// <summary>
/// API للتذكير بتاريخ انتهاء الصلاحية
/// </summary>
[ApiController]
[Route("api/[controller]")]
[PermissionAuthorize("InventoryManagement.Read")]
public class ExpiryRemindersController : ControllerBase
{
    private readonly IExpiryReminderService _expiryReminderService;

    public ExpiryRemindersController(IExpiryReminderService expiryReminderService)
    {
        _expiryReminderService = expiryReminderService;
    }

    /// <summary>
    /// الحصول على جميع التذكيرات المستحقة
    /// </summary>
    [HttpGet("due")]
    public async Task<IActionResult> GetDueReminders()
    {
        var result = await _expiryReminderService.GetDueRemindersAsync();
        return result.ErrorCode == ErrorCodes.Success
            ? Ok(result.Data)
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// الحصول على تذكيرات المستخدم الحالي
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReminders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var result = await _expiryReminderService.GetRemindersForUserAsync(userGuid);
        return result.ErrorCode == ErrorCodes.Success
            ? Ok(result.Data)
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// الحصول على المنتجات القريبة من انتهاء الصلاحية
    /// </summary>
    /// <param name="daysThreshold">عدد الأيام للبحث (افتراضي 30)</param>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringProducts([FromQuery] int daysThreshold = 30)
    {
        var result = await _expiryReminderService.GetExpiringProductsAsync(daysThreshold);
        return result.ErrorCode == ErrorCodes.Success
            ? Ok(result.Data)
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// معالجة التذكيرات وإرسال الإشعارات
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessReminders()
    {
        var result = await _expiryReminderService.ProcessRemindersAsync();
        return result.ErrorCode == ErrorCodes.Success
            ? Ok(result.Data)
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// الحصول على ملخص التذكيرات للـ Dashboard
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetRemindersSummary()
    {
        var result = await _expiryReminderService.GetExpiringProductsAsync(30);
        
        if (result.ErrorCode != ErrorCodes.Success || result.Data == null)
            return result.ErrorCode == ErrorCodes.Success
                ? Ok(new List<ExpiryReminderDto>())
                : StatusCode(500, result.Message);

        var summary = new ExpiryReminderSummaryDto
        {
            TotalCount = result.Data.Count,
            ExpiredCount = result.Data.Count(r => r.IsExpired),
            ExpiringIn7Days = result.Data.Count(r => !r.IsExpired && r.DaysUntilExpiry <= 7),
            ExpiringIn30Days = result.Data.Count(r => !r.IsExpired && r.DaysUntilExpiry > 7 && r.DaysUntilExpiry <= 30),
            TopExpiring = result.Data.Take(5).ToList()
        };

        return Ok(summary);
    }
}

/// <summary>
/// ملخص التذكيرات للـ Dashboard
/// </summary>
public class ExpiryReminderSummaryDto
{
    public int TotalCount { get; set; }
    public int ExpiredCount { get; set; }
    public int ExpiringIn7Days { get; set; }
    public int ExpiringIn30Days { get; set; }
    public List<ExpiryReminderDto> TopExpiring { get; set; } = new();
}
