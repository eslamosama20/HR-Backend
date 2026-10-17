using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════════
// Pharmacy Inventory Service – PDF spec §9, §10
// ═══════════════════════════════════════════════════════════════════════
public interface IPharmacyInventoryService
{
    Task<object> GetInventoryStocksAsync(Guid? storeId, Guid? productId, string? searchTerm,
        bool includeExpired, bool onlyAvailable, string? batchNo, DateTime? expiryDate,
        int page, int pageSize);

    Task<object> GetBatchesAsync(Guid? storeId, string? search, int page, int pageSize);
    Task<object?> GetBatchByIdAsync(Guid id);
}

public class PharmacyInventoryService : IPharmacyInventoryService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyInventoryService> _logger;

    public PharmacyInventoryService(ERPContext db, ILogger<PharmacyInventoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET /v1/pharmacy/inventory — Unified pharmacy inventory read model (PDF §10)
    /// Returns per-store, per-item, per-batch stock rows with FEFO ordering.
    /// </summary>
    public async Task<object> GetInventoryStocksAsync(
        Guid? storeId, Guid? productId, string? searchTerm,
        bool includeExpired, bool onlyAvailable, string? batchNo, DateTime? expiryDate,
        int page, int pageSize)
    {
        // Build stock from PharmacyBatch joined with item master & store
        var q = _db.PharmacyBatches.AsNoTracking().AsQueryable();

        // Only pharmacy stores
        if (storeId.HasValue)
            q = q.Where(b => b.StoreDetailsId == storeId);

        if (productId.HasValue)
        {
            // Match by MainPurchaseClass code
            var item = await _db.MainPurchaseClasses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productId.Value);
            if (item != null)
                q = q.Where(b => b.ItemCode == item.ReferenceCode);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
            q = q.Where(b => b.ItemCode.Contains(searchTerm) || b.ItemName.Contains(searchTerm));

        if (!string.IsNullOrWhiteSpace(batchNo))
            q = q.Where(b => b.BatchNo == batchNo);

        if (expiryDate.HasValue)
            q = q.Where(b => b.ExpiryDate.Date == expiryDate.Value.Date);

        if (!includeExpired)
            q = q.Where(b => b.ExpiryDate > DateTime.UtcNow);

        if (onlyAvailable)
            q = q.Where(b => b.Quantity > 0);

        // FEFO order: earliest expiry first
        q = q.OrderBy(b => b.ExpiryDate).ThenBy(b => b.ItemCode);

        var total = await q.CountAsync();
        var batches = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        // Enrich with item master data
        var itemCodes = batches.Select(b => b.ItemCode).Distinct().ToList();
        var items = await _db.MainPurchaseClasses.AsNoTracking()
            .Where(m => itemCodes.Contains(m.ReferenceCode))
            .ToListAsync();

        var storeIds = batches.Where(b => b.StoreDetailsId.HasValue)
            .Select(b => b.StoreDetailsId!.Value).Distinct().ToList();
        var stores = await _db.StoreDetails.AsNoTracking()
            .Where(s => storeIds.Contains(s.Id))
            .ToListAsync();

        var data = batches.Select(b =>
        {
            var item = items.FirstOrDefault(i => i.ReferenceCode == b.ItemCode);
            var store = stores.FirstOrDefault(s => s.Id == b.StoreDetailsId);
            return new
            {
                productId = item?.Id,
                itemCode = b.ItemCode,
                itemName = b.ItemName,
                storeId = b.StoreDetailsId,
                storeName = store?.ArabicName ?? b.StoreName,
                storeCategory = store?.StoreCategory ?? "general",
                availableQuantity = b.Quantity,
                unitPrice = 0m, // Will be enriched from latest purchase
                needsBatch = item?.NeedsBatch ?? false,
                needsExpiry = item?.NeedsExpiry ?? false,
                isPharmacyItem = item?.IsPharmacyItem ?? false,
                dosageForm = (string?)null,
                strength = (string?)null,
                batchNo = b.BatchNo,
                expiryDate = b.ExpiryDate,
                status = b.Status,
                preferredBatch = (string?)null // FEFO: first row is preferred
            };
        }).ToList();

        // Mark first batch per item as preferred
        var grouped = data.GroupBy(d => d.itemCode);
        var result = new List<object>();
        foreach (var g in grouped)
        {
            bool first = true;
            foreach (var row in g)
            {
                result.Add(new
                {
                    row.productId, row.itemCode, row.itemName,
                    row.storeId, row.storeName, row.storeCategory,
                    row.availableQuantity, row.unitPrice,
                    row.needsBatch, row.needsExpiry, row.isPharmacyItem,
                    row.dosageForm, row.strength,
                    row.batchNo, row.expiryDate, row.status,
                    preferredBatch = first ? row.batchNo : null
                });
                first = false;
            }
        }

        return new { total, page, pageSize, data = result };
    }

    public async Task<object> GetBatchesAsync(Guid? storeId, string? search, int page, int pageSize)
    {
        var q = _db.PharmacyBatches.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(b => b.StoreDetailsId == storeId);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(b => b.ItemCode.Contains(search) || b.ItemName.Contains(search) || b.BatchNo.Contains(search));

        var total = await q.CountAsync();
        var data = await q.OrderBy(b => b.ExpiryDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new { total, page, pageSize, data };
    }

    public async Task<object?> GetBatchByIdAsync(Guid id)
        => await _db.PharmacyBatches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
}
