using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationView;
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

public class ReservationViewService
    : ServiceBase
    <
        ReservationView,
        ReservationViewPostDto,
        ReservationViewPutDto,
        ReservationViewResponseDto,
        ReservationViewFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public ReservationViewService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<ReservationViewResponseDto>>> GetAllFrontalAsync()
    {
        var frontalViews = await _repositoriesFactory.GetRepository<ReservationView>().Filter(rv => rv.IsFrontal);

        if (frontalViews.Data == null || !frontalViews.Data.Any())
        {
            return new OperationResponse<List<ReservationViewResponseDto>>(
                ErrorCodes.Success,
                new List<ReservationViewResponseDto>()
            );
        }

        var viewDtos = _mapper.Map<List<ReservationViewResponseDto>>(frontalViews.Data);

        return new OperationResponse<List<ReservationViewResponseDto>>(
            ErrorCodes.Success,
            viewDtos
        );
    }
    public override async Task<OperationResponse<ReservationViewResponseDto>> AddAsync(ReservationViewPostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationView>(true);
        var mainSalesRepo = _repositoriesFactory.GetRepository<MainSalesClass>();

        var result = new OperationResponse<ReservationViewResponseDto>(ErrorCodes.GeneralFail, null);

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

                if (exists.Data == null || !exists.Data.Any())
                {
                    var detailContainerSalesClassId = (await _repositoriesFactory.GetRepository<DetailSalesClass>()
                        .Filter(x => x.EnglishName == "Reservation Items" || x.ArabicName == "أصناف الحجوزات")).Data?.FirstOrDefault()?.Id;
                    if (detailContainerSalesClassId == null)
                        throw new Exception("Reservation Items Sales Class not found.");

                    var newSalesClass = new MainSalesClass
                    {
                        ArabicName = dto.ArabicName,
                        EnglishName = dto.EnglishName,
                        ReferenceCode = $"RES-VIEW-{result.Data.Id:N}",
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

                    if (!existingItem.IsReservationItem)
                    {
                        existingItem.IsReservationItem = true;
                        await mainSalesRepo.Update(existingItem);
                    }
                }

                // نحفظ الـ SalesClassId على الإطلالة
                var view = await _repositoriesFactory.GetRepository<ReservationView>()
                    .Filter(x => x.Id == result.Data.Id);
                var viewEntity = view.Data?.FirstOrDefault();
                if (viewEntity != null)
                {
                    viewEntity.SalesClassId = salesClassId;
                    await _repositoriesFactory.GetRepository<ReservationView>().Update(viewEntity);
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


    public async Task<OperationResponse<ReservationViewResponseDto>> MakeFrontalAsync(Guid viewId)
    {
        var viewQuery = await _repositoriesFactory.GetRepository<ReservationView>().Filter(rv => rv.Id == viewId);
        var view = viewQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (view == null)
        {
            return new OperationResponse<ReservationViewResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        view.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<ReservationView>().Update(view);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationViewResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<ReservationViewResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<ReservationViewResponseDto>(updateResult.Data)
        );
    }
}