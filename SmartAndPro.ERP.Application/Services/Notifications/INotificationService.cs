using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.Notifications
{
    public interface INotificationService
    {

        Task<OperationResponse<NotificationResponseDto>> SendInSystemNotificationAsync(
      NotificationPostDto dto,
      List<Guid>? userIds = null,
      List<string>? permissions = null);

        Task<OperationResponse<NotificationResponseDto>> SendEmailNotificationAsync(
      NotificationPostDto dto,
      List<Guid>? userIds = null,
      List<string>? permissions = null);

    }
}
