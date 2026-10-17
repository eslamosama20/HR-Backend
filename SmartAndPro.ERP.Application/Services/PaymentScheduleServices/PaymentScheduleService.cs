using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.Extensions.DependencyInjection;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.PaymentScheduleServices
{

    public class PaymentScheduleService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ERPContext _context;

        public PaymentScheduleService(
            RepositoriesFactory repositoriesFactory,
            IMapper mapper,
            ServiceBaseFactory serviceBaseFactory,
            IServiceProvider serviceProvider,
            ERPContext context)
        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
            _serviceProvider = serviceProvider;
            _context = context;
        }



        public async Task<OperationResponse<PaymentScheduleResponseDto>> AddAsync(PaymentSchedulePostDto dto)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<PaymentSchedule>();

                // ✅ تحقق من وجود الفاتورة
                bool invoiceExists = await CheckInvoiceExistsAsync(dto.InvoiceId, dto.InvoiceType);
                if (!invoiceExists)
                    return new OperationResponse<PaymentScheduleResponseDto>(
                        ErrorCodes.NotFound,
                        null,
                        message: "Invoice not found"
                    );

                // ✅ تحقق من عدم تكرار رقم المرجع على نفس الفاتورة
                if (!string.IsNullOrWhiteSpace(dto.ReferenceNumber))
                {
                    var duplicateQuery = await repo.Filter(p =>
                        p.InvoiceId == dto.InvoiceId &&
                        p.InvoiceType == dto.InvoiceType &&
                        p.ReferenceNumber == dto.ReferenceNumber.Trim());

                    if (duplicateQuery.Data != null && duplicateQuery.Data.Any())
                    {
                        return new OperationResponse<PaymentScheduleResponseDto>(
                            ErrorCodes.GeneralFail,
                            null,
                            message: $"رقم المرجع '{dto.ReferenceNumber}' مستخدم مسبقاً على هذه الفاتورة. يرجى استخدام رقم مرجع مختلف. | Reference number '{dto.ReferenceNumber}' is already used on this invoice. Please use a different reference number."
                        );
                    }
                }

                if (dto.InvoiceType == InvoiceTypeEnum.SaleInvoice && !dto.TreasuryId.HasValue)
                {
                    return new OperationResponse<PaymentScheduleResponseDto>(
                        ErrorCodes.GeneralFail,
                        null,
                        message: "Treasury is required to create the payment receipt."
                    );
                }


                // 1️⃣ حساب EntryNo تلقائي
                // (معلق)

                SaleInvoice saleInvoice = null;

                // 🌟 إنشاء سند قبض تلقائي في scope مستقل (لتجنب تعارض الـ DbContext)
                if (dto.TreasuryId.HasValue && dto.Amount > 0 && dto.InvoiceType == InvoiceTypeEnum.SaleInvoice)
                {
                    if (int.TryParse(dto.InvoiceId, out int targetInvoiceId))
                    {
                        var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
                        var saleInvoiceQuery = await saleInvoiceRepo.Filter(x => x.Id == targetInvoiceId);
                        saleInvoice = await saleInvoiceQuery.Data!
                            .Include(x => x.ReceiptConnections)
                            .FirstOrDefaultAsync();
                    }
                    
                    if (saleInvoice != null)
                    {
                        var customerRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Accounting.CustomerProfile>();
                        var customerQuery = await customerRepo.Filter(c => c.Id == saleInvoice.CustomerProfileId);
                        var customer = customerQuery.Data?.FirstOrDefault();
                        
                        if (customer != null && customer.MainAccountId.HasValue)
                        {
                            var invoiceTotal = (saleInvoice.NetPrice + saleInvoice.TaxAmount) > 0 ? (saleInvoice.NetPrice + saleInvoice.TaxAmount) : saleInvoice.TotalPrice;
                            var currentlyPaid = Math.Max(saleInvoice.PaidAmount, saleInvoice.ReceiptConnections?.Sum(x => x.LinkedAmount) ?? 0m);
                            var remainingAmount = Math.Max(0m, Math.Round(invoiceTotal - currentlyPaid, 2));
                            var linkedAmount = Math.Min(dto.Amount, remainingAmount);
                            var overPaymentAmount = Math.Max(0m, Math.Round(dto.Amount - linkedAmount, 2));
                            var normalizedNotes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
                            var autoNotes = BuildPaymentNotes(normalizedNotes, saleInvoice.InvoiceNo, linkedAmount, overPaymentAmount);

                            var receiptDto = new PaycheckReceiptPostDto
                            {
                                Amount = dto.Amount,
                                TreasuryId = dto.TreasuryId.Value,
                                ReceiptDate = DateOnly.FromDateTime(dto.PaymentDate),
                                Description = autoNotes,
                                PaymentMethodType = dto.PaymentMethodType,
                                PaycheckReceiptAccounts = new List<PaycheckReceiptAccountPostDto>
                                {
                                    new PaycheckReceiptAccountPostDto
                                    {
                                        MainAccountId = customer.MainAccountId.Value,
                                        Amount = dto.Amount
                                    }
                                },
                                ReceiptConnections = new List<ReceiptConnectionPostDto>
                                {
                                    new ReceiptConnectionPostDto
                                    {
                                        SaleInvoiceId = saleInvoice.Id,
                                        LinkedAmount = linkedAmount,
                                        ReceiptConnectionType = SmartAndPro.ERP.SharedKernel.Enums.ReceiptConnectionType.SaleInvoice
                                    }
                                }
                            };

                            if (linkedAmount <= 0)
                            {
                                receiptDto.ReceiptConnections = null;
                            }

                            // إنشاء السند في scope مستقل لتجنب تعارض الـ DbContext
                            OperationResponse<PaycheckReceiptResponseDto> receiptResult;
                            using (var receiptScope = _serviceProvider.CreateScope())
                            {
                                var paycheckReceiptService = receiptScope.ServiceProvider.GetRequiredService<PaycheckReceiptService>();
                                receiptResult = await paycheckReceiptService.AddAsync(receiptDto);
                            }

                            if (receiptResult.ErrorCode != ErrorCodes.Success)
                            {
                                return new OperationResponse<PaymentScheduleResponseDto>(receiptResult.ErrorCode, null, message: receiptResult.Message ?? "Failed to create receipt");
                            }

                            dto.PaycheckReceiptId = receiptResult.Data?.Id;
                            dto.PaymentReceiptId = receiptResult.Data?.Id;
                            dto.ReceiptNo = receiptResult.Data?.ReferenceNumber;
                            dto.Notes = autoNotes;
                        }
                        else
                        {
                            return new OperationResponse<PaymentScheduleResponseDto>(ErrorCodes.GeneralFail, null, message: "Customer does not have a linked Main Account");
                        }
                    }
                }

                // 2️⃣ تحويل DTO لـ Entity
                var payment = _mapper.Map<PaymentSchedule>(dto);
                //payment.EntryNo = nextEntryNo.ToString();

                payment.TotalAmount = payment.Amount + (payment.VAT ?? 0);

                // 3️⃣ إضافة الدفعة
                var addResult = await repo.Add(payment);
                if (addResult.ErrorCode != ErrorCodes.Success)
                {
                    return new OperationResponse<PaymentScheduleResponseDto>(
                        ErrorCodes.GeneralFail,
                        null,
                        message: "Failed to save payment schedule"
                    );
                }

                // 4️⃣ تحديث حالة الفاتورة في scope مستقل
                if (saleInvoice != null)
                {
                    try
                    {
                        using var updateScope = _serviceProvider.CreateScope();
                        var salesInvoicesService = updateScope.ServiceProvider.GetService<ISalesInvoicesService>();
                        if (salesInvoicesService != null)
                            await salesInvoicesService.UpdatePaymentStatusAsync(saleInvoice.Id);
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError($"UpdatePaymentStatusAsync failed after payment commit for invoiceId={saleInvoice.Id}: {ex.Message}");
                    }
                }

                // 5️⃣ تحويل للـ Response
                var result = _mapper.Map<PaymentScheduleResponseDto>(payment);
                return new OperationResponse<PaymentScheduleResponseDto>(ErrorCodes.Success, result);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<PaymentScheduleResponseDto>(
                    ErrorCodes.GeneralFail,
                    null,
                   message: ex.Message
                );
            }
        }

        // 👇 private method للتحقق من وجود الفاتورة لأي نوع
        private async Task<bool> CheckInvoiceExistsAsync(string invoiceId, InvoiceTypeEnum type)
        {
            if (!int.TryParse(invoiceId, out int parsedId)) return false;

            switch (type)
            {
                case InvoiceTypeEnum.HotelAllotmentInvoice:
                    {
                        var repo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
                        var response = await repo.Filter(x => x.Id == parsedId);
                        return response.ErrorCode == ErrorCodes.Success && response.Data.Any();
                    }

                case InvoiceTypeEnum.PurchaseInvoice:
                    {
                        var repo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
                        var response = await repo.Filter(x => x.Id == parsedId);
                        return response.ErrorCode == ErrorCodes.Success && response.Data.Any();
                    }

                case InvoiceTypeEnum.SaleInvoice:
                    {
                        var repo = _repositoriesFactory.GetRepository<SaleInvoice>();
                        var response = await repo.Filter(x => x.Id == parsedId);
                        return response.ErrorCode == ErrorCodes.Success && response.Data.Any();
                    }

                default:
                    return false;
            }
        }
        public async Task<List<PaymentScheduleResponseDto>> GetAllPaymentSchedulesByInvoiceIdAsync(string invoiceId, InvoiceTypeEnum invoiceType)
        {
            var repo = _repositoriesFactory.GetRepository<PaymentSchedule>();

            // فلترة الدفعات حسب الفاتورة والنوع
            var response = await repo.Filter(x => x.InvoiceId == invoiceId && x.InvoiceType == invoiceType);

            if (response.ErrorCode != ErrorCodes.Success || response.Data == null)
                return new List<PaymentScheduleResponseDto>();

            try
            {
                var payments = await response.Data
                                             .AsNoTracking()
                                             .OrderBy(x => x.CreatedAt)
                                             .ToListAsync();

                return _mapper.Map<List<PaymentScheduleResponseDto>>(payments);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                throw;
            }

        }

        /// <summary>
        /// ربط سند صرف (PaycheckReceipt) بدفعة مجدولة محددة
        /// يُستخدم بعد إنشاء سند الصرف يدوياً من شاشة مشتريات الحجوزات
        /// </summary>
        public async Task<OperationResponse<PaymentScheduleResponseDto>> LinkReceiptToScheduleAsync(
            Guid scheduleId,
            Guid paycheckReceiptId,
            string receiptNo)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<PaymentSchedule>();
                var filter = await repo.Filter(x => x.Id == scheduleId);
                var schedule = await filter.Data.FirstOrDefaultAsync();

                if (schedule == null)
                    return new OperationResponse<PaymentScheduleResponseDto>(
                        ErrorCodes.NotFound, null,message: "Payment schedule not found");

                // تحديث الربط
                schedule.PaycheckReceiptId = paycheckReceiptId;
                schedule.ReceiptNo = receiptNo;

                await repo.Update(schedule);

                var result = _mapper.Map<PaymentScheduleResponseDto>(schedule);
                return new OperationResponse<PaymentScheduleResponseDto>(ErrorCodes.Success, result);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<PaymentScheduleResponseDto>(
                    ErrorCodes.GeneralFail, null,message: ex.Message);
            }
        }


        private static string BuildPaymentNotes(string? userNotes, string invoiceNo, decimal linkedAmount, decimal overPaymentAmount)
        {
            var notes = new List<string>();

            if (!string.IsNullOrWhiteSpace(userNotes))
            {
                notes.Add(userNotes.Trim());
            }

            if (linkedAmount > 0)
            {
                notes.Add($"سداد فاتورة {invoiceNo} بمبلغ {linkedAmount:F2}");
            }

            if (overPaymentAmount > 0)
            {
                notes.Add($"دفعة زيادة للعميل تُرحّل إلى الرصيد الدائن بمبلغ {overPaymentAmount:F2}");
            }

            return string.Join(" - ", notes.Distinct());
        }




        //public async Task<decimal> GetTotalPaymentsAsync(int invoiceId)
        //{
        //    var repo = _repositoriesFactory.GetRepository<PaymentSchedule>();

        //    var response = await repo.Filter(x => x.HotelAllotmentInvoiceId == invoiceId);

        //    if (response.ErrorCode != ErrorCodes.Success || response.Data == null || !response.Data.Any())
        //        return 0;

        //    var payments = await response.Data
        //                                 .Select(x => x.TotalAmount)
        //                                 .ToListAsync();

        //    return payments.Sum();
        //}

        //#region Update
        //public async Task<OperationResponse<PaymentScheduleResponseDto>> UpdateAsync(PaymentSchedulePutDto dto)
        //{
        //    try
        //    {
        //        var repo = _repositoriesFactory.GetRepository<PaymentSchedule>();

        //        var filter = await repo.Filter(x => x.Id == dto.Id);
        //        var existing = await filter.Data.FirstOrDefaultAsync();

        //        if (existing == null)
        //            return new OperationResponse<PaymentScheduleResponseDto>(ErrorCodes.NotFound, null, "Payment not found");

        //        // تحديث البيانات باستخدام AutoMapper
        //        _mapper.Map(dto, existing);

        //        await repo.Update(existing);

        //        var result = _mapper.Map<PaymentScheduleResponseDto>(existing);
        //        return new OperationResponse<PaymentScheduleResponseDto>(ErrorCodes.Success, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return new OperationResponse<PaymentScheduleResponseDto>(ErrorCodes.GeneralFail, null, ex.Message);
        //    }
        //}
        //#endregion

        //#region Delete
        //public async Task<OperationResponse<bool>> DeleteAsync(int id)
        //{
        //    try
        //    {
        //        var repo = _repositoriesFactory.GetRepository<PaymentSchedule>();
        //        var filter = await repo.Filter(x => x.Id == id);
        //        var payment = await filter.Data.FirstOrDefaultAsync();

        //        if (payment == null)
        //            return new OperationResponse<bool>(ErrorCodes.NotFound, false, "Payment not found");

        //        await repo.Delete(payment);
        //        return new OperationResponse<bool>(ErrorCodes.Success, true);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.LogError(ex);
        //        return new OperationResponse<bool>(ErrorCodes.GeneralFail, false, ex.Message);
        //    }
        //}
        //#endregion



    }
}

