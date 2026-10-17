using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;
using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.WebAPI.BackgroundServices;

/// <summary>
/// خدمة خلفية للتحقق من تأخر مراحل تنفيذ المشاريع وإرسال التنبيهات
/// </summary>
public class ProjectDelayCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProjectDelayCheckBackgroundService> _logger;

    // فحص يومي للمراحل المتأخرة
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public ProjectDelayCheckBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ProjectDelayCheckBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("خدمة التحقق من تأخر مراحل المشاريع بدأت");

        // انتظار 15 ثانية بعد تشغيل السيرفر للبدء في الفحص الأول
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDelayChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في فحص تأخر مراحل المشاريع");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("خدمة التحقق من تأخر مراحل المشاريع توقفت");
    }

    private async Task ProcessDelayChecksAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ERPContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        _logger.LogInformation("جاري فحص مراحل المشاريع المتأخرة...");

        var today = DateTime.Now;
        var delayedMilestones = await context.ProjectExecutionMilestones
            .Include(m => m.Project)
            .Where(m => m.IsActive && !m.IsCompleted && m.TargetEndDate.HasValue && m.TargetEndDate.Value < today)
            .ToListAsync(stoppingToken);

        foreach (var milestone in delayedMilestones)
        {
            var title = $"تأخر في تنفيذ المشروع: {milestone.Project.ProjectName}";
            var content = $"المشروع متأخر في المرحلة: '{milestone.MilestoneName}'. تاريخ الانتهاء المستهدف كان {milestone.TargetEndDate.Value:yyyy-MM-dd}.";
            var link = $"/projects/{milestone.ProjectId}";

            // تجنب تكرار الإشعار للمرحلة نفسها إذا تم إرسال إشعار خلال الـ 24 ساعة الماضية
            var yesterday = DateTime.Now.AddDays(-1);
            var alreadyNotified = await context.Set<SmartAndPro.ERP.Domains.Entities.Notifications.Notification>()
                .AnyAsync(n => n.Link == link && n.Title == title && n.CreatedAt > yesterday, stoppingToken);

            if (alreadyNotified)
            {
                continue;
            }

            var notificationDto = new NotificationPostDto
            {
                Title = title,
                Content = content,
                Link = link,
                NotificationType = NotificationType.InSystem
            };

            await notificationService.SendInSystemNotificationAsync(notificationDto, null, new List<string> { "Projects.View" });
            _logger.LogInformation("تم إرسال إشعار تأخير للمشروع {ProjectName} في المرحلة {MilestoneName}", milestone.Project.ProjectName, milestone.MilestoneName);
        }
    }
}
