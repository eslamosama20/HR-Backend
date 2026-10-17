using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════════
// Restricted Drugs Service
// Endpoints:
//   GET  /api/v1/pharmacy/restricted-drugs/lookup-stock
//   POST /api/v1/pharmacy/restricted-drugs/dispense
//   GET  /api/v1/pharmacy/restricted-drugs/dispenses
//   PATCH /api/v1/pharmacy/restricted-drugs/dispenses/{id}/status
// ═══════════════════════════════════════════════════════════════════════

#region DTOs

public class RestrictedDrugStockItemDto
{
    public Guid? ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal AvailableQty { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? RestrictionType { get; set; }
}

public class DispenseRestrictedDrugRequest
{
    public Guid? StoreDetailsId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? PharmacistName { get; set; }
    public string? AuthorizationNo { get; set; }
    public string Status { get; set; } = "pending";
    public string? DispenseReason { get; set; }
}

public class RestrictedDrugDispenseDto
{
    public Guid Id { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? PharmacistName { get; set; }
    public string? AuthorizationNo { get; set; }
    public string? DispenseReason { get; set; }
    public string? RestrictionType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionItemDetailDto>? PrescriptionItems { get; set; }
}

public class PrescriptionItemDetailDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal RequestedQty { get; set; }
    public decimal DispensedQty { get; set; }
    public decimal UnitPrice { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsControlled { get; set; }
}

public class UpdateDispenseStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
}

#endregion

public interface IRestrictedDrugsService
{
    Task<List<RestrictedDrugStockItemDto>> LookupStockAsync(Guid? storeDetailsId, string? search);
    Task<(RestrictedDrugDispenseDto? result, string? error)> DispenseAsync(DispenseRestrictedDrugRequest req, Guid userId);
    Task<object> GetDispensesAsync(Guid? storeDetailsId, string? status, string? search, int page, int size);
    Task<(bool ok, string? error)> UpdateStatusAsync(Guid id, UpdateDispenseStatusRequest req, Guid userId);
}

public class RestrictedDrugsService : IRestrictedDrugsService
{
    private readonly ERPContext _db;
    private readonly ILogger<RestrictedDrugsService> _logger;

    private static readonly HashSet<string> ValidStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        { "pending", "approved", "rejected", "dispensed", "cancelled" };

