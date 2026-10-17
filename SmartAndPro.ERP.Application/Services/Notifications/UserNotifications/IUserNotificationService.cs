using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Common.Dtos.UserNotifications;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.Notifications.UserNotifications
{
    public interface IUserNotificationService
    {
        Task<OperationResponse<UserNotificationResponseDto>> MarkAsRead(Guid userNotificationId);

        Task<OperationResponse<UserNotificationResponseDto>> IgnoreNotification(Guid userNotificationId);

        Task<OperationResponse<List<NotificationResponseDto>>> GetAllNotificationsForUser(Guid userId);

        Task<OperationResponse<List<NotificationResponseDto>>> GetAllNotificationsForUserIncludingIgnored(Guid userId);

        Task<OperationResponse<UserNotificationResponseDto?>> IgnoreAllForUser(Guid userId);

    }
}
