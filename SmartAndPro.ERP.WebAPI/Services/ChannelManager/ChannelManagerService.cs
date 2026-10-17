using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Domains.Entities.ChannelManager;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.ChannelManager;

public class ChannelManagerService : IChannelManagerService
{
    private readonly ERPContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChannelManagerService> _logger;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChannelManagerService(
        ERPContext db,
        HttpClient httpClient,
        ILogger<ChannelManagerService> logger,
        IConfiguration config,
        IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
        _scopeFactory = scopeFactory;
    }

    // ═══════════════════════════ Outgoing ARI ═══════════════════════════

    public async Task<ChannelServiceResult> PushAvailabilityAsync(Guid hotelId)
    {
        var connection = await _db.ChannelConnections
            .Include(c => c.RoomMappings)
            .FirstOrDefaultAsync(c => c.HotelId == hotelId && c.IsActive);

        if (connection == null)
        {
            return new ChannelServiceResult { IsSuccess = false, Error = "No active Channel Connection found for hotel." };
        }

        var result = new ChannelServiceResult { IsSuccess = true };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(30);

        var payload = new ChannexAvailabilityPayload
        {
            PropertyId = connection.ChannelPropertyId
        };

        foreach (var mapping in connection.RoomMappings)
        {
            // Load physical count from ERP RoomType
            var roomType = await _db.RoomTypes.FindAsync(mapping.ErpRoomTypeId);
            if (roomType == null) continue;

            int physicalCount = mapping.MaxAllocationOverride ?? roomType.PersonNo; // Fallback or override count

            for (var date = today; date <= endDate; date = date.AddDays(1))
            {
                // Advanced Availability Calculation
                // Available Rooms = Physical - Confirmed - Maintenance - OutOfService - HouseUse + OverbookingLimit
                
                int confirmedReservations = await _db.Reservations
                    .Where(r => r.HotelId == hotelId && !r.IsCancelled)
                    .Where(r => r.ArrivalDate <= date && r.DepartureDate > date)
                    .Where(r => r.ReservationRoomEntries.Any(re => re.RoomTypeId == mapping.ErpRoomTypeId))
                    .CountAsync();

                // Mock maintenance / out of service blocks (In actual system, query maintenance entity)
                int maintenanceBlocks = 0; 
                int houseUse = 0;
                int overbookingLimit = 1; // 1 room overbooking tolerance allowed

                int available = physicalCount - (confirmedReservations + maintenanceBlocks + houseUse) + overbookingLimit;
                if (available < 0) available = 0;

                payload.Availability.Add(new ChannexAvailabilityEntry
                {
                    RoomTypeId = mapping.ChannelRoomId,
                    Date = date.ToString("yyyy-MM-dd"),
                    Availability = available
                });
            }
        }

        // Outbox Pattern: Save payload as an Outbox event first
        var outboxEvent = new ChannelOutboxEvent
        {
            HotelId = hotelId,
            EventName = "availability.update",
            Payload = JsonSerializer.Serialize(payload),
            Status = 0, // Pending
            IdempotencyKey = Guid.NewGuid().ToString(),
            IsActive = true
        };

        _db.ChannelOutboxEvents.Add(outboxEvent);
        await _db.SaveChangesAsync();

        // Dispatch background job asynchronously (Mocked trigger here, handled by worker)
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IChannelManagerService>();
            await service.ProcessOutboxJobAsync(outboxEvent.Id);
        });

        result.ProcessedCount = payload.Availability.Count;
        return result;
    }

    public async Task<ChannelServiceResult> PushRatesAsync(Guid hotelId)
    {
        var connection = await _db.ChannelConnections
            .Include(c => c.RateMappings)
            .Include(c => c.RoomMappings)
            .FirstOrDefaultAsync(c => c.HotelId == hotelId && c.IsActive);

        if (connection == null)
        {
            return new ChannelServiceResult { IsSuccess = false, Error = "No active Channel Connection found." };
        }

        var result = new ChannelServiceResult { IsSuccess = true };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddDays(30);

        var payload = new ChannexRatePayload
        {
            PropertyId = connection.ChannelPropertyId
        };

        foreach (var rateMap in connection.RateMappings)
        {
            var season = await _db.ReservationSeasons.FindAsync(rateMap.ErpSeasonId);
            if (season == null) continue;

            foreach (var roomMap in connection.RoomMappings)
            {
                // In ERP, calculate season rates or use a default price
                decimal erpBasePrice = 350.00m; // Mock rate retrieval from season logic

                // Apply dynamic markup config
                decimal finalPrice = erpBasePrice * (1.00m + (rateMap.MarkupPercentage / 100.00m));

                for (var date = today; date <= endDate; date = date.AddDays(1))
                {
                    payload.Rates.Add(new ChannexRateEntry
                    {
                        RatePlanId = rateMap.ChannelRateId,
                        RoomTypeId = roomMap.ChannelRoomId,
                        Date = date.ToString("yyyy-MM-dd"),
                        Price = finalPrice,
                        Currency = "SAR"
                    });
                }
            }
        }

        var outboxEvent = new ChannelOutboxEvent
        {
            HotelId = hotelId,
            EventName = "rate.update",
            Payload = JsonSerializer.Serialize(payload),
            Status = 0,
            IdempotencyKey = Guid.NewGuid().ToString(),
            IsActive = true
        };

        _db.ChannelOutboxEvents.Add(outboxEvent);
        await _db.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IChannelManagerService>();
            await service.ProcessOutboxJobAsync(outboxEvent.Id);
        });

        result.ProcessedCount = payload.Rates.Count;
        return result;
    }

    // ═══════════════════════════ Outbox & Webhook Queue Processors ═══════════════════════════

    public async Task QueueWebhookProcessingJob(string rawPayload)
    {
        using var doc = JsonDocument.Parse(rawPayload);
        var root = doc.RootElement;
        
        string eventId = root.TryGetProperty("event_id", out var ev) ? ev.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
        string propertyId = root.TryGetProperty("property_id", out var pr) ? pr.GetString() ?? string.Empty : string.Empty;

        var connection = await _db.ChannelConnections
            .FirstOrDefaultAsync(c => c.ChannelPropertyId == propertyId && c.IsActive);

        if (connection == null)
        {
            _logger.LogWarning("Discarding webhook: Property {Prop} is not mapped.", propertyId);
            return;
        }

        // Prevent duplicates (Idempotency)
        if (await _db.ChannelWebhookQueues.AnyAsync(q => q.ExternalEventId == eventId))
        {
            _logger.LogInformation("Webhook event {EventId} already queued, skipping.", eventId);
            return;
        }

        var webhookQueue = new ChannelWebhookQueue
        {
            HotelId = connection.HotelId,
            ExternalEventId = eventId,
            Payload = rawPayload,
            Status = 0, // Pending
            IsActive = true
        };

        _db.ChannelWebhookQueues.Add(webhookQueue);
        await _db.SaveChangesAsync();

        // Dispatch background processing task
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IChannelManagerService>();
            await service.ProcessWebhookJobAsync(webhookQueue.Id);
        });
    }

    public async Task ProcessWebhookJobAsync(Guid queueId)
    {
        var job = await _db.ChannelWebhookQueues.FindAsync(queueId);
        if (job == null || job.Status == 2) return; // Already processed

        job.Status = 1; // Processing
        job.LockedBy = Environment.MachineName;
        job.LockedUntil = DateTime.UtcNow.AddMinutes(5);
        await _db.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        try
        {
            var payload = JsonSerializer.Deserialize<ChannexWebhookPayload>(job.Payload);
            if (payload == null) throw new InvalidOperationException("Failed to deserialize JSON.");

            var connection = await _db.ChannelConnections
                .Include(c => c.RoomMappings)
                .Include(c => c.RateMappings)
                .FirstOrDefaultAsync(c => c.ChannelPropertyId == payload.PropertyId && c.IsActive);

            if (connection == null) throw new InvalidOperationException("Connection not found.");

            if (payload.EventType == "booking.cancelled")
            {
                await HandleCancellationAsync(connection, payload.Booking);
            }
            else
            {
                await HandleUpsertAsync(connection, payload.Booking);
            }

            job.Status = 2; // Completed
            job.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await LogSyncAsync(connection.HotelId, "Incoming", payload.EventType, job.Payload, 200, true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process webhook job {JobId}", queueId);
            job.Status = 3; // Failed
            job.RetryCount++;
            job.ErrorMessage = ex.Message;

            if (job.RetryCount >= job.MaxRetries)
            {
                // Move to Dead Letter Queue (DLQ)
                var dlq = new ChannelDeadLetterQueue
                {
                    SourceTable = "ChannelWebhookQueue",
                    OriginalEventId = queueId,
                    HotelId = job.HotelId,
                    EventName = "webhook.error",
                    Payload = job.Payload,
                    ErrorMessage = ex.Message,
                    IsActive = true
                };
                _db.ChannelDeadLetterQueues.Add(dlq);
            }
            await _db.SaveChangesAsync();

            await LogSyncAsync(job.HotelId, "Incoming", "webhook.error", job.Payload, 500, false, ex.Message);
        }
    }

    private async Task HandleCancellationAsync(ChannelConnection connection, ChannexBookingData booking)
    {
        var mapping = await _db.ChannelReservationMappings
            .Include(m => m.Reservation)
            .FirstOrDefaultAsync(m => m.ChannelReservationId == booking.Id);

        if (mapping == null) return;

        mapping.OtaStatus = "cancelled";
        mapping.Reservation.IsCancelled = true;
        await _db.SaveChangesAsync();

        // Push real-time inventory update
        await PushAvailabilityAsync(connection.HotelId);
    }

    private async Task HandleUpsertAsync(ChannelConnection connection, ChannexBookingData booking)
    {
        var existingMapping = await _db.ChannelReservationMappings
            .FirstOrDefaultAsync(m => m.ChannelReservationId == booking.Id);

        if (existingMapping != null && existingMapping.Version >= booking.Version)
        {
            // Out of order safety check
            _logger.LogInformation("Discarding out-of-order webhook payload for booking {Id}", booking.Id);
            return;
        }

        // Initialize transaction
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var checkIn = DateOnly.Parse(booking.CheckInDate);
            var checkOut = DateOnly.Parse(booking.CheckOutDate);
            int nights = (checkOut.DayNumber - checkIn.DayNumber);

            var reservation = new SmartAndPro.ERP.Domains.Entities.Reservation.Reservation
            {
                ReferenceNumber = booking.OtaReference ?? Guid.NewGuid().ToString().Substring(0, 8),
                HotelId = connection.HotelId,
                GuestName = booking.Customer.Name,
                GuestPhone = booking.Customer.Phone,
                GuestEmail = booking.Customer.Email,
                Nationality = booking.Customer.Nationality ?? "Saudi",
                ArrivalDate = checkIn,
                DepartureDate = checkOut,
                NumberOfNights = nights,
                IsConfirmed = true,
                IsCancelled = false,
                IsActive = true,
                ReservationRoomEntries = new List<ReservationRoomEntry>()
            };

            // Locate defaults (Meal, View, Seasons)
            var defaultSeason = await _db.ReservationSeasons.FirstOrDefaultAsync();
            var clientCompany = await _db.ReservationCompanies.FirstOrDefaultAsync();

            reservation.ReservationSeasonId = defaultSeason?.Id ?? Guid.NewGuid();
            reservation.ReservationClientCompanyId = clientCompany?.Id ?? Guid.NewGuid();

            foreach (var roomItem in booking.Rooms)
            {
                var roomMap = connection.RoomMappings.FirstOrDefault(r => r.ChannelRoomId == roomItem.RoomTypeId);
                if (roomMap == null) throw new InvalidOperationException($"Room type mapping {roomItem.RoomTypeId} is missing.");

                reservation.ReservationRoomEntries.Add(new ReservationRoomEntry
                {
                    RoomTypeId = roomMap.ErpRoomTypeId,
                    NumberOfRooms = roomItem.Count,
                    RoomRate = roomItem.Rate,
                    RoomCost = roomItem.Rate,
                    IsActive = true
                });
            }

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            if (existingMapping == null)
            {
                var mapping = new ChannelReservationMapping
                {
                    ChannelConnectionId = connection.Id,
                    ErpReservationId = reservation.Id,
                    ChannelReservationId = booking.Id,
                    OtaReservationNumber = booking.OtaReference,
                    OtaName = booking.OtaName,
                    OtaStatus = booking.Status,
                    Version = booking.Version,
                    RawPayload = JsonSerializer.Serialize(booking),
                    IsActive = true
                };

                _db.ChannelReservationMappings.Add(mapping);
                await _db.SaveChangesAsync();

                var payment = new ChannelReservationPayment
                {
                    ChannelReservationMappingId = mapping.Id,
                    PaymentModel = booking.PaymentModel,
                    OriginalCurrency = booking.Currency,
                    ExchangeRate = 1.00m,
                    OtaCommissionAmount = booking.CommissionAmount,
                    VatAmount = booking.TaxAmount,
                    IsActive = true
                };

                if (booking.VccCard != null)
                {
                    payment.EncryptedVccCardNumber = booking.VccCard.CardNumber; // In prod, encrypt using AES-256
                    payment.EncryptedVccCvv = booking.VccCard.Cvv;
                    payment.VccExpiryMonth = booking.VccCard.ExpiryMonth;
                    payment.VccExpiryYear = booking.VccCard.ExpiryYear;
                    payment.VccBalanceAmount = booking.VccCard.Balance;
                    if (DateOnly.TryParse(booking.VccCard.ActivationDate, out var actDate))
                    {
                        payment.VccActivationDate = actDate;
                    }
                }

                _db.ChannelReservationPayments.Add(payment);
            }
            else
            {
                existingMapping.Version = booking.Version;
                existingMapping.OtaStatus = booking.Status;
                existingMapping.RawPayload = JsonSerializer.Serialize(booking);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Recalculate and push availability
            await PushAvailabilityAsync(connection.HotelId);
        }
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task ProcessOutboxJobAsync(Guid outboxId)
    {
        var ev = await _db.ChannelOutboxEvents.FindAsync(outboxId);
        if (ev == null || ev.Status == 2) return;

        ev.Status = 1; // Processing
        ev.LockedBy = Environment.MachineName;
        ev.LockedUntil = DateTime.UtcNow.AddMinutes(5);
        await _db.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        try
        {
            var connection = await _db.ChannelConnections.FirstOrDefaultAsync(c => c.HotelId == ev.HotelId && c.IsActive);
            if (connection == null) throw new InvalidOperationException("Active connection missing.");

            string endpoint = ev.EventName == "availability.update" ? "/availability" : "/restrictions";
            var request = new HttpRequestMessage(HttpMethod.Post, connection.ApiEndpoint + endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.ApiKey);
            request.Content = new StringContent(ev.Payload, Encoding.UTF8, "application/json");

            // Mocking call inside sandbox to prevent network errors, but executing REST flow
            HttpResponseMessage response;
            if (connection.ApiEndpoint.Contains("localhost"))
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            else
            {
                response = await _httpClient.SendAsync(request);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"API returned error code: {response.StatusCode}");
            }

            ev.Status = 2; // Completed
            ev.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await LogSyncAsync(connection.HotelId, "Outgoing", ev.EventName, ev.Payload, (int)response.StatusCode, true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send outbox payload {EventId}", outboxId);
            ev.Status = 3; // Failed
            ev.RetryCount++;
            ev.ErrorMessage = ex.Message;

            if (ev.RetryCount >= ev.MaxRetries)
            {
                var dlq = new ChannelDeadLetterQueue
                {
                    SourceTable = "ChannelOutboxEvents",
                    OriginalEventId = outboxId,
                    HotelId = ev.HotelId,
                    EventName = ev.EventName,
                    Payload = ev.Payload,
                    ErrorMessage = ex.Message,
                    IsActive = true
                };
                _db.ChannelDeadLetterQueues.Add(dlq);
            }
            await _db.SaveChangesAsync();

            await LogSyncAsync(ev.HotelId, "Outgoing", ev.EventName, ev.Payload, 500, false, ex.Message);
        }
    }

    // ═══════════════════════════ Nightly Reconciliation ═══════════════════════════

    public async Task NightlyReconciliationJobAsync()
    {
        var activeHotels = await _db.ChannelConnections
            .Where(c => c.IsActive)
            .Select(c => c.HotelId)
            .ToListAsync();

        foreach (var hotelId in activeHotels)
        {
            _logger.LogInformation("Running nightly reconciliation for hotel {Id}", hotelId);
            await PushAvailabilityAsync(hotelId);
            await PushRatesAsync(hotelId);
        }
    }

    private async Task LogSyncAsync(Guid hotelId, string direction, string eventType, string payload, int responseCode, bool success, string? error)
    {
        var log = new ChannelSyncLog
        {
            HotelId = hotelId,
            Direction = direction,
            EventType = eventType,
            Payload = payload,
            ResponseCode = responseCode,
            IsSuccess = success,
            ErrorMessage = error,
            IsActive = true
        };

        _db.ChannelSyncLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
