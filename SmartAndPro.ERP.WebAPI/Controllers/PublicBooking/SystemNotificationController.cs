using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.SharedKernel.Enums;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.PublicBooking;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[SmartAndPro.ERP.WebAPI.Attributes.PermissionAuthorize("SystemNotifications.Read")]
public class SystemNotificationController : ControllerBase
{
    private readonly SystemNotificationService _notificationService;

    public SystemNotificationController(SystemNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get all notifications with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SystemNotificationFiltersDto filters, int? pageNumber = 1, int? pageSize = 20)
    {
        try
        {
            var result = await _notificationService.GetFilteredAsync(filters, pageNumber, pageSize);
            return result.ErrorCode == ErrorCodes.Success
                ? Ok(result.Data)
                : Ok(new List<object>());
        }
        catch
        {
            return Ok(new List<object>());
        }
    }

    /// <summary>
    /// Get notification by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _notificationService.GetAsync(id);

        return result.ErrorCode switch
        {
            ErrorCodes.Success => Ok(result.Data),
            ErrorCodes.NotFound => NotFound(),
            _ => StatusCode(500, result.Message)
        };
    }

    /// <summary>
    /// Get unread notifications count for current user
    /// </summary>
    [HttpGet("unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;

        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _notificationService.GetUnreadCountAsync(userId);

        return result.ErrorCode == ErrorCodes.Success
            ? Ok(new { count = result.Data })
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// Get notifications for current user
    /// </summary>
    [HttpGet("my-notifications")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications([FromQuery] SystemNotificationFiltersDto filters, int? pageNumber = 1, int? pageSize = 20)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        filters.UserId = userId;
        var result = await _notificationService.GetFilteredAsync(filters, pageNumber, pageSize);

        return result.ErrorCode == ErrorCodes.Success
            ? Ok(result.Data)
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// Create new notification (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] SystemNotificationPostDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _notificationService.AddAsync(dto);

        return result.ErrorCode == ErrorCodes.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result.Data)
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// Update notification (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SystemNotificationPutDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        dto.Id = id;
        var result = await _notificationService.UpdateAsync(dto);

        return result.ErrorCode switch
        {
            ErrorCodes.Success => Ok(result.Data),
            ErrorCodes.NotFound => NotFound(),
            _ => StatusCode(500, result.Message)
        };
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPatch("{id}/mark-read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);

        return result.ErrorCode switch
        {
            ErrorCodes.Success => Ok(result.Data),
            ErrorCodes.NotFound => NotFound(),
            _ => StatusCode(500, result.Message)
        };
    }

    /// <summary>
    /// Mark all notifications as read for current user
    /// </summary>
    [HttpPatch("mark-all-read")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;

        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _notificationService.MarkAllAsReadAsync(userId);

        return result.ErrorCode == ErrorCodes.Success
            ? Ok(new { markedCount = result.Data })
            : StatusCode(500, result.Message);
    }

    /// <summary>
    /// Delete notification (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _notificationService.DeleteAsync(id);

        return result.ErrorCode switch
        {
            ErrorCodes.Success => NoContent(),
            ErrorCodes.NotFound => NotFound(),
            _ => StatusCode(500, result.Message)
        };
    }

    /// <summary>
    /// Delete old read notifications (Admin only)
    /// </summary>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CleanupOldNotifications([FromQuery] int daysOld = 30)
    {
        var result = await _notificationService.DeleteOldNotificationsAsync(daysOld);

        return result.ErrorCode == ErrorCodes.Success
            ? Ok(new { deletedCount = result.Data })
            : StatusCode(500, result.Message);
    }
}
