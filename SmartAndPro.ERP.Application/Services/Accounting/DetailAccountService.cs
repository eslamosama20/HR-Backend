using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.DetailAccount;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class DetailAccountService
    : ServiceBase
    <
        DetailAccount,
        DetailAccountPostDto,
        DetailAccountPutDto,
        DetailAccountResponseDto,
        DetailAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public DetailAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<DetailAccountResponseDto>> AddAsync(DetailAccountPostDto detailAccountPost)
    {
        var detailAccount = _mapper.Map<DetailAccount>(detailAccountPost);
        var referenceCode = detailAccount.ReferenceCode;
        // check if root account or a headerAccount or a detailAccount or a MainAccount has that referenceCode
        var rootAccountRepository = _repositoriesFactory.GetRepository<RootAccount>();
        var rootAccountOperation = await rootAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (rootAccountOperation.Data.Any())
            return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var headerAccountRepository = _repositoriesFactory.GetRepository<HeaderAccount>();
        var headerAccountOperation = await headerAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (headerAccountOperation.Data.Any())
            return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var detailAccountRepository = _repositoriesFactory.GetRepository<DetailAccount>();
        var detailAccountOperation = await detailAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (detailAccountOperation.Data.Any())
            return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var mainAccountOperation = await mainAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (mainAccountOperation.Data.Any())
            return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        return await base.AddAsync(detailAccountPost);
    }
    
    public override async Task<OperationResponse<DetailAccountResponseDto>> DeleteAsync(Guid id)
    {
        try{
            // check if detail account - MainAccount has any transactionEntries
            var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
            var transactionEntries = await transactionEntryRepository.GetAll();
            var mainAccountsId = transactionEntries.Data.Select(x => x.MainAccountId).Distinct().ToList();
            var detailAccountRepository = _repositoriesFactory.GetRepository<DetailAccount>();
            var detailAccount = await detailAccountRepository.Filter(x => x.Id == id);
            if (detailAccount.Data.Any(x => x.MainAccounts.Any(w => mainAccountsId.Contains(w.Id))))
                return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.GeneralFail, null);

            // check if detail account has any MainAccounts
            if (detailAccount.Data.Any(x => x.MainAccounts.Any()))
                return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.Conflict, null,
                    message:"DetailAccount has MainAccounts");
            
            if (detailAccount.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = detailAccount.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.NotFound, null);
            
            var operation = await _repositoriesFactory.GetRepository<DetailAccount>().Delete(entity);
            return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.Success, null);
        }catch(Exception ex){
            LogService.LogError(ex);
            return new OperationResponse<DetailAccountResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }
    
}