using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Notifications;
using SmartAndPro.ERP.Infrastructure.External;

using SmartAndPro.ERP.Infrastructure.External;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.WebSockets;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.Notifications
{
    public class NotificationService : ServiceBase<Notification, NotificationPostDto, NotificationPutDto, NotificationResponseDto, NotificationFilterDto>,
       INotificationService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly WebSocketConnectionManager _wsManager;


        public NotificationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, WebSocketConnectionManager wsManager) : base(repositoriesFactory, appSettings, mapper)

        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _wsManager = wsManager;
        }
 
        public async Task<OperationResponse<NotificationResponseDto>> SendInSystemNotificationAsync
            (NotificationPostDto dto, List<Guid>? userIds = null, List<string>? permissions = null)
        {
            try
            {
                dto.NotificationType = NotificationType.InSystem;
                var notificationAdded = await AddAsync(dto);

                if (notificationAdded.Data == null)
                    return new OperationResponse<NotificationResponseDto>(ErrorCodes.GeneralFail, null);

                var notification = notificationAdded.Data;


                var userRepo = _repositoriesFactory.GetRepository<User>();
                var userNotificationRepo = _repositoriesFactory.GetRepository<UserNotification>();

                var targetUsers = new List<User>();

                if (userIds != null && userIds.Any())
                {
                    var usersByIds = await userRepo.Filter(u => userIds.Contains(u.Id));
                    if (usersByIds.Data != null)
                        targetUsers.AddRange(usersByIds.Data);
                }

                if (permissions != null && permissions.Any())
                {
                    var usersByPermissions = await userRepo.Filter(u => u.UserRoles.Any
                    (ur => ur.Role.RolePermissions.Any(rp =>
                        permissions.Contains(rp.Permission.Name))));


                    if (usersByPermissions.Data != null)
                        targetUsers.AddRange(usersByPermissions.Data);
                }

                targetUsers = targetUsers.DistinctBy(u => u.Id).ToList();

                var userNotifications = targetUsers.Select(u => new UserNotification
                {
                    UserId = u.Id,
                    NotificationId = notification.Id
                }).ToList();

                var addUserNotifications = await userNotificationRepo.AddRange(userNotifications);

                if (addUserNotifications.ErrorCode != ErrorCodes.Success)
                    throw new Exception("Failed to add user notifications");

                var persistedUserNotifications = addUserNotifications.Data ?? userNotifications;
                var basePayload = _mapper.Map<NotificationResponseDto>(notification);

                foreach (var userNotification in persistedUserNotifications)
                {
                    try
                    {
                        var payload = _mapper.Map<NotificationResponseDto>(notification);
                        payload.UserNotificationId = userNotification.Id;
                        payload.IsRead = userNotification.IsRead;
                        payload.IsIgnored = userNotification.IsIgnored;
                        var json = System.Text.Json.JsonSerializer.Serialize(payload);
                        await _wsManager.SendAsync(userNotification.UserId.ToString(), json);
                    }
                    catch(Exception ex)
                    {
                        LogService.LogError(ex);
                    }
                }
                return new OperationResponse<NotificationResponseDto>(ErrorCodes.Success, basePayload);

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<NotificationResponseDto>(ErrorCodes.GeneralFail, null);

            }

        }
        public async Task<OperationResponse<NotificationResponseDto>> SendEmailNotificationAsync(NotificationPostDto dto, List<Guid>? userIds = null, List<string>? permissions = null)
        {
            try
            {
                dto.NotificationType = NotificationType.Email;

                var notificationAdded = await AddAsync(dto);

                if (notificationAdded.Data == null)
                    return new OperationResponse<NotificationResponseDto>(ErrorCodes.GeneralFail, null);

                var notification = notificationAdded.Data;


                var userRepo = _repositoriesFactory.GetRepository<User>();
                var userNotificationRepo = _repositoriesFactory.GetRepository<UserNotification>();

                var targetUsers = new List<User>();

                if (userIds != null && userIds.Any())
                {
                    var usersByIds = await userRepo.Filter(u => userIds.Contains(u.Id));
                    if (usersByIds.Data != null)
                        targetUsers.AddRange(usersByIds.Data);
                }

                if (permissions != null && permissions.Any())
                {
                    var usersByPermissions = await userRepo.Filter(u => u.UserRoles.Any
                    (ur => ur.Role.RolePermissions.Any(rp =>
                        permissions.Contains(rp.Permission.Name))));


                    if (usersByPermissions.Data != null)
                        targetUsers.AddRange(usersByPermissions.Data);
                }

                targetUsers = targetUsers.DistinctBy(u => u.Id).ToList();

                var userNotifications = targetUsers.Select(u => new UserNotification
                {
                    UserId = u.Id,
                    NotificationId = notification.Id
                }).ToList();

                var addUserNotifications = await userNotificationRepo.AddRange(userNotifications);

                if (addUserNotifications.ErrorCode != ErrorCodes.Success)
                    throw new Exception("Failed to add user notifications");

                var persistedUserNotifications = addUserNotifications.Data ?? userNotifications;
                var basePayload = _mapper.Map<NotificationResponseDto>(notification);

                foreach (var userNotification in persistedUserNotifications)
                {
                    try
                    {
                        var payload = _mapper.Map<NotificationResponseDto>(notification);
                        payload.UserNotificationId = userNotification.Id;
                        payload.IsRead = userNotification.IsRead;
                        payload.IsIgnored = userNotification.IsIgnored;
                        var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    }
                    catch
                    {
                    }
                }
                foreach (var user in targetUsers)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        try
                        {
                            Console.WriteLine($"Sending email to {user.Email}");
                            Console.WriteLine($@"{basePayload.Link ?? "غير متوفر"} : {basePayload.Content} رابط الفاتورة");
                            
                            var emailBody =
                                $@"{basePayload.Link ?? "غير متوفر"} : {basePayload.Content} يرجى زيارة الرابط التالي";

                            await EmailService.SendMail(
                               to: user.Email,
                                subject: basePayload.Title,
                                body: emailBody
                                );
                        }
                        catch (Exception ex)
                        {
                            LogService.LogError(ex);
                        }
                    }
                }

                return new OperationResponse<NotificationResponseDto>(ErrorCodes.Success, basePayload);

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<NotificationResponseDto>(ErrorCodes.GeneralFail, null);

            }

        }


    }
}
