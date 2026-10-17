using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Common.Dtos.UserNotifications;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Notifications;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.Notifications.UserNotifications
{

    public class UserNotificationService : ServiceBase<UserNotification, UserNotificationPostDto, UserNotificationPutDto, UserNotificationResponseDto, UserNotificationFilterDto>, IUserNotificationService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;

        public UserNotificationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
            : base(repositoriesFactory, appSettings, mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
        }



        public async Task<OperationResponse<List<NotificationResponseDto>>> GetAllNotificationsForUser(Guid userId)
        {
            var userNotificationRepo = _repositoriesFactory.GetRepository<UserNotification>();

            var userNotifications = await userNotificationRepo
                .Filter(un => un.UserId == userId);

            if (userNotifications.Data == null || !userNotifications.Data.Any())
                return new OperationResponse<List<NotificationResponseDto>>(ErrorCodes.Success, new List<NotificationResponseDto>());

            var notifications = userNotifications.Data
                .Include(un => un.Notification)
                .Where(un => un.Notification != null && !un.IsIgnored)
                .AsEnumerable()
                .OrderByDescending(un => un.Notification!.CreatedAt)
                .Select(un =>
                {
                    var dto = _mapper.Map<NotificationResponseDto>(un.Notification);
                    dto.IsRead = un.IsRead;
                    dto.IsIgnored = un.IsIgnored;
                    dto.UserNotificationId = un.Id;
                    return dto;
                })
                .ToList();

            return new OperationResponse<List<NotificationResponseDto>>(ErrorCodes.Success, notifications);
        }

        public async Task<OperationResponse<List<NotificationResponseDto>>> GetAllNotificationsForUserIncludingIgnored(Guid userId)
        {
            var userNotificationRepo = _repositoriesFactory.GetRepository<UserNotification>();

            var userNotifications = await userNotificationRepo
                .Filter(un => un.UserId == userId);

            if (userNotifications.Data == null || !userNotifications.Data.Any())
                return new OperationResponse<List<NotificationResponseDto>>(ErrorCodes.Success, new List<NotificationResponseDto>());

            var notifications = userNotifications.Data
                .Include(un => un.Notification)
                .Where(un => un.Notification != null)
                .AsEnumerable()
                .OrderByDescending(un => un.Notification!.CreatedAt)
                .Select(un =>
                {
                    var dto = _mapper.Map<NotificationResponseDto>(un.Notification);
                    dto.IsRead = un.IsRead;
                    dto.IsIgnored = un.IsIgnored;
                    dto.UserNotificationId = un.Id;
                    return dto;
                })
                .ToList();

            return new OperationResponse<List<NotificationResponseDto>>(ErrorCodes.Success, notifications);
        }



        public async Task<OperationResponse<UserNotificationResponseDto>> IgnoreNotification(Guid userNotificationId)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<UserNotification>();

                var entityQuery = await repo.Filter(x => x.Id == userNotificationId);
                var entity = entityQuery.Data?.FirstOrDefault();

                if (entity == null)
                    return new OperationResponse<UserNotificationResponseDto>(ErrorCodes.NotFound, null);

                entity.IsIgnored = true;

                await repo.Update(entity);
                var dto =  _mapper.Map<UserNotificationResponseDto>(entity);


                return new OperationResponse<UserNotificationResponseDto>(ErrorCodes.Success, dto);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);

                return new OperationResponse<UserNotificationResponseDto>(ErrorCodes.GeneralFail, null);
            }
        }
        

        public async Task<OperationResponse<UserNotificationResponseDto>> MarkAsRead(Guid userNotificationId)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<UserNotification>();

                var entityQuery = await repo.Filter(x => x.Id == userNotificationId);
                var entity = entityQuery.Data?.FirstOrDefault();

                if (entity == null)
                    return new OperationResponse<UserNotificationResponseDto>(ErrorCodes.NotFound, null);

                entity.IsRead = true;
                entity.ReadAt = DateTime.UtcNow;

                await repo.Update(entity);

                var dto = _mapper.Map<UserNotificationResponseDto>(entity);


                return new OperationResponse<UserNotificationResponseDto>(ErrorCodes.Success, dto);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<UserNotificationResponseDto>(ErrorCodes.GeneralFail, null);
            }
        }

        public async Task<OperationResponse<UserNotificationResponseDto?>> IgnoreAllForUser(Guid userId)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<UserNotification>();

                var userNotifications = await repo.Filter(x => x.UserId == userId && !x.IsIgnored);

                if (userNotifications.Data == null)
                {
                    return new OperationResponse<UserNotificationResponseDto?>(ErrorCodes.Success, null);
                }

                var notificationsToUpdate = userNotifications.Data.ToList();

                if (!notificationsToUpdate.Any())
                {
                    return new OperationResponse<UserNotificationResponseDto?>(ErrorCodes.Success, null);
                }

                foreach (var entity in notificationsToUpdate)
                {
                    entity.IsIgnored = true;
                }

                await repo.UpdateRange(notificationsToUpdate);

                return new OperationResponse<UserNotificationResponseDto?>(ErrorCodes.Success, null);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<UserNotificationResponseDto?>(ErrorCodes.GeneralFail, null);
            }
        }

      
    }
}
