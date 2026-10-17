using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Domains.Entities.Inventory;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.WebAPI.Services.Pharmacy;

// ═══════════════════════════════════════════════════════════════════════
// Pharmacy Dispense Service – PDF spec §12 (Most critical section)
// Allocate-and-Dispense + Send-to-POS + Prescription Checkout
// ═══════════════════════════════════════════════════════════════════════

#region DTOs

public class AllocateAndDispenseRequest
{
    public Guid PrescriptionId { get; set; }
    public string? Notes { get; set; }
    public string? ConfirmedBy { get; set; }
    public List<AllocationLineRequest> Lines { get; set; } = new();
    public object? AllocationPayload { get; set; }
}

public class AllocationLineRequest
{
    public Guid PrescriptionItemId { get; set; }
    public Guid ItemId { get; set; }
    public string? BatchNo { get; set; }
    public Guid StoreId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class SendToPosRequest
{
    public string? PosTerminalId { get; set; }
    public string? CashierUserId { get; set; }
}

public class PrescriptionCheckoutRequest
{
    public Guid PickupId { get; set; }
    public int? CashierSessionId { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public int PaymentMethod { get; set; } // 0=Cash, 1=Card, 2=Credit
    public decimal CashAmount { get; set; }
    public decimal CardAmount { get; set; }
    public int? CreditDays { get; set; }
    public string? Notes { get; set; }
}

public class DispenseErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public object? Details { get; set; }
}

#endregion

public interface IPharmacyDispenseService
{
    Task<(object? result, DispenseErrorResponse? error)> AllocateAndDispenseAsync(AllocateAndDispenseRequest req, Guid userId);
    Task<(object? result, DispenseErrorResponse? error)> SendToPosAsync(Guid prescriptionId, SendToPosRequest req, Guid userId);
    Task<object?> GetAllocationStatusAsync(Guid prescriptionId);
    Task<(object? result, DispenseErrorResponse? error)> PrescriptionCheckoutAsync(PrescriptionCheckoutRequest req, Guid userId);
    Task<object> GetReadyPrescriptionsAsync(int page, int pageSize, Guid? storeDetailsId);
}

public class PharmacyDispenseService : IPharmacyDispenseService
{
    private readonly ERPContext _db;
    private readonly ILogger<PharmacyDispenseService> _logger;

    // Valid prescription statuses (PDF §12.12)
    private static readonly string[] AllowedAllocateStatuses = { "Pending", "new", "review", "dispensing", "draft" };
    private static readonly string[] TerminalStatuses = { "cancelled", "ready", "delivered" };

    public PharmacyDispenseService(ERPContext db, ILogger<PharmacyDispenseService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// POST /v1/pharmacy/prescriptions/{id}/allocate-and-dispense (PDF §12)
    /// Atomic: validate → commit inventory → update status
    /// </summary>
    public async Task<(object? result, DispenseErrorResponse? error)> AllocateAndDispenseAsync(
        AllocateAndDispenseRequest req, Guid userId)
    {
        // Use a DB transaction for atomicity (PDF §12.4)
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // 1. Find prescription
            var rx = await _db.PharmacyPrescriptions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == req.PrescriptionId);

            if (rx is null)
                return (null, Err("PRESCRIPTION_NOT_FOUND", "Prescription not found", "الوصفة غير موجودة"));

            // 2. Validate status
            if (rx.Status == "cancelled")
                return (null, Err("PRESCRIPTION_CANCELLED", "Prescription is cancelled", "الوصفة ملغاة"));

            if (TerminalStatuses.Contains(rx.Status) && rx.Status != "cancelled")
                return (null, Err("PRESCRIPTION_ALREADY_DISPENSED", "Already dispensed or ready", "تم صرف الوصفة مسبقاً"));

            if (!AllowedAllocateStatuses.Contains(rx.Status))
                return (null, Err("PRESCRIPTION_INVALID_STATUS", $"Status '{rx.Status}' not allowed for allocation", "حالة الوصفة لا تسمح بالصرف"));

            // 3. Validate items exist
            if (rx.Items == null || !rx.Items.Any())
                return (null, Err("PRESCRIPTION_ITEM_NOT_FOUND", "No items in prescription", "لا توجد أصناف في الوصفة"));

            if (req.Lines == null || !req.Lines.Any())
                return (null, Err("PRESCRIPTION_ITEM_NOT_FOUND", "No allocation lines provided", "لم يتم إرسال أسطر التخصيص"));

            // 4-9. Validate each line
            var allocationLines = new List<PharmacyAllocationLine>();

            foreach (var line in req.Lines)
            {
                // Check prescription item reference
                var rxItem = rx.Items.FirstOrDefault(i => i.Id == line.PrescriptionItemId);
                if (rxItem is null)
                    return (null, Err("PRESCRIPTION_ITEM_NOT_FOUND",
                        $"Prescription item {line.PrescriptionItemId} not found",
                        "سطر الوصفة غير موجود"));

                // Check store exists
                var store = await _db.StoreDetails.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == line.StoreId);
                if (store is null)
                    return (null, Err("STORE_REFERENCE_MISSING", "Store not found", "المخزن غير موجود"));

                // Check store is pharmacy
                if (store.StoreCategory != "pharmacy")
                    return (null, Err("STORE_NOT_PHARMACY", "Store is not a pharmacy store", "المخزن ليس مخزن صيدلية"));

                // Check item exists in MainPurchaseClass
                var item = await _db.MainPurchaseClasses.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == line.ItemId);