    public RestrictedDrugsService(ERPContext db, ILogger<RestrictedDrugsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── 1. Lookup stock ──────────────────────────────────────────────
    /// <summary>
    /// Returns available pharmacy batches for medication items only,
    /// filtered to non-expired, quantity > 0, optionally by store and search term.
    /// </summary>
    public async Task<List<RestrictedDrugStockItemDto>> LookupStockAsync(Guid? storeDetailsId, string? search)
    {
        // PharmacyBatch is the real-time stock table populated by CreateTransaction
        var q = _db.PharmacyBatches.AsNoTracking()
            .Where(b => b.Quantity > 0 && b.ExpiryDate > DateTime.UtcNow);

        if (storeDetailsId.HasValue)
            q = q.Where(b => b.StoreDetailsId == storeDetailsId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(b => b.ItemCode.Contains(term) || b.ItemName.Contains(term));
        }

        var batches = await q.OrderBy(b => b.ExpiryDate).ToListAsync();

        // Enrich: resolve ItemId from MainPurchaseClass and check IsPharmacyItem / restriction type
        var itemCodes = batches.Select(b => b.ItemCode).Distinct().ToList();
        var masterItems = await _db.MainPurchaseClasses.AsNoTracking()
            .Where(m => itemCodes.Contains(m.ReferenceCode))
            .ToListAsync();

        var result = new List<RestrictedDrugStockItemDto>();
        foreach (var b in batches)
        {
            var master = masterItems.FirstOrDefault(m => m.ReferenceCode == b.ItemCode);

            // Only medication items
            if (master != null && master.ItemType != ItemType.Medication)
                continue;

            result.Add(new RestrictedDrugStockItemDto
            {
                ItemId = master?.Id,
                ItemCode = b.ItemCode,
                ItemName = b.ItemName,
                AvailableQty = b.Quantity,
                BatchNo = b.BatchNo,
                ExpiryDate = b.ExpiryDate,
                RestrictionType = null,
            });
        }
        return result;
    }

    // ── 2. Dispense ──────────────────────────────────────────────────
    public async Task<(RestrictedDrugDispenseDto? result, string? error)> DispenseAsync(
        DispenseRestrictedDrugRequest req, Guid userId)
    {
        // --- Validate quantity
        if (req.Quantity <= 0)
            return (null, "Quantity must be greater than zero.");

        // --- Find batch
        var batchQ = _db.PharmacyBatches
            .Where(b => b.ItemCode == req.ItemCode && b.IsActive);

        if (req.StoreDetailsId.HasValue)
            batchQ = batchQ.Where(b => b.StoreDetailsId == req.StoreDetailsId);

        if (!string.IsNullOrWhiteSpace(req.BatchNo))
            batchQ = batchQ.Where(b => b.BatchNo == req.BatchNo);

        var batch = await batchQ.OrderBy(b => b.ExpiryDate).FirstOrDefaultAsync();

        // --- Validate batch exists
        if (batch == null)
            return (null, $"No matching batch found for item '{req.ItemCode}' in the selected store.");

        // --- Validate expiry
        if (batch.ExpiryDate <= DateTime.UtcNow)
            return (null, $"Batch '{batch.BatchNo}' has expired on {batch.ExpiryDate:yyyy-MM-dd}.");

        // --- Validate stock
        if (batch.Quantity < req.Quantity)
            return (null, $"Insufficient stock. Available: {batch.Quantity}, Requested: {req.Quantity}.");

        // --- Deduct stock
        batch.Quantity -= req.Quantity;
        if (batch.Quantity == 0)
            batch.Status = "Exhausted";
        _db.PharmacyBatches.Update(batch);

        // --- Resolve restriction type
        var master = await _db.MainPurchaseClasses.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ReferenceCode == req.ItemCode);

        // --- Create dispense record
        var entity = new RestrictedDrugDispense
        {
            ItemId = req.ItemId ?? master?.Id,
            ItemCode = req.ItemCode,
            ItemName = req.ItemName,
            BatchNo = batch.BatchNo,
            ExpiryDate = batch.ExpiryDate,
            Quantity = req.Quantity,
            StoreDetailsId = req.StoreDetailsId ?? batch.StoreDetailsId,
            PatientName = req.PatientName,
            DoctorName = req.DoctorName,
            PharmacistName = req.PharmacistName,
            AuthorizationNo = req.AuthorizationNo,
            DispenseReason = req.DispenseReason,
            RestrictionType = null,
            Status = string.IsNullOrWhiteSpace(req.Status) ? "pending" : req.Status.ToLower(),
            DispensedBy = userId,
            DispensedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        _db.RestrictedDrugDispenses.Add(entity);

        // --- Audit log (reuse PharmacyControlledDrugRecord table)
        _db.PharmacyControlledDrugRecords.Add(new PharmacyControlledDrugRecord
        {
            LogNo = $"RD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            PatientName = req.PatientName ?? string.Empty,
            DrugName = req.ItemName,
            Schedule = "restricted",
            Quantity = req.Quantity,
            Prescriber = req.DoctorName ?? string.Empty,
            Pharmacist = req.PharmacistName ?? string.Empty,
            PermitNo = req.AuthorizationNo,
            Reason = req.DispenseReason,
            Status = "Active",
            DispenseDate = DateTime.UtcNow,
            StoreDetailsId = entity.StoreDetailsId,
        });

        await _db.SaveChangesAsync();

        return (MapToDto(entity), null);
    }

    // ── 3. List dispenses ────────────────────────────────────────────
    public async Task<object> GetDispensesAsync(
        Guid? storeDetailsId, string? status, string? search, int page, int size)
    {
        var q = _db.RestrictedDrugDispenses.AsNoTracking().AsQueryable();

        if (storeDetailsId.HasValue)
            q = q.Where(d => d.StoreDetailsId == storeDetailsId);

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(d => d.Status == status.ToLower());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(d => d.ItemCode.Contains(term)
                           || d.ItemName.Contains(term)
                           || (d.PatientName != null && d.PatientName.Contains(term))
                           || (d.BatchNo != null && d.BatchNo.Contains(term)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var rxNos = items.Select(d => d.AuthorizationNo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        var rxMap = await _db.PharmacyPrescriptions.AsNoTracking()
            .Include(p => p.Items)
            .Where(p => rxNos.Contains(p.RxNo))
            .ToDictionaryAsync(p => p.RxNo);

        var userIds = items.Select(d => d.DispensedBy).Where(id => id.HasValue && id.Value != Guid.Empty).Select(id => id!.Value)
            .Concat(items.Select(d => d.CreatedBy).Where(id => id != Guid.Empty))
            .Distinct().ToList();

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

        var data = items.Select(e =>
        {
            var dto = MapToDto(e);

            if (string.IsNullOrWhiteSpace(dto.PharmacistName) || dto.PharmacistName == "Prescription Checkout" || dto.PharmacistName == "Cashier")
            {
                if (e.DispensedBy.HasValue && userMap.TryGetValue(e.DispensedBy.Value, out var uName))
                {
                    dto.PharmacistName = uName;
                }
                else if (userMap.TryGetValue(e.CreatedBy, out var cName))
                {
                    dto.PharmacistName = cName;
                }
                else
                {
                    dto.PharmacistName = "Admin";
                }
            }

            if (!string.IsNullOrEmpty(e.AuthorizationNo) && rxMap.TryGetValue(e.AuthorizationNo, out var rx))
            {
                dto.PrescriptionItems = rx.Items.Select(i => new PrescriptionItemDetailDto
                {
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    RequestedQty = i.RequestedQty,
                    DispensedQty = i.DispensedQty,
                    UnitPrice = i.UnitPrice,
                    BatchNo = i.BatchNo,
                    ExpiryDate = i.ExpiryDate,
                    IsControlled = i.IsControlled
                }).ToList();
            }
            return dto;
        }).ToList();

        return new { total, page, size, data };
    }

    // ── 4. Update status ─────────────────────────────────────────────
    public async Task<(bool ok, string? error)> UpdateStatusAsync(
        Guid id, UpdateDispenseStatusRequest req, Guid userId)
    {
        if (!ValidStatuses.Contains(req.Status))
            return (false, $"Invalid status '{req.Status}'. Valid values: {string.Join(", ", ValidStatuses)}.");

        var entity = await _db.RestrictedDrugDispenses.FirstOrDefaultAsync(d => d.Id == id);
        if (entity == null)
            return (false, null); // caller returns 404

        var newStatus = req.Status.ToLower();
        entity.Status = newStatus;

        if (newStatus == "approved")
        {
            entity.ApprovedBy = userId;
            entity.ApprovedAt = DateTime.UtcNow;
        }
        else if (newStatus == "rejected")
        {
            entity.RejectionReason = req.RejectionReason;
        }
        else if (newStatus == "dispensed")
        {
            entity.DispensedBy = userId;
            entity.DispensedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return (true, null);
    }

    // ── Mapping helper ───────────────────────────────────────────────
    private static RestrictedDrugDispenseDto MapToDto(RestrictedDrugDispense e) => new()
    {
        Id = e.Id,
        ItemId = e.ItemId,
        ItemCode = e.ItemCode,
        ItemName = e.ItemName,
        BatchNo = e.BatchNo,
        ExpiryDate = e.ExpiryDate,
        Quantity = e.Quantity,
        StoreDetailsId = e.StoreDetailsId,
        PatientName = e.PatientName,
        DoctorName = e.DoctorName,
        PharmacistName = e.PharmacistName,
        AuthorizationNo = e.AuthorizationNo,
        DispenseReason = e.DispenseReason,
        RestrictionType = e.RestrictionType,
        Status = e.Status,
        RejectionReason = e.RejectionReason,
        ApprovedAt = e.ApprovedAt,
        DispensedAt = e.DispensedAt,
        CreatedAt = e.CreatedAt,
    };
}
