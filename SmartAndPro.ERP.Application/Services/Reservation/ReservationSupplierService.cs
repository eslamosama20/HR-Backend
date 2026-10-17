using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSupplier;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class ReservationSupplierService
    : ServiceBase
    <
        ReservationSupplier,
        ReservationSupplierPostDto,
        ReservationSupplierPutDto,
        ReservationSupplierResponseDto,
        ReservationSupplierFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public ReservationSupplierService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
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
                .Where(da => da.ArabicName == "دائنون")
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

    public override async Task<OperationResponse<ReservationSupplierResponseDto>> AddAsync(ReservationSupplierPostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationSupplier>(true);
        var dbContext = repo.GetDbContext();

        try
        {
            await repo.BeginTransactionAsync();

            if (dto.MainAccountId == null)
            {
                dto.MainAccountId = await ResolveMainAccountIdAsync(dto.ArabicName, dto.EnglishName, dto.DetailAccountId, dbContext);
            }
            var result = await base.AddAsync(dto);
            if (result.ErrorCode != ErrorCodes.Success)
                throw new Exception("Reservation Supplier not added");

            await repo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationSupplierResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message + (ex.InnerException?.Message ?? ""));
        }
    }

    public override async Task<OperationResponse<ReservationSupplierResponseDto>> UpdateAsync(ReservationSupplierPutDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationSupplier>(true);
        var dbContext = repo.GetDbContext();

        try
        {
            await repo.BeginTransactionAsync();

            if (dto.MainAccountId == null)
            {
                dto.MainAccountId = await ResolveMainAccountIdAsync(dto.ArabicName, dto.EnglishName, dto.DetailAccountId, dbContext);
            }

            var result = await base.UpdateAsync(dto);
            if (result.ErrorCode != ErrorCodes.Success)
                throw new Exception("Reservation Supplier not updated");

            await repo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationSupplierResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message + (ex.InnerException?.Message ?? ""));
        }
    }

    public async Task<OperationResponse<List<ReservationSupplierResponseDto>>> GetAllFrontalAsync()
    {
        var frontalSuppliers = await _repositoriesFactory.GetRepository<ReservationSupplier>().Filter(rs => rs.IsFrontal);

        if (frontalSuppliers.Data == null || !frontalSuppliers.Data.Any())
        {
            return new OperationResponse<List<ReservationSupplierResponseDto>>(
                ErrorCodes.Success,
                new List<ReservationSupplierResponseDto>()
            );
        }

        var supplierDtos = _mapper.Map<List<ReservationSupplierResponseDto>>(frontalSuppliers.Data);

        return new OperationResponse<List<ReservationSupplierResponseDto>>(
            ErrorCodes.Success,
            supplierDtos
        );
    }

    public async Task<OperationResponse<ReservationSupplierResponseDto>> MakeFrontalAsync(Guid supplierId)
    {
        var supplierQuery = await _repositoriesFactory.GetRepository<ReservationSupplier>().Filter(rs => rs.Id == supplierId);
        var supplier = supplierQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (supplier == null)
        {
            return new OperationResponse<ReservationSupplierResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        supplier.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<ReservationSupplier>().Update(supplier);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationSupplierResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<ReservationSupplierResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<ReservationSupplierResponseDto>(updateResult.Data)
        );
    }
}