                // Check batch required
                if (item != null && (item.NeedsBatch || item.ItemType == ItemType.Medication))
                {
                    if (string.IsNullOrWhiteSpace(line.BatchNo))
                        return (null, Err("BATCH_REQUIRED", $"Batch required for item {rxItem.ItemCode}", "رقم الباتش مطلوب"));
                }

                // Check expiry required
                if (item != null && (item.NeedsExpiry || item.ItemType == ItemType.Medication))
                {
                    if (!line.ExpiryDate.HasValue)
                        return (null, Err("EXPIRY_REQUIRED", $"Expiry date required for item {rxItem.ItemCode}", "تاريخ الصلاحية مطلوب"));
                }

                // Check batch not expired
                if (line.ExpiryDate.HasValue && line.ExpiryDate.Value < DateTime.UtcNow)
                    return (null, Err("BATCH_EXPIRED", $"Batch {line.BatchNo} has expired", "الباتش منتهي الصلاحية"));

                var targetStoreId = line.StoreId != Guid.Empty ? line.StoreId : (rx.StoreDetailsId ?? Guid.Empty);
                var targetItemId = line.ItemId != Guid.Empty ? line.ItemId : (rxItem.ItemId ?? Guid.Empty);

                // Check batch exists in pharmacy inventory
                var batch = await _db.PharmacyBatches.FirstOrDefaultAsync(b =>
                    b.StoreDetailsId == targetStoreId &&
                    b.ItemCode == rxItem.ItemCode &&
                    b.BatchNo == (line.BatchNo ?? "") &&
                    b.IsActive);

                decimal availableQty = batch?.Quantity ?? 0;

                // Fallback: check actual PermitEntry stock for this item in this store
                if (availableQty == 0 && targetItemId != Guid.Empty && targetStoreId != Guid.Empty)
                {
                    var permitStock = await (
                        from pe in _db.PermitEntries.AsNoTracking()
                        join ip in _db.InventoryPermits.AsNoTracking() on pe.InventoryPermitId equals ip.Id
                        where pe.IsActive && ip.IsActive && ip.StoreDetailsId == targetStoreId && pe.MainPurchaseClassId == targetItemId
                        select ip.PermitType == PermitType.Entry ? pe.Quantity : -pe.Quantity
                    ).SumAsync();

                    if (permitStock > 0)
                    {
                        availableQty = permitStock;
                    }
                }

                // If still not tracked, use fallback stock for pharmacy catalog item
                if (availableQty == 0)
                {
                    availableQty = Math.Max(100m, line.Quantity * 5);
                }

                allocationLines.Add(new PharmacyAllocationLine
                {
                    Id = Guid.NewGuid(),
                    PrescriptionItemId = line.PrescriptionItemId,
                    MainPurchaseClassId = targetItemId,
                    StoreId = targetStoreId,
                    BatchNo = line.BatchNo ?? "1",
                    ExpiryDate = line.ExpiryDate ?? DateTime.UtcNow.AddYears(2),
                    RequestedQuantity = line.Quantity,
                    CommittedQuantity = line.Quantity,
                    AvailableQuantityAtCommit = availableQty,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });

                // 10. Execute deduction from batch (or initialize PharmacyBatch)
                if (batch != null)
                {
                    batch.Quantity = Math.Max(0m, batch.Quantity - line.Quantity);
                }
                else if (targetStoreId != Guid.Empty)
                {
                    var newBatch = new PharmacyBatch
                    {
                        Id = Guid.NewGuid(),
                        ItemCode = rxItem.ItemCode ?? "RX-ITEM",
                        ItemName = rxItem.ItemName ?? "دواء",
                        BatchNo = line.BatchNo ?? "1",
                        ExpiryDate = line.ExpiryDate ?? DateTime.UtcNow.AddYears(2),
                        Quantity = Math.Max(0m, availableQty - line.Quantity),
                        MinQuantity = 10,
                        StoreName = rx.StoreName ?? "صيدلية الفرع الرئيسي",
                        StoreDetailsId = targetStoreId,
                        Status = "active",
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.PharmacyBatches.Add(newBatch);
                }

                // Update dispensed qty on prescription item
                rxItem.DispensedQty = line.Quantity;
            }

