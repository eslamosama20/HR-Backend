using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Notifications;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Services.Inventory.PermitManagement;

/// <summary>
/// خدمة التذكير بتاريخ انتهاء الصلاحية
/// </summary>
public interface IExpiryReminderService
{
    /// <summary>
    /// الحصول على جميع التذكيرات المستحقة
    /// </summary>
    Task<OperationResponse<List<ExpiryReminderDto>>> GetDueRemindersAsync();
    
    /// <summary>
    /// الحصول على تذكيرات مستخدم معين
    /// </summary>
    Task<OperationResponse<List<ExpiryReminderDto>>> GetRemindersForUserAsync(Guid userId);
    
    /// <summary>
    /// معالجة التذكيرات وإرسال الإشعارات
    /// </summary>
    Task<OperationResponse<int>> ProcessRemindersAsync();
    
    /// <summary>
    /// الحصول على المنتجات القريبة من انتهاء الصلاحية
    /// </summary>
    Task<OperationResponse<List<ExpiryReminderDto>>> GetExpiringProductsAsync(int daysThreshold = 30);
}

public class ExpiryReminderService : IExpiryReminderService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public ExpiryReminderService(
        RepositoriesFactory repositoriesFactory,
        INotificationService notificationService,
        IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<ExpiryReminderDto>>> GetDueRemindersAsync()
    {
        var permitRepo = _repositoriesFactory.GetRepository<InventoryPermit>();
        var today = DateTime.UtcNow.Date;

        // الحصول على الأذونات التي:
        // 1. التذكير مفعل
        // 2. نوع الإذن Entry
        // 3. تاريخ التذكير التالي <= اليوم أو null (لم يتم التذكير من قبل)
        var permitsResult = await permitRepo.Filter(p => 
            p.ReminderEnabled && 
            p.PermitType == PermitType.Entry &&
            (p.NextReminderDate == null || p.NextReminderDate <= today));

        if (permitsResult.Data == null || !permitsResult.Data.Any())
            return new OperationResponse<List<ExpiryReminderDto>>(ErrorCodes.Success, new List<ExpiryReminderDto>());

        var permits = await permitsResult.Data
            .Include(p => p.PermitEntries)
                .ThenInclude(pe => pe.MainPurchaseClass)
            .Include(p => p.StoreDetails)
            .ToListAsync();

        var reminders = new List<ExpiryReminderDto>();

        foreach (var permit in permits)
        {
            foreach (var entry in permit.PermitEntries.Where(e => e.ExpiryDate.HasValue))
            {
                var expiryDate = entry.ExpiryDate!.Value;
                var daysUntilExpiry = (expiryDate.ToDateTime(TimeOnly.MinValue) - today).Days;

                reminders.Add(new ExpiryReminderDto
                {
                    PermitId = permit.Id,
                    PermitReferenceNumber = permit.ReferenceNumber,
                    PermitEntryId = entry.Id,
                    ProductId = entry.MainPurchaseClassId,
                    ProductName = entry.MainPurchaseClass?.ArabicName ?? entry.MainPurchaseClass?.EnglishName ?? "غير معروف",
                    StoreName = permit.StoreDetails?.ArabicName ?? permit.StoreDetails?.EnglishName ?? "غير معروف",
                    ExpiryDate = expiryDate,
                    DaysUntilExpiry = daysUntilExpiry,
                    Quantity = entry.Quantity,
                    ShelfNumber = entry.ShelfNumber,
                    RowNumber = entry.RowNumber,
                    IsExpired = daysUntilExpiry < 0,
                    ReminderFrequency = permit.ReminderFrequency?.ToString(),
                    CustomReminderDays = permit.CustomReminderDays
                });
            }
        }

        return new OperationResponse<List<ExpiryReminderDto>>(ErrorCodes.Success, 
            reminders.OrderBy(r => r.DaysUntilExpiry).ToList());
    }

    public async Task<OperationResponse<List<ExpiryReminderDto>>> GetRemindersForUserAsync(Guid userId)
    {
        // حالياً نرجع كل التذكيرات - يمكن تعديلها لاحقاً لتصفية حسب المستخدم
        return await GetDueRemindersAsync();
    }

    public async Task<OperationResponse<List<ExpiryReminderDto>>> GetExpiringProductsAsync(int daysThreshold = 30)
    {
        var permitRepo = _repositoriesFactory.GetRepository<InventoryPermit>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thresholdDate = today.AddDays(daysThreshold);

        // الحصول على جميع أذونات الدخول
        var permitsResult = await permitRepo.Filter(p => p.PermitType == PermitType.Entry);

        if (permitsResult.Data == null || !permitsResult.Data.Any())
            return new OperationResponse<List<ExpiryReminderDto>>(ErrorCodes.Success, new List<ExpiryReminderDto>());

        var permits = await permitsResult.Data
            .Include(p => p.PermitEntries)
                .ThenInclude(pe => pe.MainPurchaseClass)
            .Include(p => p.StoreDetails)
            .ToListAsync();

        var reminders = new List<ExpiryReminderDto>();

        foreach (var permit in permits)
        {
            foreach (var entry in permit.PermitEntries.Where(e => 
                e.ExpiryDate.HasValue && e.ExpiryDate.Value <= thresholdDate))
            {
                var expiryDate = entry.ExpiryDate!.Value;
                var daysUntilExpiry = (expiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;

                reminders.Add(new ExpiryReminderDto
                {
                    PermitId = permit.Id,
                    PermitReferenceNumber = permit.ReferenceNumber,
                    PermitEntryId = entry.Id,
                    ProductId = entry.MainPurchaseClassId,
                    ProductName = entry.MainPurchaseClass?.ArabicName ?? entry.MainPurchaseClass?.EnglishName ?? "غير معروف",
                    StoreName = permit.StoreDetails?.ArabicName ?? permit.StoreDetails?.EnglishName ?? "غير معروف",
                    ExpiryDate = expiryDate,
                    DaysUntilExpiry = daysUntilExpiry,
                    Quantity = entry.Quantity,
                    ShelfNumber = entry.ShelfNumber,
                    RowNumber = entry.RowNumber,
                    IsExpired = daysUntilExpiry < 0,
                    ReminderFrequency = permit.ReminderFrequency?.ToString(),
                    CustomReminderDays = permit.CustomReminderDays
                });
            }
        }

        return new OperationResponse<List<ExpiryReminderDto>>(ErrorCodes.Success, 
            reminders.OrderBy(r => r.DaysUntilExpiry).ToList());
    }

    public async Task<OperationResponse<int>> ProcessRemindersAsync()
    {
        var remindersResult = await GetDueRemindersAsync();
        if (remindersResult.ErrorCode != ErrorCodes.Success || remindersResult.Data == null || !remindersResult.Data.Any())
            return new OperationResponse<int>(ErrorCodes.Success, 0);

        var permitRepo = _repositoriesFactory.GetRepository<InventoryPermit>();
        var notificationRepo = _repositoriesFactory.GetRepository<Notification>();
        var userNotificationRepo = _repositoriesFactory.GetRepository<UserNotification>();
        var userRepo = _repositoriesFactory.GetRepository<User>();

        // تجميع التذكيرات حسب الإذن
        var groupedReminders = remindersResult.Data.GroupBy(r => r.PermitId);
        int processedCount = 0;

        foreach (var group in groupedReminders)
        {
            var permitId = group.Key;
            var items = group.ToList();
            
            // إنشاء نص الإشعار
            var expiredItems = items.Where(i => i.IsExpired).ToList();
            var expiringItems = items.Where(i => !i.IsExpired).ToList();

            string title = "⚠️ تنبيه انتهاء صلاحية";
            string content = "";

            if (expiredItems.Any())
            {
                content += $"🔴 منتجات منتهية الصلاحية ({expiredItems.Count}):\n";
                foreach (var item in expiredItems.Take(5))
                {
                    content += $"  • {item.ProductName} - الكمية: {item.Quantity}\n";
                }
            }

            if (expiringItems.Any())
            {
                content += $"\n🟡 منتجات قريبة من الانتهاء ({expiringItems.Count}):\n";
                foreach (var item in expiringItems.Take(5))
                {
                    content += $"  • {item.ProductName} - متبقي {item.DaysUntilExpiry} يوم\n";
                }
            }

            // إنشاء الإشعار
            var notification = new Notification
            {
                Title = title,
                Content = content,
                Link = $"/permits/inventory-permits/{permitId}",
                NotificationType = NotificationType.InSystem
            };

            var notificationResult = await notificationRepo.Add(notification);
            if (notificationResult.ErrorCode == ErrorCodes.Success && notificationResult.Data != null)
            {
                // إرسال الإشعار لجميع المستخدمين (يمكن تعديله لاحقاً)
                var usersResult = await userRepo.GetAll();
                if (usersResult.ErrorCode == ErrorCodes.Success && usersResult.Data != null)
                {
                    foreach (var user in await usersResult.Data.ToListAsync())
                    {
                        var userNotification = new UserNotification
                        {
                            UserId = user.Id,
                            NotificationId = notificationResult.Data.Id,
                            IsRead = false
                        };
                        await userNotificationRepo.Add(userNotification);
                    }
                }

                // تحديث تاريخ التذكير التالي للإذن
                var permitResult = await permitRepo.Filter(p => p.Id == permitId);
                if (permitResult.ErrorCode == ErrorCodes.Success && permitResult.Data != null)
                {
                    var permit = await permitResult.Data.FirstOrDefaultAsync();
                    if (permit != null)
                    {
                        permit.LastReminderDate = DateTime.UtcNow;
                        permit.NextReminderDate = CalculateNextReminderDate(permit);
                        await permitRepo.Update(permit);
                    }
                }

                processedCount++;
            }
        }

        return new OperationResponse<int>(ErrorCodes.Success, processedCount);
    }

    private DateTime CalculateNextReminderDate(InventoryPermit permit)
    {
        var today = DateTime.UtcNow;
        
        if (permit.ReminderFrequency == null)
            return today.AddDays(7); // افتراضي أسبوعي

        return permit.ReminderFrequency switch
        {
            ReminderType.Daily => today.AddDays(1),
            ReminderType.Weekly => today.AddDays(7),
            ReminderType.Monthly => today.AddDays(30),
            ReminderType.Custom => today.AddDays(permit.CustomReminderDays ?? 7),
            _ => today.AddDays(7)
        };
    }
}

/// <summary>
/// DTO للتذكير بانتهاء الصلاحية
/// </summary>
public class ExpiryReminderDto
{
    public Guid PermitId { get; set; }
    public string PermitReferenceNumber { get; set; } = string.Empty;
    public Guid PermitEntryId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public decimal Quantity { get; set; }
    public string? ShelfNumber { get; set; }
    public string? RowNumber { get; set; }
    public bool IsExpired { get; set; }
    public string? ReminderFrequency { get; set; }
    public int? CustomReminderDays { get; set; }
}
