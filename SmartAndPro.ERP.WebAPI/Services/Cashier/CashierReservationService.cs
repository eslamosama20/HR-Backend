using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Permits;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Application.Services.Notes.CreditNotes;

namespace SmartAndPro.ERP.WebAPI.Services.Cashier;

public interface ICashierReservationService
{
    Task<IReadOnlyDictionary<Guid, decimal>> GetReservedQuantitiesByClassAsync(Guid storeDetailsId, IReadOnlyCollection<Guid> classIds, int? excludeInvoiceId = null, CancellationToken cancellationToken = default);
    Task ReserveForInvoiceAsync(int invoiceId, int? sessionId, Guid storeDetailsId, Guid userGuid, IReadOnlyDictionary<Guid, decimal> quantitiesByClass, string source = "POS", CancellationToken cancellationToken = default);
    Task ConfirmInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task ReleaseInvoiceAsync(int invoiceId, string reason, CancellationToken cancellationToken = default);
    Task<Guid?> CreateInventoryExitPermitAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateInventoryEntryPermitForRefundAsync(Guid refundId, CancellationToken cancellationToken = default);
    Task CreateSalesCreditNoteForRefundAsync(Guid refundId, CancellationToken cancellationToken = default);
}

public class CashierReservationService : ICashierReservationService
{
    private readonly ERPContext _context;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ILogger<CashierReservationService> _logger;
    private static readonly string _debugLogPath = Path.Combine(AppContext.BaseDirectory, "cashier-debug.log");

    private static void DebugLog(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        Console.WriteLine(line);
        try { File.AppendAllText(_debugLogPath, line + Environment.NewLine); } catch { }
    }