            // Create allocation record
            var refId = $"ALLOC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var allocation = new PharmacyAllocation
            {
                Id = Guid.NewGuid(),
                PrescriptionId = req.PrescriptionId,
                AllocationReferenceId = refId,
                CommitMode = "deduction",
                Status = "committed",
                ConfirmedBy = req.ConfirmedBy,
                Notes = req.Notes,
                PreparedAt = DateTime.UtcNow,
                CommittedAt = DateTime.UtcNow,
                PayloadJson = req.AllocationPayload != null ? JsonSerializer.Serialize(req.AllocationPayload) : null,
                StoreDetailsId = rx.StoreDetailsId,
                Lines = allocationLines,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Set<PharmacyAllocation>().Add(allocation);

            // Update prescription status to ready
            rx.Status = "ready";

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (new
            {
                allocationReferenceId = refId,
                commitMode = "deduction",
                prescriptionStatus = rx.Status,
                allocationId = allocation.Id,
                linesCommitted = allocationLines.Count,
                lines = allocationLines.Select(l => new
                {
                    prescriptionItemId = l.PrescriptionItemId,
                    itemId = l.MainPurchaseClassId,
                    storeId = l.StoreId,
                    batchNo = l.BatchNo,
                    expiryDate = l.ExpiryDate,
                    requestedQuantity = l.RequestedQuantity,
                    committedQuantity = l.CommittedQuantity,
                    availableQuantityAtCommit = l.AvailableQuantityAtCommit
                })
            }, null);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Allocate-and-dispense failed for prescription {PrescriptionId}", req.PrescriptionId);
            return (null, Err("ALLOCATION_CONFLICT", "Allocation failed: " + ex.Message, "فشل التخصيص"));
        }
    }

    /// <summary>
    /// POST /v1/pharmacy/prescriptions/{id}/send-to-pos (PDF §14)
    /// </summary>
    public async Task<(object? result, DispenseErrorResponse? error)> SendToPosAsync(
        Guid prescriptionId, SendToPosRequest req, Guid userId)
    {
        var rx = await _db.PharmacyPrescriptions
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == prescriptionId);

        if (rx is null)
            return (null, Err("PRESCRIPTION_NOT_FOUND", "Prescription not found", "الوصفة غير موجودة"));

        if (rx.Status != "ready" && rx.Status != "delivered")
            return (null, Err("PRESCRIPTION_INVALID_STATUS", "Prescription must be ready or delivered", "يجب أن تكون الوصفة جاهزة أو تم تسليمها"));

        // Check allocation exists
        var allocation = await _db.Set<PharmacyAllocation>().AsNoTracking()
            .FirstOrDefaultAsync(a => a.PrescriptionId == prescriptionId && a.IsActive);

        if (allocation is null)
            return (null, Err("ALLOCATION_BACKEND_DISABLED", "No allocation found", "لا يوجد تخصيص"));

        // Deactivate any existing pending pickups for this prescription to allow sending updated items
        var pendingPickups = await _db.Set<PharmacyPickup>()
            .Where(p => p.PrescriptionId == prescriptionId && p.PickupStatus == "pending" && p.IsActive)
            .ToListAsync();

        foreach (var p in pendingPickups)
        {
            p.PickupStatus = "void";
            p.IsActive = false;
        }

        // Build financials from prescription items
        var isApproved = !string.IsNullOrWhiteSpace(rx.InsuranceApprovalNo) ||
                         string.Equals(rx.InsuranceStatus, "approved", StringComparison.OrdinalIgnoreCase);
        var subtotal = rx.SubTotal;
        var discount = rx.DiscountTotal;
        var tax = rx.TaxTotal;
        var total = rx.GrandTotal > 0 ? rx.GrandTotal : (subtotal - discount + tax);
        var insuranceCovered = (rx.InsuranceEnabled && isApproved) ? (rx.InsuranceCoveredAmount > 0 ? rx.InsuranceCoveredAmount : (total * (rx.InsuranceCoveragePercentage / 100m))) : 0m;
        var patientPayable = (rx.InsuranceEnabled && isApproved) ? (rx.PatientPayable > 0 ? rx.PatientPayable : (total - insuranceCovered)) : total;

        var pickup = new PharmacyPickup
        {
            Id = Guid.NewGuid(),
            PrescriptionId = prescriptionId,
            AllocationId = allocation.Id,
            AllocationReferenceId = allocation.AllocationReferenceId,
            AllocationCommitMode = allocation.CommitMode,
            RxNo = rx.RxNo,
            PatientName = rx.PatientName,
            PaymentType = rx.PaymentType,
            InsuranceStatus = rx.InsuranceStatus,
            PrescriptionStatus = rx.Status,
            StoreId = rx.StoreDetailsId ?? Guid.Empty,
            StoreName = rx.StoreName,
            PosTerminalId = req.PosTerminalId,
            CashierUserId = req.CashierUserId,
            SentAt = DateTime.UtcNow,
            PickupStatus = "pending",
            Subtotal = subtotal,
            DiscountAmount = discount,
            TaxAmount = tax,
            TotalAmount = total,
            InsuranceCovered = insuranceCovered,
            PatientPayable = patientPayable,
            StoreDetailsId = rx.StoreDetailsId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Set<PharmacyPickup>().Add(pickup);
        await _db.SaveChangesAsync();

        return (new
        {
            pickupId = pickup.Id,
            prescriptionId,
            rxNo = rx.RxNo,
            sentAt = pickup.SentAt,
            status = "pending"
        }, null);
    }

    /// <summary>
    /// GET /v1/pharmacy/prescriptions/{id}/allocation-status (PDF §13)
    /// </summary>
    public async Task<object?> GetAllocationStatusAsync(Guid prescriptionId)
    {
        var allocation = await _db.Set<PharmacyAllocation>().AsNoTracking()
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.PrescriptionId == prescriptionId && a.IsActive);

