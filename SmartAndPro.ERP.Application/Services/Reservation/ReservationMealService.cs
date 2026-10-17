using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationMeal;
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

public class ReservationMealService
    : ServiceBase
    <
        ReservationMeal,
        ReservationMealPostDto,
        ReservationMealPutDto,
        ReservationMealResponseDto,
        ReservationMealFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public ReservationMealService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<ReservationMealResponseDto>>> GetAllFrontalAsync()
    {
        var frontalMeals = await _repositoriesFactory.GetRepository<ReservationMeal>().Filter(rm => rm.IsFrontal);

        if (frontalMeals.Data == null || !frontalMeals.Data.Any())
        {
            return new OperationResponse<List<ReservationMealResponseDto>>(
                ErrorCodes.Success,
                new List<ReservationMealResponseDto>()
            );
        }

        var mealDtos = _mapper.Map<List<ReservationMealResponseDto>>(frontalMeals.Data);

        return new OperationResponse<List<ReservationMealResponseDto>>(
            ErrorCodes.Success,
            mealDtos
        );
    }
    public override async Task<OperationResponse<ReservationMealResponseDto>> AddAsync(ReservationMealPostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationMeal>(true);
        var mainSalesRepo = _repositoriesFactory.GetRepository<MainSalesClass>();

        var result = new OperationResponse<ReservationMealResponseDto>(ErrorCodes.GeneralFail, null);

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
                        ReferenceCode = $"RES-MEAL-{result.Data.Id:N}",
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
                    // صنف موجود بالفعل - نتأكد إنه مميز كحجز
                    var existingItem = exists.Data.First();
                    salesClassId = existingItem.Id;

                    if (!existingItem.IsReservationItem)
                    {
                        existingItem.IsReservationItem = true;
                        await mainSalesRepo.Update(existingItem);
                    }
                }

                // نحفظ الـ SalesClassId على الوجبة
                var meal = await _repositoriesFactory.GetRepository<ReservationMeal>()
                    .Filter(x => x.Id == result.Data.Id);
                var mealEntity = meal.Data?.FirstOrDefault();
                if (mealEntity != null)
                {
                    mealEntity.SalesClassId = salesClassId;
                    await _repositoriesFactory.GetRepository<ReservationMeal>().Update(mealEntity);
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


    public async Task<OperationResponse<ReservationMealResponseDto>> MakeFrontalAsync(Guid mealId)
    {
        var mealQuery = await _repositoriesFactory.GetRepository<ReservationMeal>().Filter(rm => rm.Id == mealId);
        var meal = mealQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (meal == null)
        {
            return new OperationResponse<ReservationMealResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        meal.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<ReservationMeal>().Update(meal);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationMealResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<ReservationMealResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<ReservationMealResponseDto>(updateResult.Data)
        );
    }
}