using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Services.PublicBooking;

public class PublicRoomTypeService
    : ServiceBase<PublicRoomType, PublicRoomTypePostDto, PublicRoomTypePutDto, PublicRoomTypeResponseDto, PublicRoomTypeFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public PublicRoomTypeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<PublicRoomTypeResponseDto>> GetAsync(Guid id)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var roomTypeQuery = await roomTypeRepo.Filter(r => r.Id == id);

        var roomType = await roomTypeQuery.Data
            .Include(r => r.Hotel)
            .Include(r => r.SeasonalPricing)
            .Include(r => r.BookingRooms)
            .FirstOrDefaultAsync();

        if (roomType == null)
        {
            return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.NotFound, null);
        }

        var response = _mapper.Map<PublicRoomTypeResponseDto>(roomType);
        response.HotelName = roomType.Hotel?.Name ?? "";
        response.AvailableQuantity = roomType.AvailableQuantity;

        return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.Success, response);
    }

    public override async Task<OperationResponse<List<PublicRoomTypeResponseDto>>> GetAllAsync(
        PublicRoomTypeFiltersDto filters,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var query = await roomTypeRepo.GetAll();

        var roomTypesQuery = query.Data
            .Include(r => r.Hotel)
            .Include(r => r.SeasonalPricing)
            .Where(r => r.IsActive) // Always filter out soft-deleted records
            .AsQueryable();

        // Apply filters
        if (filters.HotelId.HasValue)
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.HotelId == filters.HotelId.Value);
        }

        if (!string.IsNullOrEmpty(filters.Name))
        {
            roomTypesQuery = roomTypesQuery.Where(r =>
                r.Name.Contains(filters.Name) ||
                (r.NameAr != null && r.NameAr.Contains(filters.Name)));
        }

        if (filters.MinGuests.HasValue)
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.MaxGuests >= filters.MinGuests.Value);
        }

        if (filters.MaxGuests.HasValue)
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.MaxGuests <= filters.MaxGuests.Value);
        }

        if (filters.MinPrice.HasValue)
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.BasePrice >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.BasePrice <= filters.MaxPrice.Value);
        }

        if (filters.IsActive.HasValue)
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.IsActive == filters.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(filters.ViewType))
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.ViewType == filters.ViewType);
        }

        if (!string.IsNullOrEmpty(filters.BedType))
        {
            roomTypesQuery = roomTypesQuery.Where(r => r.BedType == filters.BedType);
        }

        // Pagination
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            roomTypesQuery = roomTypesQuery
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var roomTypes = await roomTypesQuery.ToListAsync();
        var response = _mapper.Map<List<PublicRoomTypeResponseDto>>(roomTypes);

        foreach (var roomType in response)
        {
            var roomTypeEntity = roomTypes.First(r => r.Id == roomType.Id);
            roomType.HotelName = roomTypeEntity.Hotel?.Name ?? "";
            roomType.AvailableQuantity = roomTypeEntity.AvailableQuantity;
        }

        return new OperationResponse<List<PublicRoomTypeResponseDto>>(ErrorCodes.Success, response);
    }

    public async Task<OperationResponse<List<PublicRoomTypeResponseDto>>> GetByHotelAsync(Guid hotelId)
    {
        var filters = new PublicRoomTypeFiltersDto
        {
            HotelId = hotelId,
            IsActive = true
        };

        return await GetAllAsync(filters);
    }

    public async Task<OperationResponse<decimal>> GetRoomPriceAsync(Guid roomTypeId, DateTime date)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var pricingRepo = _repositoriesFactory.GetRepository<PublicRoomPricing>();

        var roomTypeQuery = await roomTypeRepo.Filter(r => r.Id == roomTypeId);
        var roomType = await roomTypeQuery.Data.FirstOrDefaultAsync();

        if (roomType == null)
        {
            return new OperationResponse<decimal>(ErrorCodes.NotFound, 0);
        }

        // Check for seasonal pricing
        var pricingQuery = await pricingRepo.Filter(p => p.RoomTypeId == roomTypeId);
        var applicablePricing = await pricingQuery.Data
            .Where(p => p.StartDate <= date &&
                       p.EndDate >= date)
            .OrderByDescending(p => p.Priority)
            .FirstOrDefaultAsync();

        var weekendDays = (_appSettings.WeekendDays ?? "Friday,Saturday")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(d => Enum.TryParse<DayOfWeek>(d, true, out var day) ? day : (DayOfWeek?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToHashSet();

        if (applicablePricing != null)
        {
            var isWeekend = weekendDays.Contains(date.DayOfWeek);
            var price = isWeekend ? applicablePricing.WeekendPrice : applicablePricing.WeekdayPrice;
            return new OperationResponse<decimal>(ErrorCodes.Success, price);
        }

        // Use base price
        var isWeekendBase = weekendDays.Contains(date.DayOfWeek);
        var basePrice = isWeekendBase && roomType.WeekendPrice.HasValue
            ? roomType.WeekendPrice.Value
            : roomType.BasePrice;

        return new OperationResponse<decimal>(ErrorCodes.Success, basePrice);
    }

    public async Task<OperationResponse<int>> GetAvailableRoomsAsync(
        Guid roomTypeId,
        DateTime checkIn,
        DateTime checkOut)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var bookingRoomRepo = _repositoriesFactory.GetRepository<PublicBookingRoom>();

        var roomTypeQuery = await roomTypeRepo.Filter(r => r.Id == roomTypeId);
        var roomType = await roomTypeQuery.Data.FirstOrDefaultAsync();

        if (roomType == null)
        {
            return new OperationResponse<int>(ErrorCodes.NotFound, 0);
        }

        // Get all bookings that overlap with the requested dates
        var bookingRoomQuery = await bookingRoomRepo.Filter(br => br.RoomTypeId == roomTypeId);
        var bookedRooms = await bookingRoomQuery.Data
            .Include(br => br.Booking)
            .Where(br => br.Booking.CheckInDate < checkOut &&
                        br.Booking.CheckOutDate > checkIn &&
                        (br.Booking.Status == SmartAndPro.ERP.SharedKernel.Enums.PublicBookingStatus.Confirmed ||
                         br.Booking.Status == SmartAndPro.ERP.SharedKernel.Enums.PublicBookingStatus.CheckedIn))
            .SumAsync(br => br.Quantity);

        // Use AvailableQuantity instead of TotalRooms for real-time inventory tracking
        var availableRooms = roomType.AvailableQuantity - bookedRooms;

        return new OperationResponse<int>(ErrorCodes.Success, availableRooms);
    }

    /// <summary>
    /// Link PublicRoomType to existing ERP RoomType for synchronization
    /// </summary>
    public async Task<OperationResponse<PublicRoomTypeResponseDto>> LinkToErpRoomTypeAsync(Guid publicRoomTypeId, Guid erpRoomTypeId)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        
        var query = await roomTypeRepo.Filter(r => r.Id == publicRoomTypeId);
        var roomType = query.Data?.FirstOrDefault();

        if (roomType == null)
        {
            return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.NotFound, null);
        }

        // Verify ERP RoomType exists
        var erpRoomTypeRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.RoomType>();
        var erpRoomTypeQuery = await erpRoomTypeRepo.Filter(r => r.Id == erpRoomTypeId);
        var erpRoomType = erpRoomTypeQuery.Data?.FirstOrDefault();

        if (erpRoomType == null)
        {
            return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.NotFound, null);
        }

        roomType.ErpRoomTypeId = erpRoomTypeId;
        var updateResult = await roomTypeRepo.Update(roomType);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicRoomTypeResponseDto>(updateResult.ErrorCode, null);
        }

        var response = _mapper.Map<PublicRoomTypeResponseDto>(roomType);
        return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.Success, response);
    }

    /// <summary>
    /// Unlink PublicRoomType from ERP RoomType
    /// </summary>
    public async Task<OperationResponse<PublicRoomTypeResponseDto>> UnlinkFromErpRoomTypeAsync(Guid publicRoomTypeId)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        
        var query = await roomTypeRepo.Filter(r => r.Id == publicRoomTypeId);
        var roomType = query.Data?.FirstOrDefault();

        if (roomType == null)
        {
            return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.NotFound, null);
        }

        roomType.ErpRoomTypeId = null;
        var updateResult = await roomTypeRepo.Update(roomType);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicRoomTypeResponseDto>(updateResult.ErrorCode, null);
        }

        var response = _mapper.Map<PublicRoomTypeResponseDto>(roomType);
        return new OperationResponse<PublicRoomTypeResponseDto>(ErrorCodes.Success, response);
    }
}
