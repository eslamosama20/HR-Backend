using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Services.PublicBooking;

public class PublicHotelService
    : ServiceBase<PublicHotel, PublicHotelPostDto, PublicHotelPutDto, PublicHotelResponseDto, PublicHotelFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public PublicHotelService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<PublicHotelResponseDto>> GetAsync(Guid id)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        var hotelQuery = await hotelRepo.Filter(h => h.Id == id);

        var hotel = await hotelQuery.Data
            .Include(h => h.RoomTypes)
            .Include(h => h.Bookings)
            .Include(h => h.HotelPaidServices)
                .ThenInclude(hps => hps.PublicPaidService)
            .FirstOrDefaultAsync();

        if (hotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        var response = _mapper.Map<PublicHotelResponseDto>(hotel);
        response.RoomTypesCount = hotel.RoomTypes?.Count ?? 0;
        response.BookingsCount = hotel.Bookings?.Count ?? 0;

        // Map paid services
        if (hotel.HotelPaidServices?.Any() == true)
        {
            response.PaidServices = hotel.HotelPaidServices
                .Where(hps => hps.IsActive)
                .Select(hps => new PublicHotelPaidServiceDto
                {
                    Id = hps.Id,
                    PublicPaidServiceId = hps.PublicPaidServiceId,
                    Price = hps.Price,
                    IsActive = hps.IsActive,
                    Notes = hps.Notes,
                    Service = _mapper.Map<PublicPaidServiceResponseDto>(hps.PublicPaidService)
                })
                .ToList();
        }

        return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.Success, response);
    }

    public override async Task<OperationResponse<List<PublicHotelResponseDto>>> GetAllAsync(
        PublicHotelFiltersDto filters, 
        int? pageNumber = null, 
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        var query = await hotelRepo.GetAll();

        var hotelsQuery = query.Data
            .Include(h => h.RoomTypes)
            .Include(h => h.Bookings)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filters.Name))
        {
            hotelsQuery = hotelsQuery.Where(h => 
                h.Name.Contains(filters.Name) || 
                (h.NameAr != null && h.NameAr.Contains(filters.Name)));
        }

        if (!string.IsNullOrEmpty(filters.City))
        {
            hotelsQuery = hotelsQuery.Where(h => h.City == filters.City);
        }

        if (!string.IsNullOrEmpty(filters.Country))
        {
            hotelsQuery = hotelsQuery.Where(h => h.Country == filters.Country);
        }

        if (filters.MinStars.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => h.Stars >= filters.MinStars.Value);
        }

        if (filters.MaxStars.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => h.Stars <= filters.MaxStars.Value);
        }

        if (filters.IsActive.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => h.IsActive == filters.IsActive.Value);
        }

        if (filters.IsFeatured.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => h.IsFeatured == filters.IsFeatured.Value);
        }

        // Geo search (simple distance calculation)
        if (filters.Latitude.HasValue && filters.Longitude.HasValue && filters.RadiusInKm.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => 
                h.Latitude.HasValue && h.Longitude.HasValue &&
                Math.Sqrt(
                    Math.Pow((h.Latitude.Value - filters.Latitude.Value) * 111.0, 2) +
                    Math.Pow((h.Longitude.Value - filters.Longitude.Value) * 111.0 * Math.Cos(filters.Latitude.Value * Math.PI / 180), 2)
                ) <= filters.RadiusInKm.Value
            );
        }

        // Pagination
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            hotelsQuery = hotelsQuery
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var hotels = await hotelsQuery.ToListAsync();
        var response = _mapper.Map<List<PublicHotelResponseDto>>(hotels);

        foreach (var hotel in response)
        {
            var hotelEntity = hotels.First(h => h.Id == hotel.Id);
            hotel.RoomTypesCount = hotelEntity.RoomTypes?.Count ?? 0;
            hotel.BookingsCount = hotelEntity.Bookings?.Count ?? 0;
        }

        return new OperationResponse<List<PublicHotelResponseDto>>(ErrorCodes.Success, response);
    }

    public async Task<OperationResponse<List<PublicHotelResponseDto>>> GetFeaturedHotelsAsync()
    {
        var filters = new PublicHotelFiltersDto
        {
            IsFeatured = true,
            IsActive = true
        };

        return await GetAllAsync(filters);
    }

    public async Task<OperationResponse<List<PublicHotelResponseDto>>> SearchHotelsAsync(
        string? searchTerm,
        DateTime? checkIn,
        DateTime? checkOut,
        int? guests)
    {
        var filters = new PublicHotelFiltersDto
        {
            Name = searchTerm,
            IsActive = true,
            CheckIn = checkIn,
            CheckOut = checkOut,
            GuestsCount = guests
        };

        return await GetAllAsync(filters);
    }

    public async Task<OperationResponse<PublicHotelResponseDto>> GetHotelWithRoomsAsync(Guid hotelId)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();

        var query = await hotelRepo.Filter(h => h.Id == hotelId);
        var hotel = await query.Data
            .Include(h => h.RoomTypes.Where(r => r.IsActive))
            .FirstOrDefaultAsync();

        if (hotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        var response = _mapper.Map<PublicHotelResponseDto>(hotel);
        response.RoomTypes = _mapper.Map<List<PublicRoomTypeResponseDto>>(hotel.RoomTypes);
        response.RoomTypesCount = hotel.RoomTypes?.Count ?? 0;

        return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.Success, response);
    }

    /// <summary>
    /// Link PublicHotel to existing ERP Hotel for synchronization
    /// </summary>
    public async Task<OperationResponse<PublicHotelResponseDto>> LinkToErpHotelAsync(Guid publicHotelId, Guid erpHotelId)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        
        var query = await hotelRepo.Filter(h => h.Id == publicHotelId);
        var hotel = query.Data?.FirstOrDefault();

        if (hotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        // Verify ERP Hotel exists
        var erpHotelRepo = _repositoriesFactory.GetRepository<Domains.Entities.Reservation.Hotel>();
        var erpHotelQuery = await erpHotelRepo.Filter(h => h.Id == erpHotelId);
        var erpHotel = erpHotelQuery.Data?.FirstOrDefault();

        if (erpHotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        hotel.ErpHotelId = erpHotelId;
        var updateResult = await hotelRepo.Update(hotel);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicHotelResponseDto>(updateResult.ErrorCode, null);
        }

        var response = _mapper.Map<PublicHotelResponseDto>(hotel);
        return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.Success, response);
    }

    /// <summary>
    /// Unlink PublicHotel from ERP Hotel
    /// </summary>
    public async Task<OperationResponse<PublicHotelResponseDto>> UnlinkFromErpHotelAsync(Guid publicHotelId)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        
        var query = await hotelRepo.Filter(h => h.Id == publicHotelId);
        var hotel = query.Data?.FirstOrDefault();

        if (hotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        hotel.ErpHotelId = null;
        var updateResult = await hotelRepo.Update(hotel);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicHotelResponseDto>(updateResult.ErrorCode, null);
        }

        var response = _mapper.Map<PublicHotelResponseDto>(hotel);
        return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.Success, response);
    }

    /// <summary>
    /// Override UpdateAsync to handle paid services
    /// </summary>
    public override async Task<OperationResponse<PublicHotelResponseDto>> UpdateAsync(PublicHotelPutDto request)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        var hotelPaidServiceRepo = _repositoriesFactory.GetRepository<PublicHotelPaidService>();

        // Get existing hotel with paid services
        var query = await hotelRepo.Filter(h => h.Id == request.Id);
        var existingHotel = query.Data?
            .Include(h => h.HotelPaidServices)
            .FirstOrDefault();

        if (existingHotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        _mapper.Map(request, existingHotel);

        if (request.PaidServices != null)
        {
            var existingServices = existingHotel.HotelPaidServices.ToList();

            // Update or add services
            foreach (var serviceDto in request.PaidServices)
            {
                var existing = existingServices
                    .FirstOrDefault(s => s.PublicPaidServiceId == serviceDto.PublicPaidServiceId);

                if (existing != null)
                {
                    existing.Price = serviceDto.Price;
                    existing.IsActive = serviceDto.IsActive;
                    existing.Notes = serviceDto.Notes;
                    await hotelPaidServiceRepo.Update(existing);
                }
                else
                {
                    var newService = new PublicHotelPaidService
                    {
                        PublicHotelId = request.Id,
                        PublicPaidServiceId = serviceDto.PublicPaidServiceId,
                        Price = serviceDto.Price,
                        IsActive = serviceDto.IsActive,
                        Notes = serviceDto.Notes
                    };
                    await hotelPaidServiceRepo.Add(newService);
                }
            }

            var servicesToRemove = existingServices
                .Where(es => !request.PaidServices.Any(ps => ps.PublicPaidServiceId == es.PublicPaidServiceId))
                .ToList();

            foreach (var serviceToRemove in servicesToRemove)
            {
                await hotelPaidServiceRepo.Delete(serviceToRemove);
            }
        }

        // Update the hotel itself
        var updateResult = await hotelRepo.Update(existingHotel);
        
        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicHotelResponseDto>(updateResult.ErrorCode, null);
        }

        // Return updated hotel with all data
        return await GetAsync(request.Id);
    }

    /// <summary>
    /// Add or update paid services for a hotel
    /// </summary>
    public async Task<OperationResponse<PublicHotelResponseDto>> UpdateHotelPaidServicesAsync(
        Guid hotelId, 
        List<PublicHotelPaidServiceDto> paidServices)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<PublicHotel>();
        var hotelPaidServiceRepo = _repositoriesFactory.GetRepository<PublicHotelPaidService>();
        
        var query = await hotelRepo.Filter(h => h.Id == hotelId);
        var hotel = query.Data?
            .Include(h => h.HotelPaidServices)
            .FirstOrDefault();

        if (hotel == null)
        {
            return new OperationResponse<PublicHotelResponseDto>(ErrorCodes.NotFound, null);
        }

        // Get existing services
        var existingServices = hotel.HotelPaidServices.ToList();

        // Update or add services
        foreach (var serviceDto in paidServices)
        {
            var existing = existingServices
                .FirstOrDefault(s => s.PublicPaidServiceId == serviceDto.PublicPaidServiceId);

            if (existing != null)
            {
                // Update existing
                existing.Price = serviceDto.Price;
                existing.IsActive = serviceDto.IsActive;
                existing.Notes = serviceDto.Notes;
                await hotelPaidServiceRepo.Update(existing);
            }
            else
            {
                // Add new
                var newService = new PublicHotelPaidService
                {
                    PublicHotelId = hotelId,
                    PublicPaidServiceId = serviceDto.PublicPaidServiceId,
                    Price = serviceDto.Price,
                    IsActive = serviceDto.IsActive,
                    Notes = serviceDto.Notes
                };
                await hotelPaidServiceRepo.Add(newService);
            }
        }

        // Remove services not in the list
        var servicesToRemove = existingServices
            .Where(es => !paidServices.Any(ps => ps.PublicPaidServiceId == es.PublicPaidServiceId))
            .ToList();

        foreach (var serviceToRemove in servicesToRemove)
        {
            await hotelPaidServiceRepo.Delete(serviceToRemove);
        }

        // Return updated hotel
        return await GetAsync(hotelId);
    }

    /// <summary>
    /// Get all available paid services (for selection in admin)
    /// </summary>
    public async Task<OperationResponse<List<PublicPaidServiceResponseDto>>> GetAllPaidServicesAsync()
    {
        var serviceRepo = _repositoriesFactory.GetRepository<PublicPaidService>();
        var query = await serviceRepo.GetAll();

        var services = await query.Data
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        var response = _mapper.Map<List<PublicPaidServiceResponseDto>>(services);
        return new OperationResponse<List<PublicPaidServiceResponseDto>>(ErrorCodes.Success, response);
    }
}
