using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════════
// Pharmacy Supporting Modules – PDF spec §20
// Insurance Claims, Alerts, Controlled Drugs, Returns, Interactions
// ═══════════════════════════════════════════════════════════════════════

public interface IPharmacySupportService
{
    // Insurance Claims (§20.3)
    Task<object> GetClaimsAsync(Guid? storeId, string? status, string? search, int page, int size);
    Task<object?> GetClaimByIdAsync(Guid id);
    Task<object> CreateClaimAsync(PharmacyInsuranceClaim claim, Guid userId);
    Task<object?> PatchClaimStatusAsync(Guid id, string status, decimal? approvedAmount, string? notes, Guid userId);

    // Alerts (§20.4)
    Task<object> GetAlertsAsync(Guid? storeId, string? state, int page, int size);
    Task<object> CreateAlertAsync(PharmacyAlert alert, Guid userId);
    Task<object?> AssignAlertAsync(Guid id, string owner, Guid userId);
    Task<object?> CloseAlertAsync(Guid id, Guid userId);

    // Controlled Drugs (§20.5)
    Task<object> GetControlledDrugsAsync(Guid? storeId, string? status, string? search, int page, int size);
    Task<object?> GetControlledDrugByIdAsync(Guid id);
    Task<object> CreateControlledDrugAsync(PharmacyControlledDrugRecord record, Guid userId);
    Task<object?> UpdateControlledDrugAsync(Guid id, PharmacyControlledDrugRecord record, Guid userId);
    Task<object?> PatchControlledDrugStatusAsync(Guid id, string status, Guid userId);

    // Returns (§20.2)
    Task<object> GetReturnsAsync(Guid? storeId, string? status, string? search, int page, int size);
    Task<object> CreateReturnAsync(PharmacyReturn ret, Guid userId);
    Task<object?> PatchReturnAsync(Guid id, string? status, decimal? approvedQty, decimal? receivedQty, string? decisionReason, string? notes, Guid userId);

    // Drug Interactions (§20.7)
    Task<object> CheckInteractionsAsync(List<int> itemIds, Guid? patientId);
    Task<object> GetInteractionsAsync(Guid? storeId, int page, int size);
}

