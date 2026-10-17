using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════
// Prescription Service
// ═══════════════════════════════════════════════════════════════════
public interface IPharmacyPrescriptionService
{
    Task<object> GetAllAsync(Guid? storeDetailsId, string? status, string? search, int page, int pageSize);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(PharmacyPrescription rx, List<PharmacyPrescriptionItem>? items, Guid userId);
    Task<object?> UpdateAsync(Guid id, PharmacyPrescription rx, Guid userId);
    Task<bool> UpdateStatusAsync(Guid id, string status, string? cancelReason, Guid userId);
    Task<bool> UpdateStatusAsync(string idOrRxNo, string status, string? cancelReason, Guid userId);
    Task<bool> DeletePrescriptionAsync(Guid id, Guid userId);
    Task<bool> DeletePrescriptionAsync(string idOrRxNo, Guid userId);
    Task<bool> MarkPrescriptionReadyAsync(Guid id, Guid userId);

    // Patient CRUD
    Task<object> GetPatientsAsync(Guid? storeDetailsId, string? search, int page, int pageSize);
    Task<object?> GetPatientByIdAsync(Guid id);
    Task<object> GetPatientPrescriptionsAsync(Guid patientId);
    Task<object> CreatePatientAsync(PharmacyPatient p, Guid userId);
    Task<object?> UpdatePatientAsync(Guid id, PharmacyPatient p, Guid userId);
    Task<bool> DeletePatientAsync(Guid id, Guid userId);

    // Doctor CRUD
    Task<object> GetDoctorsAsync(Guid? storeDetailsId, string? search, int page, int pageSize);
    Task<object> CreateDoctorAsync(PharmacyDoctor d, Guid userId);

    // Insurance Company CRUD
    Task<object> GetInsuranceCompaniesAsync(Guid? storeDetailsId, string? search, int page, int pageSize);
    Task<object> CreateInsuranceCompanyAsync(PharmacyInsuranceCompany ic, Guid userId);

    // Prescription Item CRUD
    Task<object> AddItemAsync(Guid prescriptionId, PharmacyPrescriptionItem item, Guid userId);
    Task<object?> UpdateItemAsync(Guid prescriptionId, Guid itemId, PharmacyPrescriptionItem item, Guid userId);
    Task<bool> DeleteItemAsync(Guid prescriptionId, Guid itemId, Guid userId);
}

