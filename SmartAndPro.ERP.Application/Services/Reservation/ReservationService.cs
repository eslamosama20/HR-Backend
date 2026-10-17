using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Bee.ZatcaHelper;
using Bee.ZatcaHelper.Contracts;
using Bee.ZatcaHelper.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices;
using SmartAndPro.ERP.Application.Services.Reservation.DocunentUtils;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using ReservationPrintTableTemplateEntity = SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class ReservationService
    : ServiceBase
    <
        Domains.Entities.Reservation.Reservation,
        ReservationPostDto,
        ReservationPutDto,
        ReservationResponseDto,
        ReservationFiltersDto
    >

{
    private static readonly IMemoryCache _staticMemoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReservationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory, IHttpContextAccessor httpContextAccessor)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
        _httpContextAccessor = httpContextAccessor;
    }


    public override async Task<OperationResponse<ReservationResponseDto>> AddAsync(ReservationPostDto reservation)
    {
        var result = new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null);
        var reservationRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>(true);

        try
        {
            await reservationRepo.BeginTransactionAsync();

            // =========================
            // 1️⃣ VALIDATION (Allotment) IF HOTEL HAS ALLOTMENT INVOICE
            // =========================
            List<HotelAllotmentRoom> allotmentRooms = new();

            if (reservation.ReservationType == ReservationType.Allotment.ToString())
            {
                var allotmentUsageService = _serviceBaseFactory.GetService<AllotmentUsageService>();

                var invoicesResponse = await _repositoriesFactory
                    .GetRepository<HotelAllotmentInvoice>()
                    .Filter(i => i.HotelId == reservation.HotelId);

                if (invoicesResponse.Data != null && invoicesResponse.Data.Any())
                {
                    // ✅ الفندق عنده فاتورة → اعمل كل حاجة Allotment
                    var invoiceIds = invoicesResponse.Data.Select(i => i.Id).ToList();

                    var roomsResponse = await _repositoriesFactory
                        .GetRepository<HotelAllotmentRoom>()
                        .Filter(r => invoiceIds.Contains(r.HotelAllotmentInvoiceId));

                    if (roomsResponse.Data == null || !roomsResponse.Data.Any())
                        throw new Exception("No allotment rooms found for this hotel");

                    allotmentRooms = roomsResponse.Data.ToList();

                    // التحقق من التوفر لكل entry قبل إضافة الحجز
                    foreach (var entry in reservation.ReservationRoomEntries)
                    {
                        var room = allotmentRooms.FirstOrDefault(r => r.RoomTypeId == entry.RoomTypeId);
                        if (room == null)
                            throw new Exception($"No allotment room found for RoomType {entry.RoomTypeId}");

                        var check = await allotmentUsageService.CheckBookingRequest(
                            room.Id,
                            reservation.ArrivalDate.ToDateTime(TimeOnly.MinValue),
                            reservation.DepartureDate.ToDateTime(TimeOnly.MinValue),
                            entry.NumberOfRooms
                        );

                        if (!check.CanBook && !reservation.ForceBooking)
                        {
                            return new OperationResponse<ReservationResponseDto>
                            {
                                ErrorCode = ErrorCodes.Overbooking,
                                Message = "Overbooking detected",
                                Data = new ReservationResponseDto
                                {
                                    OverbookedDays = check.OverbookedDays
                                }
                            };
                        }
                    }
                }
                // 🚫 الفندق مفيهوش فاتورة → الحجز يتضاف عادي بدون أي Allotment checks
            }

            // =========================
            // 2️⃣ ADD RESERVATION
            // =========================
            result = await base.AddAsync(reservation);
            if (result.Data == null)
                throw new Exception("Failed to add reservation");

            // =========================
            // 3️⃣ FRONTAL
            // =========================
            if (result.Data.IsFrontal)
            {
                var makeFrontalResult = await MakeFrontalAsync(result.Data.Id);
                if (makeFrontalResult.ErrorCode == ErrorCodes.Success)
                    result.Data.ReferenceNumber = makeFrontalResult.Data.ReferenceNumber;
                else
                    throw new Exception("Failed to make frontal reservation");
            }

            // =========================
            // 4️⃣ ADD ALLOTMENT USAGE IF HOTEL HAS ALLOTMENT INVOICE
            // =========================
            if (reservation.ReservationType == ReservationType.Allotment.ToString() &&
                allotmentRooms.Any()) // Only if hotel has allotment rooms
            {
                await ProcessAllotmentUsagesAsync(reservation, result.Data.Id, allotmentRooms);
            }

            await LogActionAsync(result.Data.Id, "Created");

            await reservationRepo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            await reservationRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            throw;
        }
    }

    private async Task ProcessAllotmentUsagesAsync(ReservationPostDto reservation, Guid reservationId, List<HotelAllotmentRoom> allotmentRooms)
    {
        var usageRepo = _repositoriesFactory.GetRepository<AllotmentUsage>(true);
        var usagesToAdd = new List<AllotmentUsage>();

        foreach (var entry in reservation.ReservationRoomEntries)
        {
            var room = allotmentRooms.FirstOrDefault(r => r.RoomTypeId == entry.RoomTypeId);
            if (room == null) continue;

            var startDate = reservation.ArrivalDate.ToDateTime(TimeOnly.MinValue);
            var endDate = reservation.DepartureDate.ToDateTime(TimeOnly.MinValue);

            var existingUsagesOperation = await usageRepo.Filter(u => 
                u.HotelAllotmentRoomId == room.Id && 
                u.UsageDate >= startDate && u.UsageDate <= endDate);
            
            var existingUsages = existingUsagesOperation.Data?.ToList() ?? new List<AllotmentUsage>();

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var usage = existingUsages.FirstOrDefault(u => u.UsageDate == date);

                int totalRoomsForDay = room.RoomsPerDay.TryGetValue(date.DayOfWeek, out var r) ? r : 0;
                int requestedRooms = entry.NumberOfRooms;

                if (usage != null)
                {
                    int currentTotal = usage.ConsumedRooms + usage.OverbookedRooms + requestedRooms;
                    usage.OverbookedRooms = Math.Max(0, currentTotal - totalRoomsForDay);
                    usage.ConsumedRooms = Math.Min(totalRoomsForDay, currentTotal);
                    await usageRepo.Update(usage);
                }
                else
                {
                    int overbooked = Math.Max(0, requestedRooms - totalRoomsForDay);
                    int consumed = Math.Min(requestedRooms, totalRoomsForDay);

                    usagesToAdd.Add(new AllotmentUsage
                    {
                        HotelAllotmentRoomId = room.Id,
                        ReservationId = reservationId,
                        UsageDate = date,
                        ConsumedRooms = consumed,
                        OverbookedRooms = overbooked,
                        PricePerRoom = entry.RoomCost ?? 0m,
                        Discount = room.Discount
                    });
                }
            }
        }

        if (usagesToAdd.Any())
            await usageRepo.AddRange(usagesToAdd);
    }


    public override async Task<OperationResponse<PaginatedResult<List<ReservationResponseDto>>>> GetAllPaginatedAsync(
        ReservationFiltersDto filters,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        return await GetAllPaginatedAsync(filters, pageNumber, pageSize, null, null);
    }

    public async Task<OperationResponse<PaginatedResult<List<ReservationResponseDto>>>> GetAllPaginatedAsync(
        ReservationFiltersDto filters,
        int? pageNumber,
        int? pageSize,
        string? searchField,
        string? searchValue)
    {
        try
        {
            //// 1️⃣ جلب بيانات الشركة
            //var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            //var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
            //if (companyDetails == null)
            //    throw new Exception("Company Details not found");

            //// 2️⃣ جلب إعدادات النظام (timezone)
            //var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            //var systemSetting = await systemSettingQuery.Data.FirstOrDefaultAsync();
            //var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";

            // 3️⃣ جلب البيانات من الـ repository مع الفلترة
            var baseRepository = _repositoriesFactory.GetRepository<ERP.Domains.Entities.Reservation.Reservation>();
            var allQuery = await baseRepository.GetAll();

            if (allQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaginatedResult<List<ReservationResponseDto>>>(allQuery.ErrorCode, null);

            // 4️⃣ تطبيق الـ filters الخاصة وتجريد الاستعلام لتسريع الأداء
            var cleanQuery = allQuery.Data
                .IgnoreAutoIncludes()
                // .Include(x => x.ReservationClientCompany) // Commented out to prevent unnecessary 3.8M row JOIN. Added only in search switch case.
                .AsNoTracking();

            var filteredQuery = filters.AllPropertiesAreNull()
                ? cleanQuery
                : cleanQuery.FilterQueryableData(filters);

            if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(searchValue))
            {
                var val = searchValue.Trim().ToLower();
                switch (searchField.ToLower())
                {
                    case "clientcompanyname":
                        filteredQuery = filteredQuery.Include(x => x.ReservationClientCompany).Where(x => 
                            x.ReservationClientCompany != null && 
                            (x.ReservationClientCompany.ArabicName.ToLower().Contains(val) || 
                             x.ReservationClientCompany.EnglishName.ToLower().Contains(val)));
                        break;
                    case "guestname":
                        filteredQuery = filteredQuery.Where(x => x.GuestName.ToLower().Contains(val));
                        break;
                    case "referencenumber":
                        filteredQuery = filteredQuery.Where(x => x.ReferenceNumber.ToLower().Contains(val));
                        break;
                    case "numberofnights":
                        if (int.TryParse(val, out var nights))
                        {
                            filteredQuery = filteredQuery.Where(x => x.NumberOfNights == nights);
                        }
                        break;
                    case "reservationtype":
                        filteredQuery = filteredQuery.Where(x => x.ReservationType.ToString().ToLower().Contains(val));
                        break;
                    case "arrivaldate":
                        if (DateOnly.TryParse(val, out var arrDate))
                        {
                            filteredQuery = filteredQuery.Where(x => x.ArrivalDate == arrDate);
                        }
                        break;
                    case "departuredate":
                        if (DateOnly.TryParse(val, out var depDate))
                        {
                            filteredQuery = filteredQuery.Where(x => x.DepartureDate == depDate);
                        }
                        break;
                    case "all":
                    default:
                        int nightsAll;
                        bool hasNights = int.TryParse(val, out nightsAll);
                        DateOnly dateAll;
                        bool hasDate = DateOnly.TryParse(val, out dateAll);

                        filteredQuery = filteredQuery.Where(x => 
                            x.ReferenceNumber.ToLower().Contains(val) ||
                            x.GuestName.ToLower().Contains(val) ||
                            x.ReservationType.ToString().ToLower().Contains(val) ||
                            (x.ReservationClientCompany != null && 
                             (x.ReservationClientCompany.ArabicName.ToLower().Contains(val) || 
                              x.ReservationClientCompany.EnglishName.ToLower().Contains(val))) ||
                            (hasNights && x.NumberOfNights == nightsAll) ||
                            (hasDate && (x.ArrivalDate == dateAll || x.DepartureDate == dateAll))
                        );
                        break;
                }
            }

            // 5️⃣ عدّ عدد السجلات بحد أقصى لتفادي الانهيار مع البيانات الضخمة
            // Optimized totalRecords with MemoryCache
            var totalRecords = await GetTotalRecordsCountAsync(cleanQuery, filteredQuery, filters, searchField, searchValue);

            // 6️⃣ تعيين pagination افتراضي لو مش موجود
            pageNumber ??= 1;
            //pageSize ??= totalRecords > 0 ? totalRecords : 1; // ❌ Memory Bomb (Commented out)
            pageSize ??= 50; // ✅ Safe default
            if (pageSize > 100) pageSize = 100; // ✅ Safety Cap
            if (totalRecords == 0)
                return new OperationResponse<PaginatedResult<List<ReservationResponseDto>>>(
                    ErrorCodes.Success,
                    new PaginatedResult<List<ReservationResponseDto>>(0, new List<ReservationResponseDto>())
                );


            var skip = (pageNumber.Value - 1) * pageSize.Value;

            // 7️⃣ تطبيق pagination واختيار الحقول المطلوبة فقط
            // Fetch users for creator name resolution
            //var usersQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Auth.Security.User>().GetAll();

            var paginatedData = await filteredQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(pageSize.Value)
                .AsNoTracking()
                .Select(x => new ReservationResponseDto
                {
                    Id = x.Id,
                    GuestName = x.GuestName,
                    ReferenceNumber = x.ReferenceNumber,
                    NumberOfNights = x.NumberOfNights,
                    ReservationType = x.ReservationType.ToString(),
                    IsConfirmed = x.IsConfirmed,
                    IsCancelled = x.IsCancelled,
                    IsFrontal = x.IsFrontal,
                    ArrivalDate = x.ArrivalDate,
                    DepartureDate = x.DepartureDate,
                    SaleInvoiceId = x.SaleInvoiceId,
                    //Nationality = x.Nationality,
                    //HotelRemarks = x.HotelRemarks,
                    //ClientRemarks = x.ClientRemarks,
                    //CreatedByName = usersQuery.Data
                    //    .Where(u => u.Id == x.CreatedBy)
                    //    .Select(u => u.FirstName ?? u.UserName)
                    //    .FirstOrDefault(),
                    CreatedByName = null,
                    ReservationCompanyId = x.ReservationCompanyId,
                    ReservationCompanyArabicName = x.ReservationCompany != null ? x.ReservationCompany.ArabicName : null,
                    ReservationCompanyEnglishName = x.ReservationCompany != null ? x.ReservationCompany.EnglishName : null,
                    ReservationClientCompanyId = x.ReservationClientCompanyId,
                    ReservationClientCompanyArabicName = x.ReservationClientCompany != null ? x.ReservationClientCompany.ArabicName : null,
                    ReservationClientCompanyEnglishName = x.ReservationClientCompany != null ? x.ReservationClientCompany.EnglishName : null,
                    //HotelId = x.HotelId,
                    //ReservationSeasonId = x.ReservationSeasonId,
                    //ReservationRoomEntries = x.ReservationRoomEntries.Select(re => new ReservationRoomEntryResponseDto
                    //{
                    //    //Id = re.Id,
                    //    //RoomTypeId = re.RoomTypeId,
                    //    //RoomType = new RoomTypeResponseDto
                    //    //{
                    //    //    // Map RoomType properties as needed
                    //    //    Id = re.RoomType.Id,
                    //    //    ArabicName = re.RoomType.ArabicName,
                    //    //    EnglishName = re.RoomType.EnglishName
                    //    //    // ... other properties
                    //    //},
                    //    //NumberOfRooms = re.NumberOfRooms,
                    //    //RoomRate = re.RoomRate,
                    //    //RoomCost = re.RoomCost,
                    //    //ReservationMealId = re.ReservationMealId,
                    //    //MealRate = re.MealRate,
                    //    //MealCost = re.MealCost,
                    //    //ReservationViewId = re.ReservationViewId,
                    //    //ViewRate = re.ViewRate,
                    //    //ViewCost = re.ViewCost,
                    //    //ReservationId = re.ReservationId,
                    //    //CreatedAt = re.CreatedAt
                    //}).ToList(),
                    //ReservationServiceEntries = x.ReservationServiceEntries.Select(se => new ReservationServiceEntryResponseDto
                    //{
                    //    Id = se.Id,
                    //    ReservationServiceId = se.ReservationServiceId,
                    //    Quantity = se.Quantity,
                    //    SalesPrice = se.SalesPrice,
                    //    CostPrice = se.CostPrice,
                    //    ReservationSupplierId = se.ReservationSupplierId,
                    //    ReservationId = se.ReservationId,
                    //}).ToList()

                })
                .ToListAsync();

            // 8️⃣ تحويل النتائج إلى DTO باستخدام AutoMapper
            //var mappedData = _mapper.Map<List<ReservationResponseDto>>(paginatedData);

            // 9️⃣ إنشاء PaginatedResult
            var result = new PaginatedResult<List<ReservationResponseDto>>(totalRecords, paginatedData);

            // 10️⃣ إرجاع النتيجة
            return new OperationResponse<PaginatedResult<List<ReservationResponseDto>>>(ErrorCodes.Success, result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaginatedResult<List<ReservationResponseDto>>>(
                ErrorCodes.GeneralFail,
                null,
                message: ex.Message
            );
        }
    }


    private async Task<int> GetTotalRecordsCountAsync(
        IQueryable<Domains.Entities.Reservation.Reservation> cleanQuery,
        IQueryable<Domains.Entities.Reservation.Reservation> filteredQuery,
        ReservationFiltersDto filters,
        string? searchField,
        string? searchValue)
    {
        bool hasSearch = !string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(searchValue);
        bool hasFilters = !filters.AllPropertiesAreNull() || hasSearch;

        if (!hasFilters)
        {
            // استخدام إحصائيات SQL Server الداخلية بدلاً من CountAsync
            // هذا يرجع العدد في أقل من 0.001 ثانية بغض النظر عن حجم الجدول
            // لا يحتاج Migration لأنه يقرأ من sys.dm_db_partition_stats الموجودة في كل SQL Server
            try
            {
                var db = _repositoriesFactory.GetDbContext();
                var result = await db.Database
                    .SqlQueryRaw<int>(
                        "SELECT ISNULL(CAST(SUM(row_count) AS INT), 0) AS Value FROM sys.dm_db_partition_stats WHERE object_id = OBJECT_ID('Reservation') AND index_id IN (0, 1)")
                    .FirstOrDefaultAsync();
                return result;
            }
            catch
            {
                // Fallback: لو فشل لأي سبب نرجع للطريقة الأصلية مع الكاش
                if (!_staticMemoryCache.TryGetValue("ReservationsTotalCount", out int cachedTotal))
                {
                    cachedTotal = await cleanQuery.CountAsync();
                    _staticMemoryCache.Set("ReservationsTotalCount", cachedTotal, TimeSpan.FromMinutes(60));
                }
                return cachedTotal;
            }
        }

        // في حالة البحث: حد أقصى 10,000 لحماية الداتابيز من عمليات LIKE الثقيلة
        return await filteredQuery.Take(10000).CountAsync();
    }

    public async Task<OperationResponse<PaginatedResult<List<ReservationLookupResponseDto>>>> GetLookupAsync(
        ReservationFiltersDto filters,
        int pageNumber = 1,
        int pageSize = 50,
        string? sortField = null)
    {
        try
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 100);

            var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().GetAll();
            if (reservationQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaginatedResult<List<ReservationLookupResponseDto>>>(reservationQuery.ErrorCode, null);

            var query = reservationQuery.Data
                .IgnoreAutoIncludes()
                .AsNoTracking();

            if (!filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }

            var totalRecords = await GetTotalRecordsCountAsync(query, query, filters, null, null);
            if (totalRecords == 0)
            {
                return new OperationResponse<PaginatedResult<List<ReservationLookupResponseDto>>>(
                    ErrorCodes.Success,
                    new PaginatedResult<List<ReservationLookupResponseDto>>(0, new List<ReservationLookupResponseDto>())
                );
            }

            query = (sortField ?? string.Empty).ToLowerInvariant() switch
            {
                "referencenumber" => query.OrderBy(x => x.ReferenceNumber),
                "guestname" => query.OrderBy(x => x.GuestName),
                "arrivaldate" => query.OrderBy(x => x.ArrivalDate),
                "departuredate" => query.OrderBy(x => x.DepartureDate),
                "createdat" => query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var skip = (pageNumber - 1) * pageSize;
            var data = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new ReservationLookupResponseDto
                {
                    Id = x.Id,
                    ReferenceNumber = x.ReferenceNumber,
                    GuestName = x.GuestName,
                    ArrivalDate = x.ArrivalDate,
                    DepartureDate = x.DepartureDate,
                    ReservationType = x.ReservationType.ToString(),
                    SaleInvoiceId = x.SaleInvoiceId,
                    IsConfirmed = x.IsConfirmed,
                    IsCancelled = x.IsCancelled,
                    IsFrontal = x.IsFrontal,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return new OperationResponse<PaginatedResult<List<ReservationLookupResponseDto>>>(
                ErrorCodes.Success,
                new PaginatedResult<List<ReservationLookupResponseDto>>(totalRecords, data)
            );
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaginatedResult<List<ReservationLookupResponseDto>>>(
                ErrorCodes.GeneralFail,
                null,
                message: ex.Message
            );
        }
    }


    public async Task<OperationResponse<object>> GetAllFrontalAsync(
    ReservationFiltersDto filters,
    int? pageNumber = null,
    int? pageSize = null)
    {
        try
        {
            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = await companyDetailsQuery.Data.FirstOrDefaultAsync();
            if (companyDetails == null)
                return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: "Company Details not found");

            var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
            var systemSetting = systemSettingQuery.Data.FirstOrDefault();
            var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";

            filters.IsFrontal = true;

            var baseResult = await base.GetAllPaginatedAsync(filters, pageNumber, pageSize);

            if (baseResult.ErrorCode != ErrorCodes.Success || baseResult.Data == null)
                return new OperationResponse<object>(baseResult.ErrorCode, null, message: baseResult.Message);

            if (!pageNumber.HasValue || !pageSize.HasValue)
            {
                return new OperationResponse<object>(ErrorCodes.Success, baseResult.Data.Data);
            }

            return new OperationResponse<object>(ErrorCodes.Success, baseResult.Data);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<object>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

   

    //    return result;
    //}

    // ✅ getById - Custom Query (Fast & Complete)
    public override async Task<OperationResponse<ReservationResponseDto>> GetAsync(Guid id)
    {
        try
        {
            var query = await _repositoriesFactory
                .GetRepository<ERP.Domains.Entities.Reservation.Reservation>()
                .Filter(x => x.Id == id);

            if (query.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<ReservationResponseDto>(query.ErrorCode, null);

            var entity = await query.Data
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Include(x => x.Hotel)
                .Include(x => x.ReservationCompany)
                .Include(x => x.ReservationClientCompany)
                .Include(x => x.SalesRepresentative)
                .Include(x => x.ReservationSeason)
                .Include(x => x.BankAccount)
                .Include(x => x.CompanyPhone)
                .Include(x => x.CompanyPolicy)
                .Include(x => x.ReservationRoomEntries)
                    .ThenInclude(re => re.RoomType)
                .Include(x => x.ReservationRoomEntries)
                    .ThenInclude(re => re.ReservationMeal)
                .Include(x => x.ReservationRoomEntries)
                    .ThenInclude(re => re.ReservationView)
                .Include(x => x.ReservationServiceEntries)
                    .ThenInclude(se => se.ReservationService)
                .Include(x => x.ReservationServiceEntries)
                    .ThenInclude(se => se.ReservationSupplier)
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync();

            if (entity is null)
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.NotFound, null);

            var dto = _mapper.Map<ReservationResponseDto>(entity);
            return new OperationResponse<ReservationResponseDto>(ErrorCodes.Success, dto);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    // ✅ GetByIds - جلب حجوزات متعددة بـ query واحد (مُحسَّن للأداء)
    public async Task<List<object>> GetByIdsAsync(List<Guid> ids)
    {
        var query = await _repositoriesFactory
            .GetRepository<ERP.Domains.Entities.Reservation.Reservation>()
            .Filter(x => ids.Contains(x.Id));

        var data = await query.Data
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Select(x => new
            {
                id = x.Id,
                guestName = x.GuestName,
                referenceNumber = x.ReferenceNumber,
                reservationType = x.ReservationType.ToString(),
                reservationCompanyId = x.ReservationCompanyId,
                reservationClientCompanyId = x.ReservationClientCompanyId,
                hotelId = x.HotelId,
                arrivalDate = x.ArrivalDate,
                departureDate = x.DepartureDate,
                saleInvoiceId = x.SaleInvoiceId
            })
            .ToListAsync();

        return data.Cast<object>().ToList();
    }


    public static decimal GetTotalAmount(ReservationResponseDto reservation)
    {
        var netAmount = GetNetAmount(reservation);
        var taxAmount = GetTaxAmount(reservation);
        return netAmount + taxAmount;
    }
    public static decimal GetNetAmount(ReservationResponseDto reservation)
    {
        var reservationRoomEntriesSalesTotal =
            reservation.ReservationRoomEntries
                .Sum(re =>
                    (re.NumberOfRooms * reservation.NumberOfNights * re.RoomRate)
                    + ((re.RoomType.PersonNo * re.NumberOfRooms) * reservation.NumberOfNights * re.MealRate ?? 0)
                    + (re.NumberOfRooms * reservation.NumberOfNights * re.ViewRate ?? 0)
                );
        var reservationServiceEntriesSalesTotal =
            reservation?.ReservationServiceEntries
                .Sum(re =>
                    (re.Quantity * re.SalesPrice)
                );
        return reservationRoomEntriesSalesTotal + (reservationServiceEntriesSalesTotal ?? 0m);
    }
    public static decimal GetTaxAmount(ReservationResponseDto reservation)
    {
        return GetNetAmount(reservation) * 0.15m;
    }

    #region SendReservationToZatca
    //public async Task SendReservationToZatca(Guid reservationId)
    //{
    //    var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
    //    var companyDetails = companyDetailsQuery.Data.FirstOrDefault();
    //    if (companyDetails == null) throw new Exception("Company Details not found");

    //    var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
    //    var systemSetting = systemSettingQuery.Data.FirstOrDefault();

    //    var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => r.Id == reservationId);
    //    var reservation = reservationQuery.Data
    //        .IgnoreAutoIncludes()
    //        .Include(r => r.ReservationClientCompany)
    //        .Include(r => r.ReservationRoomEntries)
    //        .ThenInclude(re => re.RoomType)
    //        .Include(r => r.ReservationRoomEntries)
    //        .ThenInclude(re => re.ReservationMeal)
    //        .Include(r => r.ReservationRoomEntries)
    //        .ThenInclude(re => re.ReservationView)
    //        .Include(r => r.ReservationServiceEntries)
    //        .ThenInclude(se => se.ReservationService)
    //        .FirstOrDefault();
    //    if (reservation == null) throw new Exception("Reservation not found");

    //    var customer = reservation.ReservationClientCompany;
    //    if (customer == null) throw new Exception("ReservationClientCompany not found");

    //    // Combine room and service entries as items
    //    var items = new List<dynamic>();
    //    foreach (var room in reservation.ReservationRoomEntries)
    //    {
    //        items.Add(new
    //        {
    //            Name = "Room: " + room.RoomType.ArabicName,
    //            Quantity = room.NumberOfRooms * reservation.NumberOfNights,
    //            Price = room.RoomRate,
    //            IsTaxExempted = false // Adjust if needed
    //        });

    //        if (room.MealRate != null && room.MealRate > 0)
    //        {
    //            items.Add(new
    //            {
    //                Name = "Meal: " + (room.ReservationMeal.ArabicName ?? "Meal"),
    //                Quantity = (room.RoomType.PersonNo * room.NumberOfRooms) * reservation.NumberOfNights,
    //                Price = room.MealRate,
    //                IsTaxExempted = false // Adjust if needed
    //            });
    //        }
    //        if (room.ViewRate != null && room.ViewRate > 0)
    //        {
    //            items.Add(new
    //            {
    //                Name = "View: " + (room.ReservationView.ArabicName ?? "View"),
    //                Quantity = room.NumberOfRooms * reservation.NumberOfNights,
    //                Price = room.ViewRate,
    //                IsTaxExempted = false // Adjust if needed
    //            });
    //        }
    //    }
    //    foreach (var service in reservation.ReservationServiceEntries)
    //    {
    //        items.Add(new
    //        {
    //            Name = "Service: " + service.ReservationService?.ArabicName,
    //            Quantity = service.Quantity,
    //            Price = service.SalesPrice,
    //            IsTaxExempted = false // Adjust if needed
    //        });
    //    }

    //    // Calculate totals
    //    // Cast Price and Quantity to decimal explicitly
    //    var lineAmounts = items.Select(i => Math.Round((decimal)i.Price * (decimal)i.Quantity, 2)).ToList();

    //    decimal sumOfLines = lineAmounts.Sum();

    //    decimal discount = 0;

    //    var lineDiscounts = lineAmounts.Select(lineNet =>
    //        sumOfLines > 0 ? Math.Round(lineNet / (decimal)sumOfLines * (decimal)discount, 2) : 0m).ToList();

    //    var netLineAmounts = lineAmounts.Select((lineNet, idx) =>
    //        Math.Round((decimal)lineNet - (decimal)lineDiscounts[idx], 2)).ToList();

    //    var lineTaxes = items.Select((item, idx) =>
    //        Math.Round((decimal)netLineAmounts[idx] * (item.IsTaxExempted ? 0m : 0.15m), 2)).ToList();

    //    decimal taxableBase = Math.Round((decimal)
    //        items.Where(i => !i.IsTaxExempted)
    //            .Sum(i => (decimal)i.Price * (decimal)i.Quantity), 2);

    //    decimal taxAmountValue = lineTaxes.Sum();

    //    decimal netLinesTotal = netLineAmounts.Sum();


    //    decimal inclusiveAmount = Math.Round((decimal)netLinesTotal + (decimal)taxAmountValue, 2);

    //    var issueDate = TimeZoneUtils.ApplyTimeZone(systemSetting?.TimeZone, DateTime.UtcNow);

    //    var standardInvoice = new StandardInvoice
    //    {
    //        Id = reservation.ReferenceNumber,
    //        UUID = reservation.Id.ToString(),
    //        IssuedDate = issueDate.ToString("yyyy-MM-dd"),
    //        IssuedTime = issueDate.ToString("HH:mm:ssZ"),
    //        InvoiceTypeCode = "388",
    //        DocumentCurrencyCode = "SAR",
    //        TaxCurrencyCode = "SAR",
    //        ActualDeliveryDate = issueDate.ToString("yyyy-MM-dd"),
    //        LatestDeliveryDate = issueDate.ToString("yyyy-MM-dd"),
    //        SupplierInfo = new SupplierInfo
    //        {
    //            RegistrationName = companyDetails.ArabicName,
    //            PartyPostalAddress = new PartyPostalAddress
    //            {
    //                StreetName = companyDetails.ArabicAddress?.Street,
    //                CityName = companyDetails.ArabicAddress?.City,
    //                Country = "SA",
    //                PostalZone = companyDetails.ArabicAddress?.ZipCode
    //            },
    //            CRN = companyDetails.TaxNumber,
    //            VAT = _appSettings.ZatcaParams.IsSandbox
    //                ? "399999999900003"
    //                : companyDetails.TaxNumber
    //        },
    //        CustomerInfo = new CustomerInfo
    //        {
    //            RegistrationName = customer.ArabicName,
    //            VAT = customer.TaxNumber,
    //        },
    //        TaxTotal = new TaxTotal
    //        {
    //            TaxableAmount = new Money("SAR", (double)taxableBase),
    //            TaxAmount = new Money("SAR", (double)taxAmountValue),
    //            Percent = "15.00",
    //            TaxCategory = "S",
    //        },
    //        LegalMonetaryTotal = new LegalMonetaryTotal
    //        {
    //            LineExtensionAmount = new Money("SAR", (double)sumOfLines),
    //            TaxExclusiveAmount = new Money("SAR", (double)netLinesTotal),
    //            TaxInclusiveAmount = new Money("SAR", (double)inclusiveAmount),
    //            PayableAmount = new Money("SAR", (double)inclusiveAmount),
    //            AllowanceTotalAmount = new Money("SAR", (double)discount),
    //            PrepaidAmount = new Money("SAR", 0),
    //        },
    //        InvoiceLineItems = items.Select((item, idx) => new InvoiceLineItem
    //        {
    //            ID = (idx + 1).ToString(),
    //            InvoicedQuantity = Math.Round((decimal)item.Quantity, 2).ToString(),
    //            LineExtensionAmount = new Money("SAR", (double)lineAmounts[idx]),
    //            TaxAmount = new Money("SAR", (double)lineTaxes[idx]),
    //            RoundingAmount = new Money("SAR", 0),
    //            TaxSchemeId = item.IsTaxExempted ? "E" : "S",
    //            TaxScheme = "VAT",
    //            TaxPercent = item.IsTaxExempted ? "0.00" : "15.00",
    //            ItemName = item.Name,
    //            PriceAmount = new Money("SAR", (double)Math.Round((decimal)item.Price, 2)),
    //        }).ToList(),
    //        DocumentLevelDiscountAmount = discount,
    //        DocumentLevelDiscountReason = "Invoice-level discount",
    //    };

    //    if (standardInvoice.CustomerInfo.VAT != null)
    //    {
    //        var standardInvoiceXml = StandardInvoiceXmlGenerator.Generate(standardInvoice, companyDetails.PrivateKey, companyDetails.ProdBinarySecurityToken);
    //        var clearanceResponse = new StandardInvoiceClearanceApiClient(
    //            new GlobalVariables
    //            {
    //                BaseUrl = _appSettings.ZatcaParams.IsSandbox ? _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
    //                InvoiceClearanceEndPoint = _appSettings.ZatcaParams.IsSandbox ? _appSettings.ZatcaParams.SandboxUrls.InvoiceClearanceEndPoint : _appSettings.ZatcaParams.ProdUrls.InvoiceClearanceEndPoint
    //            }).ClearInvoice(new InvoiceClearanceRequest(standardInvoiceXml, companyDetails.ProdBinarySecurityToken, companyDetails.ProdSecret));
    //        if (clearanceResponse.ClearanceStatus != "CLEARED") throw new Exception("Clearance failed");
    //        reservation.IsSentToZatca = true;
    //        reservation.InvoiceHash = clearanceResponse.Hash;
    //        reservation.GeneratedQR = clearanceResponse.GeneratedQR;
    //        reservation.Attachments = null;
    //        reservation.ReservationRoomEntries = null;
    //        reservation.ReservationClientCompany = null;
    //        reservation.ReservationServiceEntries = null;
    //        await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Update(reservation);
    //    }
    //    else
    //    {
    //        var simplifiedInvoiceXml = StandardInvoiceXmlGenerator.GenerateSimplifiedInvoice(standardInvoice, companyDetails.PrivateKey, companyDetails.ProdBinarySecurityToken);
    //        var reportingClient = new SimplifiedInvoiceReportingApiClient(new GlobalVariables
    //        {
    //            BaseUrl = _appSettings.ZatcaParams.IsSandbox ? _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
    //            InvoiceReportingEndPoint = _appSettings.ZatcaParams.IsSandbox ? _appSettings.ZatcaParams.SandboxUrls.InvoiceReportingEndPoint : _appSettings.ZatcaParams.ProdUrls.InvoiceReportingEndPoint
    //        });
    //        /*var invoiceValidator = new Zatca.EInvoice.SDK.EInvoiceValidator();
    //        var pemCertificateBase64 = Convert.FromBase64String(companyDetails.ProdBinarySecurityToken);
    //        var cert = Encoding.UTF8.GetString(pemCertificateBase64);
    //        var result = invoiceValidator
    //            .ValidateEInvoice(simplifiedInvoiceXml.invoiceXml, cert, companyDetails.Secret);*/
    //        var reportingResponse = reportingClient.ReportSimplifiedInvoice(new InvoiceClearanceRequest(simplifiedInvoiceXml, companyDetails.ProdBinarySecurityToken, companyDetails.ProdSecret));
    //        if (reportingResponse.ReportingStatus != "REPORTED") throw new Exception("Reporting failed");
    //        reservation.IsSentToZatca = true;
    //        reservation.InvoiceHash = simplifiedInvoiceXml.invoiceHash;
    //        reservation.GeneratedQR = simplifiedInvoiceXml.GeneratedQR;
    //        reservation.Attachments = null;
    //        reservation.ReservationRoomEntries = null;
    //        reservation.ReservationClientCompany = null;
    //        reservation.ReservationServiceEntries = null;
    //        await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Update(reservation);
    //    }

    //    if (reservation.TransactionId == null)
    //    {
    //        await CreateTransactionForReservationAsync(reservation.Id);
    //    }
    //}
    #endregion

    #region CreateTransactionForReservationAsync
    //public async Task<OperationResponse<ReservationResponseDto>> CreateTransactionForReservationAsync(Guid reservationId)
    //{
    //    var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>(true);
    //    var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);
    //    var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>(true);

    //    try
    //    {
    //        await reservationRepository.BeginTransactionAsync();

    //        var reservationQuery = await reservationRepository.Filter(r => r.Id == reservationId);
    //        var reservation = reservationQuery.Data
    //            .Include(r => r.Hotel)
    //            .Include(r => r.ReservationCompany)
    //            .Include(r => r.ReservationSeason)
    //            .Include(r => r.SalesRepresentative)
    //            .Include(r => r.ReservationClientCompany)
    //            .FirstOrDefault();
    //        if (reservation == null)
    //        {
    //            return new OperationResponse<ReservationResponseDto>(ErrorCodes.NotFound, null);
    //        }


    //        // Fetch the transaction type
    //        var transactionTypeQuery = await _repositoriesFactory.GetRepository<TransactionType>()
    //            .Filter(tt => tt.ArabicName == "قيد حجز");
    //        var transactionType = transactionTypeQuery.Data.FirstOrDefault();
    //        if (transactionType == null)
    //        {
    //            throw new Exception("Transaction Type not found");
    //        }

    //        var transaction = new Transaction
    //        {
    //            TransactionTypeId = transactionType.Id,
    //            Description = $"{reservation.ReservationType.ToString()}",
    //            TransactionDate = reservation.ArrivalDate,
    //            TransactionEntries = new List<TransactionEntry>(),
    //            ReservationId = reservation.Id
    //        };

    //        var reservationRoomEntriesSalesTotal =
    //            reservation.ReservationRoomEntries
    //                .Sum(re =>
    //                    (re.NumberOfRooms * reservation.NumberOfNights * re.RoomRate)
    //                    + ((re.RoomType.PersonNo * re.NumberOfRooms) * reservation.NumberOfNights * re.MealRate ?? 0)
    //                    + (re.NumberOfRooms * reservation.NumberOfNights * re.ViewRate ?? 0)
    //                );

    //        var reservationRoomEntriesCostTotal =
    //            reservation.ReservationRoomEntries
    //                .Sum(re =>
    //                    (re.NumberOfRooms * reservation.NumberOfNights * re.RoomCost)
    //                    + ((re.RoomType.PersonNo * re.NumberOfRooms) * reservation.NumberOfNights * re.MealCost ?? 0)
    //                    + (re.NumberOfRooms * reservation.NumberOfNights * re.ViewCost ?? 0)
    //                );

    //        var reservationServiceEntriesSalesTotal =
    //            reservation.ReservationServiceEntries
    //                .Sum(re =>
    //                    (re.Quantity * re.SalesPrice)
    //                );

    //        var reservationServiceEntriesCostTotal =
    //            reservation.ReservationServiceEntries
    //                .Sum(re =>
    //                    (re.Quantity * re.CostPrice)
    //                );

    //        var totalSalesPrice = reservationRoomEntriesSalesTotal + reservationServiceEntriesSalesTotal;
    //        var totalCostPrice = reservationRoomEntriesCostTotal + reservationServiceEntriesCostTotal;

    //        if (reservation.ReservationClientCompany.MainAccountId == null)
    //        {
    //            throw new Exception("Client company account not found");
    //        }

    //        // client company account
    //        transaction.TransactionEntries.Add(new TransactionEntry
    //        {
    //            CreditAmount = 0,
    //            DebitAmount = totalSalesPrice + (totalSalesPrice * (decimal)0.15),
    //            MainAccountId = (Guid)reservation.ReservationClientCompany.MainAccountId,
    //            Description = "Client Company Account",
    //        });

    //        // Season Room Revenue Entry
    //        var roomRevenue = reservation.ReservationRoomEntries
    //            .Sum(re => re.NumberOfRooms * re.RoomRate * reservation.NumberOfNights);
    //        if (reservation.ReservationSeason.RoomRevenueMainAccountId == null)
    //        {
    //            throw new Exception("Room Revenue account not found");
    //        }
    //        transaction.TransactionEntries.Add(new TransactionEntry
    //        {
    //            CreditAmount = roomRevenue,
    //            DebitAmount = 0,
    //            MainAccountId = (Guid)reservation.ReservationSeason.RoomRevenueMainAccountId,
    //            Description = "",
    //        });

    //        // Season Room Expense Entry
    //        var roomExpense = reservation.ReservationRoomEntries
    //            .Sum(re => re.NumberOfRooms * re.RoomCost * reservation.NumberOfNights);

    //        if (reservation.ReservationType != ReservationType.Allotment)
    //        {
    //            if (reservation.ReservationSeason.RoomExpenseMainAccountId == null)
    //            {
    //                throw new Exception("Room Expense account not found");
    //            }
    //            transaction.TransactionEntries.Add(new TransactionEntry
    //            {
    //                CreditAmount = 0,
    //                DebitAmount = roomExpense,
    //                MainAccountId = (Guid)reservation.ReservationSeason.RoomExpenseMainAccountId,
    //                Description = "",
    //            });
    //        }

    //        var mealRevenue = reservation.ReservationRoomEntries
    //            .Sum(re => (re.RoomType.PersonNo * re.NumberOfRooms) * (re.MealRate ?? 0) * reservation.NumberOfNights);
    //        var mealExpense = reservation.ReservationRoomEntries
    //            .Sum(re => (re.RoomType.PersonNo * re.NumberOfRooms) * (re.MealCost ?? 0) * reservation.NumberOfNights);
    //        if (reservation.ReservationRoomEntries.Any(re => re.MealRate != null && re.MealCost != null))
    //        {
    //            if (reservation.ReservationSeason.MealRevenueMainAccountId == null)
    //            {
    //                throw new Exception("Meal Revenue account not found");
    //            }
    //            transaction.TransactionEntries.Add(new TransactionEntry
    //            {
    //                CreditAmount = mealRevenue,
    //                DebitAmount = 0,
    //                MainAccountId = (Guid)reservation.ReservationSeason.MealRevenueMainAccountId,
    //                Description = "",
    //            });

    //            // Season Meal Expense Entry

    //            if (reservation.ReservationSeason.MealExpenseMainAccountId == null)
    //            {
    //                throw new Exception("Meal Expense account not found");
    //            }

    //            if (reservation.ReservationType != ReservationType.Allotment)
    //            {
    //                transaction.TransactionEntries.Add(new TransactionEntry
    //                {
    //                    CreditAmount = 0,
    //                    DebitAmount = mealExpense,
    //                    MainAccountId = (Guid)reservation.ReservationSeason.MealExpenseMainAccountId,
    //                    Description = "",
    //                });
    //            }
    //        }

    //        var otherRevenue = reservation.ReservationRoomEntries
    //            .Sum(re => re.NumberOfRooms * (re.ViewRate ?? 0) * reservation.NumberOfNights);
    //        var otherExpense = reservation.ReservationRoomEntries
    //            .Sum(re => re.NumberOfRooms * (re.ViewCost ?? 0) * reservation.NumberOfNights);

    //        if (reservation.ReservationRoomEntries.Any(re => re.ViewRate != null && re.ViewCost != null))
    //        {

    //            if (reservation.ReservationSeason.OtherRevenueMainAccountId == null)
    //            {
    //                throw new Exception("Other Revenue account not found");
    //            }
    //            transaction.TransactionEntries.Add(new TransactionEntry
    //            {
    //                CreditAmount = otherRevenue,
    //                DebitAmount = 0,
    //                MainAccountId = (Guid)reservation.ReservationSeason.OtherRevenueMainAccountId,
    //                Description = "",
    //            });

    //            if (reservation.ReservationType != ReservationType.Allotment)
    //            {
    //                // Season Other Expense Entry
    //                if (reservation.ReservationSeason.OtherExpenseMainAccountId == null)
    //                {
    //                    throw new Exception("Other Expense account not found");
    //                }
    //                transaction.TransactionEntries.Add(new TransactionEntry
    //                {
    //                    CreditAmount = 0,
    //                    DebitAmount = otherExpense,
    //                    MainAccountId = (Guid)reservation.ReservationSeason.OtherExpenseMainAccountId,
    //                    Description = "",
    //                });
    //            }
    //        }
    //        var vatSalesAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
    //        var vatSalesAccount = vatSalesAccountQuery.Data.FirstOrDefault();
    //        if (vatSalesAccount == null)
    //        {
    //            throw new Exception("VAT Sales account not found");
    //        }
    //        transaction.TransactionEntries.Add(new TransactionEntry
    //        {
    //            CreditAmount = (roomRevenue + mealRevenue + otherRevenue) * (decimal)0.15,
    //            DebitAmount = 0,
    //            MainAccountId = vatSalesAccount.Id,
    //            Description = "",
    //        });
    //        var vatPurchaseAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
    //        var vatPurchaseAccount = vatPurchaseAccountQuery.Data.FirstOrDefault();
    //        if (vatPurchaseAccount == null)
    //        {
    //            throw new Exception("Purchase VAT account not found");
    //        }
    //        if (reservation.ReservationType != ReservationType.Allotment)
    //        {

    //            transaction.TransactionEntries.Add(new TransactionEntry
    //            {
    //                CreditAmount = 0,
    //                DebitAmount = (roomExpense + mealExpense + otherExpense) * (decimal)0.15,
    //                MainAccountId = vatPurchaseAccount.Id,
    //                Description = "",
    //            });
    //        }



    //        if (reservation.ReservationType is ReservationType.AgentDirect or ReservationType.HotelDirect)
    //        {

    //            if (reservation.ReservationType == ReservationType.AgentDirect)
    //            {
    //                // reservation supplier account
    //                if (reservation.ReservationCompany.MainAccountId == null)
    //                {
    //                    throw new Exception("Reservation supplier account not found");
    //                }
    //                transaction.TransactionEntries.Add(new TransactionEntry
    //                {
    //                    CreditAmount = (roomExpense + mealExpense + otherExpense) +
    //                                   ((roomExpense + mealExpense + otherExpense) * (decimal)0.15),
    //                    DebitAmount = 0,
    //                    MainAccountId = (Guid)reservation.ReservationCompany.MainAccountId,
    //                    Description = "Reservation Supplier Account",
    //                });
    //            }

    //            if (reservation.ReservationType == ReservationType.HotelDirect)
    //            {
    //                // reservation supplier account
    //                if (reservation.Hotel.MainAccountId == null)
    //                {
    //                    throw new Exception("Reservation Hotel account not found");
    //                }
    //                transaction.TransactionEntries.Add(new TransactionEntry
    //                {
    //                    CreditAmount = (roomExpense + mealExpense + otherExpense) +
    //                                   ((roomExpense + mealExpense + otherExpense) * (decimal)0.15),
    //                    DebitAmount = 0,
    //                    MainAccountId = (Guid)reservation.Hotel.MainAccountId,
    //                    Description = "Reservation Hotel Account",
    //                });
    //            }
    //        }
    //        // Group services on reservationService
    //        var groupedServiceEntries = reservation.ReservationServiceEntries
    //            .GroupBy(re => re.ReservationServiceId)
    //            .Select(g => new
    //            {
    //                Key = g.Key,
    //                ReservationServiceEntries = g.ToList(),
    //                ReservationService = g.First().ReservationService,
    //            })
    //            .ToList();

    //        foreach (var serviceEntry in groupedServiceEntries)
    //        {
    //            if (serviceEntry.ReservationService.RevenueMainAccountId != null && serviceEntry.ReservationService.ExpenseMainAccountId != null)
    //            {
    //                var serviceRevenue = serviceEntry.ReservationServiceEntries
    //                    .Sum(re => re.Quantity * re.SalesPrice);
    //                if (serviceRevenue > 0)
    //                {
    //                    transaction.TransactionEntries.Add(new TransactionEntry
    //                    {
    //                        CreditAmount = serviceRevenue,
    //                        DebitAmount = 0,
    //                        MainAccountId = (Guid)serviceEntry.ReservationService.RevenueMainAccountId,
    //                        Description = "Service Revenue Account",
    //                    });

    //                    transaction.TransactionEntries.Add(new TransactionEntry
    //                    {
    //                        CreditAmount = serviceRevenue * (decimal)0.15,
    //                        DebitAmount = 0,
    //                        MainAccountId = vatSalesAccount.Id,
    //                        Description = "",
    //                    });
    //                }

    //                var serviceExpense = serviceEntry.ReservationServiceEntries
    //                    .Sum(re => re.Quantity * re.CostPrice);
    //                if (serviceExpense > 0)
    //                {
    //                    transaction.TransactionEntries.Add(new TransactionEntry
    //                    {
    //                        CreditAmount = 0,
    //                        DebitAmount = serviceExpense,
    //                        MainAccountId = (Guid)serviceEntry.ReservationService.ExpenseMainAccountId,
    //                        Description = "Service Expense Account",
    //                    });
    //                    transaction.TransactionEntries.Add(new TransactionEntry
    //                    {
    //                        CreditAmount = 0,
    //                        DebitAmount = serviceExpense * (decimal)0.15,
    //                        MainAccountId = vatPurchaseAccount.Id,
    //                        Description = "",
    //                    });
    //                }
    //                ;

    //                // Reservation Supplier Account
    //                serviceEntry.ReservationServiceEntries.GroupBy(re => re.ReservationSupplierId)
    //                    .Select(g => new
    //                    {
    //                        Key = g.Key,
    //                        ReservationServiceEntries = g.ToList(),
    //                        ReservationSupplier = g.First().ReservationSupplier
    //                    })
    //                    .ToList()
    //                    .ForEach(s =>
    //                    {
    //                        if (s.ReservationSupplier.MainAccountId == null)
    //                        {
    //                            throw new Exception("Reservation supplier account not found");
    //                        }
    //                        var supplierServiceExpense = s.ReservationServiceEntries.Sum(re => re.Quantity * re.CostPrice);
    //                        if (supplierServiceExpense > 0)
    //                        {
    //                            transaction.TransactionEntries.Add(new TransactionEntry
    //                            {
    //                                CreditAmount = supplierServiceExpense + (supplierServiceExpense * (decimal)0.15),
    //                                DebitAmount = 0,
    //                                MainAccountId = (Guid)s.ReservationSupplier.MainAccountId,
    //                                Description = "Reservation Supplier Account",
    //                            });
    //                        }
    //                    });
    //            }
    //        }
    //        Console.WriteLine("Transaction Entries Credit: " + transaction.TransactionEntries.Sum(te => te.CreditAmount));
    //        Console.WriteLine("Transaction Entries Debit: " + transaction.TransactionEntries.Sum(te => te.DebitAmount));
    //        var transactionEnforcer = new TransactionEnforcer();
    //        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
    //        if (enforcerResult.IsSuccess)
    //        {
    //            var result = await transactionRepository.Add(transaction);
    //            if (result.ErrorCode == ErrorCodes.Success)
    //            {
    //                reservation.TransactionId = result.Data.Id;
    //                reservation.ReservationServiceEntries = null;
    //                reservation.ReservationRoomEntries = null;
    //                reservation.ReservationCompany = null;
    //                reservation.Hotel = null;
    //                reservation.ReservationSeason = null;
    //                reservation.SalesRepresentative = null;
    //                await reservationRepository.Update(reservation);
    //            }
    //        }

    //        await reservationRepository.CommitTransactionAsync();

    //        return new OperationResponse<ReservationResponseDto>(ErrorCodes.Success, null);
    //    }


    //    catch (Exception ex)
    //    {
    //        await reservationRepository.RollbackTransactionAsync();
    //        LogService.LogError(ex);
    //        return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null);

    //    }
    //}
    #endregion

    public override async Task<OperationResponse<ReservationResponseDto>> UpdateAsync(ReservationPutDto reservationPut)
    {
        Console.WriteLine($"[DEBUG] UpdateAsync called for reservation ID: {reservationPut.Id}");
        var reservation = _mapper.Map<Domains.Entities.Reservation.Reservation>(reservationPut);
        GuidHelper.NormalizeGuids<Domains.Entities.Reservation.Reservation>(reservation);

        var result = new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null);
        var reservationRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>(true);
        try
        {
            var existingReservationQuery = await reservationRepo.Filter(r => r.Id == reservation.Id);
            var existingReservation = existingReservationQuery.Data?.FirstOrDefault();
            if (existingReservation == null)
            {
                Console.WriteLine($"[DEBUG] UpdateAsync: Reservation not found in database for ID {reservation.Id}");
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.NotFound, null, message: "Reservation not found");
            }

            if (existingReservation.SaleInvoiceId != null)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "لا يمكن التعديل على حجز تم إصدار فاتورة له مسبقاً. يرجى إلغاء الفاتورة أولاً.");
            }

            Console.WriteLine($"[DEBUG] UpdateAsync: Existing Reservation found. IsConfirmed={existingReservation.IsConfirmed}, IsCancelled={existingReservation.IsCancelled}");

            if (existingReservation.IsConfirmed || existingReservation.IsCancelled)
            {
                Console.WriteLine($"[DEBUG] UpdateAsync: Cannot edit because IsConfirmed or IsCancelled is true");
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Cannot edit a confirmed or cancelled reservation");
            }

            await reservationRepo.BeginTransactionAsync();
            Console.WriteLine($"[DEBUG] UpdateAsync: Transaction started");

            var roomEntries = reservationPut.ReservationRoomEntries;
            var existingPermitEntriesQuery = await _repositoriesFactory.GetRepository<ReservationRoomEntry>()
                .Filter(x => x.ReservationId == reservation.Id);
            var existingPermitEntries = existingPermitEntriesQuery.Data!.ToList();
            Console.WriteLine($"[DEBUG] UpdateAsync: Found {existingPermitEntries.Count} existing room entries");

            foreach (var roomEntry in roomEntries)
            {
                var entryEntity = _mapper.Map<ReservationRoomEntry>(roomEntry);
                GuidHelper.NormalizeGuids<ReservationRoomEntry>(entryEntity);

                var existingEntry = existingPermitEntries.FirstOrDefault(x => x.Id == roomEntry.Id);

                if (existingEntry != null)
                {
                    Console.WriteLine($"[DEBUG] UpdateAsync: Updating room entry {roomEntry.Id}");
                    await _repositoriesFactory.GetRepository<ReservationRoomEntry>().Update(entryEntity);
                }
                else
                {
                    Console.WriteLine($"[DEBUG] UpdateAsync: Adding new room entry {roomEntry.Id}");
                    await _repositoriesFactory.GetRepository<ReservationRoomEntry>().Add(entryEntity);
                }
            }

            var roomEntriesToDelete = existingPermitEntries
                .Where(x => !roomEntries.Any(pra => pra.Id == x.Id)).ToList();

            foreach (var roomEntryToDelete in roomEntriesToDelete)
            {
                await _repositoriesFactory.GetRepository<ReservationRoomEntry>().Delete(roomEntryToDelete);
            }
            // Service Entries
            var serviceEntries = reservationPut.ReservationServiceEntries;
            var existingServiceEntriesQuery = await _repositoriesFactory.GetRepository<ReservationServiceEntry>()
                .Filter(x => x.ReservationId == reservation.Id);
            var existingServiceEntries = existingServiceEntriesQuery.Data!.ToList();

            // Update or Add paycheck receipt accounts
            foreach (var serviceEntry in serviceEntries)
            {
                var entryEntity = _mapper.Map<ReservationServiceEntry>(serviceEntry);
                GuidHelper.NormalizeGuids<ReservationServiceEntry>(entryEntity);

                var existingEntry = existingServiceEntries.FirstOrDefault(x => x.Id == serviceEntry.Id);

                if (existingEntry != null)
                {
                    await _repositoriesFactory.GetRepository<ReservationServiceEntry>().Update(entryEntity);
                }
                else
                {
                    await _repositoriesFactory.GetRepository<ReservationServiceEntry>().Add(entryEntity);
                }
            }

            var serviceEntriesToDelete = existingServiceEntries
                .Where(x => !serviceEntries.Any(pra => pra.Id == x.Id)).ToList();

            foreach (var serviceEntryToDelete in serviceEntriesToDelete)
            {
                await _repositoriesFactory.GetRepository<ReservationServiceEntry>().Delete(serviceEntryToDelete);
            }

            // We already queried existingReservation at the beginning of the try block, 
            // no need to query it again.

            reservation.ReservationRoomEntries = null;
            reservation.ReservationServiceEntries = null;
            reservation.ReservationCompany = null;
            reservation.ReservationClientCompany = null;
            reservation.ReservationSeason = null;
            reservation.Hotel = null;
            reservation.SalesRepresentative = null;
            reservation.TransactionId = existingReservation.TransactionId;
            var operation = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Update(reservation);

            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<ReservationResponseDto>(operation.ErrorCode, null);

            var responseData = _mapper.Map<ReservationResponseDto>(operation.Data);
            // check if transactionId null create transaction if not update
            if (reservation.TransactionId != null)
                await UpdateTransactionForReservationAsync(reservation.Id);

            await LogActionAsync(reservation.Id, "Updated");

            await reservationRepo.CommitTransactionAsync();
            return new OperationResponse<ReservationResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            await reservationRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
        }
        return result;
    }

    private async Task UpdateTransactionForReservationAsync(Guid reservationId)
    {
        var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>(true);

        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>(true);

        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>(true);

        var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>(true);

        try
        {
            await reservationRepository.BeginTransactionAsync();


        var reservationQuery = await reservationRepository.Filter((p) => p.Id == reservationId);
        var reservation = reservationQuery.Data
            .Include(r => r.ReservationRoomEntries)
            .Include(r => r.ReservationServiceEntries)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.SalesRepresentative)
            .FirstOrDefault();
        if (reservation == null)
        {
            throw new Exception("Reservation not found");
        }


        var transactionQuery = await transactionRepository.Filter(t => t.ReservationId == reservationId);
        var transaction = transactionQuery.Data.FirstOrDefault();
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }
       
        var existingTransactionEntriesQuery = await transactionEntryRepository
            .Filter(te => te.TransactionId == transaction.Id);
        var existingTransactionEntries = existingTransactionEntriesQuery.Data.ToList();

        foreach (var existingEntry in existingTransactionEntries)
        {
            existingEntry.MainAccount = null;
            existingEntry.Transaction = null;
            await transactionEntryRepository.Delete(existingEntry);
        }

        var reservationRoomEntriesSalesTotal =
            reservation.ReservationRoomEntries
                .Sum(re =>
                    (re.NumberOfRooms * reservation.NumberOfNights * re.RoomRate)
                    + ((re.RoomType.PersonNo * re.NumberOfRooms) * reservation.NumberOfNights * re.MealRate ?? 0)
                    + (re.NumberOfRooms * reservation.NumberOfNights * re.ViewRate ?? 0)
                );

        var reservationRoomEntriesCostTotal =
            reservation.ReservationRoomEntries
                .Sum(re =>
                    (re.NumberOfRooms * reservation.NumberOfNights * (re.RoomCost ?? 0m))
                    + ((re.RoomType.PersonNo * re.NumberOfRooms) * reservation.NumberOfNights * (re.MealCost ?? 0m))
                    + (re.NumberOfRooms * reservation.NumberOfNights * (re.ViewCost ?? 0m))
                );

        var reservationServiceEntriesSalesTotal =
            reservation.ReservationServiceEntries
                .Sum(re =>
                    (re.Quantity * re.SalesPrice)
                );

        var reservationServiceEntriesCostTotal =
            reservation.ReservationServiceEntries
                .Sum(re =>
                    (re.Quantity * (re.CostPrice ?? 0m))
                );

        var totalSalesPrice = reservationRoomEntriesSalesTotal + reservationServiceEntriesSalesTotal;
        var totalCostPrice = reservationRoomEntriesCostTotal + reservationServiceEntriesCostTotal;

        if (reservation.ReservationClientCompany.MainAccountId == null)
        {
            throw new Exception("Client company account not found");
        }
        var transactionEntries = new List<TransactionEntry>();

        // client company account
        transactionEntries.Add(new TransactionEntry
        {
            CreditAmount = 0,
            DebitAmount = totalSalesPrice + (totalSalesPrice * (decimal)0.15),
            MainAccountId = (Guid)reservation.ReservationClientCompany.MainAccountId,
            Description = "Client Company Account",
            TransactionId = transaction.Id
        });

        // Season Room Revenue Entry
        var roomRevenue = reservation.ReservationRoomEntries
            .Sum(re => re.NumberOfRooms * re.RoomRate * reservation.NumberOfNights);
        if (reservation.ReservationSeason.RoomRevenueMainAccountId == null)
        {
            throw new Exception("Room Revenue account not found");
        }
        transactionEntries.Add(new TransactionEntry
        {
            CreditAmount = roomRevenue,
            DebitAmount = 0,
            MainAccountId = (Guid)reservation.ReservationSeason.RoomRevenueMainAccountId,
            Description = "",
            TransactionId = transaction.Id,
        });

        // Season Room Expense Entry
        var roomExpense = reservation.ReservationRoomEntries
            .Sum(re => re.NumberOfRooms * (re.RoomCost ?? 0m) * reservation.NumberOfNights);
        if (reservation.ReservationSeason.RoomExpenseMainAccountId == null)
        {
            throw new Exception("Room Expense account not found");
        }

        if (reservation.ReservationType != ReservationType.Allotment)
        {
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = roomExpense,
                MainAccountId = (Guid)reservation.ReservationSeason.RoomExpenseMainAccountId,
                Description = "",
                TransactionId = transaction.Id,
            });
        }

        var mealRevenue = reservation.ReservationRoomEntries
            .Sum(re => (re.RoomType.PersonNo * re.NumberOfRooms) * (re.MealRate ?? 0) * reservation.NumberOfNights);
        var mealExpense = reservation.ReservationRoomEntries
            .Sum(re => (re.RoomType.PersonNo * re.NumberOfRooms) * (re.MealCost ?? 0) * reservation.NumberOfNights);
        if (reservation.ReservationRoomEntries.Any(re => re.MealRate != null && re.MealCost != null))
        {
            if (reservation.ReservationSeason.MealRevenueMainAccountId == null)
            {
                throw new Exception("Meal Revenue account not found");
            }
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = mealRevenue,
                DebitAmount = 0,
                MainAccountId = (Guid)reservation.ReservationSeason.MealRevenueMainAccountId,
                Description = "",
                TransactionId = transaction.Id,
            });

            // Season Meal Expense Entry

            if (reservation.ReservationSeason.MealExpenseMainAccountId == null)
            {
                throw new Exception("Meal Expense account not found");
            }

            if (reservation.ReservationType != ReservationType.Allotment)
            {
                transactionEntries.Add(new TransactionEntry
                {
                    CreditAmount = 0,
                    DebitAmount = mealExpense,
                    MainAccountId = (Guid)reservation.ReservationSeason.MealExpenseMainAccountId,
                    Description = "",
                    TransactionId = transaction.Id,
                });
            }
        }

        var otherRevenue = reservation.ReservationRoomEntries
            .Sum(re => re.NumberOfRooms * (re.ViewRate ?? 0) * reservation.NumberOfNights);
        var otherExpense = reservation.ReservationRoomEntries
            .Sum(re => re.NumberOfRooms * (re.ViewCost ?? 0) * reservation.NumberOfNights);

        if (reservation.ReservationRoomEntries.Any(re => re.ViewRate != null && re.ViewCost != null))
        {

            if (reservation.ReservationSeason.OtherRevenueMainAccountId == null)
            {
                throw new Exception("Other Revenue account not found");
            }
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = otherRevenue,
                DebitAmount = 0,
                MainAccountId = (Guid)reservation.ReservationSeason.OtherRevenueMainAccountId,
                Description = "",
                TransactionId = transaction.Id,
            });

            // Season Other Expense Entry
            if (reservation.ReservationSeason.OtherExpenseMainAccountId == null)
            {
                throw new Exception("Other Expense account not found");
            }

            if (reservation.ReservationType != ReservationType.Allotment)
            {
                transactionEntries.Add(new TransactionEntry
                {
                    CreditAmount = 0,
                    DebitAmount = otherExpense,
                    MainAccountId = (Guid)reservation.ReservationSeason.OtherExpenseMainAccountId,
                    Description = "",
                    TransactionId = transaction.Id,
                });
            }
        }
        var vatSalesAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المبيعات");
        var vatSalesAccount = vatSalesAccountQuery.Data.FirstOrDefault();
        if (vatSalesAccount == null)
        {
            throw new Exception("VAT Sales account not found");
        }
        transactionEntries.Add(new TransactionEntry
        {
            CreditAmount = (roomRevenue + mealRevenue + otherRevenue) * (decimal)0.15,
            DebitAmount = 0,
            MainAccountId = vatSalesAccount.Id,
            Description = "",
            TransactionId = transaction.Id,
        });
        var vatPurchaseAccountQuery = await mainAccountRepository.Filter(ma => ma.ArabicName == "ضريبة القيمة المضافة  المشتريات");
        var vatPurchaseAccount = vatPurchaseAccountQuery.Data.FirstOrDefault();
        if (vatPurchaseAccount == null)
        {
            throw new Exception("Purchase VAT account not found");
        }

        if (reservation.ReservationType != ReservationType.Allotment)
        {
            transactionEntries.Add(new TransactionEntry
            {
                CreditAmount = 0,
                DebitAmount = (roomExpense + mealExpense + otherExpense) * (decimal)0.15,
                MainAccountId = vatPurchaseAccount.Id,
                Description = "",
                TransactionId = transaction.Id,
            });
        }



        if (reservation.ReservationType is ReservationType.AgentDirect or ReservationType.HotelDirect)
        {

            if (reservation.ReservationType == ReservationType.AgentDirect)
            {
                // reservation supplier account
                if (reservation.ReservationCompany.MainAccountId == null)
                {
                    throw new Exception("Reservation supplier account not found");
                }
                transactionEntries.Add(new TransactionEntry
                {
                    CreditAmount = (roomExpense + mealExpense + otherExpense) +
                                   ((roomExpense + mealExpense + otherExpense) * (decimal)0.15),
                    DebitAmount = 0,
                    MainAccountId = (Guid)reservation.ReservationCompany.MainAccountId,
                    Description = "Reservation Supplier Account",
                    TransactionId = transaction.Id,
                });
            }

            if (reservation.ReservationType == ReservationType.HotelDirect)
            {
                // reservation supplier account
                if (reservation.Hotel.MainAccountId == null)
                {
                    throw new Exception("Reservation Hotel account not found");
                }
                transactionEntries.Add(new TransactionEntry
                {
                    CreditAmount = (roomExpense + mealExpense + otherExpense) +
                                   ((roomExpense + mealExpense + otherExpense) * (decimal)0.15),
                    DebitAmount = 0,
                    MainAccountId = (Guid)reservation.Hotel.MainAccountId,
                    Description = "Reservation Hotel Account",
                    TransactionId = transaction.Id,
                });
            }
        }
        // Group services on reservationService
        var groupedServiceEntries = reservation.ReservationServiceEntries
            .GroupBy(re => re.ReservationServiceId)
            .Select(g => new
            {
                Key = g.Key,
                ReservationServiceEntries = g.ToList(),
                ReservationService = g.First().ReservationService,
            })
            .ToList();

        foreach (var serviceEntry in groupedServiceEntries)
        {
            if (serviceEntry.ReservationService.RevenueMainAccountId != null && serviceEntry.ReservationService.ExpenseMainAccountId != null)
            {
                var serviceRevenue = serviceEntry.ReservationServiceEntries
                    .Sum(re => re.Quantity * re.SalesPrice);
                if (serviceRevenue > 0)
                {
                    transactionEntries.Add(new TransactionEntry
                    {
                        CreditAmount = serviceRevenue,
                        DebitAmount = 0,
                        MainAccountId = (Guid)serviceEntry.ReservationService.RevenueMainAccountId,
                        Description = "Service Revenue Account",
                        TransactionId = transaction.Id,
                    });

                    transactionEntries.Add(new TransactionEntry
                    {
                        CreditAmount = serviceRevenue * (decimal)0.15,
                        DebitAmount = 0,
                        MainAccountId = vatSalesAccount.Id,
                        Description = "",
                        TransactionId = transaction.Id,
                    });
                }

                var serviceExpense = serviceEntry.ReservationServiceEntries
                    .Sum(re => re.Quantity * (re.CostPrice ?? 0m));
                if (serviceExpense > 0)
                {
                    transactionEntries.Add(new TransactionEntry
                    {
                        CreditAmount = 0,
                        DebitAmount = serviceExpense,
                        MainAccountId = (Guid)serviceEntry.ReservationService.ExpenseMainAccountId,
                        Description = "Service Expense Account",
                        TransactionId = transaction.Id,
                    });
                    transactionEntries.Add(new TransactionEntry
                    {
                        CreditAmount = 0,
                        DebitAmount = serviceExpense * (decimal)0.15,
                        MainAccountId = vatPurchaseAccount.Id,
                        Description = "",
                        TransactionId = transaction.Id,
                    });
                }
                ;

                // Reservation Supplier Account
                serviceEntry.ReservationServiceEntries.GroupBy(re => re.ReservationSupplierId)
                    .Select(g => new
                    {
                        Key = g.Key,
                        ReservationServiceEntries = g.ToList(),
                        ReservationSupplier = g.First().ReservationSupplier
                    })
                    .ToList()
                    .ForEach(s =>
                    {
                        if (s.ReservationSupplier.MainAccountId == null)
                        {
                            throw new Exception("Reservation supplier account not found");
                        }
                        var supplierServiceExpense = s.ReservationServiceEntries.Sum(re => re.Quantity * (re.CostPrice ?? 0m));
                        if (supplierServiceExpense > 0)
                        {
                            transactionEntries.Add(new TransactionEntry
                            {
                                CreditAmount = supplierServiceExpense + (supplierServiceExpense * (decimal)0.15),
                                DebitAmount = 0,
                                MainAccountId = (Guid)s.ReservationSupplier.MainAccountId,
                                Description = "Reservation Supplier Account",
                                TransactionId = transaction.Id,
                            });
                        }
                    });
            }
        }
        Console.WriteLine("Transaction Entries Credit: " + transactionEntries.Sum(te => te.CreditAmount));
        Console.WriteLine("Transaction Entries Debit: " + transactionEntries.Sum(te => te.DebitAmount));


        var tempTransaction = transaction;
        tempTransaction.TransactionEntries = transactionEntries;
        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(tempTransaction);
        if (enforcerResult.IsSuccess)
        {
            await transactionEntryRepository.AddRange(transactionEntries);
        }
        else
        {
            throw new Exception("Transaction Enforcer Error");
        }
            await reservationRepository.CommitTransactionAsync();
    }
        catch (Exception ex)
        {
            await reservationRepository.RollbackTransactionAsync();
            LogService.LogError(ex);
        }
        }

    public override async Task<OperationResponse<ReservationResponseDto>> DeleteAsync(Guid reservationId)
    {
        var result = new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null);
        var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>(true);
        var reservationEntryRepository = _repositoriesFactory.GetRepository<ReservationRoomEntry>(true);
        var reservationServiceEntryRepository = _repositoriesFactory.GetRepository<ReservationServiceEntry>(true);

        try
        {
            await reservationRepository.BeginTransactionAsync();
           
            // Fetch the Reservation
            var reservationQuery = await reservationRepository.Filter(r => r.Id == reservationId);
            var reservation = reservationQuery.Data.FirstOrDefault();
            if (reservation == null)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.NotFound, null);
            }

            // Undo related ReservationRoomEntries
            var reservationRoomEntriesToDelete = reservation.ReservationRoomEntries;
            foreach (var reservationRoomEntryToDelete in reservationRoomEntriesToDelete)
            {
                reservationRoomEntryToDelete.ReservationMeal = null;
                reservationRoomEntryToDelete.ReservationView = null;
                reservationRoomEntryToDelete.Reservation = null;
                await reservationEntryRepository.Delete(reservationRoomEntryToDelete);
            }

            // Undo related ReservationServiceEntries
            var reservationServiceEntriesToDelete = reservation.ReservationServiceEntries;
            foreach (var reservationServiceEntryToDelete in reservationServiceEntriesToDelete)
            {
                reservationServiceEntryToDelete.ReservationSupplier = null;
                reservationServiceEntryToDelete.Reservation = null;
                await reservationServiceEntryRepository.Delete(reservationServiceEntryToDelete);
            }

            // Delete the Transaction
            var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
            var transactionQuery = await transactionRepository.Filter(t => t.ReservationId == reservationId);
            var transaction = transactionQuery.Data.FirstOrDefault();
            if (transaction != null)
            {
                await _serviceBaseFactory.GetService<TransactionService>().DeleteAsync(transaction.Id);
            }

            // Delete the Reservation itself
            reservation.ReservationRoomEntries = null;
            reservation.ReservationServiceEntries = null;
            await reservationRepository.Delete(reservation);

            await reservationRepository.CommitTransactionAsync();

            return new OperationResponse<ReservationResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            await reservationRepository.RollbackTransactionAsync();
            LogService.LogError(ex);
        }
        return result;
    }

    public async Task<OperationResponse<ReservationResponseDto>> CancelAsync(Guid reservationId)
    {
        try
        {
            var dbContext = _repositoriesFactory.GetDbContext();

            var reservation = await dbContext.Reservations
                .AsNoTracking()
                .Where(r => r.Id == reservationId)
                .Select(r => new { r.Id, r.IsCancelled, r.IsSentToZatca, r.SaleInvoiceId })
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.NotFound, null, message: "Reservation Not Found");
            }

            if (reservation.IsSentToZatca)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Reservation already sent to Zatca and cannot be cancelled");
            }

            if (reservation.IsCancelled)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Reservation is already cancelled");
            }

            if (reservation.SaleInvoiceId.HasValue)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "لا يمكن إلغاء الحجز لأنه مرتبط بفاتورة مبيعات. يرجى إلغاء الفاتورة أولاً.");
            }

            var cancelledAt = DateTime.UtcNow;
            var currentUserId = GetCurrentUserId();

            await dbContext.Reservations
                .Where(r => r.Id == reservationId)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsCancelled, true));

            await dbContext.Transactions
                .Where(t => t.ReservationId == reservationId && t.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));

            var log = new Domains.Entities.Reservation.ReservationActionLog
            {
                Id = Guid.NewGuid(),
                ReservationId = reservationId,
                ActionType = "Cancelled",
                ExecutedByUserId = currentUserId,
                ExecutedBy = string.Empty,
                OccurredAt = cancelledAt,
                IsActive = true,
                CreatedBy = Guid.Empty,
                CreatedAt = cancelledAt
            };

            await dbContext.ReservationActionLogs.AddAsync(log);
            await dbContext.SaveChangesAsync();

            return new OperationResponse<ReservationResponseDto>(ErrorCodes.Success, null, message: "Reservation cancelled successfully");
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public async Task<OperationResponse<ReservationResponseDto>> ReactivateAsync(Guid reservationId)
    {
        try
        {
            var dbContext = _repositoriesFactory.GetDbContext();

            var reservation = await dbContext.Reservations
                .AsNoTracking()
                .Where(r => r.Id == reservationId)
                .Select(r => new { r.Id, r.IsCancelled, r.IsSentToZatca })
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.NotFound, null, message: "Reservation Not Found");
            }

            if (!reservation.IsCancelled)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Reservation is not cancelled");
            }

            if (reservation.IsSentToZatca)
            {
                return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Reservation sent to Zatca cannot be reactivated");
            }

            var reactivatedAt = DateTime.UtcNow;
            var currentUserId = GetCurrentUserId();

            await dbContext.Reservations
                .Where(r => r.Id == reservationId)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsCancelled, false));

            await dbContext.Transactions
                .Where(t => t.ReservationId == reservationId && !t.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, true));

            var log = new Domains.Entities.Reservation.ReservationActionLog
            {
                Id = Guid.NewGuid(),
                ReservationId = reservationId,
                ActionType = "Reactivated",
                ExecutedByUserId = currentUserId,
                ExecutedBy = string.Empty,
                OccurredAt = reactivatedAt,
                IsActive = true,
                CreatedBy = Guid.Empty,
                CreatedAt = reactivatedAt
            };

            await dbContext.ReservationActionLogs.AddAsync(log);
            await dbContext.SaveChangesAsync();

            return new OperationResponse<ReservationResponseDto>(ErrorCodes.Success, null, message: "Reservation reactivated successfully");
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<ReservationResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public async Task<OperationResponse<ConfirmReservationResponseDto>> ConfirmAsync(Guid reservationId)
    {
        try
        {
            var dbContext = _repositoriesFactory.GetDbContext();

            var reservation = await dbContext.Reservations
                .AsNoTracking()
                .Where(r => r.Id == reservationId)
                .Select(r => new { r.Id, r.IsConfirmed })
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                return new OperationResponse<ConfirmReservationResponseDto>(ErrorCodes.NotFound, null, message: "Reservation Not Found");
            }

            if (reservation.IsConfirmed)
            {
                return new OperationResponse<ConfirmReservationResponseDto>(ErrorCodes.GeneralFail, null, message: "Reservation is already confirmed");
            }

            var confirmedAt = DateTime.UtcNow;
            var currentUserId = GetCurrentUserId();

            await dbContext.Reservations
                .Where(r => r.Id == reservationId)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsConfirmed, true));

            var log = new Domains.Entities.Reservation.ReservationActionLog
            {
                Id = Guid.NewGuid(),
                ReservationId = reservationId,
                ActionType = "Confirmed",
                ExecutedByUserId = currentUserId,
                ExecutedBy = string.Empty,
                OccurredAt = confirmedAt,
                IsActive = true,
                CreatedBy = Guid.Empty,
                CreatedAt = confirmedAt
            };

            await dbContext.ReservationActionLogs.AddAsync(log);
            await dbContext.SaveChangesAsync();

            string executedByName = string.Empty;
            if (currentUserId.HasValue)
            {
                var user = await dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == currentUserId.Value)
                    .Select(u => new { u.FirstName, u.LastName, u.UserName })
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.FirstName))
                    {
                        executedByName = string.IsNullOrEmpty(user.LastName)
                            ? user.FirstName
                            : $"{user.FirstName} {user.LastName}".Trim();
                    }
                    else
                    {
                        executedByName = user.UserName ?? string.Empty;
                    }
                }
            }

            var lastLogDto = new ReservationActionLogResponseDto
            {
                Id = log.Id,
                ReservationId = log.ReservationId,
                ActionType = log.ActionType,
                ExecutedBy = executedByName,
                OccurredAt = log.OccurredAt
            };

            var confirmDto = new ConfirmReservationResponseDto
            {
                Id = reservationId,
                IsConfirmed = true,
                ConfirmedAt = confirmedAt,
                LastLog = lastLogDto
            };

            return new OperationResponse<ConfirmReservationResponseDto>(
                ErrorCodes.Success,
                confirmDto,
                message: "Reservation confirmed successfully"
            );
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<ConfirmReservationResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    public async Task<byte[]> GetDocument(Guid reservationId)
    {
        var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => r.Id == reservationId);
        var reservation = reservationQuery.Data
            .AsNoTracking()
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.RoomType)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationMeal)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationView)
            .Include(r => r.ReservationServiceEntries)
                .ThenInclude(se => se.ReservationService)
            .AsSplitQuery()
            .FirstOrDefault();
        if (reservation == null)
        {
            return null;
        }
        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data.AsNoTracking().FirstOrDefault();
        if (companyDetails == null)
        {
            return null;
        }

        if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
        {
            companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
        }

        if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
        {
            companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
        }

        var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data.AsNoTracking().FirstOrDefault();

        var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        var systemSetting = systemSettingQuery.Data.AsNoTracking().FirstOrDefault();
        var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";


        var reservationDocument = new ReservationDocument(reservation, companyDetails!, pdfTemplateConfig!,
            showStamp: systemSetting?.ShowStampInReservationPrint ?? true,
            showCompanySignature: systemSetting?.ShowCompanySignatureInReservationPrint ?? true,
            showCustomerSignature: systemSetting?.ShowCustomerSignatureInReservationPrint ?? true);
        var document = reservationDocument.GeneratePdf();
        return document;
    }

    public async Task<byte[][]> GetDocuments(Guid[] reservationIds)
    {
        var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => reservationIds.Contains(r.Id));
        var reservations = reservationQuery.Data
            .AsNoTracking()
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.RoomType)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationMeal)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationView)
            .Include(r => r.ReservationServiceEntries)
                .ThenInclude(se => se.ReservationService)
            .AsSplitQuery()
            .ToList();
        if (reservations == null)
        {
            return null;
        }

        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data.FirstOrDefault();
        if (companyDetails == null)
        {
            return null;
        }

        if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
        {
            companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
        }

        if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
        {
            companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
        }

        var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data.FirstOrDefault();

        var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        var systemSetting = systemSettingQuery.Data.FirstOrDefault();
        var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";

        var reservationDocuments = new List<byte[]>();
        foreach (var reservation in reservations)
        {

            var reservationDocument = new ReservationDocument(reservation, companyDetails!, pdfTemplateConfig!,
                showStamp: systemSetting?.ShowStampInReservationPrint ?? true,
                showCompanySignature: systemSetting?.ShowCompanySignatureInReservationPrint ?? true,
                showCustomerSignature: systemSetting?.ShowCustomerSignatureInReservationPrint ?? true);
            var document = reservationDocument.GeneratePdf();
            reservationDocuments.Add(document);
        }
        return reservationDocuments.ToArray();
    }

    public async Task<byte[]> GetCostSheet(Guid reservationId)
    {
        var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => r.Id == reservationId);
        var reservation = reservationQuery.Data
            .AsNoTracking()
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.RoomType)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationMeal)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationView)
            .Include(r => r.ReservationServiceEntries)
                .ThenInclude(se => se.ReservationService)
            .AsSplitQuery()
            .FirstOrDefault();
        if (reservation == null)
        {
            return null;
        }

        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data.AsNoTracking().FirstOrDefault();
        if (companyDetails == null)
        {
            return null;
        }

        if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
        {
            companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
        }

        if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
        {
            companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
        }

        var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data.AsNoTracking().FirstOrDefault();

        var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        var systemSetting = systemSettingQuery.Data.AsNoTracking().FirstOrDefault();
        var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";



        var reservationDocument = new CostSheetDocument(reservation, companyDetails!, pdfTemplateConfig!,
            showStamp: systemSetting?.ShowStampInReservationPrint ?? true,
            showCompanySignature: systemSetting?.ShowCompanySignatureInReservationPrint ?? true,
            showCustomerSignature: systemSetting?.ShowCustomerSignatureInReservationPrint ?? true);
        var document = reservationDocument.GeneratePdf();
        return document;
    }

    public async Task<byte[][]?> GetCostSheets(Guid[] reservationIds)
    {
        var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => reservationIds.Contains(r.Id));
        var reservations = reservationQuery.Data?
            .AsNoTracking()
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.RoomType)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationMeal)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationView)
            .Include(r => r.ReservationServiceEntries)
                .ThenInclude(se => se.ReservationService)
            .AsSplitQuery()
            .ToList();
        if (reservations == null || !reservations.Any())
        {
            return null;
        }

        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
        if (companyDetails == null)
        {
            return null;
        }

        if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
        {
            companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
        }

        if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
        {
            companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
        }

        var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

        var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
        var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";

        var reservationDocuments = new List<byte[]>();
        foreach (var reservation in reservations)
        {

            if (reservation != null && companyDetails != null)
            {
                var reservationDocument = new CostSheetDocument(reservation, companyDetails, pdfTemplateConfig,
                    showStamp: systemSetting?.ShowStampInReservationPrint ?? true,
                    showCompanySignature: systemSetting?.ShowCompanySignatureInReservationPrint ?? true,
                    showCustomerSignature: systemSetting?.ShowCustomerSignatureInReservationPrint ?? true);
                var document = reservationDocument.GeneratePdf();
                reservationDocuments.Add(document);
            }
        }
        return reservationDocuments.ToArray();
    }

    public async Task<byte[]?> GetHotelRequest(Guid reservationId)
    {
        var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => r.Id == reservationId);
        var reservation = reservationQuery.Data?
            .AsNoTracking()
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.RoomType)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationMeal)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationView)
            .Include(r => r.ReservationServiceEntries)
                .ThenInclude(se => se.ReservationService)
            .AsSplitQuery()
            .FirstOrDefault();
        if (reservation == null)
        {
            return null;
        }
        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data?.AsNoTracking().FirstOrDefault();
        if (companyDetails == null)
        {
            return null;
        }

        if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
        {
            companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
        }

        if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
        {
            companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
        }

        var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.AsNoTracking().FirstOrDefault();

        var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        var systemSetting = systemSettingQuery.Data?.AsNoTracking().FirstOrDefault();
        var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";



        if (reservation != null && companyDetails != null)
        {
            var reservationDocument = new HotelRequestDocument(reservation, companyDetails, pdfTemplateConfig,
                showStamp: systemSetting?.ShowStampInReservationPrint ?? true,
                showCompanySignature: systemSetting?.ShowCompanySignatureInReservationPrint ?? true,
                showCustomerSignature: systemSetting?.ShowCustomerSignatureInReservationPrint ?? true);
            var document = reservationDocument.GeneratePdf();
            return document;
        }
        return null;
    }

    public async Task<byte[][]?> GetHotelRequests(Guid[] reservationIds)
    {
        var reservationQuery = await _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>().Filter(r => reservationIds.Contains(r.Id));
        var reservations = reservationQuery.Data?
            .AsNoTracking()
            .Include(r => r.ReservationClientCompany)
            .Include(r => r.ReservationCompany)
            .Include(r => r.ReservationSeason)
            .Include(r => r.Hotel)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.RoomType)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationMeal)
            .Include(r => r.ReservationRoomEntries)
                .ThenInclude(re => re.ReservationView)
            .Include(r => r.ReservationServiceEntries)
                .ThenInclude(se => se.ReservationService)
            .ToList();
        if (reservations == null || !reservations.Any())
        {
            return null;
        }

        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
        if (companyDetails == null)
        {
            return null;
        }

        if ((companyDetails.Logo == null || companyDetails.Logo.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
        {
            companyDetails.Logo = await ImageDownloader.DownloadImageBytesAsync(companyDetails.LogoUrl);
        }

        if ((companyDetails.Stamp == null || companyDetails.Stamp.Length == 0) && !string.IsNullOrWhiteSpace(companyDetails.StampUrl))
        {
            companyDetails.Stamp = await ImageDownloader.DownloadImageBytesAsync(companyDetails.StampUrl);
        }

        var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

        var systemSettingQuery = await _repositoriesFactory.GetRepository<Domains.Entities.SystemSetting>().GetAll();
        var systemSetting = systemSettingQuery.Data?.FirstOrDefault();
        var timeZone = systemSetting?.TimeZone ?? "Asia/Riyadh";

        var reservationDocuments = new List<byte[]>();
        foreach (var reservation in reservations)
        {


            if (reservation != null && companyDetails != null)
            {
                var reservationDocument = new HotelRequestDocument(reservation, companyDetails, pdfTemplateConfig,
                    showStamp: systemSetting?.ShowStampInReservationPrint ?? true,
                    showCompanySignature: systemSetting?.ShowCompanySignatureInReservationPrint ?? true,
                    showCustomerSignature: systemSetting?.ShowCustomerSignatureInReservationPrint ?? true);
                var document = reservationDocument.GeneratePdf();
                reservationDocuments.Add(document);
            }
        }
        return reservationDocuments.ToArray();
    }

    public async Task<byte[]> GenerateTemplateDocument(ReservationPrintTemplateCommand command)
    {
        // Get company details
        var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
        var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();

        if (companyDetails == null)
            throw new Exception("Company details not found");

        // Get template
        var templateRepository = _repositoriesFactory.GetRepository<ReservationPrintTableTemplateEntity>();
        var templateQuery = await templateRepository.Filter(t => t.Id == command.TemplateId);
        var template = templateQuery.Data != null ? await templateQuery.Data.FirstOrDefaultAsync() : null;

        if (template == null)
            throw new Exception("Template not found");

        // Get reservations
        //var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
        //var reservations = new List<Domains.Entities.Reservation.Reservation>();

        //foreach (var reservationId in command.ReservationIds)
        //{
        //    var reservationQuery = await reservationRepository.Filter(r => r.Id == reservationId);
        //    if (reservationQuery.Data == null) continue;

        //    var reservation = reservationQuery.Data
        //        .Include(r => r.ReservationClientCompany)
        //        .Include(r => r.ReservationCompany)
        //        .Include(r => r.ReservationSeason)
        //        .Include(r => r.Hotel)
        //        .Include(r => r.ReservationRoomEntries)
        //            .ThenInclude(re => re.RoomType)
        //        .Include(r => r.ReservationServiceEntries)
        //        .FirstOrDefault();

        //    if (reservation != null)
        //    {
        //        reservations.Add(reservation);
        //    }
        //}

        // Get reservations
        var reservationIds = command.ReservationIds;
        if (reservationIds == null || !reservationIds.Any())
            throw new Exception("No reservation IDs provided");

        var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
        
        var reservationsQuery = await reservationRepository.Filter(r => reservationIds.Contains(r.Id));
        
        if (reservationsQuery.Data == null) 
            throw new Exception("No valid reservations found");

        var reservations = reservationsQuery.Data
                .Include(r => r.ReservationClientCompany)
                .Include(r => r.ReservationCompany)
                .Include(r => r.ReservationSeason)
                .Include(r => r.Hotel)
                .Include(r => r.ReservationRoomEntries)
                    .ThenInclude(re => re.RoomType)
                .Include(r => r.ReservationServiceEntries)
                .AsSplitQuery()
                .ToList();

        if (!reservations.Any())
            throw new Exception("No valid reservations found");

        // Get PDF template configuration
        var pdfTemplateConfigRepository = _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>();
        var pdfTemplateConfigQuery = await pdfTemplateConfigRepository.GetAll();
        var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

        // Generate PDF
        var document = new DocunentUtils.ReservationTableTemplateDocument(
            reservations,
            template,
            companyDetails,
            pdfTemplateConfig);

        return document.GeneratePdf();
    }

    public async Task<OperationResponse<ReservationResponseDto>> MakeFrontalAsync(Guid reservationId)
    {
        var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
        var reservationQuery = await reservationRepository.Filter(r => r.Id == reservationId);
        var reservation = reservationQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (reservation == null)
        {
            return new OperationResponse<ReservationResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        if (!reservation.IsFrontal || string.IsNullOrEmpty(reservation.ReferenceNumber) || !reservation.ReferenceNumber.StartsWith("FRES-"))
        {
            reservation.IsFrontal = true;
            
            // Generate new frontal reference number with numerical max logic
            var lastFrontalReservationQuery = await reservationRepository
                .Filter(r => r.IsFrontal && r.ReferenceNumber != null && r.ReferenceNumber.StartsWith("FRES-"));

            var allRefs = lastFrontalReservationQuery.Data?
                .Select(r => r.ReferenceNumber)
                .ToList();

            int maxNumber = 0;
            if (allRefs != null && allRefs.Any())
            {
                foreach (var refNum in allRefs)
                {
                    if (!string.IsNullOrEmpty(refNum) && refNum.StartsWith("FRES-"))
                    {
                        var numberPart = refNum.Substring("FRES-".Length);
                        if (int.TryParse(numberPart, out int numVal))
                        {
                            if (numVal > maxNumber) maxNumber = numVal;
                        }
                    }
                }
            }

            reservation.ReferenceNumber = $"FRES-{maxNumber + 1}";
        }
        
        var updateResult = await reservationRepository.Update(reservation);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        var mappedReservation = _mapper.Map<ReservationResponseDto>(updateResult.Data);

        return new OperationResponse<ReservationResponseDto>(
            ErrorCodes.Success,
            mappedReservation
        );
    }

    private async Task<Domains.Entities.Reservation.ReservationActionLog?> LogActionAsync(Guid reservationId, string actionType)
    {
        try
        {
            var logRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.ReservationActionLog>();
            var currentUserId = GetCurrentUserId();
            var log = new Domains.Entities.Reservation.ReservationActionLog
            {
                Id = Guid.NewGuid(),
                ReservationId = reservationId,
                ActionType = actionType,
                ExecutedByUserId = currentUserId,
                ExecutedBy = string.Empty,
                OccurredAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };
            await logRepo.Add(log);
            return log;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex, "Failed to log reservation action");
            return null;
        }
    }

    private Guid? GetCurrentUserId()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            // This app's JWT puts the user ID in ClaimTypes.Name (see TokenService.cs:80).
            // Some other apps use ClaimTypes.NameIdentifier. Read both for safety.
            var userIdStr = user?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                         ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildDisplayName(Domains.Entities.Reservation.ReservationActionLog log)
    {
        if (log.ExecutedByUser != null)
        {
            if (!string.IsNullOrEmpty(log.ExecutedByUser.FirstName))
            {
                return string.IsNullOrEmpty(log.ExecutedByUser.LastName)
                    ? log.ExecutedByUser.FirstName
                    : $"{log.ExecutedByUser.FirstName} {log.ExecutedByUser.LastName}".Trim();
            }
            return log.ExecutedByUser.UserName;
        }
        return log.ExecutedBy;
    }

    public async Task<List<SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation.ReservationActionLogResponseDto>> GetReservationLogsAsync(Guid reservationId)
    {
        var logRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.ReservationActionLog>();
        var query = await logRepo.Filter(l => l.ReservationId == reservationId);
        var logs = await query.Data
            .Include(l => l.ExecutedByUser)
            .AsNoTracking()
            .OrderByDescending(l => l.OccurredAt)
            .ToListAsync();

        return logs.Select(log => new SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation.ReservationActionLogResponseDto
        {
            Id = log.Id,
            ReservationId = log.ReservationId,
            ActionType = log.ActionType,
            ExecutedBy = BuildDisplayName(log),
            OccurredAt = log.OccurredAt
        }).ToList();
    }

    public async Task<string> GetReferenceNumberAsync(Guid id)
    {
        var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
        var query = await reservationRepository.Filter(r => r.Id == id);
        var refNum = query.Data?.Select(r => r.ReferenceNumber).FirstOrDefault();
        return !string.IsNullOrEmpty(refNum) ? refNum : id.ToString();
    }

    public async Task<Dictionary<Guid, string>> GetReferenceNumbersAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        var reservationRepository = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Reservation>();
        var query = await reservationRepository.Filter(r => idList.Contains(r.Id));
        var dict = query.Data?
            .Select(r => new { r.Id, r.ReferenceNumber })
            .ToDictionary(r => r.Id, r => !string.IsNullOrEmpty(r.ReferenceNumber) ? r.ReferenceNumber : r.Id.ToString())
            ?? new Dictionary<Guid, string>();

        foreach (var id in idList)
        {
            if (!dict.ContainsKey(id))
            {
                dict[id] = id.ToString();
            }
        }
        return dict;
    }
}