    public CashierReservationService(
        ERPContext context, 
        ServiceBaseFactory serviceBaseFactory,
        ILogger<CashierReservationService> logger)
    {
        _context = context;
        _serviceBaseFactory = serviceBaseFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetReservedQuantitiesByClassAsync(
        Guid storeDetailsId,
        IReadOnlyCollection<Guid> classIds,
        int? excludeInvoiceId = null,
        CancellationToken cancellationToken = default)
    {
        if (classIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var normalizedClassIds = classIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (normalizedClassIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var query = _context.CashierStockReservations
            .AsNoTracking()
            .Where(x => x.StoreDetailsId == storeDetailsId
                        && x.Status == CashierReservationStatus.Reserved
                        && normalizedClassIds.Contains(x.MainSalesClassId));

        if (excludeInvoiceId.HasValue)
        {
            var excluded = excludeInvoiceId.Value;
            query = query.Where(x => x.InvoiceId != excluded);
        }

        return await query
            .GroupBy(x => x.MainSalesClassId)
            .Select(group => new { group.Key, Qty = group.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.Key, x => x.Qty, cancellationToken);
    }

    public async Task ReserveForInvoiceAsync(
        int invoiceId,
        int? sessionId,
        Guid storeDetailsId,
        Guid userGuid,
        IReadOnlyDictionary<Guid, decimal> quantitiesByClass,
        string source = "POS",
        CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0 || quantitiesByClass.Count == 0)
        {
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var existing = await _context.CashierStockReservations
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync(cancellationToken);

            if (existing.Count > 0)
            {
                _context.CashierStockReservations.RemoveRange(existing);
                await _context.SaveChangesAsync(cancellationToken);
            }

            foreach (var entry in quantitiesByClass)
            {
                if (entry.Key == Guid.Empty || entry.Value <= 0m)
                {
                    continue;
                }

                _context.CashierStockReservations.Add(new CashierStockReservation
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoiceId,
                    SessionId = sessionId,
                    StoreDetailsId = storeDetailsId,
                    MainSalesClassId = entry.Key,
                    Quantity = entry.Value,
                    Status = CashierReservationStatus.Reserved,
                    ReservedAtUtc = DateTime.UtcNow,
                    Source = source,
                    CreatedBy = userGuid,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ConfirmInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return;
        }

        Console.WriteLine($"==================== [CashierReservationService] ConfirmInvoiceAsync called for invoice {invoiceId} ====================");
        DebugLog($"ConfirmInvoiceAsync called for invoice {invoiceId}");
        _logger.LogInformation("[CashierReservationService] ConfirmInvoiceAsync called for invoice {InvoiceId}", invoiceId);

        // Use Serializable isolation to prevent concurrent stock modifications
        await using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, 
            cancellationToken);
        
        try
        {
            var reservations = await _context.CashierStockReservations
                .Where(x => x.InvoiceId == invoiceId && x.Status == CashierReservationStatus.Reserved)
                .ToListAsync(cancellationToken);

            Console.WriteLine($"[CashierReservationService] Found {reservations.Count} reservations for invoice {invoiceId}");
            _logger.LogInformation("[CashierReservationService] Found {Count} reservations for invoice {InvoiceId}", reservations.Count, invoiceId);

            if (reservations.Count == 0)
            {
                _logger.LogInformation("[CashierReservationService] No reservations to confirm for invoice {InvoiceId}", invoiceId);
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var now = DateTime.UtcNow;
            
            // Mark all reservations as confirmed
            // Stock deduction will be handled by CreateInventoryExitPermitAsync via InventoryPermit system
            foreach (var reservation in reservations)
            {
                reservation.Status = CashierReservationStatus.Confirmed;
                reservation.ConfirmedAtUtc = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            Console.WriteLine($"[CashierReservationService] Reservations confirmed for invoice {invoiceId}, now creating exit permit");
            _logger.LogInformation("[CashierReservationService] Reservations confirmed for invoice {InvoiceId}, now creating exit permit", invoiceId);

            // إنشاء إذن خروج تلقائي بعد تأكيد الدفع
            // هذا سيخصم الكميات من المخزون عبر نظام إدارة المخازن
            // IMPORTANT: This is called OUTSIDE the transaction to avoid nested transaction conflicts
            try
            {
                await CreateInventoryExitPermitAsync(invoiceId, cancellationToken);
                Console.WriteLine($"[CashierReservationService] Exit permit created successfully for invoice {invoiceId}");
                _logger.LogInformation("[CashierReservationService] Exit permit created successfully for invoice {InvoiceId}", invoiceId);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the entire payment
                // The payment has already been committed, so we need to handle this gracefully
                Console.WriteLine($"[CashierReservationService] ERROR creating exit permit for invoice {invoiceId}: {ex.Message}");
                Console.WriteLine($"[CashierReservationService] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "[CashierReservationService] ERROR creating exit permit for invoice {InvoiceId}", invoiceId);
                
                // TODO: Queue for retry or manual intervention
                // For now, we log it but don't throw to avoid breaking the payment flow
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException(
                "Stock was modified by another user during payment processing. Please retry the transaction.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Guid?> CreateInventoryExitPermitAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"==================== [CashierReservationService] Starting CreateInventoryExitPermitAsync for invoice {invoiceId} ====================");
            _logger.LogInformation("[CashierReservationService] Starting CreateInventoryExitPermitAsync for invoice {InvoiceId}", invoiceId);
            
            // جلب الفاتورة والحجوزات المرتبطة
            var reservations = await _context.CashierStockReservations
                .Where(x => x.InvoiceId == invoiceId && x.Status == CashierReservationStatus.Confirmed)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("[CashierReservationService] Found {Count} confirmed reservations", reservations.Count);

            if (reservations.Count == 0)
            {
                _logger.LogInformation("[CashierReservationService] No confirmed reservations found for invoice {InvoiceId}", invoiceId);
                return null; // لا توجد حجوزات لهذه الفاتورة
            }

            var storeDetailsId = reservations.First().StoreDetailsId;
            _logger.LogInformation("[CashierReservationService] Store ID: {StoreDetailsId}", storeDetailsId);

            // جلب بيانات المخزن للحصول على MainAccountId
            var store = await _context.StoreDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storeDetailsId, cancellationToken);

            if (store == null)
            {
                throw new InvalidOperationException($"Store {storeDetailsId} not found");
            }

            _logger.LogInformation("[CashierReservationService] Store MainAccountId: {MainAccountId}", store.MainAccountId);

            // استخدام MainAccountId من المخزن، أو استخدام Guid افتراضي
            var mainAccountId = store.MainAccountId ?? Guid.Empty;
            if (mainAccountId == Guid.Empty)
            {
                _logger.LogInformation("[CashierReservationService] Store has no MainAccountId, searching for default account");
                
                // محاولة جلب حساب المخزون الافتراضي
                var defaultInventoryAccount = await _context.MainAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ReferenceCode == "1-1-01" || a.ArabicName.Contains("مخزون"), cancellationToken);
                
                if (defaultInventoryAccount != null)
                {
                    mainAccountId = defaultInventoryAccount.Id;
                    _logger.LogInformation("[CashierReservationService] Found default inventory account: {MainAccountId}", mainAccountId);
                }
                else
                {
                    // إذا لم يتم العثور على حساب، استخدم أول حساب متاح
                    var firstAccount = await _context.MainAccounts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);
                    
                    if (firstAccount != null)
                    {
                        mainAccountId = firstAccount.Id;
                        _logger.LogInformation("[CashierReservationService] Using first available account: {MainAccountId}", mainAccountId);
                    }
                    else
                    {
                        throw new InvalidOperationException("لا يوجد حسابات محاسبية متاحة في النظام. يرجى إنشاء حساب للمخزون.");
                    }
                }
            }

            // إنشاء رقم مرجعي فريد
            var referenceNumber = $"CASHIER-EXIT-{invoiceId}-{DateTime.Now:yyyyMMddHHmmss}";
            _logger.LogInformation("[CashierReservationService] Creating permit with reference: {ReferenceNumber}", referenceNumber);

            // إنشاء DTO لإذن الخروج
            var inventoryPermitDto = new InventoryPermitPostDto
            {
                ReferenceNumber = referenceNumber,
                Description = $"إذن خروج تلقائي - فاتورة كاشير رقم {invoiceId}",
                PermitDate = DateOnly.FromDateTime(DateTime.UtcNow),
                StoreDetailsId = storeDetailsId,
                MainAccountId = mainAccountId,
                PermitType = PermitType.Exit.ToString(),
                ShippingCost = 0,
                PermitEntries = new List<InventoryPermitEntryPostDto>()
            };

            // إنشاء بنود إذن الخروج من الحجوزات
            // --- بناء خريطة MainSalesClassId → MainPurchaseClassId عبر مطابقة الاسم/الكود ---
            var salesClassIds = reservations
                .Select(r => r.MainSalesClassId)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var salesClasses = await _context.MainSalesClasses
                .AsNoTracking()
                .Where(sc => salesClassIds.Contains(sc.Id))
                .ToListAsync(cancellationToken);

            DebugLog($"Loaded {salesClasses.Count} MainSalesClass records for {salesClassIds.Count} IDs");

            // تحميل جميع تصنيفات الشراء لمطابقتها
            var purchaseClasses = await _context.MainPurchaseClasses
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            DebugLog($"Loaded {purchaseClasses.Count} MainPurchaseClass records");

            // بناء قواميس بحث حسب الكود والاسم العربي والاسم الانجليزي
            var purchaseByRefCode = new Dictionary<string, MainPurchaseClass>();
            var purchaseByArabicName = new Dictionary<string, MainPurchaseClass>();
            var purchaseByEnglishName = new Dictionary<string, MainPurchaseClass>();

            foreach (var pc in purchaseClasses)
            {
                var normalizedRef = NormalizeLookup(pc.ReferenceCode);
                if (normalizedRef.Length > 0 && !purchaseByRefCode.ContainsKey(normalizedRef))
                    purchaseByRefCode[normalizedRef] = pc;

                var normalizedAr = NormalizeLookup(pc.ArabicName);
                if (normalizedAr.Length > 0 && !purchaseByArabicName.ContainsKey(normalizedAr))
                    purchaseByArabicName[normalizedAr] = pc;

                var normalizedEn = NormalizeLookup(pc.EnglishName);
                if (normalizedEn.Length > 0 && !purchaseByEnglishName.ContainsKey(normalizedEn))
                    purchaseByEnglishName[normalizedEn] = pc;
            }

            // بناء خريطة MainSalesClassId → MainPurchaseClassId
            var salesToPurchaseMap = new Dictionary<Guid, Guid>();
            foreach (var sc in salesClasses)
            {
                var matchedPurchase = TryMapSalesClassToPurchaseClass(
                    sc, purchaseByRefCode, purchaseByArabicName, purchaseByEnglishName);

                if (matchedPurchase.HasValue)
                {
                    salesToPurchaseMap[sc.Id] = matchedPurchase.Value;
                    DebugLog($"Mapped SalesClass '{sc.ArabicName}' ({sc.Id}) → PurchaseClass ({matchedPurchase.Value})");
                }
                else
                {
                    DebugLog($"WARNING: No matching PurchaseClass for SalesClass '{sc.ArabicName}' (Ref={sc.ReferenceCode}, En={sc.EnglishName})");
                }
            }

            // جلب أذونات المخزون الحالية لمعرفة الكميات المتاحة حسب (المنتج + الصلاحية + الرف + الصف)
            var existingPermits = await _context.InventoryPermits
                .AsNoTracking()
                .Include(p => p.PermitEntries)
                .Where(p => p.StoreDetailsId == storeDetailsId && p.IsActive)
                .ToListAsync(cancellationToken);

            // بناء خريطة المخزون المتاح: (PurchaseClassId, ExpiryDate, Shelf, Row) → Available Qty
            var stockBuckets = new Dictionary<(Guid purchaseClassId, DateOnly? expiry, string shelf, string row), decimal>();
            foreach (var permit in existingPermits)
            {
                foreach (var entry in permit.PermitEntries.Where(e => e.IsActive))
                {
                    var key = (entry.MainPurchaseClassId, entry.ExpiryDate, entry.ShelfNumber ?? "", entry.RowNumber ?? "");
                    if (!stockBuckets.ContainsKey(key))
                        stockBuckets[key] = 0;

                    if (permit.PermitType == PermitType.Entry)
                        stockBuckets[key] += entry.Quantity;
                    else if (permit.PermitType == PermitType.Exit)
                        stockBuckets[key] -= entry.Quantity;
                }
            }

            DebugLog($"Stock buckets loaded: {stockBuckets.Count} unique (product+expiry+shelf+row) combinations");

            foreach (var reservation in reservations)
            {
                _logger.LogInformation("[CashierReservationService] Processing reservation: MainSalesClassId={MainSalesClassId}, Qty={Quantity}", reservation.MainSalesClassId, reservation.Quantity);
                
                // الحصول على MainPurchaseClassId من خريطة المطابقة
                if (!salesToPurchaseMap.TryGetValue(reservation.MainSalesClassId, out var mainPurchaseClassId))
                {
                    DebugLog($"WARNING: No PurchaseClass mapping for reservation MainSalesClassId={reservation.MainSalesClassId}");
                    _logger.LogWarning("[CashierReservationService] No PurchaseClass mapping for MainSalesClassId={MainSalesClassId}", reservation.MainSalesClassId);
                    continue;
                }

                // توزيع الكمية المطلوبة على الـ buckets المتاحة (FIFO بحسب تاريخ الصلاحية)
                var remainingQty = reservation.Quantity;
                var relevantBuckets = stockBuckets
                    .Where(b => b.Key.purchaseClassId == mainPurchaseClassId && b.Value > 0)
                    .OrderBy(b => b.Key.expiry) // أقرب صلاحية أولاً (FEFO)
                    .ToList();

                DebugLog($"PurchaseClassId={mainPurchaseClassId}: need {remainingQty}, found {relevantBuckets.Count} buckets with stock");

                foreach (var bucket in relevantBuckets)
                {
                    if (remainingQty <= 0) break;

                    var deductQty = Math.Min(remainingQty, bucket.Value);

                    var permitEntry = new InventoryPermitEntryPostDto
                    {
                        UnitPrice = 0,
                        Quantity = deductQty,
                        MainPurchaseClassId = mainPurchaseClassId,
                        MainSalesClassId = reservation.MainSalesClassId,
                        ExpiryDate = bucket.Key.expiry,
                        ShelfNumber = string.IsNullOrEmpty(bucket.Key.shelf) ? null : bucket.Key.shelf,
                        RowNumber = string.IsNullOrEmpty(bucket.Key.row) ? null : bucket.Key.row
                    };

                    inventoryPermitDto.PermitEntries.Add(permitEntry);
                    stockBuckets[bucket.Key] -= deductQty;
                    remainingQty -= deductQty;

                    DebugLog($"  Bucket (expiry={bucket.Key.expiry}, shelf={bucket.Key.shelf}, row={bucket.Key.row}): deduct {deductQty}, remaining={remainingQty}");
                }

                if (remainingQty > 0)
                {
                    DebugLog($"WARNING: Not enough stock for PurchaseClassId={mainPurchaseClassId}. Short by {remainingQty}");
                }
            }

            _logger.LogInformation("[CashierReservationService] Created {Count} permit entries", inventoryPermitDto.PermitEntries.Count);
            DebugLog($"Created {inventoryPermitDto.PermitEntries.Count} permit entries for invoice {invoiceId}");

            // إنشاء إذن الخروج باستخدام InventoryPermitService
            // هذا سيضمن إنشاء InventoryOperation الذي يخصم الكميات من المخزون
            if (inventoryPermitDto.PermitEntries.Count > 0)
            {
                DebugLog($"Calling InventoryPermitService.AddAsync with {inventoryPermitDto.PermitEntries.Count} entries for invoice {invoiceId}");
                var inventoryPermitService = _serviceBaseFactory.GetService<InventoryPermitService>();
                var result = await inventoryPermitService.AddAsync(inventoryPermitDto);
                
                DebugLog($"AddAsync result: ErrorCode={result.ErrorCode}, HasData={result.Data != null}, PermitId={result.Data?.Id}");
                
                if (result.Data != null)
                {
                    DebugLog($"EXIT PERMIT CREATED SUCCESSFULLY: PermitId={result.Data.Id} for invoice {invoiceId}");
                    return result.Data.Id;
                }
                else
                {
                    DebugLog($"ERROR: AddAsync returned no data. ErrorCode={result.ErrorCode}");
                    throw new InvalidOperationException($"فشل إنشاء إذن الصرف: {result.ErrorCode}");
                }
            }
            else
            {
                DebugLog($"No valid permit entries to create for invoice {invoiceId}. Check SalesClass→PurchaseClass name mapping.");
                _logger.LogWarning("[CashierReservationService] No valid permit entries to create for invoice {InvoiceId}", invoiceId);
            }

            return null;
        }
        catch (Exception ex)
        {
            DebugLog($"CRITICAL ERROR creating exit permit for invoice {invoiceId}: {ex.Message}");
            DebugLog($"Stack: {ex.StackTrace}");
            if (ex.InnerException != null)
                DebugLog($"Inner: {ex.InnerException.Message}");
            _logger.LogError(ex, "[CashierReservationService] CRITICAL: Failed to create exit permit for invoice {InvoiceId}", invoiceId);
            
            // Return null instead of throwing to allow payment to complete
            return null;
        }
    }

    public async Task<Guid?> CreateInventoryEntryPermitForRefundAsync(Guid refundId, CancellationToken cancellationToken = default)
    {
        try
        {
            // ─── Automatically Create Sales Credit Note & Ledger Entries ───
            try
            {
                await CreateSalesCreditNoteForRefundAsync(refundId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CashierReservationService] Failed to automatically create Sales Credit Note for refund {RefundId}", refundId);
            }

            Console.WriteLine($"==================== [CashierReservationService] Starting CreateInventoryEntryPermitForRefundAsync for refund {refundId} ====================");
            _logger.LogInformation("[CashierReservationService] Starting CreateInventoryEntryPermitForRefundAsync for refund {RefundId}", refundId);

            // Fetch the refund and its items
            var refund = await _context.CashierRefunds
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == refundId, cancellationToken);

            if (refund == null || refund.Items == null || refund.Items.Count == 0)
            {
                _logger.LogWarning("[CashierReservationService] Refund {RefundId} not found or has no items", refundId);
                return null;
            }

            var storeDetailsId = refund.StoreDetailsId;
            if (!storeDetailsId.HasValue)
            {
                _logger.LogWarning("[CashierReservationService] Refund {RefundId} has no StoreDetailsId", refundId);
                return null;
            }

            // Fetch store to get MainAccountId
            var store = await _context.StoreDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storeDetailsId.Value, cancellationToken);

            if (store == null)
            {
                throw new InvalidOperationException($"Store {storeDetailsId.Value} not found");
            }

            var mainAccountId = store.MainAccountId ?? Guid.Empty;
            if (mainAccountId == Guid.Empty)
            {
                var defaultInventoryAccount = await _context.MainAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ReferenceCode == "1-1-01" || a.ArabicName.Contains("مخزون"), cancellationToken);
                if (defaultInventoryAccount != null)
                {
                    mainAccountId = defaultInventoryAccount.Id;
                }
                else
                {
                    var firstAccount = await _context.MainAccounts.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
                    if (firstAccount != null)
                    {
                        mainAccountId = firstAccount.Id;
                    }
                }
            }

            var referenceNumber = $"REFUND-ENTRY-{refund.RefundNo}-{DateTime.Now:yyyyMMddHHmmss}";

            var inventoryPermitDto = new InventoryPermitPostDto
            {
                ReferenceNumber = referenceNumber,
                Description = $"إذن دخول تلقائي مرتجع كاشير - فاتورة رقم {refund.InvoiceNo} - مرتجع رقم {refund.RefundNo}",
                PermitDate = DateOnly.FromDateTime(DateTime.UtcNow),
                StoreDetailsId = storeDetailsId.Value,
                MainAccountId = mainAccountId,
                PermitType = PermitType.Entry.ToString(),
                ShippingCost = 0,
                PermitEntries = new List<InventoryPermitEntryPostDto>()
            };

            // Build purchase classes lists
            var purchaseClasses = await _context.MainPurchaseClasses.AsNoTracking().ToListAsync(cancellationToken);
            var purchaseByRefCode = new Dictionary<string, MainPurchaseClass>();
            var purchaseByArabicName = new Dictionary<string, MainPurchaseClass>();
            var purchaseByEnglishName = new Dictionary<string, MainPurchaseClass>();

            foreach (var pc in purchaseClasses)
            {
                var normalizedRef = NormalizeLookup(pc.ReferenceCode);
                if (normalizedRef.Length > 0 && !purchaseByRefCode.ContainsKey(normalizedRef))
                    purchaseByRefCode[normalizedRef] = pc;

                var normalizedAr = NormalizeLookup(pc.ArabicName);
                if (normalizedAr.Length > 0 && !purchaseByArabicName.ContainsKey(normalizedAr))
                    purchaseByArabicName[normalizedAr] = pc;

                var normalizedEn = NormalizeLookup(pc.EnglishName);
                if (normalizedEn.Length > 0 && !purchaseByEnglishName.ContainsKey(normalizedEn))
                    purchaseByEnglishName[normalizedEn] = pc;
            }

            // Map SaleItemId to MainSalesClass
            foreach (var item in refund.Items)
            {
                var saleItem = await _context.SaleItems
                    .AsNoTracking()
                    .Include(si => si.MainSalesClass)
                    .FirstOrDefaultAsync(si => si.Id == item.SaleItemId, cancellationToken);

                if (saleItem == null || saleItem.MainSalesClass == null)
                {
                    _logger.LogWarning("[CashierReservationService] SaleItem {SaleItemId} not found or has no MainSalesClass", item.SaleItemId);
                    continue;
                }

                var matchedPurchase = TryMapSalesClassToPurchaseClass(
                    saleItem.MainSalesClass, purchaseByRefCode, purchaseByArabicName, purchaseByEnglishName);

                if (!matchedPurchase.HasValue)
                {
                    _logger.LogWarning("[CashierReservationService] No purchase class match for SalesClass '{Name}'", saleItem.MainSalesClass.ArabicName);
                    continue;
                }

                var permitEntry = new InventoryPermitEntryPostDto
                {
                    UnitPrice = item.UnitPrice,
                    Quantity = item.ReturnQty,
                    MainPurchaseClassId = matchedPurchase.Value,
                    MainSalesClassId = saleItem.MainSalesClassId,
                    ExpiryDate = null,
                    ShelfNumber = null,
                    RowNumber = null
                };

                inventoryPermitDto.PermitEntries.Add(permitEntry);
            }

            if (inventoryPermitDto.PermitEntries.Count > 0)
            {
                var inventoryPermitService = _serviceBaseFactory.GetService<InventoryPermitService>();
                var result = await inventoryPermitService.AddAsync(inventoryPermitDto);
                if (result.Data != null)
                {
                    _logger.LogInformation("[CashierReservationService] Refund entry permit created: {PermitId}", result.Data.Id);
                    return result.Data.Id;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CashierReservationService] Failed to create entry permit for refund {RefundId}", refundId);
            return null;
        }
    }

    public async Task CreateSalesCreditNoteForRefundAsync(Guid refundId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[CashierReservationService] Starting CreateSalesCreditNoteForRefundAsync for refund {RefundId}", refundId);

        var refund = await _context.CashierRefunds
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == refundId, cancellationToken);

        if (refund == null)
        {
            _logger.LogWarning("[CashierReservationService] Refund {RefundId} not found", refundId);
            return;
        }

        // Prevent duplicate Credit Notes
        var creditNoteExists = await _context.SalesCreditNotes.AnyAsync(cn => 
            cn.Description != null && cn.Description.Contains(refund.RefundNo), cancellationToken);

        if (creditNoteExists)
        {
            _logger.LogInformation("[CashierReservationService] Sales Credit Note already exists for refund {RefundNo}. Skipping.", refund.RefundNo);
            return;
        }

        var invoice = await _context.SaleInvoices
            .Include(si => si.SaleItems)
            .FirstOrDefaultAsync(si => si.Id == refund.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning("[CashierReservationService] Original invoice {InvoiceId} not found for refund {RefundId}", refund.InvoiceId, refundId);
            return;
        }

        var creditNoteNo = $"CN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var creditNote = new SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote.SalesCreditNote
        {
            Id = Guid.NewGuid(),
            AdjustmentReason = refund.Reason ?? "مرتجع كاشير",
            SaleInvoiceId = invoice.Id,
            Description = $"مرتجع كاشير تلقائي - رقم المرتجع: {refund.RefundNo}",
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            SalesCreditNoteNo = creditNoteNo,
            DiscountAmount = 0m,
            CustomerProfileId = invoice.CustomerProfileId,
            IsCleared = false,
            IsActive = true,
            CreatedBy = refund.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            SalesCreditNoteItems = new List<SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote.SalesCreditNoteItem>()
        };

        foreach (var item in refund.Items)
        {
            var saleItem = invoice.SaleItems.FirstOrDefault(si => si.Id == item.SaleItemId);
            if (saleItem == null) continue;

            creditNote.SalesCreditNoteItems.Add(new SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote.SalesCreditNoteItem
            {
                Id = Guid.NewGuid(),
                MainSalesClassId = saleItem.MainSalesClassId,
                NoOfNights = 1,
                Price = item.UnitPrice,
                Quantity = item.ReturnQty,
                IsActive = true,
                CreatedBy = refund.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.SalesCreditNotes.Add(creditNote);
        await _context.SaveChangesAsync(cancellationToken);

        // Create transaction entries / ledger journal postings
        var creditNoteService = _serviceBaseFactory.GetService<SalesCreditNoteService>();
        await creditNoteService.CreateTransaction(creditNote.Id);

        _logger.LogInformation("[CashierReservationService] Successfully created Sales Credit Note {CreditNoteNo} and transaction entries for refund {RefundNo}", creditNoteNo, refund.RefundNo);
    }

    public async Task ReleaseInvoiceAsync(int invoiceId, string reason, CancellationToken cancellationToken = default)
    {
        if (invoiceId <= 0)
        {
            return;
        }

        var reservations = await _context.CashierStockReservations
            .Where(x => x.InvoiceId == invoiceId && x.Status == CashierReservationStatus.Reserved)
            .ToListAsync(cancellationToken);

        if (reservations.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var reservation in reservations)
        {
            reservation.Status = CashierReservationStatus.Released;
            reservation.ReleasedAtUtc = now;
            reservation.ReleaseReason = string.IsNullOrWhiteSpace(reason) ? "Released" : reason.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// مطابقة تصنيف المبيعات بتصنيف المشتريات عبر الكود المرجعي أو الاسم العربي أو الاسم الانجليزي
    /// (Same fuzzy matching logic used in CashierInvoiceController.GetAvailableQuantityBySalesClassAsync)
    /// </summary>
    private static Guid? TryMapSalesClassToPurchaseClass(
        MainSalesClass salesClass,
        IReadOnlyDictionary<string, MainPurchaseClass> purchaseByRefCode,
        IReadOnlyDictionary<string, MainPurchaseClass> purchaseByArabicName,
        IReadOnlyDictionary<string, MainPurchaseClass> purchaseByEnglishName)
    {
        var normalizedRef = NormalizeLookup(salesClass.ReferenceCode);
        if (normalizedRef.Length > 0 && purchaseByRefCode.TryGetValue(normalizedRef, out var matchByRef))
            return matchByRef.Id;

        var normalizedAr = NormalizeLookup(salesClass.ArabicName);
        if (normalizedAr.Length > 0 && purchaseByArabicName.TryGetValue(normalizedAr, out var matchByAr))
            return matchByAr.Id;

        var normalizedEn = NormalizeLookup(salesClass.EnglishName);
        if (normalizedEn.Length > 0 && purchaseByEnglishName.TryGetValue(normalizedEn, out var matchByEn))
            return matchByEn.Id;

        return null;
    }

    private static string NormalizeLookup(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }
}
