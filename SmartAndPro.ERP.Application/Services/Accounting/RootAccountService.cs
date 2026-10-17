using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.RootAccount;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Application.Common.Extensions;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class RootAccountService
    : ServiceBase
    <
        RootAccount,
        RootAccountPostDto,
        RootAccountPutDto,
        RootAccountResponseDto,
        RootAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public RootAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<List<RootAccountResponseDto>>> GetAllAsync(
        RootAccountFiltersDto filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<RootAccountResponseDto>>(ErrorCodes.GeneralFail, null);
        try
        {
            var operation = await _repositoriesFactory.GetRepository<RootAccount>().GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<RootAccountResponseDto>>(operation.ErrorCode, null);

            IQueryable<RootAccount> query = operation.Data
                .IgnoreAutoIncludes()
                .Include(x => x.HeaderAccounts)
                    .ThenInclude(x => x.ClientProfile)
                .Include(x => x.HeaderAccounts)
                    .ThenInclude(x => x.DetailAccounts)
                        .ThenInclude(x => x.ClientProfile)
                .Include(x => x.HeaderAccounts)
                    .ThenInclude(x => x.DetailAccounts)
                        .ThenInclude(x => x.MainAccounts)
                            .ThenInclude(x => x.ClientProfile);

            var totalRecords = await query.CountAsync();

            pageNumber ??= 1;
            pageSize ??= 50;
            if (pageSize > 100) pageSize = 100;

            if (pageNumber > 0 && pageSize > 0)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
            }

            query = query.ApplySearchKeyword(filters.SearchKeyword);

            var operationData =
                filters.AllPropertiesAreNull()
                    ? query!.ToList()
                    : query!.FilterQueryableData(filters).ToList();

            if (operationData.Count == 0)
                return new OperationResponse<List<RootAccountResponseDto>>(ErrorCodes.NoContent, null);

            var responseData = _mapper.Map<List<RootAccountResponseDto>>(operationData);
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                return new OperationResponse<List<RootAccountResponseDto>>(
                    ErrorCodes.Success,
                    responseData,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<RootAccountResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<RootAccountResponseDto>> AddAsync(RootAccountPostDto rootAccountPost)
    {
        var rootAccount = _mapper.Map<RootAccount>(rootAccountPost);
        var referenceCode = rootAccount.ReferenceCode;

        var context = (ERPContext)_repositoriesFactory.GetRepository<RootAccount>().GetDbContext();

        if (await context.RootAccounts.AnyAsync(x => x.ReferenceCode == referenceCode))
            return new OperationResponse<RootAccountResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");

        if (await context.HeaderAccounts.AnyAsync(x => x.ReferenceCode == referenceCode))
            return new OperationResponse<RootAccountResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");

        if (await context.DetailAccounts.AnyAsync(x => x.ReferenceCode == referenceCode))
            return new OperationResponse<RootAccountResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");

        if (await context.MainAccounts.AnyAsync(x => x.ReferenceCode == referenceCode))
            return new OperationResponse<RootAccountResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");

        return await base.AddAsync(rootAccountPost);
    }
    

    public override async Task<OperationResponse<RootAccountResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            var context = (ERPContext)_repositoriesFactory.GetRepository<RootAccount>().GetDbContext();

            if (await context.TransactionEntries.AnyAsync(te =>
                    context.MainAccounts.Any(ma =>
                        ma.Id == te.MainAccountId &&
                        context.DetailAccounts.Any(da =>
                            da.Id == ma.DetailAccountId &&
                            context.HeaderAccounts.Any(ha =>
                                ha.Id == da.HeaderAccountId && ha.RootAccountId == id)))))
                return new OperationResponse<RootAccountResponseDto>(ErrorCodes.GeneralFail, null);

            if (await context.HeaderAccounts.AnyAsync(h => h.RootAccountId == id))
                return new OperationResponse<RootAccountResponseDto>(ErrorCodes.Conflict, null,
                    message: "RootAccount has headerAccounts");

            var rootAccount = await context.RootAccounts.FirstOrDefaultAsync(r => r.Id == id);
            if (rootAccount is null)
                return new OperationResponse<RootAccountResponseDto>(ErrorCodes.NotFound, null);

            var operation = await _repositoriesFactory.GetRepository<RootAccount>().Delete(rootAccount);
            return new OperationResponse<RootAccountResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<RootAccountResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }
    
}