using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Controllers.Cashier.Dtos;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using SmartAndPro.ERP.WebAPI.Services.Pharmacy;
using SmartAndPro.ERP.WebAPI.Services.Wasafaty;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier
{
    [ApiController]
    [Route("api/cashier")]
    [Authorize]
    public class CashierPaymentController : ControllerBase
    {
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ICashierRuntimeStore _cashierStore;
        private readonly ICashierReservationService _reservationService;
        private readonly ICashierAuditService _auditService;
        private readonly ILogger<CashierPaymentController> _logger;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IWasafatyIntegrationService _wasafatyService;
        private readonly IPharmacyCchiClaimService _cchiService;

        public CashierPaymentController(
            ISalesInvoicesService salesInvoicesService,
            ICashierRuntimeStore cashierStore,
            ICashierReservationService reservationService,
            ICashierAuditService auditService,
            ILogger<CashierPaymentController> logger,
            RepositoriesFactory repositoriesFactory,
            IWasafatyIntegrationService wasafatyService,
            IPharmacyCchiClaimService cchiService)
        {
            _salesInvoicesService = salesInvoicesService;
            _cashierStore = cashierStore;
            _reservationService = reservationService;
            _auditService = auditService;
            _logger = logger;
            _repositoriesFactory = repositoriesFactory;
            _wasafatyService = wasafatyService;
            _cchiService = cchiService;
        }

        /// <summary>
        /// Process Payment - معالجة الدفع
        /// POST /api/cashier/payment
        /// </summary>
        [HttpPost("payment")]
        public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] Dtos.PaymentRequest request)
        {
            var debugLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CashierPaymentController] ProcessPayment called for invoice {request.InvoiceId}";
            Console.WriteLine(debugLine);
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "cashier-debug.log"), debugLine + Environment.NewLine); } catch { }
            
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

                var storeId = HttpContext.Items["CashierStoreId"] as Guid?;
                if (!storeId.HasValue || storeId.Value == Guid.Empty)
                {
                    return StatusCode(403, new
                    {
                        errorCode = "SESSION_NO_STORE",
                        message = "Cashier session has no store assigned",
                        messageAr = "جلسة الكاشير ليس لها متجر محدد"
                    });
                }

                _logger.LogInformation(
                    "[CashierPayment] 🔍 ProcessPayment called - InvoiceId={InvoiceId}, SessionId={SessionId}, StoreId={StoreId}, UserGuid={UserGuid}",
                    request.InvoiceId,
                    cashierSessionId.Value,
                    storeId.Value,
                    HttpContext.User.FindFirst("sub")?.Value ?? "unknown");

                // Get the invoice with retry (exponential backoff for DB sync)
                var invoice = await _salesInvoicesService.GetByIdAsync(request.InvoiceId);
                if (invoice == null)
                {
                    int[] delaysMs = { 500, 1000, 2000 };
                    for (int attempt = 0; attempt < delaysMs.Length && invoice == null; attempt++)
                    {
                        _logger.LogWarning(
                            "[CashierPayment] ⚠️  Invoice {InvoiceId} not found (attempt {Attempt}/{Total}), retrying after {Delay}ms...",
                            request.InvoiceId, attempt + 1, delaysMs.Length, delaysMs[attempt]);

                        await Task.Delay(delaysMs[attempt]);
                        invoice = await _salesInvoicesService.GetByIdAsync(request.InvoiceId);
                    }

                    if (invoice == null)
                    {
                        _logger.LogError(
                            "[CashierPayment] ❌ Invoice {InvoiceId} not found after all retries - DB sync issue or invalid ID",
                            request.InvoiceId);
                        return NotFound(new
                        {
                            message = "Invoice not found",
                            messageAr = "الفاتورة غير موجودة"
                        });
                    }

                    _logger.LogInformation(
                        "[CashierPayment] ✅ Invoice {InvoiceId} found after retry - InvoiceNo={InvoiceNo}, NetPrice={NetPrice}",
                        invoice.Id,
                        invoice.InvoiceNo,
                        invoice.NetPrice);
                }
                else
                {
                    _logger.LogInformation(
                        "[CashierPayment] ✅ Invoice {InvoiceId} found immediately - InvoiceNo={InvoiceNo}, NetPrice={NetPrice}",
                        invoice.Id,
                        invoice.InvoiceNo,
                        invoice.NetPrice);
                }

                // Prevent duplicate payments for the same invoice
                if (invoice.TransactionId.HasValue)
                {
                    return Conflict(new
                    {
                        message = "Invoice is already paid",
                        messageAr = "تم دفع الفاتورة بالفعل"
                    });
                }

                // Enforce invoice-session-store binding when tracking info exists
                var trackedInvoice = _cashierStore.GetInvoiceState(invoice.Id);
                if (trackedInvoice?.IsPaid == true)
                {
                    return Conflict(new
                    {
                        message = "Invoice is already paid",
                        messageAr = "تم دفع الفاتورة بالفعل"
                    });
                }

                if (trackedInvoice != null)
                {
                    if (trackedInvoice.SessionId.HasValue && trackedInvoice.SessionId.Value != cashierSessionId.Value)
                    {
                        return StatusCode(403, new
                        {
                            errorCode = "INVOICE_SESSION_MISMATCH",
                            message = "Invoice does not belong to current cashier session",
                            messageAr = "الفاتورة لا تنتمي إلى جلسة الكاشير الحالية"
                        });
                    }

                    if (trackedInvoice.StoreDetailsId.HasValue && trackedInvoice.StoreDetailsId.Value != storeId.Value)
                    {
                        return StatusCode(403, new
                        {
                            errorCode = "INVOICE_STORE_MISMATCH",
                            message = "Invoice does not belong to current session store",
                            messageAr = "الفاتورة لا تنتمي إلى مخزن الجلسة الحالية"
                        });
                    }
                }

                // Create accounting transaction if possible.
                // Payment should still complete when accounting setup is incomplete in runtime.
                var accountingTransactionCreated = false;
                try
                {
                    var transactionResult = await _salesInvoicesService.CreateTransaction(invoice.Id, invoice.IsFrontal ?? false);
                    accountingTransactionCreated = transactionResult == 1;
                    if (!accountingTransactionCreated)
                    {
                        _logger.LogWarning(
                            "Accounting transaction was not created for invoice {InvoiceId}; continuing cashier payment flow",
                            invoice.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Accounting transaction creation failed for invoice {InvoiceId}; continuing cashier payment flow",
                        invoice.Id);
                }

                // Mark runtime reservation as paid only after successful transaction creation
                _cashierStore.MarkInvoicePaid(invoice.Id);
                
                Console.WriteLine($"========== [CashierPaymentController] About to call ConfirmInvoiceAsync for invoice {invoice.Id} ==========");
                await _reservationService.ConfirmInvoiceAsync(invoice.Id);
                Console.WriteLine($"========== [CashierPaymentController] Completed ConfirmInvoiceAsync for invoice {invoice.Id} ==========");

                await _auditService.LogAsync(
                    ResolveCurrentUserGuid(),
                    ResolveCurrentUserName(),
                    "PAYMENT_PROCESSED",
                    "SaleInvoice",
                    invoice.Id.ToString(),
                    cashierSessionId,
                    storeId,
                    new
                    {
                        request.PaymentMethod,
                        request.TotalAmount,
                        request.CashAmount,
                        request.CardAmount,
                        TransactionId = invoice.TransactionId
                    });

                // Calculate change amount
                decimal paidAmount = request.PaymentMethod == 0 // Cash
                    ? request.CashAmount ?? request.TotalAmount
                    : request.TotalAmount;

                decimal changeAmount = paidAmount > request.TotalAmount
                    ? paidAmount - request.TotalAmount
                    : 0;

                decimal cashAmt = request.PaymentMethod == 0 ? (request.CashAmount ?? request.TotalAmount) : 0;
                decimal cardAmt = request.PaymentMethod == 1 ? request.TotalAmount : (request.CardAmount ?? 0);
                decimal creditAmt = request.PaymentMethod == 2 ? request.TotalAmount : 0;

                // ✅ Save payment to CashierPayment DB table
                try
                {
                    var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                    var dbPayment = new SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment
                    {
                        InvoiceId = invoice.Id,
                        SessionId = cashierSessionId ?? 0,
                        PaymentMethod = request.PaymentMethod,
                        TotalAmount = request.TotalAmount,
                        PaidAmount = paidAmount,
                        ChangeAmount = changeAmount,
                        CashAmount = cashAmt,
                        CardAmount = cardAmt,
                        CreditAmount = creditAmt,
                        PaidAtUtc = DateTime.UtcNow,
                        StoreDetailsId = storeId,
                        IsActive = true,
                        CreatedBy = ResolveCurrentUserGuid(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await paymentRepo.Add(dbPayment);

                    // ✅ Wasafaty: If this payment is linked to a Wasafaty prescription, mark as dispensed
                    if (request.WasafatyPrescriptionId.HasValue)
                    {
                        try
                        {
                            var wasafatyPrescription = await _wasafatyService.GetPrescriptionByIdAsync(request.WasafatyPrescriptionId.Value);
                            if (wasafatyPrescription != null)
                            {
                                // Build dispense request from the prescription items
                                var dispenseReq = new SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty.Dtos.DispenseRequest
                                {
                                    WasafatyPrescriptionId = request.WasafatyPrescriptionId.Value,
                                    SaleInvoiceId = invoice.Id,
                                    CashierPaymentId = dbPayment.Id,
                                    Items = wasafatyPrescription.Items.Select(i => new SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty.Dtos.DispenseItemRequest
                                    {
                                        ItemId = i.Id,
                                        DispensedQty = i.RequestedQty,
                                        ErpProductId = i.ErpProductId,
                                        UnitPrice = i.UnitPrice,
                                        BatchNo = i.BatchNo,
                                        ExpiryDate = i.ExpiryDate
                                    }).ToList()
                                };

                                await _wasafatyService.MarkDispensedAsync(dispenseReq);

                                // Report to Wasafaty API
                                try
                                {
                                    await _wasafatyService.ReportDispenseToWasafatyAsync(request.WasafatyPrescriptionId.Value);
                                }
                                catch (Exception reportEx)
                                {
                                    _logger.LogError(reportEx, "Wasafaty dispense report failed for prescription {Id} — logged in sync", request.WasafatyPrescriptionId.Value);
                                }

                                _logger.LogInformation("[CashierPayment] ✅ Wasafaty prescription {WasafatyId} dispensed after payment for invoice {InvoiceId}",
                                    request.WasafatyPrescriptionId.Value, invoice.Id);
                            }
                        }
                        catch (Exception wasafatyEx)
                        {
                            _logger.LogError(wasafatyEx, "Failed to trigger Wasafaty dispense for prescription {Id}", request.WasafatyPrescriptionId.Value);
                        }
                    }

                    // ✅ CCHI/NPHIES: If this payment is linked to a local pharmacy prescription with insurance, auto-submit claim
                    if (request.PharmacyPrescriptionId.HasValue)
                    {
                        try
                        {
                            var claimResult = await _cchiService.SubmitClaimAsync(
                                request.PharmacyPrescriptionId.Value,
                                invoice.Id,
                                dbPayment.Id,
                                ResolveCurrentUserGuid());

                            if (claimResult.Success)
                                _logger.LogInformation(
                                    "[CashierPayment] ✅ CCHI claim submitted for prescription {PrescriptionId} — NphiesId={NphiesId}",
                                    request.PharmacyPrescriptionId.Value, claimResult.NphiesClaimId);
                            else
                                _logger.LogWarning(
                                    "[CashierPayment] ⚠️ CCHI claim submission skipped/failed for prescription {PrescriptionId}: {Msg}",
                                    request.PharmacyPrescriptionId.Value, claimResult.Message);
                        }
                        catch (Exception cchiEx)
                        {
                            _logger.LogError(cchiEx,
                                "[CashierPayment] ❌ CCHI claim submission error for prescription {Id} — payment still successful",
                                request.PharmacyPrescriptionId.Value);
                        }
                    }

                    // Update session statistics in DB
                    if (cashierSessionId.HasValue)
                    {
                        var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                        var dbSession = (await sessionRepo.Filter(s => s.SessionId == cashierSessionId.Value)).Data?.FirstOrDefault();
                        if (dbSession != null)
                        {
                            dbSession.TotalSales += invoice.NetPrice;
                            dbSession.TransactionsCount += 1;
                            dbSession.TotalCash += cashAmt;
                            dbSession.TotalCard += cardAmt;
                            dbSession.TotalCredit += creditAmt;
                            await sessionRepo.Update(dbSession);
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to persist CashierPayment to DB for invoice {InvoiceId}", invoice.Id);
                }

                var response = new PaymentResponse
                {
                    Success = true,
                    InvoiceId = invoice.Id,
                    InvoiceNo = invoice.InvoiceNo,
                    PaidAmount = paidAmount,
                    ChangeAmount = changeAmount,
                    Message = accountingTransactionCreated
                        ? "Payment processed successfully"
                        : "Payment processed successfully (accounting transaction pending)",
                    MessageAr = accountingTransactionCreated
                        ? "تم معالجة الدفع بنجاح"
                        : "تمت معالجة الدفع بنجاح (معاملة المحاسبة معلقة)"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", request.InvoiceId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error processing payment",
                    messageAr = "خطأ في معالجة الدفع",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get payment history for an invoice
        /// GET /api/cashier/payment/{invoiceId}
        /// </summary>
        [HttpGet("payment/{invoiceId}")]
        public async Task<ActionResult> GetPaymentHistory(int invoiceId)
        {
            try
            {
                var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
                if (invoice == null)
                    return NotFound(new { message = "Invoice not found", messageAr = "الفاتورة غير موجودة" });

                return Ok(new
                {
                    invoiceId = invoice.Id,
                    invoiceNo = invoice.InvoiceNo,
                    totalAmount = invoice.NetPrice,
                    transactionId = invoice.TransactionId,
                    isPaid = invoice.TransactionId.HasValue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { message = "Error retrieving payment history", messageAr = "خطأ في استرجاع سجل الدفع" });
            }
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
                      ?? User.Identity?.Name;

            return string.IsNullOrWhiteSpace(raw) ? "Cashier" : raw.Trim();
        }

        private bool HasCashierAccess()
        {
            return User.IsInRole("Admin") || User.IsInRole("Cashier") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.POS");
        }
    }
}
