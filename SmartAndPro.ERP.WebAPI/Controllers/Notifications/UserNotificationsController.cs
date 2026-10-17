using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Services.Notifications.UserNotifications;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Notifications
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserNotificationsController : ControllerBase
    {
        private readonly IUserNotificationService _userNotificationService;

        public UserNotificationsController(IUserNotificationService userNotificationService)
        {
            _userNotificationService = userNotificationService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllNotificationsForCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var notificationsResponse = await _userNotificationService.GetAllNotificationsForUser(userId);

                if (notificationsResponse == null || notificationsResponse.Data == null)
                {
                    return Ok(new List<NotificationResponseDto>());
                }

                return Ok(notificationsResponse.Data);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetAllIncludingIgnored")]
        public async Task<IActionResult> GetAllNotificationsForCurrentUserIncludingIgnored()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var notificationsResponse = await _userNotificationService.GetAllNotificationsForUserIncludingIgnored(userId);

                if (notificationsResponse == null || notificationsResponse.Data == null)
                {
                    return Ok(new List<NotificationResponseDto>());
                }

                return Ok(notificationsResponse.Data);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("{userNotificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid userNotificationId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IActionResult result = NoContent();
            try
            {
                var responseData = await _userNotificationService.MarkAsRead(userNotificationId);
                result = responseData.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return result;
        }


        [HttpPut("{userNotificationId}/ignore")]
        public async Task<IActionResult> IgnoreNotification(Guid userNotificationId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IActionResult result = NoContent();
            try
            {
                var responseData = await _userNotificationService.IgnoreNotification(userNotificationId);
                result = responseData.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return result;
        }


        [HttpPut("ignore-all")]
        public async Task<IActionResult> IgnoreAllNotifications()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IActionResult result = NoContent();
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var responseData = await _userNotificationService.IgnoreAllForUser(userId);
                result = responseData.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return result;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.Name)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }
}
