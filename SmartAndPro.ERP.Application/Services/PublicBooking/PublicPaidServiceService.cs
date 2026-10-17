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

public class PublicPaidServiceService
    : ServiceBase<PublicPaidService, PublicPaidServicePostDto, PublicPaidServicePutDto, PublicPaidServiceResponseDto, PublicPaidServiceFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public PublicPaidServiceService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<List<PublicPaidServiceResponseDto>>> GetAllAsync(
        PublicPaidServiceFiltersDto filters = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        var serviceRepo = _repositoriesFactory.GetRepository<PublicPaidService>();
        var query = await serviceRepo.GetAll();

        var servicesQuery = query.Data.AsQueryable();

        // Apply filters if provided
        if (filters != null)
        {
            if (filters.IsActive.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.IsActive == filters.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(filters.ServiceTypeCode))
            {
                servicesQuery = servicesQuery.Where(s => s.ServiceTypeCode == filters.ServiceTypeCode);
            }
        }

        // Order by DisplayOrder and then by NameEn
        servicesQuery = servicesQuery
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.NameEn);

        // Apply pagination if provided
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            servicesQuery = servicesQuery
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var services = await servicesQuery.ToListAsync();
        var response = _mapper.Map<List<PublicPaidServiceResponseDto>>(services);

        return new OperationResponse<List<PublicPaidServiceResponseDto>>(ErrorCodes.Success, response);
    }

    /// <summary>
    /// Get only active paid services
    /// </summary>
    public async Task<OperationResponse<List<PublicPaidServiceResponseDto>>> GetActiveServicesAsync()
    {
        var serviceRepo = _repositoriesFactory.GetRepository<PublicPaidService>();
        var query = await serviceRepo.GetAll();

        var services = await query.Data
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.NameEn)
            .ToListAsync();

        var response = _mapper.Map<List<PublicPaidServiceResponseDto>>(services);

        return new OperationResponse<List<PublicPaidServiceResponseDto>>(ErrorCodes.Success, response);
    }

    public override async Task<OperationResponse<PublicPaidServiceResponseDto>> AddAsync(PublicPaidServicePostDto dto)
    {
        var serviceRepo = _repositoriesFactory.GetRepository<PublicPaidService>();

        // Create new service entity
        var service = new PublicPaidService
        {
            Id = Guid.NewGuid(),
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            DescriptionEn = dto.DescriptionEn,
            DescriptionAr = dto.DescriptionAr,
            Icon = dto.Icon,
            ServiceTypeCode = dto.ServiceTypeCode,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            CreatedBy = Guid.Empty, // System-created entities
            CreatedAt = DateTime.UtcNow
        };

        var result = await serviceRepo.Add(service);

        if (result.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicPaidServiceResponseDto>(result.ErrorCode, null);
        }

        var response = _mapper.Map<PublicPaidServiceResponseDto>(result.Data);
        return new OperationResponse<PublicPaidServiceResponseDto>(ErrorCodes.Success, response);
    }

    public override async Task<OperationResponse<PublicPaidServiceResponseDto>> UpdateAsync(PublicPaidServicePutDto dto)
    {
        var serviceRepo = _repositoriesFactory.GetRepository<PublicPaidService>();

        // Get existing service
        var existingQuery = await serviceRepo.GetAll();
        var existingService = await existingQuery.Data
            .FirstOrDefaultAsync(s => s.Id == dto.Id);

        if (existingService == null)
        {
            return new OperationResponse<PublicPaidServiceResponseDto>(ErrorCodes.NotFound, null);
        }

        // Update properties
        existingService.NameEn = dto.NameEn;
        existingService.NameAr = dto.NameAr;
        existingService.DescriptionEn = dto.DescriptionEn;
        existingService.DescriptionAr = dto.DescriptionAr;
        existingService.Icon = dto.Icon;
        existingService.ServiceTypeCode = dto.ServiceTypeCode;
        existingService.IsActive = dto.IsActive;
        existingService.DisplayOrder = dto.DisplayOrder;

        var result = await serviceRepo.Update(existingService);

        if (result.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicPaidServiceResponseDto>(result.ErrorCode, null);
        }

        var response = _mapper.Map<PublicPaidServiceResponseDto>(result.Data);
        return new OperationResponse<PublicPaidServiceResponseDto>(ErrorCodes.Success, response);
    }

    public override async Task<OperationResponse<PublicPaidServiceResponseDto>> DeleteAsync(Guid id)
    {
        var serviceRepo = _repositoriesFactory.GetRepository<PublicPaidService>();
        var hotelPaidServiceRepo = _repositoriesFactory.GetRepository<PublicHotelPaidService>();

        // Get existing service
        var existingQuery = await serviceRepo.GetAll();
        var existingService = await existingQuery.Data
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingService == null)
        {
            return new OperationResponse<PublicPaidServiceResponseDto>(ErrorCodes.NotFound, null);
        }

        // Check if service is used by any hotels
        var hotelServicesQuery = await hotelPaidServiceRepo.GetAll();
        var usedByHotels = await hotelServicesQuery.Data
            .AnyAsync(hps => hps.PublicPaidServiceId == id && hps.IsActive);

        if (usedByHotels)
        {
            return new OperationResponse<PublicPaidServiceResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                message: "Cannot delete this service as it is currently assigned to one or more hotels.");
        }

        // Soft delete
        var result = await serviceRepo.Delete(existingService);

        if (result.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<PublicPaidServiceResponseDto>(result.ErrorCode, null);
        }

        var response = _mapper.Map<PublicPaidServiceResponseDto>(existingService);
        return new OperationResponse<PublicPaidServiceResponseDto>(ErrorCodes.Success, response);
    }
}
