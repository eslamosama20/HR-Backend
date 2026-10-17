using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Domains.Entities.ChannelManager;
using SmartAndPro.ERP.WebAPI.Services.ChannelManager;

namespace SmartAndPro.ERP.WebAPI.Controllers.Integrations.ChannelManager;

[ApiController]
[Route("api/integrations/channel-manager")]
public class ChannelManagerController : ControllerBase
{
    private readonly IChannelManagerService _channelService;
    private readonly ERPContext _db;

    public ChannelManagerController(IChannelManagerService channelService, ERPContext db)
    {
        _channelService = channelService;
        _db = db;
    }

    /// <summary>
    /// GET /api/integrations/channel-manager/connections
    /// Returns all active channel manager connections with hotel info.
    /// </summary>
    [HttpGet("connections")]
    public async Task<IActionResult> GetConnections()
    {
        var connections = await _db.ChannelConnections
            .Include(c => c.Hotel)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(connections);
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/connections
    /// Upserts a channel manager connection configuration.
    /// </summary>
    [HttpPost("connections")]
    public async Task<IActionResult> SaveConnection([FromBody] ChannelConnectionDto dto)
    {
        if (dto.HotelId == Guid.Empty)
            return BadRequest("HotelId is required.");

        var connection = await _db.ChannelConnections
            .FirstOrDefaultAsync(c => c.HotelId == dto.HotelId);

        bool isNew = false;
        if (connection == null)
        {
            isNew = true;
            connection = new ChannelConnection
            {
                Id = Guid.NewGuid(),
                HotelId = dto.HotelId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Parse("0cd8de11-1172-407b-dd3c-08dda8a3edc7"), // Default admin ID
                IsActive = true
            };
        }

        connection.ProviderName = dto.ProviderName ?? "Channex";
        connection.ApiKey = dto.ApiKey;
        connection.WebhookSecret = dto.WebhookSecret;
        connection.ChannelPropertyId = dto.ChannelPropertyId;
        connection.ApiEndpoint = dto.ApiEndpoint ?? "https://api.channex.io/api/v1";
        connection.Salt = dto.Salt ?? "default_salt";
        connection.ConnectionCode = dto.ConnectionCode ?? "default_code";

        if (isNew)
            _db.ChannelConnections.Add(connection);

        await _db.SaveChangesAsync();
        return Ok(connection);
    }

    /// <summary>
    /// GET /api/integrations/channel-manager/room-mappings?connectionId=GUID
    /// </summary>
    [HttpGet("room-mappings")]
    public async Task<IActionResult> GetRoomMappings([FromQuery] Guid connectionId)
    {
        var mappings = await _db.ChannelRoomMappings
            .Include(m => m.RoomType)
            .Where(m => m.ChannelConnectionId == connectionId)
            .ToListAsync();

        return Ok(mappings);
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/room-mappings
    /// Replaces all room mappings for a connection.
    /// </summary>
    [HttpPost("room-mappings")]
    public async Task<IActionResult> SaveRoomMappings([FromBody] SaveRoomMappingsDto dto)
    {
        if (dto.ChannelConnectionId == Guid.Empty)
            return BadRequest("ChannelConnectionId is required.");

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var oldMappings = await _db.ChannelRoomMappings
                .Where(m => m.ChannelConnectionId == dto.ChannelConnectionId)
                .ToListAsync();

            _db.ChannelRoomMappings.RemoveRange(oldMappings);

            foreach (var mapDto in dto.Mappings)
            {
                var mapping = new ChannelRoomMapping
                {
                    Id = Guid.NewGuid(),
                    ChannelConnectionId = dto.ChannelConnectionId,
                    ErpRoomTypeId = mapDto.ErpRoomTypeId,
                    ChannelRoomId = mapDto.ChannelRoomId,
                    RoomName = mapDto.RoomName,
                    MaxAllocationOverride = mapDto.MaxAllocationOverride,
                    IsActive = true,
                    CreatedBy = Guid.Parse("0cd8de11-1172-407b-dd3c-08dda8a3edc7"),
                    CreatedAt = DateTime.UtcNow
                };
                _db.ChannelRoomMappings.Add(mapping);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { message = "Room mappings saved successfully." });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// GET /api/integrations/channel-manager/rate-mappings?connectionId=GUID
    /// </summary>
    [HttpGet("rate-mappings")]
    public async Task<IActionResult> GetRateMappings([FromQuery] Guid connectionId)
    {
        var mappings = await _db.ChannelRateMappings
            .Include(m => m.ReservationSeason)
            .Where(m => m.ChannelConnectionId == connectionId)
            .ToListAsync();

        return Ok(mappings);
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/rate-mappings
    /// Replaces all rate mappings for a connection.
    /// </summary>
    [HttpPost("rate-mappings")]
    public async Task<IActionResult> SaveRateMappings([FromBody] SaveRateMappingsDto dto)
    {
        if (dto.ChannelConnectionId == Guid.Empty)
            return BadRequest("ChannelConnectionId is required.");

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var oldMappings = await _db.ChannelRateMappings
                .Where(m => m.ChannelConnectionId == dto.ChannelConnectionId)
                .ToListAsync();

            _db.ChannelRateMappings.RemoveRange(oldMappings);

            foreach (var mapDto in dto.Mappings)
            {
                var mapping = new ChannelRateMapping
                {
                    Id = Guid.NewGuid(),
                    ChannelConnectionId = dto.ChannelConnectionId,
                    ErpSeasonId = mapDto.ErpSeasonId,
                    ChannelRateId = mapDto.ChannelRateId,
                    RateName = mapDto.RateName,
                    MarkupPercentage = mapDto.MarkupPercentage,
                    IsActive = true,
                    CreatedBy = Guid.Parse("0cd8de11-1172-407b-dd3c-08dda8a3edc7"),
                    CreatedAt = DateTime.UtcNow
                };
                _db.ChannelRateMappings.Add(mapping);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { message = "Rate mappings saved successfully." });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// GET /api/integrations/channel-manager/logs
    /// Returns recent integration logs for audit/reconciliation tracking.
    /// </summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] Guid? hotelId)
    {
        var query = _db.ChannelSyncLogs.AsQueryable();
        if (hotelId.HasValue && hotelId != Guid.Empty)
        {
            query = query.Where(l => l.HotelId == hotelId.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// GET /api/integrations/channel-manager/dead-letters
    /// Returns DLQ errors for manual inspection.
    /// </summary>
    [HttpGet("dead-letters")]
    public async Task<IActionResult> GetDeadLetters([FromQuery] Guid? hotelId)
    {
        var query = _db.ChannelDeadLetterQueues.AsQueryable();
        if (hotelId.HasValue && hotelId != Guid.Empty)
        {
            query = query.Where(dlq => dlq.HotelId == hotelId.Value);
        }

        var errors = await query
            .OrderByDescending(dlq => dlq.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(errors);
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/sync-availability?hotelId=GUID
    /// </summary>
    [HttpPost("sync-availability")]
    public async Task<IActionResult> SyncAvailability([FromQuery] Guid hotelId)
    {
        if (hotelId == Guid.Empty)
            return BadRequest("HotelId is required.");

        var result = await _channelService.PushAvailabilityAsync(hotelId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { message = $"Availability sync queued successfully. Processed {result.ProcessedCount} date-room mappings." });
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/sync-rates?hotelId=GUID
    /// </summary>
    [HttpPost("sync-rates")]
    public async Task<IActionResult> SyncRates([FromQuery] Guid hotelId)
    {
        if (hotelId == Guid.Empty)
            return BadRequest("HotelId is required.");

        var result = await _channelService.PushRatesAsync(hotelId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new { message = $"Rates sync queued successfully. Processed {result.ProcessedCount} date-rate mappings." });
    }

    /// <summary>
    /// POST /api/integrations/channel-manager/reconcile
    /// </summary>
    [HttpPost("reconcile")]
    public async Task<IActionResult> Reconcile()
    {
        await _channelService.NightlyReconciliationJobAsync();
        return Ok(new { message = "Full database audit and reconciliation job triggered." });
    }
}

// DTO Declarations
public class ChannelConnectionDto
{
    public Guid HotelId { get; set; }
    public string? ProviderName { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ChannelPropertyId { get; set; } = string.Empty;
    public string? ApiEndpoint { get; set; }
    public string? Salt { get; set; }
    public string? ConnectionCode { get; set; }
}

public class SaveRoomMappingsDto
{
    public Guid ChannelConnectionId { get; set; }
    public List<RoomMappingItemDto> Mappings { get; set; } = new();
}

public class RoomMappingItemDto
{
    public Guid ErpRoomTypeId { get; set; }
    public string ChannelRoomId { get; set; } = string.Empty;
    public string? RoomName { get; set; }
    public int? MaxAllocationOverride { get; set; }
}

public class SaveRateMappingsDto
{
    public Guid ChannelConnectionId { get; set; }
    public List<RateMappingItemDto> Mappings { get; set; } = new();
}

public class RateMappingItemDto
{
    public Guid ErpSeasonId { get; set; }
    public string ChannelRateId { get; set; } = string.Empty;
    public string? RateName { get; set; }
    public decimal MarkupPercentage { get; set; }
}

