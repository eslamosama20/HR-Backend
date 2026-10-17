using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════
// Pharmacy Alerts Service
// ═══════════════════════════════════════════════════════════════════
public interface IPharmacyAlertsService
{
    Task<object> GetAllAsync(Guid? storeDetailsId, string? type, string? level, string? state, string? search, int page, int size);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(PharmacyAlert alert, Guid userId);
    Task<bool> AssignAsync(Guid id, string owner, Guid userId);
    Task<bool> CloseAsync(Guid id, Guid userId);
}

public class PharmacyAlertsService : IPharmacyAlertsService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyAlertsService> _logger;

    public PharmacyAlertsService(ERPContext db, ILogger<PharmacyAlertsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<object> GetAllAsync(Guid? storeDetailsId, string? type, string? level, string? state, string? search, int page, int size)
    {
        try
        {
            var q = _db.PharmacyAlerts.AsNoTracking().AsQueryable();
            if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
            if (!string.IsNullOrEmpty(type)) q = q.Where(x => x.Type == type);
            if (!string.IsNullOrEmpty(level)) q = q.Where(x => x.Level == level);
            if (!string.IsNullOrEmpty(state)) q = q.Where(x => x.State == state);
            if (!string.IsNullOrEmpty(search))
                q = q.Where(x => x.Code.Contains(search) || x.Title.Contains(search));

            var totalCount = await q.CountAsync();
            var data = await q.OrderByDescending(x => x.CreatedAt)
                              .Skip((page - 1) * size).Take(size)
                              .ToListAsync();

            // Enum fallback: map any unknown/invalid enum values to safe defaults
            var items = data.Select(x => new
            {
                id = x.Id,
                code = x.Code ?? "",
                type = NormalizeAlertType(x.Type),
                level = NormalizeAlertLevel(x.Level),
                title = x.Title ?? "",
                message = x.Message ?? "",
                createdAt = x.CreatedAt
            }).ToList();

            return new { items, totalCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync alerts failed");
            return new { items = new List<object>(), totalCount = 0 };
        }
    }

    private static string NormalizeAlertType(string? raw) => raw?.ToLowerInvariant() switch
    {
        "stock" or "expiry" or "interaction" or "recall" => raw.ToLowerInvariant(),
        _ => string.IsNullOrEmpty(raw) ? "stock" : raw.ToLowerInvariant()
    };

    private static string NormalizeAlertLevel(string? raw) => raw?.ToLowerInvariant() switch
    {
        "critical" or "warning" or "info" => raw.ToLowerInvariant(),
        "high" => "critical",
        "medium" => "warning",
        "low" => "info",
        _ => string.IsNullOrEmpty(raw) ? "info" : raw.ToLowerInvariant()
    };

    public async Task<object?> GetByIdAsync(Guid id)
        => await _db.PharmacyAlerts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<object> CreateAsync(PharmacyAlert alert, Guid userId)
    {
        alert.Id = Guid.NewGuid();
        alert.CreatedBy = userId;
        alert.CreatedAt = DateTime.UtcNow;
        alert.IsActive = true;

        if (string.IsNullOrEmpty(alert.Code))
            alert.Code = $"ALR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        _db.PharmacyAlerts.Add(alert);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Alert {Code} created: {Title}", alert.Code, alert.Title);
        return alert;
    }

    public async Task<bool> AssignAsync(Guid id, string owner, Guid userId)
    {
        var alert = await _db.PharmacyAlerts.FirstOrDefaultAsync(x => x.Id == id);
        if (alert is null) return false;
        alert.Owner = owner;
        alert.State = "Assigned";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseAsync(Guid id, Guid userId)
    {
        var alert = await _db.PharmacyAlerts.FirstOrDefaultAsync(x => x.Id == id);
        if (alert is null) return false;
        alert.State = "Closed";
        await _db.SaveChangesAsync();
        return true;
    }
}
