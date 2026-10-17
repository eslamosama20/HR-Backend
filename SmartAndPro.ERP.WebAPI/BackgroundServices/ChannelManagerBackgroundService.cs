using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Services.ChannelManager;

namespace SmartAndPro.ERP.WebAPI.BackgroundServices;

public class ChannelManagerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChannelManagerBackgroundService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

    public ChannelManagerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ChannelManagerBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Channel Manager Background Queue Processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingQueueItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing Channel Manager queues.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("Channel Manager Background Queue Processor stopped.");
    }

    private async Task ProcessPendingQueueItemsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ERPContext>();
        var channelService = scope.ServiceProvider.GetRequiredService<IChannelManagerService>();

        // 1. Process Webhook Queue
        var pendingWebhooks = await db.ChannelWebhookQueues
            .Where(q => q.Status == 0) // Pending
            .OrderBy(q => q.CreatedAt)
            .Take(10)
            .Select(q => q.Id)
            .ToListAsync();

        foreach (var webhookId in pendingWebhooks)
        {
            try
            {
                await channelService.ProcessWebhookJobAsync(webhookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook {WebhookId}", webhookId);
            }
        }

        // 2. Process Outbox Events
        var pendingOutbox = await db.ChannelOutboxEvents
            .Where(o => o.Status == 0) // Pending
            .OrderBy(o => o.CreatedAt)
            .Take(10)
            .Select(o => o.Id)
            .ToListAsync();

        foreach (var outboxId in pendingOutbox)
        {
            try
            {
                await channelService.ProcessOutboxJobAsync(outboxId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox event {OutboxId}", outboxId);
            }
        }
    }
}