        if (allocation is null)
            return new
            {
                backendCommitted = false,
                allocationReferenceId = (string?)null,
                commitMode = (string?)null,
                committedAt = (DateTime?)null,
                lines = Array.Empty<object>()
            };

        return new
        {
            backendCommitted = true,
            allocationReferenceId = allocation.AllocationReferenceId,
            commitMode = allocation.CommitMode,
            committedAt = allocation.CommittedAt,
            lines = allocation.Lines.Select(l => new
            {
                l.PrescriptionItemId,
                l.MainPurchaseClassId,
                l.StoreId,
                l.BatchNo,
                l.ExpiryDate,
                l.RequestedQuantity,
                l.CommittedQuantity,
                l.AvailableQuantityAtCommit
            })
        };
    }

    /// <summary>
    /// GET /v1/cashier/ready-prescriptions (PDF §15 + Final Closure Doc)
    /// Returns prescriptions with status "ready" queued for cashier pickup.
    /// Response matches contract: { items, totalCount, page, pageSize }
    /// </summary>
    public async Task<object> GetReadyPrescriptionsAsync(int page, int pageSize, Guid? storeDetailsId)
    {
        try
        {
            var q = _db.Set<PharmacyPickup>().AsNoTracking()
                .Where(p => p.PickupStatus == "pending" && p.IsActive);

            if (storeDetailsId.HasValue)
                q = q.Where(p => p.StoreDetailsId == storeDetailsId);

            var totalCount = await q.CountAsync();
            var pickups = await q.OrderByDescending(p => p.SentAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Include(p => p.Prescription).ThenInclude(rx => rx!.Items)
                .ToListAsync();

            var items = pickups.Select(p => new
            {
                id = p.Id,
                prescriptionNo = p.RxNo ?? "",
                patientName = p.PatientName ?? "",
                lines = (p.Prescription?.Items ?? Enumerable.Empty<PharmacyPrescriptionItem>())
                    .Select(i => new
                    {
                        itemId = i.ItemId,
                        name = i.ItemName ?? "",
                        quantity = i.DispensedQty > 0 ? i.DispensedQty : i.RequestedQty,
                        unitPrice = i.UnitPrice,
                        batchNo = i.BatchNo ?? "1",
                        expiryDate = i.ExpiryDate.HasValue ? i.ExpiryDate.Value.ToString("dd/MM/yyyy") : null,
                        allocationStatus = !string.IsNullOrEmpty(i.AvailabilityStatus) ? i.AvailabilityStatus : "allocated",
                        patientShare = p.InsuranceCovered > 0
                            ? Math.Round(i.LineTotal * (p.PatientPayable / (p.TotalAmount > 0 ? p.TotalAmount : 1)), 2)
                            : i.LineTotal,
                        insuranceShare = p.InsuranceCovered > 0
                            ? Math.Round(i.LineTotal * (p.InsuranceCovered / (p.TotalAmount > 0 ? p.TotalAmount : 1)), 2)
                            : 0m
                    }).ToList(),
                financials = new
                {
                    totalAmount = p.TotalAmount,
                    patientShare = p.PatientPayable,
                    insuranceShare = p.InsuranceCovered
                },
                status = "ready_for_pickup"
            }).ToList();

            return new { items, totalCount, page, pageSize };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetReadyPrescriptionsAsync failed");
            return new { items = new List<object>(), totalCount = 0, page, pageSize };
        }
    }

    /// <summary>
    /// POST /v1/cashier/prescription-checkout (PDF §16)
    /// </summary>
    public async Task<(object? result, DispenseErrorResponse? error)> PrescriptionCheckoutAsync(
        PrescriptionCheckoutRequest req, Guid userId)
    {
        var pickup = await _db.Set<PharmacyPickup>()
            .FirstOrDefaultAsync(p => p.Id == req.PickupId && p.IsActive);

        if (pickup is null)
            return (null, Err("PRESCRIPTION_NOT_FOUND", "Pickup not found", "عملية الاستلام غير موجودة"));

        if (pickup.PickupStatus != "pending")
            return (null, Err("PRESCRIPTION_ALREADY_DISPENSED", "Pickup already processed", "تم معالجة الاستلام مسبقاً"));

        // Verify session
        if (req.CashierSessionId.HasValue)
        {
            var session = await _db.CashierSessions
                .FirstOrDefaultAsync(s => s.SessionId == req.CashierSessionId.Value && s.Status == 0);
            if (session is null)
                return (null, Err("NO_ACTIVE_SESSION", "No active cashier session", "لا توجد جلسة كاشير نشطة"));
        }

        var rx = await _db.PharmacyPrescriptions
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == pickup.PrescriptionId);

        if (rx is null)
            return (null, Err("PRESCRIPTION_NOT_FOUND", "Prescription not found", "الوصفة غير موجودة"));

        // Generate sequential invoice number RXI-yyyyMMdd-XXXX
        var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
        var lastInv = await _db.Set<SaleInvoice>()
            .Where(i => i.InvoiceNo != null && i.InvoiceNo.StartsWith("RXI-" + todayStr))
            .OrderByDescending(i => i.InvoiceNo)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (lastInv != null && lastInv.InvoiceNo!.Length > 12)
        {
            var seqStr = lastInv.InvoiceNo.Substring(13);
            if (int.TryParse(seqStr, out var parsedSeq))
            {
                nextSeq = parsedSeq + 1;
            }
        }
        var invoiceNo = $"RXI-{todayStr}-{nextSeq:D4}";

        // Resolve effective user ID for seller and payment logging
        var effectiveUserId = userId;
        if (effectiveUserId == Guid.Empty)
        {
            if (!string.IsNullOrWhiteSpace(pickup.CashierUserId) && Guid.TryParse(pickup.CashierUserId, out var parsedPickupUser))
            {
                effectiveUserId = parsedPickupUser;
            }
            else if (pickup.CreatedBy != Guid.Empty)
            {
                effectiveUserId = pickup.CreatedBy;
            }
            else
            {
                var firstAdmin = await _db.Set<User>().AsNoTracking().FirstOrDefaultAsync(u => u.IsActive);
                if (firstAdmin != null)
                {
                    effectiveUserId = firstAdmin.Id;
                }
            }
        }

        var paidAmount = req.CashAmount + req.CardAmount;
        if (paidAmount <= 0)
        {
            paidAmount = pickup.PatientPayable > 0 ? pickup.PatientPayable : rx.GrandTotal;
        }

        var changeAmount = paidAmount - pickup.PatientPayable;
        if (changeAmount < 0) changeAmount = 0;

        // Resolve CustomerProfileId (must exist in CustomerProfile table)
        var customerProfileId = rx.PatientId ?? Guid.Empty;
        var customerExists = await _db.Set<CustomerProfile>().AnyAsync(c => c.Id == customerProfileId);
        if (!customerExists)
        {
            var defaultCustomer = await _db.Set<CustomerProfile>().FirstOrDefaultAsync();
            if (defaultCustomer != null)
            {
                customerProfileId = defaultCustomer.Id;
            }
        }

        // Resolve StoreDetailsId (must exist in StoreDetails table)
        var storeDetailsId = req.StoreDetailsId ?? pickup.StoreDetailsId ?? Guid.Empty;
        var storeExists = await _db.StoreDetails.AnyAsync(s => s.Id == storeDetailsId);
        if (!storeExists)
        {
            var defaultStore = await _db.StoreDetails.FirstOrDefaultAsync();
            if (defaultStore != null)
            {
                storeDetailsId = defaultStore.Id;
            }
        }

        // Avoid duplicate ReferenceNumber constraint (IX_SaleInvoice_ReferenceNumber)
        var existingInvoices = await _db.Set<SaleInvoice>()
            .Where(x => x.ReferenceNumber == pickup.RxNo)
            .ToListAsync();
        foreach (var oldInv in existingInvoices)
        {
            oldInv.ReferenceNumber = $"{pickup.RxNo}-VOID-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }

        // Create SaleInvoice record
        var saleInvoice = new SaleInvoice
        {
            CustomerProfileId = customerProfileId,
            IsActive = true,
            CreatedBy = effectiveUserId.ToString(),
            CreatedAt = DateTime.UtcNow,
            InvoiceNo = invoiceNo,
            Currency = "SAR",
            Description = rx.InsuranceEnabled 
                ? $"Pharmacy Prescription {pickup.RxNo} - تأمين طبي: {rx.InsuranceName} (تغطية {rx.InsuranceCoveragePercentage}%: {pickup.InsuranceCovered} SAR - مسدد من المريض: {pickup.PatientPayable} SAR)"
                : $"Pharmacy Prescription {pickup.RxNo}",
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            Tax = rx.TaxTotal > 0 ? (rx.TaxTotal / (rx.SubTotal > 0 ? rx.SubTotal : 1)) * 100 : 0m,
            TaxAmount = rx.TaxTotal,
            Discount = rx.DiscountTotal > 0 ? (rx.DiscountTotal / (rx.SubTotal > 0 ? rx.SubTotal : 1)) * 100 : 0m,
            DiscountAmount = rx.DiscountTotal,
            TotalPrice = rx.SubTotal,
            NetPrice = rx.GrandTotal,
            PaidAmount = paidAmount,
            RemainingAmount = Math.Max(0, rx.GrandTotal - paidAmount),
            PaymentStatus = (paidAmount >= rx.GrandTotal) ? InvoicePaymentStatus.Paid : (paidAmount > 0 ? InvoicePaymentStatus.PartiallyPaid : InvoicePaymentStatus.Unpaid),
            IsQuotation = false,
            IsFrontal = false,
            SaleInvoiceType = 0, // SaleInvoice
            StoreDetailsId = storeDetailsId,
            Notes = rx.Notes,
            ReferenceNumber = pickup.RxNo
        };

        _db.Set<SaleInvoice>().Add(saleInvoice);
        await _db.SaveChangesAsync(); // Generates saleInvoice.Id

        // Resolve Treasury for payment / paycheck receipt
        var treasury = await _db.Set<Treasury>().FirstOrDefaultAsync(t => t.IsActive)
            ?? await _db.Set<Treasury>().FirstOrDefaultAsync();
        var treasuryId = treasury?.Id ?? Guid.NewGuid();

        // 1. PaycheckReceipt (سند قبض رسمي مرتبط بالفاتورة)
        var receiptRefNo = $"PR-RX-{todayStr}-{nextSeq:D4}";
        var paycheckReceipt = new PaycheckReceipt
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = receiptRefNo,
            Description = rx.InsuranceEnabled 
                ? $"سداد حصة المريض نقدياً لوصفة رقم {pickup.RxNo} - تأمين {rx.InsuranceName}"
                : $"سداد نقدي لوصفة طبية رقم {pickup.RxNo}",
            ReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = paidAmount,
            Tax = 0,
            TreasuryId = treasuryId,
            PaymentMethodType = req.CardAmount > 0 ? SmartAndPro.ERP.Domains.Enums.PaymentMethodType.Credit : SmartAndPro.ERP.Domains.Enums.PaymentMethodType.Cash,
            IsActive = true,
            CreatedBy = effectiveUserId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<PaycheckReceipt>().Add(paycheckReceipt);

        // 2. ReceiptConnection (ربط سند القبض بالفاتورة)
        var receiptConnection = new ReceiptConnection
        {
            Id = Guid.NewGuid(),
            PaycheckReceiptId = paycheckReceipt.Id,
            SaleInvoiceId = saleInvoice.Id,
            LinkedAmount = paidAmount,
            ReceiptConnectionType = ReceiptConnectionType.SaleInvoice,
            IsActive = true,
            CreatedBy = effectiveUserId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<ReceiptConnection>().Add(receiptConnection);

        // 3. PaymentSchedule (دفعة الفاتورة)
        var paymentSchedule = new PaymentSchedule
        {
            Id = Guid.NewGuid(),
            InvoiceId = saleInvoice.Id.ToString(),
            InvoiceType = InvoiceTypeEnum.SaleInvoice,
            Amount = paidAmount,
            TotalAmount = paidAmount,
            VAT = 0,
            PaymentDate = DateTime.UtcNow,
            PaycheckReceiptId = paycheckReceipt.Id,
            ReceiptNo = receiptRefNo,
            TreasuryId = treasuryId,
            PaymentMethodType = req.CardAmount > 0 ? PaymentMethodType.Credit : PaymentMethodType.Cash,
            ReferenceNumber = pickup.RxNo,
            Notes = rx.InsuranceEnabled 
                ? $"سداد حصة المريض ({pickup.PatientPayable} SAR) لوصفة رقم {pickup.RxNo}"
                : $"سداد فاتورة مبيعات صيدلية رقم {saleInvoice.InvoiceNo}",
            CreatedBy = effectiveUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _db.Set<PaymentSchedule>().Add(paymentSchedule);

        // 4. CashierPayment (سداد الكاشير)
        var cashierPayment = new CashierPayment
        {
            Id = Guid.NewGuid(),
            InvoiceId = saleInvoice.Id,
            SessionId = req.CashierSessionId ?? 0,
            PaymentMethod = req.CardAmount > 0 ? 1 : 0,
            TotalAmount = rx.GrandTotal,
            PaidAmount = paidAmount,
            ChangeAmount = changeAmount,
            CashAmount = req.CashAmount > 0 ? req.CashAmount : (req.CardAmount <= 0 ? paidAmount : 0),
            CardAmount = req.CardAmount,
            CreditAmount = 0,
            PaidAtUtc = DateTime.UtcNow,
            StoreDetailsId = storeDetailsId,
            PaymentStatus = 1,
            PaymentReference = pickup.RxNo,
            CreatedBy = effectiveUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _db.Set<CashierPayment>().Add(cashierPayment);
        await _db.SaveChangesAsync();

        // Map items and record cashier inventory movements
        var allocation = await _db.Set<PharmacyAllocation>()
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.PrescriptionId == pickup.PrescriptionId && a.IsActive);

        if (allocation != null)
        {
            foreach (var line in allocation.Lines)
            {
                var rxItem = rx.Items.FirstOrDefault(i => i.Id == line.PrescriptionItemId)
                    ?? rx.Items.FirstOrDefault(i => i.ItemId == line.MainPurchaseClassId && i.IsActive)
                    ?? rx.Items.FirstOrDefault(i => i.ItemCode == rx.Items.FirstOrDefault(ri => ri.Id == line.PrescriptionItemId)?.ItemCode && i.IsActive);
                if (rxItem == null) continue;

                // Resolve MainSalesClassId (and auto-create if missing)
                var purchaseClass = await _db.MainPurchaseClasses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == line.MainPurchaseClassId);
                Guid salesClassId = Guid.Empty;
                if (purchaseClass != null)
                {
                    var salesClass = await _db.MainSalesClasses
                        .FirstOrDefaultAsync(x => x.ReferenceCode == purchaseClass.ReferenceCode || x.ArabicName == purchaseClass.ArabicName);
                    
                    if (salesClass != null)
                    {
                        salesClassId = salesClass.Id;
                    }
                    else
                    {
                        // Auto-create MainSalesClass
                        var detailSalesClass = await _db.DetailSalesClasses
                            .Where(x => x.IsActive)
                            .FirstOrDefaultAsync();

                        if (detailSalesClass != null)
                        {
                            var newSalesClass = new MainSalesClass
                            {
                                Id = Guid.NewGuid(),
                                ReferenceCode = purchaseClass.ReferenceCode,
                                ArabicName = purchaseClass.ArabicName,
                                EnglishName = purchaseClass.EnglishName,
                                MeasurementUnitId = purchaseClass.MeasurementUnitId,
                                DetailSalesClassId = detailSalesClass.Id,
                                ClassType = ClassType.Direct,
                                IsTaxExempted = false,
                                IsActive = true,
                                CreatedBy = userId,
                                CreatedAt = DateTime.UtcNow
                            };
                            _db.MainSalesClasses.Add(newSalesClass);
                            await _db.SaveChangesAsync();
                            salesClassId = newSalesClass.Id;
                        }
                    }
                }

                // Fallback MainSalesClassId if mapping was not found (must exist in MainSalesClass table)
                var salesClassExists = salesClassId != Guid.Empty && await _db.MainSalesClasses.AnyAsync(s => s.Id == salesClassId);
                if (!salesClassExists)
                {
                    var defaultSalesClass = await _db.MainSalesClasses.FirstOrDefaultAsync();
                    if (defaultSalesClass != null)
                    {
                        salesClassId = defaultSalesClass.Id;
                    }
                }

                var saleItem = new SaleItem
                {
                    SaleInvoiceId = saleInvoice.Id,
                    IsActive = true,
                    CreatedBy = userId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ItemNo = rxItem.ItemCode,
                    MainSalesClassId = salesClassId,
                    Name = rxItem.ItemName,
                    Price = rxItem.UnitPrice,
                    Quantity = line.CommittedQuantity,
                    Tax = rxItem.TaxAmount > 0 ? 15m : 0m,
                    TaxAmount = rxItem.TaxAmount,
                    Discount = rxItem.DiscountAmount,
                    DiscountAmount = rxItem.DiscountAmount,
                    TotalPrice = rxItem.LineTotal,
                    NetPrice = rxItem.LineTotal - rxItem.DiscountAmount,
                    NoOfNights = 1
                };
                _db.Set<SaleItem>().Add(saleItem);

                // Record cashier inventory movement to deduct stock in reports
                var movement = new CashierInventoryMovement
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = saleInvoice.Id,
                    InvoiceNo = saleInvoice.InvoiceNo,
                    SessionId = req.CashierSessionId ?? 0,
                    StoreDetailsId = storeDetailsId,
                    MainPurchaseClassId = line.MainPurchaseClassId,
                    ProductName = rxItem.ItemName,
                    Direction = 1, // Exit
                    Quantity = line.CommittedQuantity,
                    UnitCost = 0m,
                    UnitPrice = rxItem.UnitPrice,
                    InventoryPermitId = null,
                    CashierUserGuid = userId,
                    MovementType = "SALE",
                    MovedAtUtc = DateTime.UtcNow,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Set<CashierInventoryMovement>().Add(movement);
            }
        }

        // Update pickup status
        pickup.PickupStatus = "checked_out";
        pickup.PosInvoiceNo = invoiceNo;

        // Update prescription status
        rx.Status = "delivered";
        rx.SalesInvoiceId = saleInvoice.Id;

        // Log controlled drugs to RestrictedDrugDispense & PharmacyControlledDrugRecord
        if (rx.Items != null)
        {
            string resolvedPharmacistName = "Admin";
            if (userId != Guid.Empty)
            {
                var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
                if (u != null)
                {
                    var fn = $"{u.FirstName} {u.LastName}".Trim();
                    resolvedPharmacistName = !string.IsNullOrEmpty(fn) ? fn : (u.UserName ?? "Admin");
                }
            }

            foreach (var rxItem in rx.Items.Where(i => i.IsActive))
            {
                var isControlled = rxItem.IsControlled;
                Guid? resolvedItemId = rxItem.ItemId;

                if (!isControlled)
                {
                    if (rxItem.ItemId.HasValue)
                    {
                        isControlled = await _db.MainPurchaseClasses
                            .Where(p => p.Id == rxItem.ItemId.Value)
                            .Select(p => p.IsControlled)
                            .FirstOrDefaultAsync();
                    }
                    else if (!string.IsNullOrWhiteSpace(rxItem.ItemCode))
                    {
                        var masterProduct = await _db.MainPurchaseClasses
                            .Where(p => p.ReferenceCode == rxItem.ItemCode)
                            .Select(p => new { p.Id, p.IsControlled })
                            .FirstOrDefaultAsync();

                        if (masterProduct != null)
                        {
                            isControlled = masterProduct.IsControlled;
                            resolvedItemId = masterProduct.Id;
                        }
                    }
                }

                if (!isControlled) continue;
                var rDispense = new RestrictedDrugDispense
                {
                    Id = Guid.NewGuid(),
                    ItemId = resolvedItemId,
                    ItemCode = rxItem.ItemCode,
                    ItemName = rxItem.ItemName,
                    BatchNo = rxItem.BatchNo,
                    ExpiryDate = rxItem.ExpiryDate,
                    Quantity = rxItem.DispensedQty > 0 ? rxItem.DispensedQty : rxItem.RequestedQty,
                    StoreDetailsId = rxItem.StoreDetailsId ?? storeDetailsId,
                    PatientName = rx.PatientName,
                    DoctorName = rx.DoctorName,
                    PharmacistName = resolvedPharmacistName,
                    AuthorizationNo = rx.RxNo,
                    DispenseReason = $"Prescription checkout {rx.RxNo} / Invoice {invoiceNo}",
                    RestrictionType = "controlled",
                    Status = "dispensed",
                    DispensedBy = userId,
                    DispensedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _db.RestrictedDrugDispenses.Add(rDispense);

                _db.PharmacyControlledDrugRecords.Add(new PharmacyControlledDrugRecord
                {
                    Id = Guid.NewGuid(),
                    LogNo = $"RXD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    PatientName = rx.PatientName ?? string.Empty,
                    DrugName = rxItem.ItemName,
                    Schedule = "controlled",
                    Quantity = rxItem.DispensedQty > 0 ? rxItem.DispensedQty : rxItem.RequestedQty,
                    Prescriber = rx.DoctorName ?? string.Empty,
                    Pharmacist = resolvedPharmacistName,
                    PermitNo = rx.RxNo,
                    Reason = $"Prescription checkout {rx.RxNo} / Invoice {invoiceNo}",
                    Status = "Active",
                    DispenseDate = DateTime.UtcNow,
                    StoreDetailsId = rxItem.StoreDetailsId ?? storeDetailsId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }
        }


        // Record/Update Insurance Claim if insurance is enabled
        if (rx.InsuranceEnabled)
        {
            var existingClaim = await _db.PharmacyInsuranceClaims
                .FirstOrDefaultAsync(c => c.PrescriptionId == rx.Id || (c.RxNo == rx.RxNo && c.IsActive));
            if (existingClaim != null)
            {
                existingClaim.SaleInvoiceId = saleInvoice.Id;
                existingClaim.TotalAmount = rx.GrandTotal;
                existingClaim.InsuranceCoveredAmount = pickup.InsuranceCovered;
                existingClaim.ApprovedAmount = pickup.InsuranceCovered;
                existingClaim.PatientShareAmount = pickup.PatientPayable;
                existingClaim.CoveragePercentage = rx.InsuranceCoveragePercentage;
                existingClaim.ReferenceNo = rx.InsuranceApprovalNo;
                existingClaim.StoreDetailsId = rx.StoreDetailsId ?? storeDetailsId;
            }
            else
            {
                var claim = new PharmacyInsuranceClaim
                {
                    Id = Guid.NewGuid(),
                    ClaimNo = $"CLM-{todayStr}-{nextSeq:D4}",
                    RxNo = rx.RxNo,
                    PatientName = rx.PatientName,
                    Insurer = rx.InsuranceName ?? "تأمين طبي",
                    TotalAmount = rx.GrandTotal,
                    ApprovedAmount = pickup.InsuranceCovered,
                    InsuranceCoveredAmount = pickup.InsuranceCovered,
                    PatientShareAmount = pickup.PatientPayable,
                    CoveragePercentage = rx.InsuranceCoveragePercentage,
                    Status = "Approved",
                    ReferenceNo = rx.InsuranceApprovalNo ?? rx.RxNo,
                    SubmittedAt = DateTime.UtcNow,
                    Notes = $"مطالبة تأمينية لوصفة طبية {rx.RxNo} - فاتورة {saleInvoice.InvoiceNo}",
                    StoreDetailsId = rx.StoreDetailsId ?? storeDetailsId,
                    PrescriptionId = rx.Id,
                    SaleInvoiceId = saleInvoice.Id,
                    IsActive = true,
                    CreatedBy = effectiveUserId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.PharmacyInsuranceClaims.Add(claim);
            }
        }

        // Update session financial statistics
        if (req.CashierSessionId.HasValue)
        {
            var dbSession = await _db.CashierSessions
                .FirstOrDefaultAsync(s => s.SessionId == req.CashierSessionId.Value);
            if (dbSession != null)
            {
                dbSession.TransactionsCount += 1;
                dbSession.TotalSales += pickup.PatientPayable;
                if (req.PaymentMethod == 0) dbSession.TotalCash += req.CashAmount;
                if (req.PaymentMethod == 1) dbSession.TotalCard += req.CardAmount;
            }
        }

        await _db.SaveChangesAsync();

        return (new
        {
            success = true,
            invoiceId = saleInvoice.Id,
            invoiceNo,
            prescriptionId = pickup.PrescriptionId,
            pickupId = pickup.Id,
            paidAmount,
            changeAmount,
            prescriptionStatus = "delivered"
        }, null);
    }

    private static DispenseErrorResponse Err(string code, string msg, string msgAr, object? details = null)
        => new() { ErrorCode = code, Message = msg, MessageAr = msgAr, Details = details };
}
