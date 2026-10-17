using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════
// Controlled Drugs Service
// ═══════════════════════════════════════════════════════════════════
public interface IPharmacyControlledDrugsService
{
    Task<object> GetAllAsync(Guid? storeDetailsId, string? schedule, string? status, string? search, int page, int size);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(PharmacyControlledDrugRecord record, Guid userId);
}

public class PharmacyControlledDrugsService : IPharmacyControlledDrugsService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyControlledDrugsService> _logger;

    public PharmacyControlledDrugsService(ERPContext db, ILogger<PharmacyControlledDrugsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<object> GetAllAsync(Guid? storeDetailsId, string? schedule, string? status, string? search, int page, int size)
    {
        var q = _db.PharmacyControlledDrugRecords.AsNoTracking().AsQueryable();
        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
        if (!string.IsNullOrEmpty(schedule)) q = q.Where(x => x.Schedule == schedule);
        if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(x => x.LogNo.Contains(search) || x.PatientName.Contains(search) || x.DrugName.Contains(search));

        var total = await q.CountAsync();
        var data = await q.OrderByDescending(x => x.DispenseDate)
                          .Skip((page - 1) * size).Take(size)
                          .ToListAsync();

        var userIds = data.Select(x => x.CreatedBy).Where(id => id != Guid.Empty).Distinct().ToList();
        var userMap = new Dictionary<Guid, string>();
        if (userIds.Count > 0)
        {
            var users = await _db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FirstName, u.LastName })
                .ToListAsync();

            foreach (var u in users)
            {
                var fn = $"{u.FirstName} {u.LastName}".Trim();
                userMap[u.Id] = !string.IsNullOrEmpty(fn) ? fn : (u.UserName ?? "User");
            }
        }

        foreach (var item in data)
        {
            if (string.IsNullOrEmpty(item.Pharmacist) || item.Pharmacist == "Cashier" || item.Pharmacist == "Prescription Checkout")
            {
                if (userMap.TryGetValue(item.CreatedBy, out var uName))
                    item.Pharmacist = uName;
                else
                    item.Pharmacist = "Admin";
            }
        }

        return new { total, page, size, data };
    }

    public async Task<object?> GetByIdAsync(Guid id)
        => await _db.PharmacyControlledDrugRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<object> CreateAsync(PharmacyControlledDrugRecord record, Guid userId)
    {
        record.Id = Guid.NewGuid();
        record.CreatedBy = userId;
        record.CreatedAt = DateTime.UtcNow;
        record.IsActive = true;

        if (string.IsNullOrEmpty(record.LogNo))
            record.LogNo = $"CDR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        _db.PharmacyControlledDrugRecords.Add(record);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Controlled drug record {LogNo} created for {Drug}", record.LogNo, record.DrugName);
        return record;
    }
}