public class PharmacyPrescriptionService : IPharmacyPrescriptionService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyPrescriptionService> _logger;

    public PharmacyPrescriptionService(ERPContext db, ILogger<PharmacyPrescriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Prescriptions ──────────────────────────────────────────────
    public async Task<object> GetAllAsync(Guid? storeDetailsId, string? status, string? search, int page, int pageSize)
    {
        var q = _db.PharmacyPrescriptions.AsNoTracking().Where(x => x.IsActive).AsQueryable();
        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            q = q.Where(x => x.Status == status);
        }
        else
        {
            q = q.Where(x => x.Status == null || (x.Status != "cancelled" && x.Status != "canceled"));
        }
        if (!string.IsNullOrEmpty(search))
            q = q.Where(x => x.RxNo.Contains(search) || x.PatientName.Contains(search));

        var total = await q.CountAsync();
        var data = await q.OrderByDescending(x => x.CreatedAt)
                          .Skip((page - 1) * pageSize).Take(pageSize)
                          .Include(x => x.Items)
                          .ToListAsync();

        var userMap = new Dictionary<Guid, string>();
        try
        {
            var userIds = data.Select(x => x.CreatedBy).Where(id => id != Guid.Empty).Distinct().ToList();
            if (userIds.Count > 0)
            {
                var userRecords = await _db.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName, u.FirstName, u.LastName })
                    .ToListAsync();

                foreach (var u in userRecords)
                {
                    var fn = $"{u.FirstName} {u.LastName}".Trim();
                    userMap[u.Id] = !string.IsNullOrEmpty(fn) ? fn : (u.UserName ?? "User");
                }
            }
        }
        catch { }

        var storeMap = new Dictionary<Guid, string>();
        try
        {
            var storeRecords = await _db.StoreDetails.AsNoTracking()
                .Select(s => new { s.Id, s.ArabicName, s.EnglishName })
                .ToListAsync();
            foreach (var s in storeRecords)
            {
                storeMap[s.Id] = !string.IsNullOrWhiteSpace(s.ArabicName) ? s.ArabicName : (s.EnglishName ?? "");
            }
        }
        catch { }

        var mappedData = data.Select(rx =>
        {
            var userName = userMap.TryGetValue(rx.CreatedBy, out var uName) ? uName : "Admin";
            var resolvedStoreName = (rx.StoreDetailsId.HasValue && storeMap.TryGetValue(rx.StoreDetailsId.Value, out var sn) && !string.IsNullOrEmpty(sn))
                ? sn
                : (!string.IsNullOrEmpty(rx.StoreName) ? rx.StoreName : "صيدلية الفرع الرئيسي");

            var calcItemsCount = rx.Items.Count(i => i.IsActive);
            var calcSubTotal = rx.Items.Where(i => i.IsActive).Sum(i => i.UnitPrice * i.RequestedQty);
            var calcDiscountTotal = rx.Items.Where(i => i.IsActive).Sum(i => i.DiscountAmount);
            var calcTaxTotal = rx.Items.Where(i => i.IsActive).Sum(i => i.TaxAmount > 0 ? i.TaxAmount : Math.Round((i.UnitPrice * i.RequestedQty - i.DiscountAmount) * 0.15m, 2));
            var calcGrandTotal = calcSubTotal - calcDiscountTotal + calcTaxTotal;

            var isApproved = !string.IsNullOrWhiteSpace(rx.InsuranceApprovalNo) ||
                             string.Equals(rx.InsuranceStatus, "approved", StringComparison.OrdinalIgnoreCase);
            var calcCovered = (rx.InsuranceEnabled && isApproved && rx.InsuranceCoveragePercentage > 0)
                ? Math.Round(calcGrandTotal * (rx.InsuranceCoveragePercentage / 100m), 2)
                : 0m;
            var calcPatientPayable = (rx.InsuranceEnabled && isApproved)
                ? Math.Max(0m, calcGrandTotal - calcCovered)
                : calcGrandTotal;

            return new
            {
                rx.Id,
                rx.RxNo,
                rx.IssueDate,
                rx.Status,
                rx.Priority,
                rx.PatientName,
                rx.PatientId,
                rx.DoctorName,
                rx.DoctorId,
                rx.PaymentType,
                rx.StoreDetailsId,
                storeName = resolvedStoreName,
                itemsCount = calcItemsCount,
                subTotal = calcSubTotal,
                taxTotal = calcTaxTotal,
                discountTotal = calcDiscountTotal,
                grandTotal = calcGrandTotal,
                patientPayable = calcPatientPayable,
                insuranceCovered = calcCovered,
                insuranceCoveredAmount = calcCovered,
                rx.InsuranceEnabled,
                rx.InsuranceCompanyId,
                rx.InsuranceName,
                rx.InsuranceMemberNo,
                rx.InsuranceApprovalNo,
                rx.InsuranceCoveragePercentage,
                rx.InsurancePatientSharePercentage,
                rx.InsurancePatientPayableAmount,
                insuranceStatus = isApproved ? "approved" : (rx.InsuranceEnabled ? "pending" : "none"),
                rx.Notes,
                rx.CreatedAt,
                createdByName = userName,
                dispensedBy = userName,
                items = rx.Items.Where(i => i.IsActive).Select(i => new
                {
                    i.Id,
                    i.LineNo,
                    i.ItemId,
                    mainPurchaseClassId = i.ItemId,
                    i.ItemCode,
                    i.ItemName,
                    quantity     = i.RequestedQty,
                    requestedQty = i.RequestedQty,
                    requestedQuantity = i.RequestedQty,
                    dispensedQty = i.DispensedQty,
                    i.UnitPrice,
                    i.DiscountAmount,
                    taxAmount = i.TaxAmount > 0 ? i.TaxAmount : Math.Round((i.UnitPrice * i.RequestedQty - i.DiscountAmount) * 0.15m, 2),
                    lineTotal    = i.LineTotal > 0 ? i.LineTotal : ((i.UnitPrice * i.RequestedQty) - i.DiscountAmount + (i.TaxAmount > 0 ? i.TaxAmount : Math.Round((i.UnitPrice * i.RequestedQty - i.DiscountAmount) * 0.15m, 2))),
                    storeId        = i.StoreId,
                    storeName      = rx.StoreName,
                    storeDetailsId = i.StoreDetailsId,
                    batchId        = i.BatchId,
                    batchNo        = i.BatchNo,
                    expiryDate     = i.ExpiryDate,
                    i.Notes,
                    i.AvailabilityStatus
                }).ToList()
            };
        }).ToList();

        return new { total, page, pageSize, data = mappedData };
    }

    public async Task<object?> GetByIdAsync(Guid id)
    {
        var rx = await _db.PharmacyPrescriptions.AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Attachments)
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (rx is null) return null;

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == rx.CreatedBy);
        var userName = user != null ? ($"{user.FirstName} {user.LastName}".Trim() is var fn && !string.IsNullOrEmpty(fn) ? fn : (user.UserName ?? "Admin")) : "Admin";
        var calcItemsCount = rx.Items.Count > 0 ? rx.Items.Count : rx.ItemsCount;
        var calcSubTotal = rx.SubTotal > 0 ? rx.SubTotal : rx.Items.Sum(i => i.UnitPrice * i.RequestedQty);
        var calcGrandTotal = rx.GrandTotal > 0 ? rx.GrandTotal : (calcSubTotal - rx.DiscountTotal + rx.TaxTotal);
        var calcPatientPayable = rx.PatientPayable > 0 ? rx.PatientPayable : calcGrandTotal;

        return new
        {
            rx.Id,
            rx.RxNo,
            rx.IssueDate,
            rx.Status,
            rx.Priority,
            rx.PatientName,
            rx.PatientId,
            rx.DoctorName,
            rx.DoctorId,
            rx.PaymentType,
            rx.BranchId,
            rx.BranchName,
            rx.StoreId,
            rx.StoreName,
            rx.StoreDetailsId,
            rx.InsuranceEnabled,
            rx.InsuranceCompanyId,
            rx.InsuranceName,
            rx.InsuranceMemberNo,
            rx.InsuranceApprovalNo,
            rx.InsuranceCoveragePercentage,
            rx.InsurancePatientSharePercentage,
            rx.InsuranceCoveredAmount,
            rx.InsurancePatientPayableAmount,
            rx.InsuranceStatus,
            itemsCount = calcItemsCount,
            subTotal = calcSubTotal,
            rx.DiscountTotal,
            rx.TaxTotal,
            grandTotal = calcGrandTotal,
            patientPayable = calcPatientPayable,
            rx.Notes,
            rx.CancelReason,
            rx.SalesInvoiceId,
            rx.PosTerminalId,
            rx.CreatedAt,
            createdByName = userName,
            dispensedBy = userName,
            items = rx.Items.Select(i => new
            {
                i.Id,
                i.LineNo,
                i.ItemId,
                mainPurchaseClassId = i.ItemId,
                i.ItemCode,
                i.ItemName,
                quantity    = i.RequestedQty,
                requestedQty = i.RequestedQty,
                dispensedQty = i.DispensedQty,
                i.UnitPrice,
                i.DiscountAmount,
                i.TaxAmount,
                lineTotal   = i.LineTotal > 0 ? i.LineTotal : (i.UnitPrice * i.RequestedQty),
                storeId      = i.StoreId,
                storeDetailsId = i.StoreDetailsId,
                batchId      = i.BatchId,
                batchNo      = i.BatchNo,
                expiryDate   = i.ExpiryDate,
                i.AllowSubstitute,
                i.IsControlled,
                i.AvailabilityStatus,
                i.Notes
            }).ToList(),
            attachments = rx.Attachments
        };
    }

    public async Task<object> CreateAsync(PharmacyPrescription rx, List<PharmacyPrescriptionItem>? items, Guid userId)
    {
        rx.Id = Guid.NewGuid();
        rx.CreatedBy = userId;
        rx.CreatedAt = DateTime.UtcNow;
        rx.IsActive = true;

        if (string.IsNullOrEmpty(rx.RxNo))
            rx.RxNo = $"RX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        if (items is { Count: > 0 })
        {
            int line = 1;
            foreach (var item in items)
            {
                item.Id = Guid.NewGuid();
                item.PrescriptionId = rx.Id;
                item.PrescriptionRxNo = rx.RxNo;
                item.LineNo = line++;
                item.CreatedBy = userId;
                item.CreatedAt = DateTime.UtcNow;
                item.IsActive = true;
                item.LineTotal = (item.UnitPrice * item.RequestedQty) - item.DiscountAmount + item.TaxAmount;
            }
            rx.Items = items;
            rx.ItemsCount = items.Count;
            rx.SubTotal = items.Sum(i => i.UnitPrice * i.RequestedQty);
            rx.DiscountTotal = items.Sum(i => i.DiscountAmount);
            rx.TaxTotal = items.Sum(i => i.TaxAmount);
            rx.GrandTotal = rx.SubTotal - rx.DiscountTotal + rx.TaxTotal;

            var isApproved = !string.IsNullOrWhiteSpace(rx.InsuranceApprovalNo) ||
                             string.Equals(rx.InsuranceStatus, "approved", StringComparison.OrdinalIgnoreCase);
            rx.InsuranceStatus = isApproved ? "approved" : (rx.InsuranceEnabled ? "pending" : "none");

            rx.PatientPayable = (rx.InsuranceEnabled && isApproved)
                ? (rx.GrandTotal * (rx.InsurancePatientSharePercentage / 100m))
                : rx.GrandTotal;
            rx.InsuranceCoveredAmount = (rx.InsuranceEnabled && isApproved)
                ? (rx.GrandTotal - rx.PatientPayable)
                : 0;
            rx.InsurancePatientPayableAmount = rx.PatientPayable;
        }

        _db.PharmacyPrescriptions.Add(rx);
        await _db.SaveChangesAsync();
        return rx;
    }

    public async Task<object?> UpdateAsync(Guid id, PharmacyPrescription updated, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (rx is null) return null;

        if (!string.IsNullOrEmpty(updated.RxNo)) rx.RxNo = updated.RxNo;
        if (!string.IsNullOrEmpty(updated.PatientName)) rx.PatientName = updated.PatientName;
        if (updated.PatientId.HasValue && updated.PatientId != Guid.Empty) rx.PatientId = updated.PatientId;
        if (!string.IsNullOrEmpty(updated.DoctorName)) rx.DoctorName = updated.DoctorName;
        if (updated.DoctorId.HasValue && updated.DoctorId != Guid.Empty) rx.DoctorId = updated.DoctorId;
        if (!string.IsNullOrEmpty(updated.PaymentType)) rx.PaymentType = updated.PaymentType;
        if (!string.IsNullOrEmpty(updated.Priority)) rx.Priority = updated.Priority;
        rx.InsuranceEnabled = updated.InsuranceEnabled;
        if (updated.InsuranceCompanyId.HasValue && updated.InsuranceCompanyId != Guid.Empty) rx.InsuranceCompanyId = updated.InsuranceCompanyId;
        if (!string.IsNullOrEmpty(updated.InsuranceName)) rx.InsuranceName = updated.InsuranceName;
        if (!string.IsNullOrEmpty(updated.InsuranceMemberNo)) rx.InsuranceMemberNo = updated.InsuranceMemberNo;
        rx.InsuranceApprovalNo = updated.InsuranceApprovalNo;
        if (updated.InsuranceCoveragePercentage > 0) rx.InsuranceCoveragePercentage = updated.InsuranceCoveragePercentage;
        if (updated.InsurancePatientSharePercentage > 0) rx.InsurancePatientSharePercentage = updated.InsurancePatientSharePercentage;
        if (updated.Notes != null) rx.Notes = updated.Notes;

        var isApproved = !string.IsNullOrWhiteSpace(rx.InsuranceApprovalNo) ||
                         string.Equals(updated.InsuranceStatus, "approved", StringComparison.OrdinalIgnoreCase);
        rx.InsuranceStatus = isApproved ? "approved" : (rx.InsuranceEnabled ? "pending" : "none");

        if (updated.Items is { Count: > 0 })
        {
            // Soft delete old active items
            foreach (var old in rx.Items.Where(i => i.IsActive))
            {
                old.SoftDelete();
            }

            int line = 1;
            foreach (var item in updated.Items)
            {
                item.Id = Guid.NewGuid();
                item.PrescriptionId = rx.Id;
                item.PrescriptionRxNo = rx.RxNo;
                item.LineNo = line++;
                item.CreatedBy = userId;
                item.CreatedAt = DateTime.UtcNow;
                item.IsActive = true;
                item.LineTotal = (item.UnitPrice * item.RequestedQty) - item.DiscountAmount + item.TaxAmount;
                _db.PharmacyPrescriptionItems.Add(item);
            }

            // Reset status to Pending on edit so it must be re-allocated/re-dispensed
            rx.Status = "Pending";

            // Deactivate existing allocations since the items have changed
            var allocations = await _db.Set<PharmacyAllocation>()
                .Where(a => a.PrescriptionId == rx.Id && a.IsActive)
                .ToListAsync();
            foreach (var a in allocations)
            {
                a.IsActive = false;
            }

            // Deactivate existing pickups
            var pickups = await _db.Set<PharmacyPickup>()
                .Where(p => p.PrescriptionId == rx.Id && p.IsActive)
                .ToListAsync();
            foreach (var p in pickups)
            {
                p.IsActive = false;
            }

            rx.ItemsCount = updated.Items.Count;
            rx.SubTotal = updated.Items.Sum(i => i.UnitPrice * i.RequestedQty);
            rx.DiscountTotal = updated.Items.Sum(i => i.DiscountAmount);
            rx.TaxTotal = updated.Items.Sum(i => i.TaxAmount);
            rx.GrandTotal = rx.SubTotal - rx.DiscountTotal + rx.TaxTotal;
        }

        rx.PatientPayable = (rx.InsuranceEnabled && isApproved)
            ? (rx.GrandTotal * (rx.InsurancePatientSharePercentage / 100m))
            : rx.GrandTotal;
        rx.InsuranceCoveredAmount = (rx.InsuranceEnabled && isApproved)
            ? (rx.GrandTotal - rx.PatientPayable)
            : 0;
        rx.InsurancePatientPayableAmount = rx.PatientPayable;

        await _db.SaveChangesAsync();
        return rx;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, string? cancelReason, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (rx is null) return false;
        
        var isCancel = status.Equals("cancelled", StringComparison.OrdinalIgnoreCase) ||
                       status.Equals("canceled", StringComparison.OrdinalIgnoreCase) ||
                       status.Equals("cancel", StringComparison.OrdinalIgnoreCase);

        if (isCancel)
        {
            rx.Status = "cancelled";
            rx.IsActive = false;
        }
        else
        {
            rx.Status = status;
        }

        if (!string.IsNullOrEmpty(cancelReason)) rx.CancelReason = cancelReason;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(string idOrRxNo, string status, string? cancelReason, Guid userId)
    {
        PharmacyPrescription? rx = null;
        if (Guid.TryParse(idOrRxNo, out var gId))
        {
            rx = await _db.PharmacyPrescriptions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == gId);
        }
        if (rx == null)
        {
            rx = await _db.PharmacyPrescriptions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RxNo == idOrRxNo);
        }
        if (rx == null) return false;

        var isCancel = status.Equals("cancelled", StringComparison.OrdinalIgnoreCase) ||
                       status.Equals("canceled", StringComparison.OrdinalIgnoreCase) ||
                       status.Equals("cancel", StringComparison.OrdinalIgnoreCase);

        if (isCancel)
        {
            rx.Status = "cancelled";
            rx.IsActive = false;
        }
        else
        {
            rx.Status = status;
        }

        if (!string.IsNullOrEmpty(cancelReason)) rx.CancelReason = cancelReason;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePrescriptionAsync(Guid id, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (rx is null) return false;
        rx.Status = "cancelled";
        rx.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePrescriptionAsync(string idOrRxNo, Guid userId)
    {
        return await UpdateStatusAsync(idOrRxNo, "cancelled", "Deleted", userId);
    }

    // ── Patients ───────────────────────────────────────────────────
    public async Task<object> GetPatientsAsync(Guid? storeDetailsId, string? search, int page, int pageSize)
    {
        var q = _db.PharmacyPatients.AsNoTracking().AsQueryable();
        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.Trim();
            q = q.Where(x => x.FullName.Contains(s) || x.PatientCode.Contains(s) || (x.Mobile != null && x.Mobile.Contains(s)) || (x.NationalId != null && x.NationalId.Contains(s)));
        }
        var total = await q.CountAsync();
        var patients = await q.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var patientIds = patients.Select(p => p.Id).ToList();
        var patientNames = patients.Select(p => p.FullName).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

        // Get prescription counts and latest dates per patient
        var rxList = await _db.PharmacyPrescriptions.AsNoTracking()
            .Where(r => (r.PatientId.HasValue && patientIds.Contains(r.PatientId.Value)) || (r.PatientName != null && patientNames.Contains(r.PatientName)))
            .Select(r => new { r.PatientId, r.PatientName, r.Status, r.CreatedAt })
            .ToListAsync();

        var data = patients.Select(p =>
        {
            var pRx = rxList.Where(r => (r.PatientId.HasValue && r.PatientId.Value == p.Id) || (r.PatientName != null && r.PatientName.Trim().Equals(p.FullName.Trim(), StringComparison.OrdinalIgnoreCase))).ToList();
            var activeCount = pRx.Count(r => !string.Equals(r.Status, "cancelled", StringComparison.OrdinalIgnoreCase));
            var latestRx = pRx.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

            return new
            {
                p.Id,
                p.SeqNo,
                p.PatientCode,
                p.FullName,
                p.Age,
                p.Gender,
                p.Dob,
                p.NationalId,
                p.Phone,
                p.Mobile,
                p.Address,
                p.Email,
                p.InsuranceClass,
                p.HasInsurance,
                ActivePrescriptions = activeCount > 0 ? activeCount : pRx.Count,
                TotalPrescriptions = pRx.Count,
                p.Allergies,
                p.ChronicDiseases,
                LastVisit = latestRx != null ? latestRx.CreatedAt : p.LastVisit,
                p.Notes,
                p.StoreDetailsId,
                p.IsActive,
                p.CreatedAt
            };
        }).ToList();

        return new { total, page, pageSize, data };
    }

    public async Task<object?> GetPatientByIdAsync(Guid id)
        => await _db.PharmacyPatients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<object> GetPatientPrescriptionsAsync(Guid patientId)
    {
        var patient = await _db.PharmacyPatients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId);
        var pName = patient?.FullName;

        var q = _db.PharmacyPrescriptions.AsNoTracking()
            .Include(r => r.Items)
            .Include(r => r.Doctor)
            .Include(r => r.InsuranceCompany)
            .Where(r => (r.PatientId == patientId) || (!string.IsNullOrEmpty(pName) && r.PatientName == pName));

        var list = await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return list.Select(r => new
        {
            r.Id,
            r.SeqNo,
            r.RxNo,
            r.IssueDate,
            r.PatientName,
            r.PatientId,
            r.DoctorName,
            r.DoctorId,
            DoctorSpecialization = r.Doctor?.Specialization,
            r.BranchName,
            r.StoreName,
            r.PaymentType,
            r.Priority,
            r.Status,
            r.InsuranceEnabled,
            r.InsuranceName,
            r.InsuranceMemberNo,
            r.InsuranceApprovalNo,
            r.InsuranceCoveragePercentage,
            r.InsuranceCoveredAmount,
            r.InsurancePatientPayableAmount,
            r.ItemsCount,
            r.SubTotal,
            r.DiscountTotal,
            r.TaxTotal,
            r.GrandTotal,
            r.PatientPayable,
            r.Notes,
            r.CreatedAt,
            items = r.Items.Select(i => new
            {
                i.Id,
                i.ItemName,
                i.ItemCode,
                Quantity = i.DispensedQty > 0 ? i.DispensedQty : (i.RequestedQty > 0 ? i.RequestedQty : 1),
                RequestedQty = i.RequestedQty,
                DispensedQty = i.DispensedQty,
                i.UnitPrice,
                TotalPrice = i.LineTotal > 0 ? i.LineTotal : (i.UnitPrice * (i.DispensedQty > 0 ? i.DispensedQty : (i.RequestedQty > 0 ? i.RequestedQty : 1))),
                i.BatchNo,
                i.ExpiryDate,
                Instructions = i.Notes ?? "",
                Dosage = "",
                Frequency = ""
            })
        });
    }

    public async Task<object> CreatePatientAsync(PharmacyPatient p, Guid userId)
    {
        p.Id = Guid.NewGuid();
        p.CreatedBy = userId;
        p.CreatedAt = DateTime.UtcNow;
        p.IsActive = true;
        if (string.IsNullOrEmpty(p.PatientCode))
            p.PatientCode = $"PT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        _db.PharmacyPatients.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<object?> UpdatePatientAsync(Guid id, PharmacyPatient updated, Guid userId)
    {
        var p = await _db.PharmacyPatients.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return null;
        p.FullName = updated.FullName;
        p.Age = updated.Age;
        p.Gender = updated.Gender;
        p.Dob = updated.Dob;
        p.NationalId = updated.NationalId;
        p.Phone = updated.Phone;
        p.Mobile = updated.Mobile;
        p.Address = updated.Address;
        p.Email = updated.Email;
        p.InsuranceClass = updated.InsuranceClass;
        p.HasInsurance = updated.HasInsurance;
        p.Allergies = updated.Allergies;
        p.ChronicDiseases = updated.ChronicDiseases;
        p.Notes = updated.Notes;
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<bool> DeletePatientAsync(Guid id, Guid userId)
    {
        var p = await _db.PharmacyPatients.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return false;
        p.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Doctors ────────────────────────────────────────────────────
    public async Task<object> GetDoctorsAsync(Guid? storeDetailsId, string? search, int page, int pageSize)
    {
        var q = _db.PharmacyDoctors.AsNoTracking().AsQueryable();
        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(x => x.Name.Contains(search) || (x.LicenseNo != null && x.LicenseNo.Contains(search)));
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { total, page, pageSize, data };
    }

    public async Task<object> CreateDoctorAsync(PharmacyDoctor d, Guid userId)
    {
        d.Id = Guid.NewGuid();
        d.CreatedBy = userId;
        d.CreatedAt = DateTime.UtcNow;
        d.IsActive = true;
        _db.PharmacyDoctors.Add(d);
        await _db.SaveChangesAsync();
        return d;
    }

    // ── Insurance Companies ────────────────────────────────────────
    public async Task<object> GetInsuranceCompaniesAsync(Guid? storeDetailsId, string? search, int page, int pageSize)
    {
        if (!await _db.PharmacyInsuranceCompanies.AnyAsync())
        {
            var seedList = new List<PharmacyInsuranceCompany>
            {
                new() { Id = Guid.NewGuid(), Name = "بوبا العربية للتأمين التعاوني (Bupa Arabia)", DefaultCoveragePercentage = 80m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "شركة التعاونية للتأمين (Tawuniya)", DefaultCoveragePercentage = 80m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "شركة ميدغلف للتأمين (MedGulf)", DefaultCoveragePercentage = 80m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "شركة تكافل الراجحي (Al Rajhi Takaful)", DefaultCoveragePercentage = 80m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "شركة سلامة للتأمين التعاوني (Salama)", DefaultCoveragePercentage = 70m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "شركة ملاذ للتأمين (Malath)", DefaultCoveragePercentage = 80m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "شركة الدرع العربي للتأمين (Arabian Shield)", DefaultCoveragePercentage = 80m, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _db.PharmacyInsuranceCompanies.AddRange(seedList);
            await _db.SaveChangesAsync();
        }

        var q = _db.PharmacyInsuranceCompanies.AsNoTracking().AsQueryable();
        if (storeDetailsId.HasValue) q = q.Where(x => x.StoreDetailsId == storeDetailsId || x.StoreDetailsId == null);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(x => x.Name.Contains(search));
        var total = await q.CountAsync();
        var data = await q.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { total, page, pageSize, data };
    }

    public async Task<object> CreateInsuranceCompanyAsync(PharmacyInsuranceCompany ic, Guid userId)
    {
        ic.Id = Guid.NewGuid();
        ic.CreatedBy = userId;
        ic.CreatedAt = DateTime.UtcNow;
        ic.IsActive = true;
        _db.PharmacyInsuranceCompanies.Add(ic);
        await _db.SaveChangesAsync();
        return ic;
    }

    private void RecalculatePrescriptionFinancials(PharmacyPrescription rx)
    {
        var activeItems = rx.Items?.Where(i => i.IsActive).ToList() ?? new List<PharmacyPrescriptionItem>();

        foreach (var item in activeItems)
        {
            if (item.TaxAmount == 0 && item.UnitPrice > 0 && item.RequestedQty > 0)
            {
                item.TaxAmount = Math.Round((item.UnitPrice * item.RequestedQty - item.DiscountAmount) * 0.15m, 2);
            }
            item.LineTotal = (item.UnitPrice * item.RequestedQty) - item.DiscountAmount + item.TaxAmount;
        }

        rx.ItemsCount = activeItems.Count;
        rx.SubTotal = activeItems.Sum(i => i.UnitPrice * i.RequestedQty);
        rx.DiscountTotal = activeItems.Sum(i => i.DiscountAmount);
        rx.TaxTotal = activeItems.Sum(i => i.TaxAmount);
        rx.GrandTotal = rx.SubTotal - rx.DiscountTotal + rx.TaxTotal;

        var isApproved = !string.IsNullOrWhiteSpace(rx.InsuranceApprovalNo) ||
                         string.Equals(rx.InsuranceStatus, "approved", StringComparison.OrdinalIgnoreCase);

        if (rx.InsuranceEnabled && isApproved)
        {
            var covPct = rx.InsuranceCoveragePercentage > 0 ? rx.InsuranceCoveragePercentage : 0m;
            rx.InsuranceCoveredAmount = Math.Round(rx.GrandTotal * (covPct / 100m), 2);
            rx.InsurancePatientPayableAmount = Math.Max(0m, rx.GrandTotal - rx.InsuranceCoveredAmount);
            rx.PatientPayable = rx.InsurancePatientPayableAmount;
        }
        else
        {
            rx.InsuranceCoveredAmount = 0m;
            rx.InsurancePatientPayableAmount = rx.GrandTotal;
            rx.PatientPayable = rx.GrandTotal;
        }
    }

    // ── Prescription Item CRUD ─────────────────────────────────────
    public async Task<object> AddItemAsync(Guid prescriptionId, PharmacyPrescriptionItem item, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == prescriptionId);
        if (rx is null) throw new KeyNotFoundException("Prescription not found");
        if (rx.Status is "delivered" or "cancelled")
            throw new InvalidOperationException($"Cannot add items to a prescription with status '{rx.Status}'");

        item.Id = Guid.NewGuid();
        item.PrescriptionId = prescriptionId;
        item.PrescriptionRxNo = rx.RxNo;
        item.LineNo = (rx.Items?.Count ?? 0) + 1;
        item.CreatedBy = userId;
        item.CreatedAt = DateTime.UtcNow;
        item.IsActive = true;
        if (item.TaxAmount == 0 && item.UnitPrice > 0 && item.RequestedQty > 0)
        {
            item.TaxAmount = Math.Round((item.UnitPrice * item.RequestedQty - item.DiscountAmount) * 0.15m, 2);
        }
        item.LineTotal = (item.UnitPrice * item.RequestedQty) - item.DiscountAmount + item.TaxAmount;

        _db.PharmacyPrescriptionItems.Add(item);
        if (rx.Items == null) rx.Items = new List<PharmacyPrescriptionItem>();
        rx.Items.Add(item);

        RecalculatePrescriptionFinancials(rx);

        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<object?> UpdateItemAsync(Guid prescriptionId, Guid itemId, PharmacyPrescriptionItem updated, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == prescriptionId);
        if (rx is null) return null;
        if (rx.Status is "delivered" or "cancelled")
            throw new InvalidOperationException($"Cannot update items on a prescription with status '{rx.Status}'");

        var item = rx.Items?.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return null;

        item.ItemId = updated.ItemId;
        item.ItemCode = updated.ItemCode;
        item.ItemName = updated.ItemName;
        item.RequestedQty = updated.RequestedQty;
        item.DispensedQty = updated.DispensedQty;
        item.UnitPrice = updated.UnitPrice;
        item.DiscountAmount = updated.DiscountAmount;
        item.TaxAmount = updated.TaxAmount;
        if (item.TaxAmount == 0 && item.UnitPrice > 0 && item.RequestedQty > 0)
        {
            item.TaxAmount = Math.Round((item.UnitPrice * item.RequestedQty - item.DiscountAmount) * 0.15m, 2);
        }
        item.LineTotal = (updated.UnitPrice * updated.RequestedQty) - updated.DiscountAmount + item.TaxAmount;
        item.BatchId = updated.BatchId;
        item.BatchNo = updated.BatchNo;
        item.ExpiryDate = updated.ExpiryDate;
        item.AllowSubstitute = updated.AllowSubstitute;
        item.IsControlled = updated.IsControlled;
        item.Notes = updated.Notes;
        item.StoreId = updated.StoreId;
        item.StoreDetailsId = updated.StoreDetailsId;

        RecalculatePrescriptionFinancials(rx);

        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteItemAsync(Guid prescriptionId, Guid itemId, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == prescriptionId);
        if (rx is null) return false;
        if (rx.Status is "delivered" or "cancelled")
            throw new InvalidOperationException($"Cannot delete items from a prescription with status '{rx.Status}'");

        var item = rx.Items?.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return false;

        item.IsActive = false; // soft delete

        RecalculatePrescriptionFinancials(rx);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkPrescriptionReadyAsync(Guid id, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (rx == null)
            throw new KeyNotFoundException("Prescription not found.");

        if (rx.PatientId == null || rx.PatientId == Guid.Empty)
            throw new InvalidOperationException("PatientId is missing on the prescription.");

        if (rx.StoreDetailsId == null || rx.StoreDetailsId == Guid.Empty)
            throw new InvalidOperationException("StoreDetailsId is missing on the prescription.");

        if (rx.Items == null || !rx.Items.Any(i => i.IsActive))
            throw new InvalidOperationException("Prescription contains no active items.");

        foreach (var item in rx.Items.Where(i => i.IsActive))
        {
            if (item.ItemId == null || item.ItemId == Guid.Empty)
                throw new InvalidOperationException($"ItemId is missing for item '{item.ItemName}'.");

            if (string.IsNullOrWhiteSpace(item.BatchNo))
                throw new InvalidOperationException($"BatchNo is not provided for item '{item.ItemName}'.");

            // Find batch in the store
            var batch = await _db.PharmacyBatches
                .FirstOrDefaultAsync(b => b.StoreDetailsId == rx.StoreDetailsId && b.BatchNo == item.BatchNo && b.ItemCode == item.ItemCode && b.IsActive);

            if (batch == null)
                throw new InvalidOperationException($"Batch '{item.BatchNo}' is not active or does not exist in the store for item '{item.ItemName}'.");

            if (batch.ExpiryDate <= DateTime.UtcNow)
                throw new InvalidOperationException($"Batch '{item.BatchNo}' for item '{item.ItemName}' has expired.");

            if (batch.Quantity < item.RequestedQty)
                throw new InvalidOperationException($"Insufficient stock for item '{item.ItemName}' in batch '{item.BatchNo}'. Available: {batch.Quantity}, Requested: {item.RequestedQty}.");
        }

        rx.Status = "ready";
        await _db.SaveChangesAsync();
        return true;
    }
}
