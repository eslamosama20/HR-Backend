using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationService;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
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

public class ReservationServiceService
    : ServiceBase
    <
        SmartAndPro.ERP.Domains.Entities.Reservation.ReservationService,
        ReservationServicePostDto,
        ReservationServicePutDto,
        ReservationServiceResponseDto,
        ReservationServiceFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public ReservationServiceService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    private async Task<Guid?> ResolveMainAccountIdAsync(string arabicName, string englishName, Guid? detailAccountId, DbContext dbContext)
    {
        var normalizedArabic = arabicName?.Trim() ?? string.Empty;
        var normalizedEnglish = englishName?.Trim() ?? string.Empty;

        if (detailAccountId == null)
        {
            detailAccountId = await dbContext.Set<DetailAccount>()
                .IgnoreQueryFilters()
                .Where(da => da.ArabicName == "شركات الحجز")
                .Select(da => (Guid?)da.Id)
                .FirstOrDefaultAsync()
                ?? await dbContext.Set<DetailAccount>()
                    .IgnoreQueryFilters()
                    .Select(da => (Guid?)da.Id)
                    .FirstOrDefaultAsync();
        }

        if (detailAccountId == null) return null;

        var existingAccount = await dbContext.Set<MainAccount>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ma => ma.ArabicName == normalizedArabic
                                   && ma.DetailAccountId == detailAccountId);

        if (existingAccount != null) return existingAccount.Id;

        var nameTakenGlobally = await dbContext.Set<MainAccount>()
            .IgnoreQueryFilters()
            .AnyAsync(ma => ma.ArabicName == normalizedArabic
                         && ma.DetailAccountId != detailAccountId);
        if (nameTakenGlobally)
            throw new Exception($"Cannot create MainAccount: ArabicName '{normalizedArabic}' is already used under a different DetailAccount. / لا يمكن إنشاء حساب رئيسي: الاسﻢ '{normalizedArabic}' مستخدم بالفعل تحت حساب تفصيلي مختلف.");

        var englishNameToUse = string.IsNullOrWhiteSpace(normalizedEnglish)
            ? normalizedArabic
            : normalizedEnglish;

        var englishTaken = await dbContext.Set<MainAccount>()
            .IgnoreQueryFilters()
            .AnyAsync(ma => ma.EnglishName == englishNameToUse);
        if (englishTaken)
            englishNameToUse = englishNameToUse + "_" + Guid.NewGuid().ToString("N")[..6];

        var newAccount = new MainAccount
        {
            ArabicName      = normalizedArabic,
            EnglishName     = englishNameToUse,
            DetailAccountId = detailAccountId.Value,
            CreatedAt       = DateTime.UtcNow
        };

        dbContext.Set<MainAccount>().Add(newAccount);
        await dbContext.SaveChangesAsync();
        return newAccount.Id;
    }

    public async Task<OperationResponse<List<ReservationServiceResponseDto>>> GetAllFrontalAsync()
    {
        var frontalServices = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationService>().Filter(rs => rs.IsFrontal);

        if (frontalServices.Data == null || !frontalServices.Data.Any())
        {
            return new OperationResponse<List<ReservationServiceResponseDto>>(
                ErrorCodes.Success,
                new List<ReservationServiceResponseDto>()
            );
        }

        var serviceDtos = _mapper.Map<List<ReservationServiceResponseDto>>(frontalServices.Data);

        return new OperationResponse<List<ReservationServiceResponseDto>>(
            ErrorCodes.Success,
            serviceDtos
        );
    }
    public override async Task<OperationResponse<ReservationServiceResponseDto>> AddAsync(ReservationServicePostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationService>(true);
        var dbContext = repo.GetDbContext();
        var mainSalesRepo = _repositoriesFactory.GetRepository<MainSalesClass>();

        try
        {
            await repo.BeginTransactionAsync();

            if (dto.RevenueMainAccountId == null)
            {
                dto.RevenueMainAccountId = await ResolveMainAccountIdAsync(dto.ArabicName, dto.EnglishName, dto.RevenueDetailAccountId, dbContext);
            }
            if (dto.ExpenseMainAccountId == null)
            {
                dto.ExpenseMainAccountId = await ResolveMainAccountIdAsync(dto.ArabicName, dto.EnglishName, dto.ExpenseDetailAccountId, dbContext);
            }

            var result = await base.AddAsync(dto);

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
                        ReferenceCode = $"RES-SVC-{result.Data.Id:N}",
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

                // نحفظ الـ SalesClassId على الخدمة
                var svcRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationService>();
                var svc = await svcRepo.Filter(x => x.Id == result.Data.Id);
                var svcEntity = svc.Data?.FirstOrDefault();
                if (svcEntity != null)
                {
                    svcEntity.SalesClassId = salesClassId;
                    await svcRepo.Update(svcEntity);
                    result.Data.SalesClassId = salesClassId;
                }
            }

            await repo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationServiceResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message + (ex.InnerException?.Message ?? ""));
        }
    }

    public override async Task<OperationResponse<ReservationServiceResponseDto>> UpdateAsync(ReservationServicePutDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationService>(true);
        var dbContext = repo.GetDbContext();

        try
        {
            await repo.BeginTransactionAsync();

            if (dto.RevenueMainAccountId == null)
            {
                dto.RevenueMainAccountId = await ResolveMainAccountIdAsync(dto.ArabicName, dto.EnglishName, dto.RevenueDetailAccountId, dbContext);
            }
            if (dto.ExpenseMainAccountId == null)
            {
                dto.ExpenseMainAccountId = await ResolveMainAccountIdAsync(dto.ArabicName, dto.EnglishName, dto.ExpenseDetailAccountId, dbContext);
            }

            var result = await base.UpdateAsync(dto);
            if (result.ErrorCode != ErrorCodes.Success)
                throw new Exception("Reservation Service not updated");

            await repo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationServiceResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message + (ex.InnerException?.Message ?? ""));
        }
    }

    public async Task<OperationResponse<ReservationServiceResponseDto>> MakeFrontalAsync(Guid serviceId)
    {
        var serviceQuery = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationService>().Filter(rs => rs.Id == serviceId);
        var service = serviceQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (service == null)
        {
            return new OperationResponse<ReservationServiceResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        service.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.ReservationService>().Update(service);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationServiceResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<ReservationServiceResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<ReservationServiceResponseDto>(updateResult.Data)
        );
    }
}