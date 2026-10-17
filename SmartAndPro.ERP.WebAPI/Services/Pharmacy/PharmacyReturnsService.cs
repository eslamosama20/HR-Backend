using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════
// Pharmacy Returns Service
// ═══════════════════════════════════════════════════════════════════
public interface IPharmacyReturnsService
{
    Task<object> GetAllAsync(Guid? storeDetailsId, string? status, string? source, string? search, int page, int size);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(PharmacyReturn ret, Guid userId);
    Task<object> CreateFromInvoiceAsync(string invoiceNo, List<ReturnItemRequest> items, Guid userId);
    Task<bool> UpdateStatusAsync(Guid id, string status, Guid userId);
    Task<object?> LookupInvoiceAsync(string invoiceNo);
}

public record ReturnItemRequest(Guid ItemId, decimal Quantity);

public class PharmacyReturnsService : IPharmacyReturnsService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyReturnsService> _logger;

    public PharmacyReturnsService(ERPContext db, ILogger<PharmacyReturnsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<object> GetAllAsync(Guid? storeDetailsId, string? status, string? source, string? search, int page, int size)
    {
        try
        {
            var q = _db.PharmacyReturns.AsNoTracking().AsQueryable();
            if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
            if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
            if (!string.IsNullOrEmpty(source)) q = q.Where(x => x.Source == source);
            if (!string.IsNullOrEmpty(search))
                q = q.Where(x => x.ReturnNo.Contains(search) || x.ItemName.Contains(search) || x.ItemCode.Contains(search));

            var totalCount = await q.CountAsync();
            var data = await q.OrderByDescending(x => x.ReturnDate)
                              .Skip((page - 1) * size).Take(size)
                              .ToListAsync();

            var items = data.Select(x => new
            {
                id = x.Id,
                returnNo = x.ReturnNo,
                referenceNo = x.ReferenceNo ?? "",
                itemName = x.ItemName,
                quantity = x.Quantity,
                status = (x.Status ?? "pending").ToLowerInvariant()
            }).ToList();

            return new { items, totalCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync returns failed");
            return new { items = new List<object>(), totalCount = 0 };
        }
    }

    public async Task<object?> GetByIdAsync(Guid id)
        => await _db.PharmacyReturns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<object> CreateAsync(PharmacyReturn ret, Guid userId)
    {
        ret.Id = Guid.NewGuid();
        ret.CreatedBy = userId;
        ret.CreatedAt = DateTime.UtcNow;
        ret.IsActive = true;
        ret.ReturnDate = DateTime.UtcNow;
        ret.Status = "pending";

        if (string.IsNullOrEmpty(ret.ReturnNo))
            ret.ReturnNo = $"RTN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        _db.PharmacyReturns.Add(ret);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Return {ReturnNo} created for {Item}", ret.ReturnNo, ret.ItemName);
        return new { id = ret.Id, returnNo = ret.ReturnNo, status = ret.Status };
    }

    /// <summary>
    /// Invoice-based return creation: PDF Section 2 §4
    /// POST /api/v1/pharmacy/returns  { invoiceNo, items: [{itemId, quantity}] }
    /// </summary>
    public async Task<object> CreateFromInvoiceAsync(string invoiceNo, List<ReturnItemRequest> items, Guid userId)
    {
        try
        {
            // Look up the sales invoice by InvoiceNo
            var invoice = await _db.SaleInvoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo);

            var referenceNo = invoiceNo;
            var invoiceIntId = invoice?.Id;
            var returnNo = $"RTN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var createdReturns = new List<object>();

            foreach (var item in items)
            {
                // Try to resolve item name from sales class (MainSalesClass)
                var salesClass = await _db.MainSalesClasses.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == item.ItemId);

                var ret = new PharmacyReturn
                {
                    Id = Guid.NewGuid(),
                    ReturnNo = returnNo,
                    Source = "Invoice",
                    ItemCode = salesClass?.ReferenceCode ?? item.ItemId.ToString(),
                    ItemName = salesClass?.ArabicName ?? salesClass?.EnglishName ?? "Unknown",
                    Quantity = item.Quantity,
                    Reason = "Customer return",
                    Status = "pending",
                    ReturnDate = DateTime.UtcNow,
                    ReferenceNo = referenceNo,
                    InvoiceId = invoiceIntId,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _db.PharmacyReturns.Add(ret);
                createdReturns.Add(new { id = ret.Id, itemId = item.ItemId, quantity = item.Quantity });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Invoice return {ReturnNo} created for invoice {InvoiceNo}", returnNo, invoiceNo);
            var firstId = createdReturns.Count > 0 ? ((dynamic)createdReturns[0]).id : Guid.Empty;
            return new { id = firstId, returnNo, status = "pending" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateFromInvoiceAsync failed for invoice {InvoiceNo}", invoiceNo);
            throw;
        }
    }

    /// <summary>
    /// PDF Section 2 §3: GET /api/v1/pharmacy/returns/lookup-invoice/{invoiceNo}
    /// </summary>
    public async Task<object?> LookupInvoiceAsync(string invoiceNo)
    {
        try
        {
            // Look for existing returns to calculate already-returned quantities
            var existingReturns = await _db.PharmacyReturns.AsNoTracking()
                .Where(r => r.ReferenceNo == invoiceNo)
                .ToListAsync();

            var returnedQtyByCode = existingReturns
                .GroupBy(r => r.ItemCode)
                .ToDictionary(g2 => g2.Key, g2 => g2.Sum(r => r.Quantity));

            // Find invoice by InvoiceNo string
            var invoice = await _db.SaleInvoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo);

            if (invoice != null)
            {
                // Get sold items from sale invoice items
                var saleItems = await _db.SaleItems.AsNoTracking()
                    .Where(si => si.SaleInvoiceId == invoice.Id)
                    .Include(si => si.MainSalesClass)
                    .ToListAsync();

                var items = saleItems.Select(si => new
                {
                    itemId = si.MainSalesClassId,
                    itemName = si.MainSalesClass?.ArabicName ?? si.MainSalesClass?.EnglishName ?? "Unknown",
                    soldQty = si.Quantity,
                    returnableQty = Math.Max(0, si.Quantity - (returnedQtyByCode.TryGetValue(si.MainSalesClass?.ReferenceCode ?? "", out var alreadyReturned) ? alreadyReturned : 0)),
                    unitPrice = si.Price
                }).ToList();

                return new { invoiceNo = invoice.InvoiceNo, invoiceId = invoice.Id, items };
            }

            // Invoice not found - return empty response (not 500)
            _logger.LogWarning("Invoice not found for lookup: {InvoiceNo}", invoiceNo);
            return new { invoiceNo = invoiceNo, invoiceId = (int?)null, items = new List<object>() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LookupInvoiceAsync failed for {InvoiceNo}", invoiceNo);
            return new { invoiceNo = invoiceNo, invoiceId = (int?)null, items = new List<object>() };
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid userId)
    {
        var ret = await _db.PharmacyReturns.FirstOrDefaultAsync(x => x.Id == id);
        if (ret is null) return false;
        ret.Status = status.ToLowerInvariant();
        await _db.SaveChangesAsync();
        _logger.LogInformation("Return {ReturnNo} → {Status}", ret.ReturnNo, status);
        return true;
    }
}
