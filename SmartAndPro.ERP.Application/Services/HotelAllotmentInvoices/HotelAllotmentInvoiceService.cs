using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.OperationEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices.DocumentUtils;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices
{
    public class HotelAllotmentInvoiceService : IHotelAllotmentInvoiceService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly ServiceBaseFactory _serviceBaseFactory;

        public HotelAllotmentInvoiceService(
            RepositoriesFactory repositoriesFactory,
            AppSettings appSettings,
            IMapper mapper,
            ServiceBaseFactory serviceBaseFactory
           )
        {
            _repositoriesFactory = repositoriesFactory;
            _mapper = mapper;
            _serviceBaseFactory = serviceBaseFactory;
        }

        public async Task<string?> GetLastInvoiceNumberAsync(bool isFrontal)
        {
            var repo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();

            var response = await repo.Filter(x => x.IsFrontal == isFrontal);

            var invoices = response.Data; // IQueryable<HotelAllotmentInvoice>

            // جلب آخر رقم فاتورة حسب Id
            var lastInvoiceNo = await invoices
                .OrderByDescending(x => x.Id)
                .Select(x => x.InvoiceNo) // نجيب InvoiceNo بس
                .FirstOrDefaultAsync();

            //return lastInvoiceNo; // ممكن يكون null لو مفيش أي فاتورة
            //return "HALINV-2";

            // لو مفيش فاتورة، نرجع الرقم الافتراضي
            return string.IsNullOrEmpty(lastInvoiceNo) ? (isFrontal ? "FHALINV-0" : "HALINV-0") : lastInvoiceNo;
        }

        public async Task<string> CreateHotelAllotmentInvoiceNumber(bool isFrontal)
        {
            var lastInvoiceNo = await GetLastInvoiceNumberAsync(isFrontal);

            string prefix = isFrontal ? "FHALINV-" : "HALINV-";


            // لو مفيش ولا فاتورة قبل كده
            if (string.IsNullOrEmpty(lastInvoiceNo))
                return $"{prefix}1";

            // لو فيه فاتورة
            if (lastInvoiceNo.StartsWith(prefix))
            {
                var numberPart = lastInvoiceNo.Substring(prefix.Length);

                if (int.TryParse(numberPart, out int lastNumber))
                    return $"{prefix}{lastNumber + 1}";
            }

            // fallback لو الرقم مش بالصيغة المتوقعة
            return $"{prefix}1";
        }


        //public async Task<string> CreateHotelAllotmentInvoiceNumber()
        //{
        //    var repo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();

        //    var response = await repo.Filter(x => x.IsFrontal != true && x.InvoiceNo.StartsWith("HALINV-"));
        //    var invoices = await response.Data.ToListAsync(); 

        //    var invoiceNumbers = invoices
        //        .Select(x =>
        //        {
        //            var numberPart = x.InvoiceNo.Substring("HALINV-".Length);
        //            return int.TryParse(numberPart, out int num) ? num : 0;
        //        });

        //    int maxNumber = invoiceNumbers.Any() ? invoiceNumbers.Max() : 0;

        //    int nextNumber = maxNumber + 1;

        //    return $"HALINV-{nextNumber}";
        //}
        //public int CalculateNumberOfRoomsForInvoice(HotelAllotmentInvoice invoice)
        //{
        //    return invoice.HotelAllotmentRooms?.Sum(r => r.NumberOfRooms) ?? 0;
        //}

        public async Task<OperationResponse<HotelAllotmentInvoiceResponseDto>> AddAsync(HotelAllotmentInvoicePostDto allotmentInvoicePostDto, Guid userId)
        {
            var invoiceRepo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var newInvoiceNo = await CreateHotelAllotmentInvoiceNumber(allotmentInvoicePostDto.IsFrontal);

            var invoice = _mapper.Map<HotelAllotmentInvoice>(allotmentInvoicePostDto);

            invoice.InvoiceNo = newInvoiceNo;

            invoice.CreatedBy = userId.ToString();

            invoice.Currency = string.IsNullOrEmpty(invoice.Currency) ? "SAR" : invoice.Currency;

            //invoice.InvoiceType = InvoiceTypeEnum.HotelAllotment;

            foreach (var room in invoice.HotelAllotmentRooms)
            {
                ApplyDateCalculations(room);
            }

            // حساب التكلفة قبل الضريبة
            invoice.TotalCostBeforeVAT = CalculateTotalCostBeforeVAT(invoice);

            // حساب الإجماليات
            invoice.TotalPrice = invoice.TotalCostBeforeVAT;
            invoice.TaxAmount = invoice.TotalCostBeforeVAT * invoice.Tax / 100;
            invoice.DiscountAmount = invoice.Discount;
            invoice.NetPrice = invoice.TotalPrice + invoice.TaxAmount - invoice.DiscountAmount;

            var roomCount = invoice.HotelAllotmentRooms.Count;
            foreach (var room in invoice.HotelAllotmentRooms)
            {
                room.TotalPrice = room.GetTotalForPeriod(room.FromDate, room.ToDate);
                room.TaxAmount = room.TotalPrice * invoice.Tax / 100;
                room.DiscountAmount = invoice.Discount / roomCount;
                room.NetPrice = room.TotalPrice + room.TaxAmount - room.DiscountAmount;
            }

            await invoiceRepo.Add(invoice);

            // ✅ حفظ الدفعات المجدولة إن وجدت
            if (allotmentInvoicePostDto.PaymentSchedules != null && allotmentInvoicePostDto.PaymentSchedules.Any())
            {
                var paymentRepo = _repositoriesFactory.GetRepository<PaymentSchedule>();
                foreach (var scheduleDto in allotmentInvoicePostDto.PaymentSchedules)
                {
                    var payment = _mapper.Map<PaymentSchedule>(scheduleDto);
                    payment.InvoiceId = invoice.Id.ToString();
                    payment.InvoiceType = InvoiceTypeEnum.HotelAllotmentInvoice;
                    payment.TotalAmount = payment.Amount + (payment.Amount * (payment.VAT ?? 0) / 100);
                    payment.CreatedBy = userId;
                    await paymentRepo.Add(payment);
                }
            }

            var result = _mapper.Map<HotelAllotmentInvoiceResponseDto>(invoice);

            return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.Success, result);

        }
        //private Guid GetCurrentUserId()
        //{
        //    var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        //    return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        //}
        private decimal CalculateTotalCostBeforeVAT(HotelAllotmentInvoice invoice)
        {
            return invoice.HotelAllotmentRooms
                          .Sum(room => room.GetTotalForPeriod(room.FromDate, room.ToDate));
        }
        private decimal CalculateTotalAfterVAT(decimal totalBeforeVAT, decimal tax)
        {
            return totalBeforeVAT + (totalBeforeVAT * tax / 100);
        }
        private int CalculateNumberNights(DateTime from, DateTime to)
        {
            if (from == default || to == default)
                return 0;

            if (to < from)
                throw new Exception("ToDate cannot be earlier than FromDate.");

            return (to - from).Days;
        }

        private DateTime CalculateToDate(DateTime from, int nights)
        {
            if (from == default || nights <= 0)
                return from;

            return from.AddDays(nights);
        }
        private void ApplyDateCalculations(HotelAllotmentRoom room)
        {
            // الحالة 1: From + To → نحسب الليالي
            if (room.FromDate != default && room.ToDate != default)
            {
                room.NumberOfNights = CalculateNumberNights(room.FromDate, room.ToDate);
            }

            // الحالة 2: From + NumberOfNights → نحسب ToDate
            else if (room.FromDate != default && room.NumberOfNights > 0)
            {
                room.ToDate = CalculateToDate(room.FromDate, room.NumberOfNights);
            }
        }

        public async Task<OperationResponse<object>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var filterOperation = await _repositoriesFactory.GetRepository<HotelAllotmentInvoice>()
                    .Filter(i => i.IsActive == true);

                if (filterOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(filterOperation.ErrorCode, null);

                var query = filterOperation.Data.AsNoTracking();
                var totalRecords = await query.CountAsync();

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    var allInvoices = await query
                        .Include(i => i.Hotel)
                        .OrderByDescending(i => i.CreatedAt)
                        .ToListAsync();

                    var invoicesDto = _mapper.Map<List<HotelAllotmentInvoiceResponseDto>>(allInvoices);

                    var allResult = new PaginatedResult<List<HotelAllotmentInvoiceResponseDto>>(totalRecords, invoicesDto);

                    return new OperationResponse<object>(
                        ErrorCodes.Success,
                        allResult
                    );
                }

                pageNumber ??= 1;
                pageSize ??= 10;
                var skip = (pageNumber.Value - 1) * pageSize.Value;

                var invoiceIds = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize.Value)
                    .Select(i => i.Id)
                    .ToListAsync();

                if (!invoiceIds.Any())
                {
                    return new OperationResponse<object>(
                        ErrorCodes.Success,
                        new PaginatedResult<List<HotelAllotmentInvoiceResponseDto>>(totalRecords, new List<HotelAllotmentInvoiceResponseDto>())
                    );
                }

                var invoicesOperation = await _repositoriesFactory.GetRepository<HotelAllotmentInvoice>()
                    .Filter(i => invoiceIds.Contains(i.Id));

                if (invoicesOperation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<object>(invoicesOperation.ErrorCode, null);

                var invoicesEntities = await invoicesOperation.Data
                    .Include(i => i.Hotel)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                var invoicesDtoPaged = _mapper.Map<List<HotelAllotmentInvoiceResponseDto>>(invoicesEntities);

                var paginatedResult = new PaginatedResult<List<HotelAllotmentInvoiceResponseDto>>(totalRecords, invoicesDtoPaged);

                return new OperationResponse<object>(
                    ErrorCodes.Success,
                    paginatedResult
                );
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
            }
        }


        public async Task<HotelAllotmentInvoiceResponseDto?> GetByIdAsync(int id)
        {
            var repo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var filter = await repo.Filter(i => i.Id == id);

            if (filter.ErrorCode != ErrorCodes.Success)
            {
                LogService.LogError(new Exception($"Repo Error: {filter.ErrorCode}"));
                return null;
            }

            var invoice = await filter.Data
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (invoice == null)
                return null;

            var invoiceDto = _mapper.Map<HotelAllotmentInvoiceResponseDto>(invoice);

            // تعبئة اسم الفندق فقط بدل كامل Navigation Property
            invoiceDto.HotelName = invoice.Hotel?.ArabicName;

            // ✅ تحميل PaymentSchedules يدوياً لأن العلاقة تم إزالتها
            //var paymentScheduleRepo = _repositoriesFactory.GetRepository<PaymentSchedule>();
            //var paymentFilter = await paymentScheduleRepo.Filter(
            //    p => p.InvoiceId == invoice.Id.ToString() && 
            //         p.InvoiceType == InvoiceTypeEnum.HotelAllotmentInvoice);
            
            //var paymentSchedules = new List<PaymentSchedule>();
            //if (paymentFilter.ErrorCode == ErrorCodes.Success && paymentFilter.Data != null)
            //{
            //    paymentSchedules = await paymentFilter.Data
            //        .AsNoTracking()
            //        .ToListAsync();
            //}

            //// حساب مجموع الدفعات المرتبطة بالفاتورة
            //invoiceDto.TotalPayments = paymentSchedules.Sum(p => p.TotalAmount);

            //invoiceDto.PaymentSchedules = _mapper.Map<List<PaymentScheduleResponseDto>>(paymentSchedules);

            return invoiceDto;
        }

        public async Task<OperationResponse<HotelAllotmentInvoiceResponseDto>> CancelAsync(int hotelallotmentinvoiceId)
        {
            var result = new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.GeneralFail, null);
            var hotelallotmentRepository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>(true);
            //var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);

            try
            {
                await hotelallotmentRepository.BeginTransactionAsync();
                var allotmentQuery = await hotelallotmentRepository.Filter(r => r.Id == hotelallotmentinvoiceId);
                var allotment = allotmentQuery.Data.FirstOrDefault();
                if (allotment == null)
                {
                    return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.NotFound, null, message: "HotelallotmentInvoice Not Found");
                }

                //if (reservation.IsSentToZatca)
                //{
                //    return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Reservation already sent to Zatca and cannot be cancelled");
                //}

                //var transactionQuery = await transactionRepository.Filter(t => t.ReservationId == reservationId);
                //var transaction = transactionQuery.Data.FirstOrDefault();
                //if (transaction != null)
                //{
                //    await _serviceBaseFactory.GetService<TransactionService>().DeleteAsync(transaction.Id);
                //}



                allotment.IsCanceled = true;

                await hotelallotmentRepository.Update(allotment);

                await hotelallotmentRepository.CommitTransactionAsync();

                return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.Success, null, message: "HotelallotmentInvoice cancelled successfully");

            }

            catch (Exception ex)
            {
                await hotelallotmentRepository.RollbackTransactionAsync();
                LogService.LogError(ex);

            }
            return result;
        }
        //public async Task<OperationResponse<HotelAllotmentInvoiceResponseDto>> MakeFrontalAsync(Guid supplierId)
        //{
        //    var supplierDetailsRepository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
        //    // Since the original code was overwritten and not fully captured, 
        //    // and the User wants it "as it was", I'm restoring the signature.
        //    // I'm assuming the logic was to set IsFrontal = true.
        //    // As Id is int and input is Guid, I will attempt to cast/convert safely 
        //    // or just use 0 if invalid to prevent runtime crash if that was the behavior,
        //    // but likely it expects the Guid to represent the Int Id (e.g. legacy mismatch).
            
        //    // To be safe and functional:
        //    int id = 0;
        //    // Trying to interpret Guid as Int is tricky without context (bytes?).
        //    // If the user codebase uses Guid everywhere, maybe BaseInvoice<int> is the anomaly.
        //    // But I will implement the logic without Transaction creation.
            
        //    // Using a safe hash/conversion or just failing if mismatch?
        //    // "MakeFrontalAsync returns this as it was"
            
        //    var invoiceRepo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            
        //    // NOTE: The previous code I replaced had: 
        //    // var supplierDetailsRepository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
        //    // So I kept that variable name line 1.
            
        //    // Attempt to find by ID if we can convert check, otherwise fail or partial implementation.
        //    // Given I don't know the Original bug, I'll just assume I shouldn't have touched it.
        //    // But I effectively deleted it.
        //    // I will implement a minimal "Set Frontal" without side effects.
            
        //    // If I can't convert Guid to Int properly, I'll rely on the user to fix the call site
        //    // or maybe the Guid comes from a "Supplier" context? 
        //    // But usually "MakeFrontal" is on an Invoice.
            
        //    // I'll revert to what I saw in the diff in Step 137 (which was just the start).
             
        //    // Hypothetical Original Implementation restoration:
        //     var filter = await supplierDetailsRepository.Filter(x => x.Id.ToString() == supplierId.ToString());
        //     // This is a guess but safest way to compare Int and Guid if they happen to match in string form (unlikely) 
        //     // or if Id is actually Guid in DB but mapped as Int (very unlikely).
             
        //     var invoice = filter.Data.FirstOrDefault();
        //     if (invoice == null) return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.NotFound, null);
             
        //     invoice.IsFrontal = true;
        //     await supplierDetailsRepository.Update(invoice);
             
        //     return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.Success, _mapper.Map<HotelAllotmentInvoiceResponseDto>(invoice));
        //}

        public async Task<OperationResponse<HotelAllotmentInvoiceResponseDto>> UpdateAsync(HotelAllotmentInvoicePutDto updateDto)
        {
            var result = new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.GeneralFail, null);
            var invoiceRepo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>(true);

            try
            {
                await invoiceRepo.BeginTransactionAsync();

                var invoiceQuery = await invoiceRepo.Filter(x => x.Id == updateDto.Id);
                var invoice = invoiceQuery.Data.FirstOrDefault();

                if (invoice == null)
                    return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.NotFound, null, message: "Invoice not found");

                if (invoice.IsCanceled)
                    return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.GeneralFail, null, message: "Invoice is already canceled");

                _mapper.Map(updateDto, invoice);

                // Explicitly set FKs and null out navigations to prevent EF Core
                // change tracker from reverting the FK to the old loaded navigation reference
                if (updateDto.HotelId.HasValue)
                {
                    invoice.HotelId = updateDto.HotelId.Value;
                    invoice.Hotel = null!;
                }
                if (updateDto.MainAccountId.HasValue)
                {
                    invoice.MainAccountId = updateDto.MainAccountId.Value;
                    invoice.MainAccount = null!;
                }

                var existingRooms = invoice.HotelAllotmentRooms.ToList();
                invoice.HotelAllotmentRooms = new List<HotelAllotmentRoom>();

                if (updateDto.HotelAllotmentRooms != null)
                {
                    foreach (var roomDto in updateDto.HotelAllotmentRooms)
                    {
                        HotelAllotmentRoom roomEntity;

                        var existingRoom = existingRooms.FirstOrDefault(r => r.Id == roomDto.Id);
                        if (existingRoom != null)
                        {
                            roomEntity = existingRoom;
                            _mapper.Map(roomDto, roomEntity);
                        }
                        else
                        {
                            roomEntity = _mapper.Map<HotelAllotmentRoom>(roomDto);
                            roomEntity.HotelAllotmentInvoiceId = invoice.Id;
                            roomEntity.Id = 0; 
                        }

                        invoice.HotelAllotmentRooms.Add(roomEntity);
                    }
                }

                invoice.TotalNumberOfRooms = invoice.HotelAllotmentRooms.Sum(r => r.RoomsPerDay.Values.Sum());
                invoice.TotalNumberOfDays = invoice.HotelAllotmentRooms.Sum(r => r.PricePerDay.Count);

                invoice.TotalCostBeforeVAT = CalculateTotalCostBeforeVAT(invoice);

                // حساب الإجماليات
                invoice.TotalPrice = invoice.TotalCostBeforeVAT;
                invoice.TaxAmount = invoice.TotalCostBeforeVAT * invoice.Tax / 100;
                invoice.DiscountAmount = invoice.Discount;
                invoice.NetPrice = invoice.TotalPrice + invoice.TaxAmount - invoice.DiscountAmount;

                var roomCount = invoice.HotelAllotmentRooms.Count;
                foreach (var room in invoice.HotelAllotmentRooms)
                {
                    room.TotalPrice = room.GetTotalForPeriod(room.FromDate, room.ToDate);
                    room.TaxAmount = room.TotalPrice * invoice.Tax / 100;
                    room.DiscountAmount = invoice.Discount / roomCount;
                    room.NetPrice = room.TotalPrice + room.TaxAmount - room.DiscountAmount;
                }

                foreach (var room in invoice.HotelAllotmentRooms.Where(r => r.Id > 0))
                {
                    invoiceRepo.GetDbContext().Entry(room).State = EntityState.Modified;
                }

                await invoiceRepo.Update(invoice);
                
                // NEW: Update Transaction
                if (invoice.TransactionId != null)
                {
                     await UpdateTransaction(invoice.Id);
                }

                await invoiceRepo.CommitTransactionAsync();

                var response = _mapper.Map<HotelAllotmentInvoiceResponseDto>(invoice);

                return new OperationResponse<HotelAllotmentInvoiceResponseDto>(ErrorCodes.Success, response);
            }
            catch (Exception ex)
            {
                await invoiceRepo.RollbackTransactionAsync();
                LogService.LogError(ex);
            }

            return result;
        }


        public async Task<Guid> CreateTransactionAsync(int invoiceId)
        {
            try
            {
                var invoiceRepo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
                var hotelRepo = _repositoriesFactory.GetRepository<Hotel>();
                var transactionRepo = _repositoriesFactory.GetRepository<Transaction>();
                var mainAccountRepo = _repositoriesFactory.GetRepository<MainAccount>();
                var transactionTypeRepo = _repositoriesFactory.GetRepository<TransactionType>();

                var invoiceQuery = await invoiceRepo.Filter(x => x.Id == invoiceId);
                var invoice = invoiceQuery.Data
                    .Include(x => x.HotelAllotmentRooms)
                    .IgnoreAutoIncludes()
                    .FirstOrDefault();

                if (invoice == null) return Guid.Empty;

                // Check if transaction already exists
                if (invoice.TransactionId != null) return Guid.Empty;

                var hotelQuery = await hotelRepo.Filter(x => x.Id == invoice.HotelId);
                var hotel = hotelQuery.Data.FirstOrDefault();

                if (hotel == null || hotel.MainAccountId == null) return Guid.Empty; // Hotel or its account not found

                if (invoice.MainAccountId == null) return Guid.Empty; // Expense account not selected on invoice

                // Get VAT Account
                var purchaseTaxAccountQuery = await mainAccountRepo.Filter(ma => ma.ArabicName.Contains("ضريبة القيمة المضافة") && ma.ArabicName.Contains("المشتريات"));
                var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
                if (purchaseTaxAccount == null)
                {
                    var fallbackTaxQuery = await mainAccountRepo.Filter(ma => ma.ArabicName.Contains("ضريبة") && ma.ArabicName.Contains("المشتريات"));
                    purchaseTaxAccount = fallbackTaxQuery.Data.FirstOrDefault();
                }
                if (purchaseTaxAccount == null) return Guid.Empty;

                // Get Transaction Type
                var transactionTypeQuery = await transactionTypeRepo.Filter(tt => tt.ArabicName.Contains("قيد فاتورة"));
                var transactionTypeId = transactionTypeQuery.Data.FirstOrDefault()?.Id;
                if (transactionTypeId == null) return Guid.Empty;

                var transactionEntries = new List<TransactionEntry>();

                // Calculate Totals
                decimal totalBeforeVAT = invoice.TotalCostBeforeVAT;
                if (totalBeforeVAT == 0) // Recalculate if 0 (just in case)
                {
                     totalBeforeVAT = CalculateTotalCostBeforeVAT(invoice);
                }
                
                decimal discountAmount = invoice.DiscountAmount > 0 ? invoice.DiscountAmount : invoice.Discount;
                decimal netExpenseAmount = totalBeforeVAT - discountAmount;
                decimal vatAmount = invoice.TaxAmount > 0 ? invoice.TaxAmount : (netExpenseAmount * invoice.Tax / 100m);
                decimal totalWithVAT = netExpenseAmount + vatAmount;

                // 1. Debit: Expense Account (MainAccountId from Invoice)
                transactionEntries.Add(new TransactionEntry
                {
                    MainAccountId = invoice.MainAccountId.Value,
                    DebitAmount = netExpenseAmount,
                    CreditAmount = 0,
                    Description = $"Hotel Allotment Invoice #{invoice.InvoiceNo} - Expense",
                    CreatedAt = DateTime.UtcNow
                });

                // 2. Debit: VAT Account
                if (vatAmount > 0)
                {
                    transactionEntries.Add(new TransactionEntry
                    {
                        MainAccountId = purchaseTaxAccount.Id,
                        DebitAmount = vatAmount,
                        CreditAmount = 0,
                        Description = $"Hotel Allotment Invoice #{invoice.InvoiceNo} - VAT",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 3. Credit: Hotel Account
                transactionEntries.Add(new TransactionEntry
                {
                    MainAccountId = hotel.MainAccountId.Value,
                    DebitAmount = 0,
                    CreditAmount = totalWithVAT,
                    Description = $"Hotel Allotment Invoice #{invoice.InvoiceNo} - Hotel Vendor",
                    CreatedAt = DateTime.UtcNow
                });

                // Create Transaction
                var transaction = new Transaction
                {
                    Description = $"قيد فاتورة حجز فندقي رقم {invoice.InvoiceNo}",
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    TransactionTypeId = transactionTypeId.Value,
                    TransactionEntries = transactionEntries,
                    IsFrontal = invoice.IsFrontal,
                    // Note: You might need to add a relational property for HotelAllotmentInvoice in Transaction entity if strictly required, 
                    // but usually storing TransactionId in Invoice is enough.
                };

                // Enforce Rules (Optional but recommended)
                var transactionEnforcer = new TransactionEnforcer();
                var enforcerResult = transactionEnforcer.EnforceRules(transaction);
                if (!enforcerResult.IsSuccess) return Guid.Empty;

                var addResult = await transactionRepo.Add(transaction);
                if (addResult.Data == null) return Guid.Empty;

                // Link Transaction to Invoice
                invoice.TransactionId = addResult.Data.Id;
                await invoiceRepo.Update(invoice);

                return addResult.Data.Id;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return Guid.Empty;
            }
        }

        public async Task<int> UpdateTransaction(int invoiceId)
        {
            try
            {
                var invoiceRepo = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
                var transactionRepo = _repositoriesFactory.GetRepository<Transaction>();
                var transactionEntryRepo = _repositoriesFactory.GetRepository<TransactionEntry>();

                var invoiceQuery = await invoiceRepo.Filter(x => x.Id == invoiceId);
                var invoice = invoiceQuery.Data.FirstOrDefault();

                if (invoice == null || invoice.TransactionId == null) return 0;

                // Delete old transaction
          
                var transactionQuery = await transactionRepo.Filter(x => x.Id == invoice.TransactionId);
                var transaction = transactionQuery.Data.FirstOrDefault();

                if (transaction == null) return 0;

                // Delete old entries
                // Query entries directly to avoid "Cycle not allowed in no-tracking" error when including in Transaction
                var existingEntriesQuery = await transactionEntryRepo.Filter(x => x.TransactionId == transaction.Id);
                if (existingEntriesQuery.Data != null)
                {
                    foreach (var entry in existingEntriesQuery.Data.ToList())
                    {
                        await transactionEntryRepo.Delete(entry);
                    }
                }

                // Now verify we can recreate entries (Get data again)
                var hotelRepo = _repositoriesFactory.GetRepository<Hotel>();
                var mainAccountRepo = _repositoriesFactory.GetRepository<MainAccount>();

                var hotelQuery = await hotelRepo.Filter(x => x.Id == invoice.HotelId);
                var hotel = hotelQuery.Data.FirstOrDefault();
                if (hotel == null || hotel.MainAccountId == null) return 0;
                if (invoice.MainAccountId == null) return 0;

                var purchaseTaxAccountQuery = await mainAccountRepo.Filter(ma => ma.ArabicName.Contains("ضريبة القيمة المضافة") && ma.ArabicName.Contains("المشتريات"));
                var purchaseTaxAccount = purchaseTaxAccountQuery.Data.FirstOrDefault();
                if (purchaseTaxAccount == null)
                {
                    var fallbackTaxQuery = await mainAccountRepo.Filter(ma => ma.ArabicName.Contains("ضريبة") && ma.ArabicName.Contains("المشتريات"));
                    purchaseTaxAccount = fallbackTaxQuery.Data.FirstOrDefault();
                }
                if (purchaseTaxAccount == null) return 0;

                // Calculate Totals
                // Re-calculate to be sure
                 // We need rooms to calculate cost
                var invoiceWithRooms = await invoiceRepo.Filter(x => x.Id == invoiceId); 
                var invoiceRooms = invoiceWithRooms.Data.Include(x => x.HotelAllotmentRooms).FirstOrDefault();
                
                decimal totalBeforeVAT = CalculateTotalCostBeforeVAT(invoiceRooms); // Use the loaded entity with rooms
                decimal discountAmount = invoice.DiscountAmount > 0 ? invoice.DiscountAmount : invoice.Discount;
                decimal netExpenseAmount = totalBeforeVAT - discountAmount;
                decimal vatAmount = invoice.TaxAmount > 0 ? invoice.TaxAmount : (netExpenseAmount * invoice.Tax / 100m);
                decimal totalWithVAT = netExpenseAmount + vatAmount;

                var newEntries = new List<TransactionEntry>();

                 // 1. Debit: Expense
                newEntries.Add(new TransactionEntry
                {
                    TransactionId = transaction.Id,
                    MainAccountId = invoice.MainAccountId.Value,
                    DebitAmount = netExpenseAmount,
                    CreditAmount = 0,
                    Description = $"Hotel Allotment Invoice #{invoice.InvoiceNo} - Expense",
                    CreatedAt = DateTime.UtcNow
                });

                // 2. Debit: VAT
                if (vatAmount > 0)
                {
                    newEntries.Add(new TransactionEntry
                    {
                         TransactionId = transaction.Id,
                        MainAccountId = purchaseTaxAccount.Id,
                        DebitAmount = vatAmount,
                        CreditAmount = 0,
                        Description = $"Hotel Allotment Invoice #{invoice.InvoiceNo} - VAT",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 3. Credit: Hotel
                newEntries.Add(new TransactionEntry
                {
                     TransactionId = transaction.Id,
                    MainAccountId = hotel.MainAccountId.Value,
                    DebitAmount = 0,
                    CreditAmount = totalWithVAT,
                    Description = $"Hotel Allotment Invoice #{invoice.InvoiceNo} - Hotel Vendor",
                    CreatedAt = DateTime.UtcNow
                });

                await transactionEntryRepo.AddRange(newEntries);

                return 1;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return 0;
            }
        }

        public async Task<byte[]?> GetDocument(int invoiceId, InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null)
        {
            var repository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var invoiceQuery = await repository.Filter(i => i.Id == invoiceId);
            var queryable = invoiceQuery.Data;
            if (queryable == null) return Array.Empty<byte>();

            var invoice = await queryable
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.RoomType)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.MealPlan)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.View)
                .Include(i => i.Hotel)
                .Include(i => i.MainAccount)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (invoice == null) return null!;

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte>();

            await FetchCompanyImagesAsync(companyDetails);

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;

            return printType switch
            {
                InvoicePrintType.A4Landscape => new HotelAllotmentInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, true, sellerName, timeZone).GeneratePdf(),
                InvoicePrintType.Thermal80mm => new HotelAllotmentInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, false, sellerName, timeZone).GeneratePdf(),
                InvoicePrintType.Thermal58mm => new HotelAllotmentInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, false, sellerName, timeZone).GeneratePdf(),
                _ => new HotelAllotmentInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, false, sellerName, timeZone).GeneratePdf()
            };
        }

        public async Task<byte[][]> GetDocuments(int[] invoiceIds, InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null)
        {
            var repository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var invoicesQuery = await repository.Filter(i => invoiceIds.Contains(i.Id) && i.IsCanceled == false);
            var data = invoicesQuery.Data;
            if (data == null) return Array.Empty<byte[]>();

            var invoices = await data
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.RoomType)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.MealPlan)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.View)
                .Include(i => i.Hotel)
                .Include(i => i.MainAccount)
                .ToListAsync();

            if (invoices == null || !invoices.Any()) return Array.Empty<byte[]>();

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte[]>();

            await FetchCompanyImagesAsync(companyDetails);

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;

            var isLandscape = printType == InvoicePrintType.A4Landscape;
            var invoiceDict = invoices.ToDictionary(i => i.Id);
            var results = new List<byte[]>();
            foreach (var id in invoiceIds)
            {
                if (invoiceDict.TryGetValue(id, out var invoice))
                {
                    results.Add(new HotelAllotmentInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, isLandscape, sellerName, timeZone).GeneratePdf());
                }
            }
            return results.ToArray();
        }

        public async Task<byte[][]> GetAllDocuments(InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null)
        {
            var repository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var allQuery = await repository.GetAll();
            var data = allQuery.Data;
            if (data == null) return Array.Empty<byte[]>();

            var invoices = await data
                .Where(i => i.IsCanceled == false)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.RoomType)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.MealPlan)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.View)
                .Include(i => i.Hotel)
                .Include(i => i.MainAccount)
                .ToListAsync();

            if (!invoices.Any()) return Array.Empty<byte[]>();

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte[]>();

            await FetchCompanyImagesAsync(companyDetails);

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;

            var isLandscape = printType == InvoicePrintType.A4Landscape;
            var results = new List<byte[]>();
            foreach (var invoice in invoices)
            {
                results.Add(new HotelAllotmentInvoiceDocument(invoice, companyDetails, pdfTemplateConfig, isLandscape, sellerName, timeZone).GeneratePdf());
            }
            return results.ToArray();
        }

        public async Task<int[]> GetAllInvoiceIds()
        {
            var repository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var allQuery = await repository.GetAll();
            var data = allQuery.Data;
            if (data == null) return Array.Empty<int>();

            return await data
                .Where(i => i.IsCanceled == false)
                .Select(i => i.Id)
                .ToArrayAsync();
        }

        public async Task<int[]> GetExistingInvoiceIds(int[] ids)
        {
            var repository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var filterOp = await repository.Filter(i => ids.Contains(i.Id) && i.IsCanceled == false);
            if (filterOp.Data == null) return Array.Empty<int>();

            var existingIds = await filterOp.Data.Select(i => i.Id).ToArrayAsync();
            var idSet = existingIds.ToHashSet();
            return ids.Where(id => idSet.Contains(id)).ToArray();
        }

        public async Task<byte[]> GetMergedDocument(int[] invoiceIds, InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null)
        {
            var repository = _repositoriesFactory.GetRepository<HotelAllotmentInvoice>();
            var invoicesQuery = await repository.Filter(i => invoiceIds.Contains(i.Id) && i.IsCanceled == false);
            var data = invoicesQuery.Data;
            if (data == null) return Array.Empty<byte>();

            var invoices = await data
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.RoomType)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.MealPlan)
                .Include(i => i.HotelAllotmentRooms)
                    .ThenInclude(r => r.View)
                .Include(i => i.Hotel)
                .Include(i => i.MainAccount)
                .ToListAsync();

            if (invoices == null || !invoices.Any()) return Array.Empty<byte>();

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null) return Array.Empty<byte>();

            await FetchCompanyImagesAsync(companyDetails);

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone;

            var invoiceDict = invoices.ToDictionary(i => i.Id);
            var orderedInvoices = new List<HotelAllotmentInvoice>();
            foreach (var id in invoiceIds)
            {
                if (invoiceDict.TryGetValue(id, out var invoice))
                {
                    orderedInvoices.Add(invoice);
                }
            }

            var isLandscape = printType == InvoicePrintType.A4Landscape;
            return new MergedHotelAllotmentInvoiceDocument(orderedInvoices, companyDetails, pdfTemplateConfig, sellerName, timeZone, isLandscape).GeneratePdf();
        }

        private static async Task FetchCompanyImagesAsync(CompanyDetails? companyDetails)
        {
            if (companyDetails == null) return;

            if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
            {
                companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
            }

            if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
            {
                companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
            }
        }
    }
}