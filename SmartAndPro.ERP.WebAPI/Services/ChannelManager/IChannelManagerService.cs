namespace SmartAndPro.ERP.WebAPI.Services.ChannelManager;

public interface IChannelManagerService
{
    // Outgoing ARI
    Task<ChannelServiceResult> PushAvailabilityAsync(Guid hotelId);
    Task<ChannelServiceResult> PushRatesAsync(Guid hotelId);

    // Outbox & Queue Handlers
    Task QueueWebhookProcessingJob(string rawPayload);
    Task ProcessWebhookJobAsync(Guid queueId);
    Task ProcessOutboxJobAsync(Guid outboxId);

    // Reconciliation
    Task NightlyReconciliationJobAsync();
}