public class PharmacySupportService : IPharmacySupportService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacySupportService> _logger;

    public PharmacySupportService(ERPContext db, ILogger<PharmacySupportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Insurance Claims ───────────────────────────────────────────────

    public async Task<object> GetClaimsAsync(Guid? storeId, string? status, string? search, int page, int size)
    {
        var q = _db.PharmacyInsuranceClaims.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(c => c.StoreDetailsId == storeId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(c => c.Status == status);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(c => c.ClaimNo.Contains(search) || c.PatientName.Contains(search) || c.RxNo!.Contains(search));
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(c => c.SubmittedAt).Skip((page - 1) * size).Take(size).ToListAsync();
        return new { total, page, size, data };
    }

    public async Task<object?> GetClaimByIdAsync(Guid id)
        => await _db.PharmacyInsuranceClaims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<object> CreateClaimAsync(PharmacyInsuranceClaim claim, Guid userId)
    {
        claim.Id = Guid.NewGuid();
        claim.CreatedBy = userId;
        claim.CreatedAt = DateTime.UtcNow;
        claim.IsActive = true;
        claim.SubmittedAt = DateTime.UtcNow;
        if (string.IsNullOrEmpty(claim.ClaimNo))
            claim.ClaimNo = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        _db.PharmacyInsuranceClaims.Add(claim);
        await _db.SaveChangesAsync();
        return claim;
    }

    public async Task<object?> PatchClaimStatusAsync(Guid id, string status, decimal? approvedAmount, string? notes, Guid userId)
    {
        var claim = await _db.PharmacyInsuranceClaims.FirstOrDefaultAsync(c => c.Id == id);
        if (claim is null) return null;
        claim.Status = status;
        if (approvedAmount.HasValue) claim.ApprovedAmount = approvedAmount.Value;
        if (notes != null) claim.Notes = notes;
        if (status is "Approved" or "Rejected") claim.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return claim;
    }

    // ── Alerts ─────────────────────────────────────────────────────────

    public async Task<object> GetAlertsAsync(Guid? storeId, string? state, int page, int size)
    {
        var q = _db.PharmacyAlerts.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(a => a.StoreDetailsId == storeId);
        if (!string.IsNullOrEmpty(state)) q = q.Where(a => a.State == state);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * size).Take(size).ToListAsync();
        return new { total, page, size, data };
    }

    public async Task<object> CreateAlertAsync(PharmacyAlert alert, Guid userId)
    {
        alert.Id = Guid.NewGuid();
        alert.CreatedBy = userId;
        alert.CreatedAt = DateTime.UtcNow;
        alert.IsActive = true;
        if (string.IsNullOrEmpty(alert.Code))
            alert.Code = $"ALR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        _db.PharmacyAlerts.Add(alert);
        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task<object?> AssignAlertAsync(Guid id, string owner, Guid userId)
    {
        var alert = await _db.PharmacyAlerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert is null) return null;
        alert.Owner = owner;
        alert.State = "Assigned";
        await _db.SaveChangesAsync();
        return alert;
    }

    public async Task<object?> CloseAlertAsync(Guid id, Guid userId)
    {
        var alert = await _db.PharmacyAlerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert is null) return null;
        alert.State = "Closed";
        await _db.SaveChangesAsync();
        return alert;
    }

    // ── Controlled Drugs ───────────────────────────────────────────────

    public async Task<object> GetControlledDrugsAsync(Guid? storeId, string? status, string? search, int page, int size)
    {
        var q = _db.PharmacyControlledDrugRecords.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(r => r.StoreDetailsId == storeId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(r => r.DrugName.Contains(search) || r.PatientName.Contains(search) || r.LogNo.Contains(search));
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(r => r.DispenseDate).Skip((page - 1) * size).Take(size).ToListAsync();

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

    public async Task<object?> GetControlledDrugByIdAsync(Guid id)
        => await _db.PharmacyControlledDrugRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<object> CreateControlledDrugAsync(PharmacyControlledDrugRecord record, Guid userId)
    {
        record.Id = Guid.NewGuid();
        record.CreatedBy = userId;
        record.CreatedAt = DateTime.UtcNow;
        record.IsActive = true;
        if (string.IsNullOrEmpty(record.LogNo))
            record.LogNo = $"CD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        _db.PharmacyControlledDrugRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<object?> UpdateControlledDrugAsync(Guid id, PharmacyControlledDrugRecord updated, Guid userId)
    {
        var r = await _db.PharmacyControlledDrugRecords.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return null;
        r.PatientName = updated.PatientName;
        r.DrugName = updated.DrugName;
        r.Schedule = updated.Schedule;
        r.Quantity = updated.Quantity;
        r.Prescriber = updated.Prescriber;
        r.Pharmacist = updated.Pharmacist;
        r.PermitNo = updated.PermitNo;
        r.Reason = updated.Reason;
        r.Notes = updated.Notes;
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task<object?> PatchControlledDrugStatusAsync(Guid id, string status, Guid userId)
    {
        var r = await _db.PharmacyControlledDrugRecords.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return null;
        r.Status = status;
        await _db.SaveChangesAsync();
        return r;
    }

    // ── Returns ────────────────────────────────────────────────────────

    public async Task<object> GetReturnsAsync(Guid? storeId, string? status, string? search, int page, int size)
    {
        var q = _db.PharmacyReturns.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(r => r.StoreDetailsId == storeId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(r => r.ItemCode.Contains(search) || r.ItemName.Contains(search) || r.ReturnNo.Contains(search));
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(r => r.ReturnDate).Skip((page - 1) * size).Take(size).ToListAsync();
        return new { total, page, size, data };
    }

    public async Task<object> CreateReturnAsync(PharmacyReturn ret, Guid userId)
    {
        ret.Id = Guid.NewGuid();
        ret.CreatedBy = userId;
        ret.CreatedAt = DateTime.UtcNow;
        ret.IsActive = true;
        if (string.IsNullOrEmpty(ret.ReturnNo))
            ret.ReturnNo = $"RET-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        _db.PharmacyReturns.Add(ret);
        await _db.SaveChangesAsync();
        return ret;
    }

    public async Task<object?> PatchReturnAsync(Guid id, string? status, decimal? approvedQty, decimal? receivedQty, string? decisionReason, string? notes, Guid userId)
    {
        var r = await _db.PharmacyReturns.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return null;
        if (!string.IsNullOrEmpty(status)) r.Status = status;
        // Note: approvedQty, receivedQty, decisionReason would need new fields on entity
        if (!string.IsNullOrEmpty(notes)) r.Reason = notes; // Map to existing reason field
        await _db.SaveChangesAsync();
        return r;
    }

    // ── Drug Interactions ──────────────────────────────────────────────

    public async Task<object> CheckInteractionsAsync(List<int> itemIds, Guid? patientId)
    {
        // Check known interactions for provided item IDs
        var interactions = await _db.PharmacyDrugInteractions.AsNoTracking()
            .Where(i => i.IsActive)
            .ToListAsync();

        // Get item names for matching
        var itemCodes = new List<string>();
        foreach (var itemId in itemIds)
        {
            var item = await _db.MainPurchaseClasses.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == (Guid)(object)itemId);
            if (item != null)
                itemCodes.Add(item.EnglishName);
        }

        var found = interactions.Where(i =>
            itemCodes.Any(c => i.PrimaryDrug.Contains(c, StringComparison.OrdinalIgnoreCase) ||
                               i.InteractingDrug.Contains(c, StringComparison.OrdinalIgnoreCase))
        ).Select(i => new
        {
            firstDrug = i.PrimaryDrug,
            secondDrug = i.InteractingDrug,
            severity = i.Severity,
            message = i.RiskDescription
        }).ToList();

        return new { interactions = found, count = found.Count };
    }

    public async Task<object> GetInteractionsAsync(Guid? storeId, int page, int size)
    {
        var q = _db.PharmacyDrugInteractions.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(i => i.StoreDetailsId == storeId);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(i => i.CheckedAt).Skip((page - 1) * size).Take(size).ToListAsync();
        return new { total, page, size, data };
    }
}
