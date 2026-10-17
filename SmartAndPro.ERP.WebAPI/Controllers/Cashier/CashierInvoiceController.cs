using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Controllers.Cashier.Dtos;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Recipes;
using Bee.ZatcaHelper;

using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.Application.Common.Dtos.Notifications;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier
{
    [ApiController]
    [Route("api/cashier")]
    [Authorize]
    public class CashierInvoiceController : ControllerBase
    {
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ICashierRuntimeStore _cashierStore;
        private readonly ICashierReservationService _reservationService;
        private readonly ICashierAuditService _auditService;
        private readonly ICashierInvoiceControlService _invoiceControl;
        private readonly ICashierStockLockService _stockLockService;
        private readonly ILogger<CashierInvoiceController> _logger;
        private readonly ERPContext _context;
        private readonly INotificationService _notificationService;

        public CashierInvoiceController(
            ISalesInvoicesService salesInvoicesService,
            RepositoriesFactory repositoriesFactory,
            ICashierRuntimeStore cashierStore,
            ICashierReservationService reservationService,
            ICashierAuditService auditService,
            ICashierInvoiceControlService invoiceControl,
            ICashierStockLockService stockLockService,
            ILogger<CashierInvoiceController> logger,
            ERPContext context,
            INotificationService notificationService)
        {
            _salesInvoicesService = salesInvoicesService;
            _repositoriesFactory = repositoriesFactory;
            _cashierStore = cashierStore;
            _reservationService = reservationService;
            _auditService = auditService;
            _invoiceControl = invoiceControl;
            _stockLockService = stockLockService;
            _logger = logger;
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Create POS Invoice - إنشاء فاتورة نقطة بيع
        /// POST /api/cashier/invoice
        /// </summary>
        [HttpPost("invoice")]
        public async Task<ActionResult<POSInvoiceResponse>> CreateInvoice([FromBody] POSInvoiceRequest request)
        {
            try
            {
                if (!HasCashierAccess())
                {
                    return StatusCode(403, new { message = "Cashier or Admin role is required" });
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validate cashier session exists (from middleware)
                var cashierSessionId = HttpContext.Items["CashierSessionId"] as int?;
                if (!cashierSessionId.HasValue)
                {
                    return StatusCode(403, new
                    {
                        errorCode = "NO_ACTIVE_SESSION",
                        message = "No active cashier session",
                        messageAr = "لا توجد جلسة كاشير نشطة"
                    });
                }

                // Validate store exists (from middleware or request)
                var storeId = request.StoreDetailsId.HasValue && request.StoreDetailsId.Value != Guid.Empty
                    ? request.StoreDetailsId
                    : HttpContext.Items["CashierStoreId"] as Guid?;
                if (!storeId.HasValue)
                {
                    return StatusCode(403, new
                    {
                        errorCode = "SESSION_NO_STORE",
                        message = "Cashier session has no store assigned",
                        messageAr = "جلسة الكاشير ليس لها متجر محدد"
                    });
                }

                // Map POSInvoiceRequest to SaleInvoice PostDto
                var saleInvoiceDto = await MapToSaleInvoicePostDtoAsync(request, storeId.Value);

                // Create invoice using sales invoice service.
                // Avoid wrapping this flow in an outer Db transaction because inner services
                // already manage their own transactions and SQL Server does not allow this nesting pattern.
                var createdInvoice = await _salesInvoicesService.AddAsync(saleInvoiceDto);
                
                _logger.LogInformation("[CashierInvoice] Invoice created with Id={InvoiceId}, InvoiceNo={InvoiceNo}", 
                    createdInvoice.Id, createdInvoice.InvoiceNo);

                _logger.LogInformation(
                    "[CashierInvoice] ✅ Invoice created successfully - Id={InvoiceId}, InvoiceNo={InvoiceNo}, NetPrice={NetPrice}, CustomerId={CustomerId}, StoreId={StoreId}, ItemCount={ItemCount}",
                    createdInvoice.Id,
                    createdInvoice.InvoiceNo,
                    createdInvoice.NetPrice,
                    createdInvoice.CustomerProfileId,
                    storeId.Value,
                    saleInvoiceDto.SaleItems.Count);

                // Track invoice-session-store binding for secure payment flow
                _cashierStore.TrackInvoice(
                    createdInvoice.Id,
                    cashierSessionId,
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    decimal.Round(createdInvoice.NetPrice, 2),
                    createdInvoice.IssueDate,
                    storeId,
                    saleInvoiceDto.SaleItems
                        .GroupBy(x => x.MainSalesClassId)
                        .ToDictionary(group => group.Key, group => group.Sum(item => Math.Max(0m, item.Quantity))),
                    request.TableId);

                var reservedByClass = saleInvoiceDto.SaleItems
                    .GroupBy(x => x.MainSalesClassId)
                    .ToDictionary(group => group.Key, group => group.Sum(item => Math.Max(0m, item.Quantity)));

                await _reservationService.ReserveForInvoiceAsync(
                    createdInvoice.Id,
                    cashierSessionId,
                    storeId.Value,
                    ResolveCurrentUserGuid(),
                    reservedByClass,
                    request.TableId.HasValue ? "RESTAURANT" : "POS");

                // ✅ Update session statistics in database immediately
                try
                {
                    var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                    var dbSession = (await sessionRepo.Filter(s => s.SessionId == cashierSessionId.Value)).Data?.FirstOrDefault();
                    if (dbSession != null)
                    {
                        // Increment transaction count when invoice is created
                        dbSession.TransactionsCount += 1;
                        await sessionRepo.Update(dbSession);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update session statistics for session {SessionId}", cashierSessionId.Value);
                }

                await _auditService.LogAsync(
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    "INVOICE_CREATED",
                    "SaleInvoice",
                    createdInvoice.Id.ToString(),
                    cashierSessionId,
                    storeId,
                    new
                    {
                        createdInvoice.InvoiceNo,
                        request.TableId,
                        TotalAmount = createdInvoice.NetPrice,
                        ItemCount = saleInvoiceDto.SaleItems.Count
                    });

                // Map response
                var response = new POSInvoiceResponse
                {
                    Id = createdInvoice.Id,
                    InvoiceNo = createdInvoice.InvoiceNo,
                    InvoiceDate = createdInvoice.IssueDate,
                    TotalAmount = createdInvoice.NetPrice,
                    Status = "Completed",
                    Message = "Invoice created successfully",
                    MessageAr = "تم إنشاء الفاتورة بنجاح"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cashier invoice");
                return StatusCode(500, new
                {
                    message = "Error creating invoice",
                    messageAr = "خطأ في إنشاء الفاتورة",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Combined Checkout: create invoice + process payment in one request.
        /// Eliminates the race condition between separate invoice/payment calls.
        /// POST /api/cashier/checkout
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] POSInvoiceRequest request)
        {
            try
            {
                if (!HasCashierAccess())
                    return StatusCode(403, new { message = "Cashier or Admin role is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var cashierSessionId = HttpContext.Items["CashierSessionId"] as int?;
                if (!cashierSessionId.HasValue)
                {
                    return StatusCode(403, new
                    {
                        errorCode = "NO_ACTIVE_SESSION",
                        message = "No active cashier session",
                        messageAr = "لا توجد جلسة كاشير نشطة"
                    });
                }

                var storeId = request.StoreDetailsId.HasValue && request.StoreDetailsId.Value != Guid.Empty
                    ? request.StoreDetailsId
                    : HttpContext.Items["CashierStoreId"] as Guid?;
                if (!storeId.HasValue)
                {
                    return StatusCode(403, new
                    {
                        errorCode = "SESSION_NO_STORE",
                        message = "Cashier session has no store assigned",
                        messageAr = "جلسة الكاشير ليس لها متجر محدد"
                    });
                }

                // ──── Idempotency Check ────
                // If the client sends Idempotency-Key header, return the cached result
                // for any duplicate request rather than creating a second invoice.
                var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
                string currentHash = "";
                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    var json = JsonSerializer.Serialize(request);
                    using (var sha = System.Security.Cryptography.SHA256.Create())
                    {
                        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                        var hashBytes = sha.ComputeHash(bytes);
                        currentHash = Convert.ToHexString(hashBytes);
                    }

                    var idRepo = _repositoriesFactory.GetRepository<CheckoutIdempotencyRecord>();
                    var existing = (await idRepo.Filter(x =>
                        x.IdempotencyKey == idempotencyKey &&
                        x.ExpiresAtUtc > DateTime.UtcNow)).Data?.FirstOrDefault();

                    if (existing != null)
                    {
                        if (!string.Equals(existing.RequestHash, currentHash, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning(
                                "[Checkout] Idempotency conflict for key={Key} — incoming request hash {IncomingHash} does not match stored hash {StoredHash}",
                                idempotencyKey, currentHash, existing.RequestHash);
                            return Conflict(new
                            {
                                success = false,
                                message = "Conflict: A different request payload was already processed with the same Idempotency-Key.",
                                messageAr = "تعارض: تم إرسال طلب مختلف بنفس مفتاح عدم التكرار."
                            });
                        }

                        _logger.LogInformation(
                            "[Checkout] Idempotency hit for key={Key} — returning cached invoice {InvoiceId}",
                            idempotencyKey, existing.InvoiceId);

                        var cached = JsonSerializer.Deserialize<CheckoutResponse>(existing.ResponseJson);
                        return Ok(cached);
                    }
                }

                // ──── Step 1: Create Invoice (under inventory row lock) ────
                var saleInvoiceDto = await MapToSaleInvoicePostDtoAsync(request, storeId.Value);

                // Acquire per-store exclusive lock to prevent concurrent sales depleting the same stock
                var itemGuids = new List<Guid>();
                // Use store-level lock as safe fallback
                IDisposable? stockLock = null;
                try
                {
                    stockLock = await _stockLockService.AcquireAsync(storeId.Value, new[] { storeId.Value });
                }
                catch (TimeoutException tex)
                {
                    _logger.LogWarning(tex, "[Checkout] Stock lock timeout for store {StoreId}", storeId.Value);
                    return StatusCode(409, new
                    {
                        errorCode = "STOCK_LOCK_TIMEOUT",
                        message = "Another transaction is processing inventory for this store. Please retry.",
                        messageAr = "عملية أخرى تستخدم المخزون في هذا المتجر، يرجى المحاولة مجدداً."
                    });
                }

                try
                {
                var createdInvoice = await _salesInvoicesService.AddAsync(saleInvoiceDto);

                _logger.LogInformation(
                    "[Checkout] ✅ Invoice created - Id={InvoiceId}, InvoiceNo={InvoiceNo}, NetPrice={NetPrice}",
                    createdInvoice.Id, createdInvoice.InvoiceNo, createdInvoice.NetPrice);

                // Track invoice-session-store binding
                _cashierStore.TrackInvoice(
                    createdInvoice.Id,
                    cashierSessionId,
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    decimal.Round(createdInvoice.NetPrice, 2),
                    createdInvoice.IssueDate,
                    storeId,
                    saleInvoiceDto.SaleItems
                        .GroupBy(x => x.MainSalesClassId)
                        .ToDictionary(group => group.Key, group => group.Sum(item => Math.Max(0m, item.Quantity))),
                    request.TableId);

                var reservedByClass = saleInvoiceDto.SaleItems
                    .GroupBy(x => x.MainSalesClassId)
                    .ToDictionary(group => group.Key, group => group.Sum(item => Math.Max(0m, item.Quantity)));

                await _reservationService.ReserveForInvoiceAsync(
                    createdInvoice.Id,
                    cashierSessionId,
                    storeId.Value,
                    ResolveCurrentUserGuid(),
                    reservedByClass,
                    request.TableId.HasValue ? "RESTAURANT" : "POS");

                // Update session transaction count
                try
                {
                    var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                    var dbSession = (await sessionRepo.Filter(s => s.SessionId == cashierSessionId.Value)).Data?.FirstOrDefault();
                    if (dbSession != null)
                    {
                        dbSession.TransactionsCount += 1;
                        await sessionRepo.Update(dbSession);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Checkout] Failed to update session transaction count for session {SessionId}", cashierSessionId.Value);
                }

                // ──── Step 2: Process Payment (inline, no DB lookup needed) ────
                var accountingTransactionCreated = false;
                try
                {
                    var transactionResult = await _salesInvoicesService.CreateTransaction(createdInvoice.Id, createdInvoice.IsFrontal ?? false);
                    accountingTransactionCreated = transactionResult == 1;
                    if (!accountingTransactionCreated)
                    {
                        _logger.LogWarning("[Checkout] Accounting transaction not created for invoice {InvoiceId}; continuing", createdInvoice.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Checkout] Accounting transaction creation failed for invoice {InvoiceId}; continuing", createdInvoice.Id);
                }

                _cashierStore.MarkInvoicePaid(createdInvoice.Id);
                await _reservationService.ConfirmInvoiceAsync(createdInvoice.Id);

                // Trigger System Notification if stock falls below alert limit
                try
                {
                    if (cashierSessionId.HasValue)
                    {
                        var dbSessionObj = await _context.CashierSessions.AsNoTracking().FirstOrDefaultAsync(s => s.SessionId == cashierSessionId.Value);
                        int alertLimit = dbSessionObj?.StockAlertLimit ?? 10;
                        _logger.LogInformation("[Checkout Alert] Session alert limit is: {Limit}", alertLimit);

                        var purchaseClassIds = new List<Guid>();
                        foreach (var item in request.Items)
                        {
                            var pId = await ResolveMainClassIdAsync(item);
                            if (pId.HasValue && !purchaseClassIds.Contains(pId.Value))
                            {
                                purchaseClassIds.Add(pId.Value);
                            }
                        }

                        if (purchaseClassIds.Count > 0)
                        {
                            var purchaseClasses = await _context.MainPurchaseClasses.AsNoTracking().Where(x => purchaseClassIds.Contains(x.Id)).ToListAsync();
                            var availMap = await GetAvailableQuantityByPurchaseClassAsync(purchaseClasses, storeId);
                            foreach (var pc in purchaseClasses)
                            {
                                availMap.TryGetValue(pc.Id, out var remainingQty);
                                _logger.LogInformation("[Checkout Alert] Product: {Name}, Remaining stock: {Remaining}, Alert limit: {Limit}", pc.ArabicName ?? pc.EnglishName, remainingQty, alertLimit);

                                if (remainingQty <= alertLimit)
                                {
                                    var pName = pc.ArabicName ?? pc.EnglishName ?? "المنتج";
                                    var notificationDto = new NotificationPostDto
                                    {
                                        Title = "تنبيه: مخزون منخفض جداً",
                                        Content = $"لقد وصل مخزون المنتج \"{pName}\" في المخزن إلى {remainingQty:0.##} (أقل من أو يساوي الحد {alertLimit})",
                                        Link = "/cashier/session"
                                    };
                                    var userIds = new List<Guid> { ResolveCurrentUserGuid() };
                                    var permissions = new List<string> { "Cashier.Session.SetAlertLimit" };
                                    await _notificationService.SendInSystemNotificationAsync(notificationDto, userIds, permissions);
                                    _logger.LogInformation("[Checkout Alert] Sent low stock system notification for product: {Name}", pName);
                                }
                            }
                        }
                    }
                }
                catch (Exception alertEx)
                {
                    _logger.LogWarning(alertEx, "[Checkout] Failed to process low stock system notification");
                }

                await _auditService.LogAsync(
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    "CHECKOUT_COMPLETED",
                    "SaleInvoice",
                    createdInvoice.Id.ToString(),
                    cashierSessionId,
                    storeId,
                    new
                    {
                        request.PaymentMethod,
                        TotalAmount = createdInvoice.NetPrice,
                        request.PaidAmount,
                        request.ChangeAmount
                    });

                // Persist CashierPayment record
                decimal cashAmt = request.CashAmount ?? (request.PaymentMethod == 0 ? request.PaidAmount : 0);
                decimal cardAmt = request.CardAmount ?? (request.PaymentMethod == 1 ? createdInvoice.NetPrice : 0);
                decimal creditAmt = request.PaymentMethod == 2 ? createdInvoice.NetPrice : 0;

                try
                {
                    var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                    var dbPayment = new SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment
                    {
                        InvoiceId = createdInvoice.Id,
                        SessionId = cashierSessionId ?? 0,
                        PaymentMethod = request.PaymentMethod,
                        TotalAmount = createdInvoice.NetPrice,
                        PaidAmount = request.PaidAmount,
                        ChangeAmount = request.ChangeAmount,
                        CashAmount = cashAmt,
                        CardAmount = cardAmt,
                        CreditAmount = creditAmt,
                        PaidAtUtc = DateTime.UtcNow,
                        StoreDetailsId = storeId,
                        PaymentStatus = 1, // Paid
                        StatusChangedAtUtc = DateTime.UtcNow,
                        IsActive = true,
                        CreatedBy = ResolveCurrentUserGuid(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await paymentRepo.Add(dbPayment);

                    // Update session financial stats
                    if (cashierSessionId.HasValue)
                    {
                        var sessionRepo2 = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                        var dbSession2 = (await sessionRepo2.Filter(s => s.SessionId == cashierSessionId.Value)).Data?.FirstOrDefault();
                        if (dbSession2 != null)
                        {
                            dbSession2.TotalSales += createdInvoice.NetPrice;
                            dbSession2.TotalCash += cashAmt;
                            dbSession2.TotalCard += cardAmt;
                            dbSession2.TotalCredit += creditAmt;
                            await sessionRepo2.Update(dbSession2);
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "[Checkout] Failed to persist CashierPayment to DB for invoice {InvoiceId}", createdInvoice.Id);
                }

                _logger.LogInformation("[Checkout] ✅ Checkout completed - InvoiceId={InvoiceId}, InvoiceNo={InvoiceNo}", createdInvoice.Id, createdInvoice.InvoiceNo);

                string? zatcaQrCode = null;
                try
                {
                    var companyRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails>();
                    var companyDetails = (await companyRepo.GetAll()).Data?.FirstOrDefault();
                    if (companyDetails != null)
                    {
                        var sellerName = companyDetails.ArabicName ?? companyDetails.EnglishName ?? "Smart And Pro";
                        var vatNo = companyDetails.TaxNumber ?? "123456789012345";
                        var totalAmt = createdInvoice.NetPrice;
                        var vatAmt = createdInvoice.TaxAmount;
                        var issueDateFormatted = createdInvoice.IssueDate.ToString("yyyy-MM-dd'T'HH:mm:ss");

                        zatcaQrCode = Bee.ZatcaHelper.StandardInvoiceXmlGenerator.GenerateZatcaQrCode(
                            sellerName,
                            vatNo,
                            totalAmt,
                            vatAmt,
                            issueDateFormatted
                        );
                    }
                }
                catch (Exception qrEx)
                {
                    _logger.LogWarning(qrEx, "[Checkout] Failed to generate ZATCA QR Code for invoice {InvoiceId}", createdInvoice.Id);
                }

                var checkoutResponse = new CheckoutResponse
                {
                    Success = true,
                    InvoiceId = createdInvoice.Id,
                    InvoiceNo = createdInvoice.InvoiceNo,
                    InvoiceDate = createdInvoice.IssueDate,
                    TotalAmount = createdInvoice.NetPrice,
                    PaidAmount = request.PaidAmount,
                    ChangeAmount = request.ChangeAmount,
                    ZatcaQrCode = zatcaQrCode,
                    Message = accountingTransactionCreated
                        ? "Checkout completed successfully"
                        : "Checkout completed (accounting transaction pending)",
                    MessageAr = accountingTransactionCreated
                        ? "تمت عملية البيع بنجاح"
                        : "تمت عملية البيع بنجاح (معاملة المحاسبة معلقة)"
                };

                // ──── Store Idempotency Record ────
                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    try
                    {
                        var idRepo2 = _repositoriesFactory.GetRepository<CheckoutIdempotencyRecord>();
                        await idRepo2.Add(new CheckoutIdempotencyRecord
                        {
                            IdempotencyKey = idempotencyKey,
                            InvoiceId = createdInvoice.Id,
                            InvoiceNo = createdInvoice.InvoiceNo,
                            TotalAmount = createdInvoice.NetPrice,
                            ResponseJson = JsonSerializer.Serialize(checkoutResponse),
                            RequestHash = currentHash,
                            CreatedAtUtc = DateTime.UtcNow,
                            ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
                            RequestedByUserGuid = ResolveCurrentUserGuid(),
                            SessionId = cashierSessionId ?? 0,
                            IsActive = true,
                            CreatedBy = ResolveCurrentUserGuid(),
                            CreatedAt = DateTime.UtcNow
                        });
                        _logger.LogInformation("[Checkout] Idempotency record stored for key={Key}", idempotencyKey);
                    }
                    catch (Exception idEx)
                    {
                        _logger.LogWarning(idEx, "[Checkout] Failed to store idempotency record for key={Key}", idempotencyKey);
                    }
                }

                // ──── Record Inventory Movements (ledger) ────
                try
                {
                    var movRepo = _repositoriesFactory.GetRepository<CashierInventoryMovement>();
                    var recipeRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Recipes.Recipe>();

                    foreach (var saleItem in saleInvoiceDto.SaleItems)
                    {
                        var recipe = (await recipeRepo.Filter(r => r.MainSalesClassId == saleItem.MainSalesClassId)).Data
                            ?.FirstOrDefault();

                        if (recipe != null && recipe.RecipeEntries != null && recipe.RecipeEntries.Any())
                        {
                            foreach (var entry in recipe.RecipeEntries)
                            {
                                var numPersons = recipe.NumberOfPersons > 0 ? recipe.NumberOfPersons : 1;
                                var deductedQty = saleItem.Quantity * (entry.Quantity / numPersons);
                                var ingName = entry.MainPurchaseClass?.ArabicName ?? entry.MainPurchaseClass?.EnglishName ?? $"Ingredient {entry.MainPurchaseClassId}";

                                await movRepo.Add(new CashierInventoryMovement
                                {
                                    InvoiceId = createdInvoice.Id,
                                    InvoiceNo = createdInvoice.InvoiceNo,
                                    SessionId = cashierSessionId ?? 0,
                                    StoreDetailsId = storeId.Value,
                                    MainPurchaseClassId = entry.MainPurchaseClassId,
                                    ProductName = ingName,
                                    Direction = 1,    // 1 = Exit (sale)
                                    Quantity = deductedQty,
                                    UnitCost = 0m,
                                    UnitPrice = 0m,
                                    InventoryPermitId = null,
                                    CashierUserGuid = ResolveCurrentUserGuid(),
                                    MovementType = "SALE",
                                    MovedAtUtc = DateTime.UtcNow,
                                    IsActive = true,
                                    CreatedBy = ResolveCurrentUserGuid(),
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                        else
                        {
                            var purchaseClassId = await MapSalesToPurchaseClassAsync(saleItem.MainSalesClassId);
                            await movRepo.Add(new CashierInventoryMovement
                            {
                                InvoiceId = createdInvoice.Id,
                                InvoiceNo = createdInvoice.InvoiceNo,
                                SessionId = cashierSessionId ?? 0,
                                StoreDetailsId = storeId.Value,
                                MainPurchaseClassId = purchaseClassId,
                                ProductName = saleItem.ItemNo ?? string.Empty,
                                Direction = 1,    // 1 = Exit (sale)
                                Quantity = saleItem.Quantity,
                                UnitCost = 0m,
                                UnitPrice = saleItem.Price,
                                InventoryPermitId = null,
                                CashierUserGuid = ResolveCurrentUserGuid(),
                                MovementType = "SALE",
                                MovedAtUtc = DateTime.UtcNow,
                                IsActive = true,
                                CreatedBy = ResolveCurrentUserGuid(),
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
                catch (Exception movEx)
                {
                    _logger.LogWarning(movEx, "[Checkout] Failed to record inventory movements for invoice {InvoiceId}", createdInvoice.Id);
                }

                return Ok(checkoutResponse);
                }
                finally
                {
                    stockLock?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout] Error during checkout");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error during checkout",
                    messageAr = "خطأ أثناء عملية البيع",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// GET /api/cashier/invoice/{id}
        /// </summary>
        [HttpGet("invoice/details/{id}")]
        public async Task<ActionResult<SaleInvoice>> GetInvoice(int id)
        {
            try
            {
                var invoice = await _salesInvoicesService.GetByIdAsync(id);
                if (invoice == null)
                    return NotFound(new { message = "Invoice not found", messageAr = "الفاتورة غير موجودة" });

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
                return StatusCode(500, new { message = "Error retrieving invoice", messageAr = "خطأ في استرجاع الفاتورة" });
            }
        }

        #region Private Methods

        private async Task<SaleInvoicePostDto> MapToSaleInvoicePostDtoAsync(POSInvoiceRequest request, Guid storeId)
        {
            // Calculate totals
            decimal subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            decimal discountAmount = request.DiscountPercentage > 0
                ? subtotal * (request.DiscountPercentage / 100)
                : request.DiscountAmount;
            decimal afterDiscount = subtotal - discountAmount;
            decimal taxAmount = request.TaxPercentage > 0
                ? afterDiscount * (request.TaxPercentage / 100)
                : request.TaxAmount;
            decimal totalAmount = afterDiscount + taxAmount;

            // Map items with MainPurchaseClassId then MainSalesClassId resolution
            var saleItems = new List<SaleItemPostDto>();
            var requestedByPurchaseClass = new Dictionary<Guid, decimal>();
            var requestedNameByClass = new Dictionary<Guid, string>();
            var purchaseToSalesMap = new Dictionary<Guid, Guid>();
            foreach (var item in request.Items)
            {
                // First resolve to MainPurchaseClassId (what's actually in inventory)
                var mainPurchaseClassId = await ResolveMainClassIdAsync(item);
                if (!mainPurchaseClassId.HasValue)
                {
                    throw new InvalidOperationException($"Unable to resolve purchase class for item '{item.ItemName ?? item.ItemCode ?? item.ItemId.ToString()}'");
                }
                
                // Then map to MainSalesClassId for the invoice
                var mainSalesClassId = await MapPurchaseToSalesClassAsync(mainPurchaseClassId.Value);
                if (!mainSalesClassId.HasValue)
                {
                    throw new InvalidOperationException($"Unable to map to sales class for item '{item.ItemName ?? item.ItemCode ?? item.ItemId.ToString()}'");
                }

                saleItems.Add(new SaleItemPostDto
                {
                    MainSalesClassId = mainSalesClassId.Value,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice,
                    Tax = item.TaxPercentage,
                    TaxAmount = item.TaxAmount,
                    Discount = item.DiscountPercentage,
                    DiscountAmount = item.DiscountAmount,
                    TotalPrice = item.Quantity * item.UnitPrice,
                    NetPrice = item.TotalAmount,
                    ItemNo = item.ItemCode,
                    NoOfNights = 1
                });

                purchaseToSalesMap[mainPurchaseClassId.Value] = mainSalesClassId.Value;

                var requestedQuantity = item.Quantity <= 0 ? 0m : item.Quantity;
                AddQuantity(requestedByPurchaseClass, mainPurchaseClassId.Value, requestedQuantity);
                if (!requestedNameByClass.ContainsKey(mainPurchaseClassId.Value))
                {
                    requestedNameByClass[mainPurchaseClassId.Value] = item.ItemName ?? item.ItemCode ?? item.ItemId.ToString();
                }
            }

            var stockValidationMessage = await ValidateStoreStockAsync(storeId, requestedByPurchaseClass, requestedNameByClass);
            if (!string.IsNullOrWhiteSpace(stockValidationMessage))
            {
                throw new InvalidOperationException(stockValidationMessage);
            }

            Guid? resolvedCustomerId = null;
            if (!string.IsNullOrWhiteSpace(request.CustomerId) && Guid.TryParse(request.CustomerId, out var parsedGuid))
            {
                resolvedCustomerId = parsedGuid;
            }

            return new SaleInvoicePostDto
            {
                InvoiceNo = null, // Will be auto-generated
                Currency = "SAR",
                CustomerProfileId = resolvedCustomerId,
                CustomerProfileName = request.CustomerName ?? "Walk-in Customer",
                CustomerPhone = request.CustomerPhone,
                CustomerTaxNumber = request.CustomerTaxNumber,
                Description = request.Notes ?? $"POS Invoice - Session {request.CashierSessionId}",
                IssueDate = request.InvoiceDate,
                DueDate = request.PaymentMethod == 2 // Credit
                    ? request.InvoiceDate.AddDays(request.CreditDays ?? 30)
                    : request.InvoiceDate,
                StoreDetailsId = storeId,
                Tax = request.TaxPercentage,
                TaxAmount = taxAmount,
                Discount = request.DiscountPercentage,
                DiscountAmount = discountAmount,
                TotalPrice = subtotal,
                NetPrice = totalAmount,
                IsQuotation = false,
                IsFrontal = false,
                SaleInvoiceType = Domains.Enums.SaleInvoiceType.SaleInvoice,
                SaleItems = saleItems
            };
        }

        private async Task<Guid?> ResolveMainClassIdAsync(POSInvoiceItemRequest item)
        {
            // Try ItemCode as Guid first (MainPurchaseClassId)
            if (!string.IsNullOrWhiteSpace(item.ItemCode) && Guid.TryParse(item.ItemCode, out var parsedGuid))
            {
                return parsedGuid;
            }

            // Try resolving from ItemId via CashierStore (returns MainPurchaseClassId)
            if (item.ItemId > 0)
            {
                var mapped = _cashierStore.TryResolveProductGuid(item.ItemId);
                if (mapped.HasValue) return mapped.Value;
            }

            // Try finding by ItemName in PurchaseClass
            if (!string.IsNullOrWhiteSpace(item.ItemName))
            {
                var fromName = await FindMainPurchaseClassByNameAsync(item.ItemName);
                if (fromName.HasValue) return fromName.Value;
            }

            return null;
        }
        
        private async Task<Guid?> MapPurchaseToSalesClassAsync(Guid purchaseClassId)
        {
            var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var purchaseClass = (await purchaseClassRepo.Filter(x => x.Id == purchaseClassId))
                .Data?
                .AsNoTracking() // Don't track to avoid conflicts when creating SalesClass
                .FirstOrDefault();
            if (purchaseClass == null) return null;
            
            var salesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
            
            // Get all sales classes with query filters ignored (including inactive ones)
            var allSalesClasses = (await salesClassRepo.GetAll()).Data?
                .IgnoreQueryFilters()
                .ToList();
                
            if (allSalesClasses == null || allSalesClasses.Count == 0)
            {
                _logger.LogWarning("[CashierInvoice] No sales classes found in database");
            }
            else
            {
                _logger.LogInformation("[CashierInvoice] Loaded {Count} sales classes (including inactive)", allSalesClasses.Count);
            }
            
            // Try to find existing sales class (prefer active ones)
            MainSalesClass? salesClass = null;
            MainSalesClass? inactiveSalesClass = null;
            
            // Search by ReferenceCode first (most reliable)
            if (!string.IsNullOrWhiteSpace(purchaseClass.ReferenceCode))
            {
                var trimmedRefCode = purchaseClass.ReferenceCode.Trim();
                salesClass = allSalesClasses?.FirstOrDefault(x => 
                    x.IsActive &&
                    !string.IsNullOrWhiteSpace(x.ReferenceCode) && 
                    x.ReferenceCode.Trim().Equals(trimmedRefCode, StringComparison.OrdinalIgnoreCase));
                    
                // If not found active, check inactive
                if (salesClass == null)
                {
                    inactiveSalesClass = allSalesClasses?.FirstOrDefault(x => 
                        !x.IsActive &&
                        !string.IsNullOrWhiteSpace(x.ReferenceCode) && 
                        x.ReferenceCode.Trim().Equals(trimmedRefCode, StringComparison.OrdinalIgnoreCase));
                }
            }
            
            // Search by ArabicName if not found
            if (salesClass == null && !string.IsNullOrWhiteSpace(purchaseClass.ArabicName))
            {
                var trimmedArabicName = purchaseClass.ArabicName.Trim();
                salesClass = allSalesClasses?.FirstOrDefault(x => 
                    x.IsActive &&
                    !string.IsNullOrWhiteSpace(x.ArabicName) && 
                    x.ArabicName.Trim().Equals(trimmedArabicName, StringComparison.OrdinalIgnoreCase));
                    
                // If not found active, check inactive
                if (salesClass == null)
                {
                    inactiveSalesClass = allSalesClasses?.FirstOrDefault(x => 
                        !x.IsActive &&
                        !string.IsNullOrWhiteSpace(x.ArabicName) && 
                        x.ArabicName.Trim().Equals(trimmedArabicName, StringComparison.OrdinalIgnoreCase));
                }
                    
                _logger.LogInformation("[CashierInvoice] Searched for SalesClass with ArabicName='{Name}', Found Active: {FoundActive}, Found Inactive: {FoundInactive}", 
                    trimmedArabicName, salesClass != null, inactiveSalesClass != null);
            }
            
            // Search by EnglishName if still not found
            if (salesClass == null && inactiveSalesClass == null && !string.IsNullOrWhiteSpace(purchaseClass.EnglishName))
            {
                var trimmedEnglishName = purchaseClass.EnglishName.Trim();
                salesClass = allSalesClasses?.FirstOrDefault(x => 
                    x.IsActive &&
                    !string.IsNullOrWhiteSpace(x.EnglishName) && 
                    x.EnglishName.Trim().Equals(trimmedEnglishName, StringComparison.OrdinalIgnoreCase));
                    
                if (salesClass == null)
                {
                    inactiveSalesClass = allSalesClasses?.FirstOrDefault(x => 
                        !x.IsActive &&
                        !string.IsNullOrWhiteSpace(x.EnglishName) && 
                        x.EnglishName.Trim().Equals(trimmedEnglishName, StringComparison.OrdinalIgnoreCase));
                }
            }
            
            // If found active sales class, return it
            if (salesClass != null)
            {
                _logger.LogInformation("[CashierInvoice] Found existing active SalesClass {Id} for PurchaseClass {Name}", 
                    salesClass.Id, purchaseClass.ArabicName ?? purchaseClass.EnglishName);
                return salesClass.Id;
            }
            
            // If found inactive sales class, activate it and return
            if (inactiveSalesClass != null)
            {
                _logger.LogInformation("[CashierInvoice] Found inactive SalesClass {Id}, activating it for PurchaseClass {Name}",
                    inactiveSalesClass.Id, purchaseClass.ArabicName ?? purchaseClass.EnglishName);
                inactiveSalesClass.IsActive = true;
                var updateResult = await salesClassRepo.Update(inactiveSalesClass);
                if (updateResult.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.Success)
                {
                    return inactiveSalesClass.Id;
                }
                _logger.LogWarning("[CashierInvoice] Failed to activate inactive SalesClass {Id}", inactiveSalesClass.Id);
            }
            
            // Auto-create SalesClass if not found
            _logger.LogInformation("[CashierInvoice] Auto-creating SalesClass for PurchaseClass: {Name}", 
                purchaseClass.ArabicName ?? purchaseClass.EnglishName);
            
            // Find a DetailSalesClass to attach the new MainSalesClass to (required FK)
            var detailSalesClassRepo = _repositoriesFactory.GetRepository<DetailSalesClass>();
            var detailSalesClass = (await detailSalesClassRepo.GetAll()).Data?
                .IgnoreQueryFilters()
                .Where(x => x.IsActive)
                .FirstOrDefault();
            
            if (detailSalesClass == null)
            {
                _logger.LogError("[CashierInvoice] No DetailSalesClass found in database. Cannot auto-create MainSalesClass for '{Name}'", 
                    purchaseClass.ArabicName ?? purchaseClass.EnglishName);
                return null;
            }
            
            var newSalesClass = new MainSalesClass
            {
                Id = Guid.NewGuid(),
                ReferenceCode = purchaseClass.ReferenceCode,
                ArabicName = purchaseClass.ArabicName,
                EnglishName = purchaseClass.EnglishName,
                MeasurementUnitId = purchaseClass.MeasurementUnitId,
                DetailSalesClassId = detailSalesClass.Id,
                ClassType = ClassType.Direct,
                // Don't include MeasurementUnit navigation property to avoid duplicate insert
                IsTaxExempted = false, // Default to taxable
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var createResult = await salesClassRepo.Add(newSalesClass);
            if (createResult.ErrorCode == SmartAndPro.ERP.SharedKernel.Enums.ErrorCodes.Success && createResult.Data != null)
            {
                _logger.LogInformation("[CashierInvoice] Successfully created SalesClass {Id} for '{Name}'", 
                    newSalesClass.Id, newSalesClass.ArabicName ?? newSalesClass.EnglishName);
                return newSalesClass.Id;
            }
            
            // If creation failed, try reading again (might have been created by another request OR might be inactive)
            _logger.LogWarning("[CashierInvoice] Failed to create SalesClass, trying to read existing one. Error: {ErrorCode}", 
                createResult.ErrorCode);
            
            // Retry search after failed insert - search WITHOUT IsActive filter
            if (!string.IsNullOrWhiteSpace(purchaseClass.ArabicName))
            {
                var trimmedArabicName = purchaseClass.ArabicName.Trim();
                var reloadedClasses = (await salesClassRepo.GetAll()).Data?
                    .IgnoreQueryFilters()
                    .Where(x => !string.IsNullOrWhiteSpace(x.ArabicName))
                    .ToList();
                    
                salesClass = reloadedClasses?.FirstOrDefault(x => 
                    x.ArabicName.Trim().Equals(trimmedArabicName, StringComparison.OrdinalIgnoreCase));
                    
                if (salesClass != null)
                {
                    // If found but inactive, activate it
                    if (!salesClass.IsActive)
                    {
                        _logger.LogInformation("[CashierInvoice] Found inactive SalesClass {Id}, activating it", salesClass.Id);
                        salesClass.IsActive = true;
                        await salesClassRepo.Update(salesClass);
                    }
                    
                    _logger.LogInformation("[CashierInvoice] Found existing SalesClass {Id} after failed insert", salesClass.Id);
                    return salesClass.Id;
                }
            }
            
            _logger.LogError("[CashierInvoice] Failed to create or find SalesClass for PurchaseClass {Id}", purchaseClassId);
            return null;
        }
        
        private async Task<Guid?> FindMainPurchaseClassByNameAsync(string name)
        {
            var normalized = name.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized)) return null;

            var existing = (await _repositoriesFactory.GetRepository<MainPurchaseClass>().GetAll()).Data?
                .IgnoreQueryFilters()
                .Where(x => x.IsActive)
                .FirstOrDefault(x =>
                    (!string.IsNullOrWhiteSpace(x.ArabicName) && x.ArabicName.ToLower() == normalized) ||
                    (!string.IsNullOrWhiteSpace(x.EnglishName) && x.EnglishName.ToLower() == normalized) ||
                    (!string.IsNullOrWhiteSpace(x.ReferenceCode) && x.ReferenceCode.ToLower() == normalized));

            return existing?.Id;
        }

        private async Task<Guid?> FindMainSalesClassByNameAsync(string name)
        {
            var normalized = name.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(normalized)) return null;

            var existing = (await _repositoriesFactory.GetRepository<MainSalesClass>().GetAll()).Data?
                .FirstOrDefault(x =>
                    (!string.IsNullOrWhiteSpace(x.ArabicName) && x.ArabicName.ToLower() == normalized) ||
                    (!string.IsNullOrWhiteSpace(x.EnglishName) && x.EnglishName.ToLower() == normalized) ||
                    (!string.IsNullOrWhiteSpace(x.ReferenceCode) && x.ReferenceCode.ToLower() == normalized));

            return existing?.Id;
        }

        private Guid ResolveCurrentUserGuid()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(ClaimTypes.Name)
                      ?? User.FindFirstValue("sub");

            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }

        private string ResolveCurrentUserName()
        {
            var raw = User.FindFirstValue(ClaimTypes.GivenName)
                      ?? User.FindFirstValue(ClaimTypes.Name)
                      ?? User.FindFirstValue(ClaimTypes.Email)
                      ?? User.Identity?.Name;

            return string.IsNullOrWhiteSpace(raw) ? "Cashier" : raw.Trim();
        }

        private async Task<string?> ValidateStoreStockAsync(
            Guid storeDetailsId,
            IReadOnlyDictionary<Guid, decimal> requestedByClass,
            IReadOnlyDictionary<Guid, string> requestedNameByClass)
        {
            if (requestedByClass.Count == 0)
            {
                return null;
            }

            var classIds = requestedByClass.Keys.ToList();
            var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var purchaseClasses = (await purchaseClassRepo.Filter(x => classIds.Contains(x.Id))).Data?.ToList() ?? new List<MainPurchaseClass>();

            var availableByClass = await GetAvailableQuantityByPurchaseClassAsync(purchaseClasses, storeDetailsId);
            var reservedByClass = await _reservationService.GetReservedQuantitiesByClassAsync(storeDetailsId, classIds);

            foreach (var requestedItem in requestedByClass)
            {
                var requestedQty = Math.Max(0m, requestedItem.Value);
                var availableQty = availableByClass.TryGetValue(requestedItem.Key, out var available) ? available : 0m;
                var reservedQty = reservedByClass.TryGetValue(requestedItem.Key, out var reserved) ? reserved : 0m;
                var netAvailableQty = Math.Max(0m, availableQty - reservedQty);
                if (netAvailableQty >= requestedQty)
                {
                    continue;
                }

                var itemName = requestedNameByClass.TryGetValue(requestedItem.Key, out var name) && !string.IsNullOrWhiteSpace(name)
                    ? name
                    : requestedItem.Key.ToString();

                return $"Insufficient stock for '{itemName}' in the selected store. Available: {decimal.Round(netAvailableQty, 2)}, requested: {decimal.Round(requestedQty, 2)}.";
            }

            return null;
        }

        private async Task<Dictionary<Guid, decimal>> GetAvailableQuantityByPurchaseClassAsync(
            IReadOnlyCollection<MainPurchaseClass> purchaseClasses,
            Guid? storeDetailsId)
        {
            var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var allPurchaseClassesQuery = (await purchaseClassRepo.GetAll()).Data;
            if (allPurchaseClassesQuery == null)
            {
                return new Dictionary<Guid, decimal>();
            }
            
            var allPurchaseClasses = allPurchaseClassesQuery
                .IgnoreQueryFilters()
                .Where(x => x.IsActive)
                .ToList();
            
            var classIds = allPurchaseClasses.Select(x => x.Id).ToHashSet();
            var quantityByClass = new Dictionary<Guid, decimal>();

            // حساب الكميات من PermitEntry مباشرة بناءً على MainPurchaseClassId
            var permitRepo = _repositoriesFactory.GetRepository<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();
            var allPermitsQuery = !storeDetailsId.HasValue 
                ? await permitRepo.GetAll()
                : await permitRepo.Filter(p => p.StoreDetailsId == storeDetailsId.Value);
            
            var allPermits = allPermitsQuery.Data?
                .IgnoreQueryFilters()
                .Where(p => p.IsActive)
                .Include(p => p.PermitEntries)
                .ToList() ?? new List<Domains.Entities.Inventory.PermitManagement.InventoryPermit>();

            // حساب الكميات: Entry بيزود، Exit بينقص
            foreach (var permit in allPermits)
            {
                if (permit.PermitEntries == null) continue;
                
                foreach (var entry in permit.PermitEntries.Where(e => e.IsActive))
                {
                    // استخدام MainPurchaseClassId مباشرة
                    if (entry.MainPurchaseClassId == Guid.Empty || !classIds.Contains(entry.MainPurchaseClassId))
                    {
                        continue;
                    }
                    
                    // إضافة أو خصم الكمية حسب نوع الإذن
                    var quantity = entry.Quantity;
                    if (permit.PermitType == Domains.Enums.PermitType.Entry)
                    {
                        AddQuantity(quantityByClass, entry.MainPurchaseClassId, quantity);
                    }
                    else if (permit.PermitType == Domains.Enums.PermitType.Exit)
                    {
                        AddQuantity(quantityByClass, entry.MainPurchaseClassId, -quantity);
                    }
                }
            }

            // ──── Apply CashierInventoryMovements Adjustment ────
            try
            {
                var movementsRepo = _repositoriesFactory.GetRepository<CashierInventoryMovement>();
                var allMovementsQuery = !storeDetailsId.HasValue 
                    ? await movementsRepo.GetAll()
                    : await movementsRepo.Filter(m => m.StoreDetailsId == storeDetailsId.Value);
                
                var allMovements = allMovementsQuery.Data?
                    .IgnoreQueryFilters()
                    .Where(m => m.IsActive)
                    .ToList() ?? new List<CashierInventoryMovement>();

                foreach (var mov in allMovements)
                {
                    if (mov.MainPurchaseClassId == Guid.Empty || !classIds.Contains(mov.MainPurchaseClassId))
                    {
                        continue;
                    }

                    if (mov.MovementType == "SALE" || mov.InvoiceId != 0 || mov.InventoryPermitId != null)
                    {
                        continue;
                    }

                    var quantity = mov.Quantity;
                    if (mov.Direction == -1 || mov.Direction == 0) // Entry (refund / stock in)
                    {
                        AddQuantity(quantityByClass, mov.MainPurchaseClassId, quantity);
                    }
                    else if (mov.Direction == 1) // Exit (sale)
                    {
                        AddQuantity(quantityByClass, mov.MainPurchaseClassId, -quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GetAvailableQuantityByPurchaseClassAsync] Failed to apply CashierInventoryMovements adjustment");
            }

            return quantityByClass;
        }

        private async Task<Dictionary<Guid, decimal>> GetAvailableQuantityBySalesClassAsync(
            IReadOnlyCollection<MainSalesClass> salesClasses,
            Guid? storeDetailsId)
        {
            if (salesClasses.Count == 0)
            {
                return new Dictionary<Guid, decimal>();
            }

            var classIds = salesClasses.Select(x => x.Id).ToHashSet();
            var classIdByReferenceCode = salesClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.ReferenceCode))
                .GroupBy(x => NormalizeLookup(x.ReferenceCode))
                .ToDictionary(group => group.Key, group => group.First().Id);
            var classIdByArabicName = salesClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.ArabicName))
                .GroupBy(x => NormalizeLookup(x.ArabicName))
                .ToDictionary(group => group.Key, group => group.First().Id);
            var classIdByEnglishName = salesClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.EnglishName))
                .GroupBy(x => NormalizeLookup(x.EnglishName))
                .ToDictionary(group => group.Key, group => group.First().Id);

            var productUnitRepo = _repositoriesFactory.GetRepository<ProductUnit>();
            var units = (await productUnitRepo.GetAll()).Data?
                .Where(x =>
                    x.Quantity.HasValue &&
                    (!storeDetailsId.HasValue || x.StoreDetailsId == storeDetailsId.Value) &&
                    (x.MainSalesClassId.HasValue || x.MainPurchaseClassId.HasValue))
                .ToList() ?? new List<ProductUnit>();

            var quantityByClass = new Dictionary<Guid, decimal>();

            foreach (var unit in units.Where(x => x.MainSalesClassId.HasValue))
            {
                var classId = unit.MainSalesClassId!.Value;
                if (!classIds.Contains(classId))
                {
                    continue;
                }

                AddQuantity(quantityByClass, classId, unit.Quantity ?? 0m);
            }

            var fallbackUnits = units
                .Where(x => !x.MainSalesClassId.HasValue && x.MainPurchaseClassId.HasValue)
                .ToList();
            var purchaseById = new Dictionary<Guid, MainPurchaseClass>();
            if (fallbackUnits.Count > 0)
            {
                var purchaseIds = fallbackUnits.Select(x => x.MainPurchaseClassId!.Value).Distinct().ToList();
                var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var purchaseClasses = (await purchaseClassRepo.Filter(x => purchaseIds.Contains(x.Id))).Data?.ToList() ?? new List<MainPurchaseClass>();
                purchaseById = purchaseClasses.ToDictionary(x => x.Id, x => x);

                foreach (var unit in fallbackUnits)
                {
                    if (!purchaseById.TryGetValue(unit.MainPurchaseClassId!.Value, out var purchaseClass))
                    {
                        continue;
                    }

                    var mappedClassId = TryMapPurchaseClassToSalesClass(
                        purchaseClass,
                        classIdByReferenceCode,
                        classIdByArabicName,
                        classIdByEnglishName);

                    if (!mappedClassId.HasValue || !classIds.Contains(mappedClassId.Value))
                    {
                        continue;
                    }

                    AddQuantity(quantityByClass, mappedClassId.Value, unit.Quantity ?? 0m);
                }
            }

            var operationEntryRepo = _repositoriesFactory.GetRepository<OperationEntry>();
            var operationEntries = (await operationEntryRepo.GetAll()).Data?
                .Where(x =>
                    (!storeDetailsId.HasValue || x.StoreDetailsId == storeDetailsId.Value) &&
                    x.MainPurchaseClassId != Guid.Empty)
                .ToList() ?? new List<OperationEntry>();

            if (operationEntries.Count > 0)
            {
                var operationPurchaseIds = operationEntries.Select(x => x.MainPurchaseClassId).Distinct().ToList();
                var missingPurchaseIds = operationPurchaseIds.Where(x => !purchaseById.ContainsKey(x)).ToList();
                if (missingPurchaseIds.Count > 0)
                {
                    var additionalPurchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                    var extraPurchaseClasses = (await additionalPurchaseClassRepo.Filter(x => missingPurchaseIds.Contains(x.Id))).Data?.ToList() ?? new List<MainPurchaseClass>();
                    foreach (var purchaseClass in extraPurchaseClasses)
                    {
                        purchaseById[purchaseClass.Id] = purchaseClass;
                    }
                }

                var quantityByClassFromOperations = new Dictionary<Guid, decimal>();
                foreach (var entry in operationEntries)
                {
                    if (!purchaseById.TryGetValue(entry.MainPurchaseClassId, out var purchaseClass))
                    {
                        continue;
                    }

                    var mappedClassId = TryMapPurchaseClassToSalesClass(
                        purchaseClass,
                        classIdByReferenceCode,
                        classIdByArabicName,
                        classIdByEnglishName);

                    if (!mappedClassId.HasValue || !classIds.Contains(mappedClassId.Value))
                    {
                        continue;
                    }

                    AddQuantity(quantityByClassFromOperations, mappedClassId.Value, entry.Quantity);
                }

                foreach (var mappedEntry in quantityByClassFromOperations)
                {
                    if (quantityByClass.ContainsKey(mappedEntry.Key))
                    {
                        continue;
                    }

                    quantityByClass[mappedEntry.Key] = mappedEntry.Value;
                }
            }

            // ──── Apply CashierInventoryMovements Adjustment ────
            try
            {
                var movementsRepo = _repositoriesFactory.GetRepository<CashierInventoryMovement>();
                var allMovementsQuery = !storeDetailsId.HasValue 
                    ? await movementsRepo.GetAll()
                    : await movementsRepo.Filter(m => m.StoreDetailsId == storeDetailsId.Value);
                
                var allMovements = allMovementsQuery.Data?
                    .IgnoreQueryFilters()
                    .Where(m => m.IsActive)
                    .ToList() ?? new List<CashierInventoryMovement>();

                var purchaseClassRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var allPurchaseClasses = (await purchaseClassRepo.GetAll()).Data?.IgnoreQueryFilters().Where(x => x.IsActive).ToList() ?? new List<MainPurchaseClass>();

                foreach (var mov in allMovements)
                {
                    if (mov.MainPurchaseClassId == Guid.Empty)
                    {
                        continue;
                    }

                    var purchaseClass = allPurchaseClasses.FirstOrDefault(x => x.Id == mov.MainPurchaseClassId);
                    if (purchaseClass == null) continue;

                    var mappedClassId = TryMapPurchaseClassToSalesClass(
                        purchaseClass,
                        classIdByReferenceCode,
                        classIdByArabicName,
                        classIdByEnglishName);

                    if (!mappedClassId.HasValue || !classIds.Contains(mappedClassId.Value))
                    {
                        continue;
                    }

                    if (mov.MovementType == "SALE" || mov.InvoiceId != 0 || mov.InventoryPermitId != null)
                    {
                        continue;
                    }

                    var quantity = mov.Quantity;
                    if (mov.Direction == -1 || mov.Direction == 0) // Entry (refund / stock in)
                    {
                        AddQuantity(quantityByClass, mappedClassId.Value, quantity);
                    }
                    else if (mov.Direction == 1) // Exit (sale)
                    {
                        AddQuantity(quantityByClass, mappedClassId.Value, -quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GetAvailableQuantityBySalesClassAsync] Failed to apply CashierInventoryMovements adjustment");
            }

            return quantityByClass;
        }

        private bool HasCashierAccess()
        {
            return User.IsInRole("Admin") || User.IsInRole("Cashier") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.POS");
        }

        private static Guid? TryMapPurchaseClassToSalesClass(
            MainPurchaseClass purchaseClass,
            IReadOnlyDictionary<string, Guid> classIdByReferenceCode,
            IReadOnlyDictionary<string, Guid> classIdByArabicName,
            IReadOnlyDictionary<string, Guid> classIdByEnglishName)
        {
            var normalizedReferenceCode = NormalizeLookup(purchaseClass.ReferenceCode);
            if (normalizedReferenceCode.Length > 0 && classIdByReferenceCode.TryGetValue(normalizedReferenceCode, out var classIdFromCode))
            {
                return classIdFromCode;
            }

            var normalizedArabicName = NormalizeLookup(purchaseClass.ArabicName);
            if (normalizedArabicName.Length > 0 && classIdByArabicName.TryGetValue(normalizedArabicName, out var classIdFromArabicName))
            {
                return classIdFromArabicName;
            }

            var normalizedEnglishName = NormalizeLookup(purchaseClass.EnglishName);
            if (normalizedEnglishName.Length > 0 && classIdByEnglishName.TryGetValue(normalizedEnglishName, out var classIdFromEnglishName))
            {
                return classIdFromEnglishName;
            }

            return null;
        }

        private static void AddQuantity(Dictionary<Guid, decimal> quantityByClass, Guid classId, decimal quantity)
        {
            if (quantity == 0m)
            {
                return;
            }

            if (quantityByClass.TryGetValue(classId, out var existingQuantity))
            {
                quantityByClass[classId] = existingQuantity + quantity;
                return;
            }

            quantityByClass[classId] = quantity;
        }

        private static string NormalizeLookup(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private async Task<Guid> MapSalesToPurchaseClassAsync(Guid salesClassId)
        {
            var purchaseRepo = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var exists = (await purchaseRepo.Filter(p => p.Id == salesClassId)).Data?.Any();
            if (exists == true)
            {
                return salesClassId;
            }

            var salesRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
            var salesClass = (await salesRepo.Filter(s => s.Id == salesClassId)).Data?.FirstOrDefault();
            if (salesClass != null)
            {
                var match = (await purchaseRepo.Filter(p => p.ReferenceCode == salesClass.ReferenceCode)).Data?.FirstOrDefault();
                if (match != null)
                {
                    return match.Id;
                }
            }

            return salesClassId;
        }

        #endregion
    }
}
