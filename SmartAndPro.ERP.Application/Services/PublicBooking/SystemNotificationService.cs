using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Hubs;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Services.PublicBooking;

public class SystemNotificationService
    : ServiceBase<SystemNotification, SystemNotificationPostDto, SystemNotificationPutDto, SystemNotificationResponseDto, SystemNotificationFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SystemNotificationService(
        RepositoriesFactory repositoriesFactory,
        AppSettings appSettings,
        IMapper mapper,
        IHubContext<NotificationHub> hubContext)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Create notification and broadcast via SignalR
    /// </summary>
    public override async Task<OperationResponse<SystemNotificationResponseDto>> AddAsync(SystemNotificationPostDto dto)
    {
        var result = await base.AddAsync(dto);

        if (result.ErrorCode == ErrorCodes.Success && result.Data != null)
        {
            // Broadcast via SignalR
            _ = Task.Run(async () =>
            {
                if (dto.UserId.HasValue)
                {
                    await _hubContext.Clients.User(dto.UserId.ToString()).SendAsync("ReceiveNotification", result.Data);
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", result.Data);
                }
            });
        }

        return result;
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async Task<OperationResponse<SystemNotificationResponseDto>> MarkAsReadAsync(Guid notificationId)
    {
        var repo = _repositoriesFactory.GetRepository<SystemNotification>(true);

        try
        {
            var query = await repo.GetAll();
            var notification = await query.Data.FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                return new OperationResponse<SystemNotificationResponseDto>(ErrorCodes.NotFound, null);
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await repo.Update(notification);

            var response = _mapper.Map<SystemNotificationResponseDto>(notification);
            return new OperationResponse<SystemNotificationResponseDto>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            return new OperationResponse<SystemNotificationResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                ex.Message);
        }
    }

    /// <summary>
    /// Mark all notifications as read for a specific user
    /// </summary>
    public async Task<OperationResponse<int>> MarkAllAsReadAsync(Guid? userId = null)
    {
        var repo = _repositoriesFactory.GetRepository<SystemNotification>(true);

        try
        {
            var query = await repo.GetAll();
            var notifications = userId.HasValue
                ? await query.Data.Where(n => !n.IsRead && n.UserId == userId.Value).ToListAsync()
                : await query.Data.Where(n => !n.IsRead).ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await repo.Update(notification);
            }

            return new OperationResponse<int>(ErrorCodes.Success, notifications.Count);
        }
        catch (Exception ex)
        {
            return new OperationResponse<int>(
                ErrorCodes.GeneralFail,
                0,
                null,
                false,
                ex.Message);
        }
    }

    /// <summary>
    /// Get unread notifications count
    /// </summary>
    public async Task<OperationResponse<int>> GetUnreadCountAsync(Guid? userId = null)
    {
        var repo = _repositoriesFactory.GetRepository<SystemNotification>();

        try
        {
            var query = await repo.GetAll();
            var count = userId.HasValue
                ? await query.Data.CountAsync(n => !n.IsRead && n.UserId == userId.Value)
                : await query.Data.CountAsync(n => !n.IsRead);

            return new OperationResponse<int>(ErrorCodes.Success, count);
        }
        catch (Exception ex)
        {
            return new OperationResponse<int>(
                ErrorCodes.GeneralFail,
                0,
                null,
                false,
                ex.Message);
        }
    }

    /// <summary>
    /// Get notifications with filters and pagination
    /// </summary>
    public async Task<OperationResponse<List<SystemNotificationResponseDto>>> GetFilteredAsync(
        SystemNotificationFiltersDto filters, int? pageNumber = null, int? pageSize = null)
    {
        var repo = _repositoriesFactory.GetRepository<SystemNotification>();

        try
        {
            var query = await repo.GetAll();
            var notificationsQuery = query.Data;

            // Apply filters
            if (!string.IsNullOrEmpty(filters.Type))
            {
                notificationsQuery = notificationsQuery.Where(n => n.Type == filters.Type);
            }

            if (filters.BookingId.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.BookingId == filters.BookingId.Value);
            }

            if (filters.UserId.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.UserId == filters.UserId.Value);
            }

            if (filters.IsRead.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.IsRead == filters.IsRead.Value);
            }

            if (!string.IsNullOrEmpty(filters.Priority))
            {
                notificationsQuery = notificationsQuery.Where(n => n.Priority == filters.Priority);
            }

            if (filters.CreatedAfter.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.CreatedAt >= filters.CreatedAfter.Value);
            }

            if (filters.CreatedBefore.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.CreatedAt <= filters.CreatedBefore.Value);
            }

            // Order by creation date descending
            notificationsQuery = notificationsQuery.OrderByDescending(n => n.CreatedAt);

            // Pagination
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                notificationsQuery = notificationsQuery
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var notifications = await notificationsQuery.ToListAsync();
            var response = _mapper.Map<List<SystemNotificationResponseDto>>(notifications);

            return new OperationResponse<List<SystemNotificationResponseDto>>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            return new OperationResponse<List<SystemNotificationResponseDto>>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                ex.Message);
        }
    }

    /// <summary>
    /// Delete old notifications
    /// </summary>
    public async Task<OperationResponse<int>> DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var repo = _repositoriesFactory.GetRepository<SystemNotification>(true);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var query = await repo.GetAll();
            var oldNotifications = await query.Data
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            foreach (var notification in oldNotifications)
            {
                await repo.Delete(notification);
            }

            return new OperationResponse<int>(ErrorCodes.Success, oldNotifications.Count);
        }
        catch (Exception ex)
        {
            return new OperationResponse<int>(
                ErrorCodes.GeneralFail,
                0,
                null,
                false,
                ex.Message);
        }
    }
}
