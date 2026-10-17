using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class RoomTypeService
    : ServiceBase
    <
        RoomType,
        RoomTypePostDto,
        RoomTypePutDto,
        RoomTypeResponseDto,
        RoomTypeFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public RoomTypeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<RoomTypeResponseDto>>> GetAllFrontalAsync()
    {
        var frontalRoomTypes = await _repositoriesFactory.GetRepository<RoomType>().Filter(rt => rt.IsFrontal);

        if (frontalRoomTypes.Data == null || !frontalRoomTypes.Data.Any())
        {
            return new OperationResponse<List<RoomTypeResponseDto>>(
                ErrorCodes.Success,
                new List<RoomTypeResponseDto>()
            );
        }

        var roomTypeDtos = _mapper.Map<List<RoomTypeResponseDto>>(frontalRoomTypes.Data);

        return new OperationResponse<List<RoomTypeResponseDto>>(
            ErrorCodes.Success,
            roomTypeDtos
        );
    }

    public async Task<OperationResponse<PaginatedResult<List<RoomTypeLookupResponseDto>>>> GetPaginatedLookupAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? sortField = null)
    {
        try
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 100);

            var roomTypesQuery = await _repositoriesFactory.GetRepository<RoomType>().GetAll();
            if (roomTypesQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaginatedResult<List<RoomTypeLookupResponseDto>>>(roomTypesQuery.ErrorCode, null);

            var query = roomTypesQuery.Data
                .IgnoreAutoIncludes()
                .AsNoTracking();

            var totalRecords = await query.CountAsync();
            if (totalRecords == 0)
            {
                return new OperationResponse<PaginatedResult<List<RoomTypeLookupResponseDto>>>(
                    ErrorCodes.Success,
                    new PaginatedResult<List<RoomTypeLookupResponseDto>>(0, new List<RoomTypeLookupResponseDto>())
                );
            }

            query = (sortField ?? string.Empty).ToLowerInvariant() switch
            {
                "arabicname" => query.OrderBy(x => x.ArabicName),
                "englishname" => query.OrderBy(x => x.EnglishName),
                "personno" => query.OrderBy(x => x.PersonNo),
                "createdat" => query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderBy(x => x.EnglishName)
            };

            var skip = (pageNumber - 1) * pageSize;

            var data = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new RoomTypeLookupResponseDto
                {
                    Id = x.Id,
                    ArabicName = x.ArabicName,
                    EnglishName = x.EnglishName,
                    PersonNo = x.PersonNo,
                    IsFrontal = x.IsFrontal
                })
                .ToListAsync();

            return new OperationResponse<PaginatedResult<List<RoomTypeLookupResponseDto>>>(
                ErrorCodes.Success,
                new PaginatedResult<List<RoomTypeLookupResponseDto>>(totalRecords, data)
            );
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaginatedResult<List<RoomTypeLookupResponseDto>>>(
                ErrorCodes.GeneralFail,
                null,
                message: ex.Message
            );
        }
    }

    public override async Task<OperationResponse<RoomTypeResponseDto>> AddAsync(RoomTypePostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<RoomType>(true);
        var mainSalesRepo = _repositoriesFactory.GetRepository<MainSalesClass>();

        var result = new OperationResponse<RoomTypeResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            await repo.BeginTransactionAsync();

            result = await base.AddAsync(dto);

            if (result.Data != null)
            {
                var unitRepo = _repositoriesFactory.GetRepository<MeasurementUnit>();
                var unit = await unitRepo.Filter(x => x.EnglishName == "-");
                Guid measurementUnitId;

                if (unit.Data == null || !unit.Data.Any())
                {
                    var newUnit = new MeasurementUnit 
                    { 
                        EnglishName = "-", 
                        ArabicName = "-", 
                        IsActive = true, 
                        CreatedAt = DateTime.UtcNow 
                    };
                    await unitRepo.Add(newUnit);
                    measurementUnitId = newUnit.Id;
                }
                else
                {
                    measurementUnitId = unit.Data.First().Id;
                }

                var exists = await mainSalesRepo.Filter(x => (x.EnglishName == dto.EnglishName || x.ArabicName == dto.ArabicName) && x.IsReservationItem == true);
                Guid salesClassId;

                if (!exists.Data.Any())
                {
                    var detailContainerSalesClassId = (await _repositoriesFactory.GetRepository<DetailSalesClass>()
                    .Filter(x => x.EnglishName == "Reservation Items" || x.ArabicName == "أصناف الحجوزات")).Data?.FirstOrDefault()?.Id;
                    if (detailContainerSalesClassId == null)
                        throw new Exception("Reservation Items Sales Class not found.");

                    var newSalesClass = new MainSalesClass
                    {
                        ArabicName = dto.ArabicName,
                        EnglishName = dto.EnglishName,
                        ReferenceCode = $"RES-ROOM-{result.Data.Id:N}",
                        MeasurementUnitId = measurementUnitId,
                        ClassType = ClassType.Direct,
                        DetailSalesClassId = (Guid)detailContainerSalesClassId,
                        IsReservationItem = true
                    };
                    await mainSalesRepo.Add(newSalesClass);
                    salesClassId = newSalesClass.Id;
                }
                else
                {
                    var existingItem = exists.Data.First();
                    salesClassId = existingItem.Id;

                    // Update the existing item to ensure it is marked as a reservation item
                    if (!existingItem.IsReservationItem)
                    {
                        existingItem.IsReservationItem = true;
                        await mainSalesRepo.Update(existingItem);
                    }
                }

                // نحفظ الـ SalesClassId على نوع الغرفة
                var roomType = await _repositoriesFactory.GetRepository<RoomType>()
                    .Filter(x => x.Id == result.Data.Id);
                var roomTypeEntity = roomType.Data?.FirstOrDefault();
                if (roomTypeEntity != null)
                {
                    roomTypeEntity.SalesClassId = salesClassId;
                    await _repositoriesFactory.GetRepository<RoomType>().Update(roomTypeEntity);
                    result.Data.SalesClassId = salesClassId;
                }
            }

            await repo.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await repo.RollbackTransactionAsync();
            LogService.LogError(ex);
        }

        return result;
    }


    public async Task<OperationResponse<RoomTypeResponseDto>> MakeFrontalAsync(Guid roomTypeId)
    {
        var roomTypeQuery = await _repositoriesFactory.GetRepository<RoomType>().Filter(rt => rt.Id == roomTypeId);
        var roomType = roomTypeQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (roomType == null)
        {
            return new OperationResponse<RoomTypeResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        roomType.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<RoomType>().Update(roomType);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<RoomTypeResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<RoomTypeResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<RoomTypeResponseDto>(updateResult.Data)
        );
    }
}
