using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════
// Insurance Claims Service
// ═══════════════════════════════════════════════════════════════════
public interface IPharmacyInsuranceClaimsService
{
    Task<object> GetAllAsync(Guid? storeDetailsId, string? status, string? insurer, string? search, int page, int size);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(PharmacyInsuranceClaim claim, Guid userId);
    Task<bool> UpdateStatusAsync(Guid id, string status, decimal? approvedAmount, string? referenceNo, string? notes, Guid userId);
}

public class PharmacyInsuranceClaimsService : IPharmacyInsuranceClaimsService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyInsuranceClaimsService> _logger;

    public PharmacyInsuranceClaimsService(ERPContext db, ILogger<PharmacyInsuranceClaimsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<object> GetAllAsync(Guid? storeDetailsId, string? status, string? insurer, string? search, int page, int size)
    {
        try
        {
            var q = _db.PharmacyInsuranceClaims.AsNoTracking().AsQueryable();
            if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
            if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
            if (!string.IsNullOrEmpty(insurer)) q = q.Where(x => x.Insurer == insurer);
            if (!string.IsNullOrEmpty(search))
                q = q.Where(x => x.ClaimNo.Contains(search) || x.PatientName.Contains(search) || (x.RxNo != null && x.RxNo.Contains(search)));

            var totalCount = await q.CountAsync();
            var data = await q.OrderByDescending(x => x.SubmittedAt)
                              .Skip((page - 1) * size).Take(size)
                              .ToListAsync();

            var items = data.Select(x => new
            {
                id = x.Id,
                claimNo = x.ClaimNo,
                rxNo = x.RxNo ?? "",
                patientName = x.PatientName,
                insurer = x.Insurer,
                status = (x.Status ?? "submitted").ToLowerInvariant(),
                nphiesStatus = x.NphiesStatus?.ToLowerInvariant() ?? "",
                nphiesClaimId = x.NphiesClaimId ?? "",
                prescriptionId = x.PrescriptionId?.ToString() ?? "",
                saleInvoiceId = x.SaleInvoiceId,
                totalAmount = x.TotalAmount,
                approvedAmount = x.ApprovedAmount,
                insuranceCoveredAmount = x.InsuranceCoveredAmount > 0 ? x.InsuranceCoveredAmount : x.ApprovedAmount,
                patientShareAmount = x.PatientShareAmount > 0 ? x.PatientShareAmount : Math.Max(0, x.TotalAmount - (x.InsuranceCoveredAmount > 0 ? x.InsuranceCoveredAmount : x.ApprovedAmount)),
                coveragePercentage = x.CoveragePercentage > 0 ? x.CoveragePercentage : (x.TotalAmount > 0 ? Math.Round(((x.InsuranceCoveredAmount > 0 ? x.InsuranceCoveredAmount : x.ApprovedAmount) / x.TotalAmount) * 100, 2) : 0),
                referenceNo = x.ReferenceNo ?? "",
                submittedAt = x.SubmittedAt,
                nphiesSubmittedAt = x.NphiesSubmittedAt,
                adjudicatedAt = x.NphiesAdjudicatedAt ?? x.ProcessedAt,
                notes = x.Notes ?? ""
            }).ToList();

            return new { items, totalCount, page, pageSize = size };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync insurance claims failed");
            return new { items = new List<object>(), totalCount = 0, page, pageSize = size };
        }
    }

    public async Task<object?> GetByIdAsync(Guid id)
        => await _db.PharmacyInsuranceClaims.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<object> CreateAsync(PharmacyInsuranceClaim claim, Guid userId)
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
        _logger.LogInformation("Insurance claim {ClaimNo} created", claim.ClaimNo);
        return claim;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, decimal? approvedAmount, string? referenceNo, string? notes, Guid userId)
    {
        var claim = await _db.PharmacyInsuranceClaims.FirstOrDefaultAsync(x => x.Id == id);
        if (claim is null) return false;

        claim.Status = status;
        if (approvedAmount.HasValue) claim.ApprovedAmount = approvedAmount.Value;
        if (!string.IsNullOrEmpty(referenceNo)) claim.ReferenceNo = referenceNo;
        if (!string.IsNullOrEmpty(notes)) claim.Notes = notes;

        if (status is "Approved" or "Rejected" or "Paid")
            claim.ProcessedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Insurance claim {ClaimNo} → {Status}", claim.ClaimNo, status);
        return true;
    }
}
