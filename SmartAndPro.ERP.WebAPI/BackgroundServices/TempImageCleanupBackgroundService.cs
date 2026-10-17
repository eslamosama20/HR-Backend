using SmartAndPro.ERP.WebAPI.Services.Cloudinary;

namespace SmartAndPro.ERP.WebAPI.BackgroundServices;

public class TempImageCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TempImageCleanupBackgroundService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);
    private readonly TimeSpan _maxTempAge = TimeSpan.FromHours(24);

    public TempImageCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<TempImageCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temp image cleanup background service started");

        // Wait a bit on startup to avoid interfering with app initialization
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();

                var deleted = await cloudinaryService.CleanupTempUploadsAsync(_maxTempAge, stoppingToken);
                if (deleted > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} temp images older than {MaxAge}", deleted, _maxTempAge);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temp image cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Temp image cleanup background service stopped");
    }
}
