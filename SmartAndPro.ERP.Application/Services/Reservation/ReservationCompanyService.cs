using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationCompany;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class ReservationCompanyService
    : ServiceBase
    <
        ReservationCompany,
        ReservationCompanyPostDto,
        ReservationCompanyPutDto,
        ReservationCompanyResponseDto,
        ReservationCompanyFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public ReservationCompanyService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
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

    private async Task SyncCustomerProfileAsync(ReservationCompany companyEntity, DbContext dbContext)
    {
        if (companyEntity.CustomerProfileId == null)
        {
            var existingProfile = await dbContext.Set<CustomerProfile>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cp => cp.ArabicName == companyEntity.ArabicName && cp.IsReservationCompany == true);

            if (existingProfile != null)
            {
                companyEntity.CustomerProfileId = existingProfile.Id;
                MapCompanyToCustomer(companyEntity, existingProfile);
            }
            else
            {
                var customerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    IsReservationCompany = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                MapCompanyToCustomer(companyEntity, customerProfile);
                await dbContext.Set<CustomerProfile>().AddAsync(customerProfile);
                
                companyEntity.CustomerProfileId = customerProfile.Id;
            }
        }
        else
        {
            var customerProfile = await dbContext.Set<CustomerProfile>().FindAsync(companyEntity.CustomerProfileId);
            if (customerProfile != null)
            {
                MapCompanyToCustomer(companyEntity, customerProfile);
            }
        }
    }

    private void MapCompanyToCustomer(ReservationCompany company, CustomerProfile customer)
    {
        customer.ArabicName = company.ArabicName;
        customer.EnglishName = company.EnglishName;
        customer.MainAccountId = company.MainAccountId;
        customer.ContactNumber = company.ContactNumber;
        customer.Email = company.Email;
        customer.BankName = company.BankName;
        customer.BankAccountNumber = company.BankAccountNumber;
        customer.IBAN = company.IBAN;
        customer.TaxNumber = company.TaxNumber;
        customer.Street = company.Street;
        customer.District = company.District;
        customer.BuildingNumber = company.BuildingNumber;
        customer.SubNumber = company.SubNumber;
        customer.PostalCode = company.PostalCode;
        customer.City = company.City;
        customer.Country = company.Country;
        customer.ShortAddress = company.ShortAddress;
        customer.AdditionalContactNumber = company.AdditionalContactNumber;
        customer.Fax = company.Fax;
        customer.Website = company.Website;
        customer.Instructions = company.Instructions;
        customer.GaztNumber = company.GaztNumber;
        customer.IsActive = company.IsActive;
        customer.IsReservationCompany = true;
    }

    public override async Task<OperationResponse<ReservationCompanyResponseDto>> AddAsync(ReservationCompanyPostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationCompany>(true);
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
                throw new Exception("ReservationCompany not added");

            var companyEntity = await dbContext.Set<ReservationCompany>().FindAsync(result.Data.Id);
            if (companyEntity != null)
            {
                await SyncCustomerProfileAsync(companyEntity, dbContext);
                await dbContext.SaveChangesAsync();
            }

            await repo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationCompanyResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message + (ex.InnerException?.Message ?? ""));
        }
    }

    public override async Task<OperationResponse<ReservationCompanyResponseDto>> UpdateAsync(ReservationCompanyPutDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationCompany>(true);
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
                throw new Exception("ReservationCompany not updated");

            var companyEntity = await dbContext.Set<ReservationCompany>().FindAsync(dto.Id);
            if (companyEntity != null)
            {
                await SyncCustomerProfileAsync(companyEntity, dbContext);
                await dbContext.SaveChangesAsync();
            }

            await repo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationCompanyResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message + (ex.InnerException?.Message ?? ""));
        }
    }

    public async Task<OperationResponse<List<ReservationCompanyResponseDto>>> GetAllFrontalAsync()
    {
        var frontalCompanies = await _repositoriesFactory.GetRepository<ReservationCompany>().Filter(rc => rc.IsFrontal);

        if (frontalCompanies.Data == null || !frontalCompanies.Data.Any())
        {
            return new OperationResponse<List<ReservationCompanyResponseDto>>(
                ErrorCodes.Success,
                new List<ReservationCompanyResponseDto>()
            );
        }

        var companyDtos = _mapper.Map<List<ReservationCompanyResponseDto>>(frontalCompanies.Data);

        return new OperationResponse<List<ReservationCompanyResponseDto>>(
            ErrorCodes.Success,
            companyDtos
        );
    }

    public async Task<OperationResponse<ReservationCompanyResponseDto>> MakeFrontalAsync(Guid companyId)
    {
        var companyQuery = await _repositoriesFactory.GetRepository<ReservationCompany>().Filter(
            rc => rc.Id == companyId
        );
        var company = companyQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (company == null)
        {
            return new OperationResponse<ReservationCompanyResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        company.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<ReservationCompany>().Update(company);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationCompanyResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<ReservationCompanyResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<ReservationCompanyResponseDto>(updateResult.Data)
        );
    }

    public override async Task<OperationResponse<ReservationCompanyResponseDto>> DeleteAsync(Guid id)
    {
        return await DeleteRangeAsync(new List<Guid> { id });
    }

    public override async Task<OperationResponse<ReservationCompanyResponseDto>> DeleteRangeAsync(List<Guid> ids)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationCompany>(true);
        var dbContext = repo.GetDbContext();

        try
        {
            await repo.BeginTransactionAsync();

            var companies = await dbContext.Set<ReservationCompany>()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (!companies.Any())
                return new OperationResponse<ReservationCompanyResponseDto>(ErrorCodes.NotFound, null);

            var accountsToDelete = companies
                .Where(p => p.MainAccountId.HasValue)
                .Select(p => p.MainAccountId!.Value)
                .Distinct()
                .ToList();

            if (accountsToDelete.Any())
            {
                var hasLinkedEntries = await dbContext.Set<TransactionEntry>()
                    .AnyAsync(te => accountsToDelete.Contains(te.MainAccountId));

                if (hasLinkedEntries)
                    return new OperationResponse<ReservationCompanyResponseDto>(
                        ErrorCodes.Conflict, null,
                        message: "لا يمكن حذف الشركة لأن الحساب المرتبط بها يحتوي على قيود محاسبية.");
            }

            var customerProfileIds = companies
                .Where(c => c.CustomerProfileId.HasValue)
                .Select(c => c.CustomerProfileId!.Value)
                .Distinct()
                .ToList();

            var customerProfiles = new List<CustomerProfile>();
            if (customerProfileIds.Any())
            {
                customerProfiles = await dbContext.Set<CustomerProfile>()
                    .Where(p => customerProfileIds.Contains(p.Id))
                    .ToListAsync();
            }

            if (accountsToDelete.Any())
            {
                foreach (var company in companies.Where(p => p.MainAccountId.HasValue))
                {
                    company.MainAccountId = null;
                    company.MainAccount = null;
                }
                foreach (var profile in customerProfiles.Where(p => p.MainAccountId.HasValue))
                {
                    profile.MainAccountId = null;
                    profile.MainAccount = null;
                }
                await dbContext.SaveChangesAsync();

                var accountsEntities = await dbContext.Set<MainAccount>()
                    .Where(ma => accountsToDelete.Contains(ma.Id))
                    .ToListAsync();

                dbContext.Set<MainAccount>().RemoveRange(accountsEntities);
                await dbContext.SaveChangesAsync();
            }

            foreach (var profile in customerProfiles)
                profile.SoftDelete();

            foreach (var company in companies)
                company.SoftDelete();

            await dbContext.SaveChangesAsync();
            await repo.CommitTransactionAsync();

            return new OperationResponse<ReservationCompanyResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { }
            LogService.LogError(ex);
            return new OperationResponse<ReservationCompanyResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public async Task<OperationResponse<string>> SyncLegacyCompaniesAsync()
    {
        try
        {
            var dbContext = _repositoriesFactory.GetRepository<ReservationCompany>().GetDbContext();
            
            var unsyncedCompanies = await dbContext.Set<ReservationCompany>()
                .Where(c => c.CustomerProfileId == null)
                .ToListAsync();

            if (!unsyncedCompanies.Any())
            {
                return new OperationResponse<string>(ErrorCodes.Success, "0", message: "No legacy companies found to sync.");
            }

            int count = 0;
            foreach (var company in unsyncedCompanies)
            {
                await SyncCustomerProfileAsync(company, dbContext);
                count++;
            }

            await dbContext.SaveChangesAsync();
            return new OperationResponse<string>(ErrorCodes.Success, count.ToString(), message: $"Successfully synced {count} legacy reservation companies.");
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<string>(ErrorCodes.GeneralFail, "0", message: ex.Message);
        }
    }
}