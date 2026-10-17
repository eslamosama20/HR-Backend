using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.HeaderAccount;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class HeaderAccountService
    : ServiceBase
    <
        HeaderAccount,
        HeaderAccountPostDto,
        HeaderAccountPutDto,
        HeaderAccountResponseDto,
        HeaderAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public HeaderAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<HeaderAccountResponseDto>> AddAsync(HeaderAccountPostDto headerAccountPost)
    {
        var headerAccount = _mapper.Map<HeaderAccount>(headerAccountPost);
        var referenceCode = headerAccount.ReferenceCode;
        // check if root account or a headerAccount or a detailAccount or a MainAccount has that referenceCode
        var rootAccountRepository = _repositoriesFactory.GetRepository<RootAccount>();
        var rootAccountOperation = await rootAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (rootAccountOperation.Data.Any())
            return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.Conflict, null, message:"ReferenceCode already exists");
        
        var headerAccountRepository = _repositoriesFactory.GetRepository<HeaderAccount>();
        var headerAccountOperation = await headerAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (headerAccountOperation.Data.Any())
            return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var detailAccountRepository = _repositoriesFactory.GetRepository<DetailAccount>();
        var detailAccountOperation = await detailAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (detailAccountOperation.Data.Any())
            return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var mainAccountOperation = await mainAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (mainAccountOperation.Data.Any())
            return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        return await base.AddAsync(headerAccountPost);
    }

    public override async Task<OperationResponse<HeaderAccountResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            // check if header account - detailAccount - MainAccount has any transactionEntries
            var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
            var transactionEntries = await transactionEntryRepository.GetAll();
            var mainAccountsId = transactionEntries.Data.Select(x => x.MainAccountId).Distinct().ToList();
            var headerAccountRepository = _repositoriesFactory.GetRepository<HeaderAccount>();
            var headerAccount = await headerAccountRepository.Filter(x => x.Id == id);
            if (headerAccount.Data.Any(x =>
                    x.DetailAccounts.Any(y => y.MainAccounts.Any(w => mainAccountsId.Contains(w.Id)))))
                return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.GeneralFail, null);

            // check if header account has any detailAccounts
            if (headerAccount.Data.Any(x => x.DetailAccounts.Any()))
                return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.Conflict, null,
                    message:"HeaderAccount has detailAccounts");

            if (headerAccount.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.GeneralFail, null);

            var entity = headerAccount.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.NotFound, null);

            var operation = await _repositoriesFactory.GetRepository<HeaderAccount>().Delete(entity);
            return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return new OperationResponse<HeaderAccountResponseDto>(ErrorCodes.GeneralFail, null);
    }
    
}