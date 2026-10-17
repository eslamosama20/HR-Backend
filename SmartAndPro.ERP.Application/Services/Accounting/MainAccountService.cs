using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class MainAccountService
    : ServiceBase
    <
        MainAccount,
        MainAccountPostDto,
        MainAccountPutDto,
        MainAccountResponseDto,
        MainAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public MainAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<MainAccountResponseDto>> AddAsync(MainAccountPostDto mainAccountPost)
    {
        var mainAccount = _mapper.Map<MainAccount>(mainAccountPost);
        var referenceCode = mainAccount.ReferenceCode;
        // check if root account or a headerAccount or a detailAccount or a MainAccount has that referenceCode
        var rootAccountRepository = _repositoriesFactory.GetRepository<RootAccount>();
        var rootAccountOperation = await rootAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (rootAccountOperation.Data.Any())
            return new OperationResponse<MainAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var headerAccountRepository = _repositoriesFactory.GetRepository<HeaderAccount>();
        var headerAccountOperation = await headerAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (headerAccountOperation.Data.Any())
            return new OperationResponse<MainAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var detailAccountRepository = _repositoriesFactory.GetRepository<DetailAccount>();
        var detailAccountOperation = await detailAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (detailAccountOperation.Data.Any())
            return new OperationResponse<MainAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
        var mainAccountOperation = await mainAccountRepository.Filter(x => x.ReferenceCode == referenceCode);
        if (mainAccountOperation.Data.Any())
            return new OperationResponse<MainAccountResponseDto>(ErrorCodes.Conflict, null,message:"ReferenceCode already exists");
        
        return await base.AddAsync(mainAccountPost);
    }
    
    public override async Task<OperationResponse<MainAccountResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            // Step 1: Check if any TransactionEntry references this MainAccount
            var transactionEntryRepository = _repositoriesFactory.GetRepository<TransactionEntry>();
            var linkedEntries = await transactionEntryRepository.Filter(x => x.MainAccountId == id);
            if (linkedEntries.Data != null && await linkedEntries.Data.AnyAsync())
                return new OperationResponse<MainAccountResponseDto>(
                    ErrorCodes.Conflict, null,
                    message: "Cannot delete this account because it is linked to existing journal entries.");

            // Step 2: Check for links in Owner entities (Customer, Supplier)
            var customerProfileRepo = _repositoriesFactory.GetRepository<CustomerProfile>();
            var linkedCustomers = await customerProfileRepo.Filter(x => x.MainAccountId == id);
            if (linkedCustomers.Data != null && await linkedCustomers.Data.AnyAsync())
                return new OperationResponse<MainAccountResponseDto>(
                    ErrorCodes.Conflict, null,
                    message: "Cannot delete this account because it is linked to a Customer Profile.");

            var supplierDetailsRepo = _repositoriesFactory.GetRepository<SupplierDetails>();
            var linkedSuppliers = await supplierDetailsRepo.Filter(x => x.MainAccountId == id);
            if (linkedSuppliers.Data != null && await linkedSuppliers.Data.AnyAsync())
                return new OperationResponse<MainAccountResponseDto>(
                    ErrorCodes.Conflict, null,
                    message: "Cannot delete this account because it is linked to a Supplier.");

            // Step 3: Fetch the entity to delete
            var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
            var mainAccountOperation = await mainAccountRepository.Filter(x => x.Id == id);
            var entity = await mainAccountOperation.Data.FirstOrDefaultAsync();
            
            if (entity is null)
                return new OperationResponse<MainAccountResponseDto>(ErrorCodes.NotFound, null);

            // Step 4: Hard delete (will trigger other FK constraints in DB)
            var deleteOperation = await mainAccountRepository.Delete(entity);
            if (deleteOperation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<MainAccountResponseDto>(ErrorCodes.GeneralFail, null);

            return new OperationResponse<MainAccountResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return new OperationResponse<MainAccountResponseDto>(ErrorCodes.GeneralFail, null);
    }

    public async Task<OperationResponse<List<MainAccountLookupDto>>> GetLookupAsync()
    {
        try
        {
            var mainAccountRepository = _repositoriesFactory.GetRepository<MainAccount>();
            var operation = await mainAccountRepository.Filter(x => x.IsActive);
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<MainAccountLookupDto>>(operation.ErrorCode, null);

            var list = await operation.Data
                .Select(x => new MainAccountLookupDto
                {
                    Id = x.Id,
                    ReferenceCode = x.ReferenceCode,
                    ArabicName = x.ArabicName,
                    EnglishName = x.EnglishName
                })
                .OrderBy(x => x.ReferenceCode)
                .ToListAsync();

            return new OperationResponse<List<MainAccountLookupDto>>(ErrorCodes.Success, list);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<MainAccountLookupDto>>(ErrorCodes.GeneralFail, null);
        }
    }
}

