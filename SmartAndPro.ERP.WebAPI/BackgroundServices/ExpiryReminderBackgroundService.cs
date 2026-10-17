using SmartAndPro.ERP.Application.Services.Inventory.PermitManagement;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.WebAPI.BackgroundServices;

/// <summary>
/// خدمة خلفية للتحقق من التذكيرات بشكل دوري
/// </summary>
public class ExpiryReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiryReminderBackgroundService> _logger;
    
    // فترة التحقق - كل دقيقة للاختبار (غيرها لساعة في Production)
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public ExpiryReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ExpiryReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("خدمة تذكيرات انتهاء الصلاحية بدأت");

        // انتظار 10 ثواني فقط للاختبار (غيرها لدقيقة في Production)
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في معالجة تذكيرات انتهاء الصلاحية");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("خدمة تذكيرات انتهاء الصلاحية توقفت");
    }

    private async Task ProcessRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var expiryReminderService = scope.ServiceProvider.GetRequiredService<IExpiryReminderService>();

        _logger.LogInformation("جاري معالجة تذكيرات انتهاء الصلاحية...");

        var result = await expiryReminderService.ProcessRemindersAsync();

        if (result.ErrorCode == ErrorCodes.Success)
        {
            _logger.LogInformation("تم معالجة {Count} تذكيرات", result.Data);
        }
        else
        {
            _logger.LogWarning("فشل في معالجة التذكيرات: {Error}", result.Message);
        }
    }
}
