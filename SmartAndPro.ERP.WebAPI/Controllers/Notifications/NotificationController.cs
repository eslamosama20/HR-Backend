using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Notifications
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpPost("sendemailnotification")]
        public async Task<IActionResult> SendEmailNotification(
            [FromBody] NotificationPostDto dto,
            [FromQuery] List<Guid>? userIds = null,
            [FromQuery] List<string>? permissions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();

            try
            {
                var responseData = await _notificationService.SendEmailNotificationAsync(dto, userIds, permissions);

                result = responseData.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }


        [HttpPost("sendinsystemnotification")]
        public async Task<IActionResult> SendInSystemNotification(
    [FromBody] NotificationPostDto dto,
    [FromQuery] List<Guid>? userIds = null,
    [FromQuery] List<string>? permissions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();

            try
            {
                var responseData = await _notificationService.SendInSystemNotificationAsync(dto, userIds, permissions);

                result = responseData.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

    }
}    
    

    

