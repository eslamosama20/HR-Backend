using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.StoreManagement.DocumentUtils;
using SmartAndPro.ERP.Application.Services.Inventory.StoreManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.Services.Inventory.StoreManagement;

public class StoreService 
    : ServiceBase
    <
        StoreDetails,
        StoreDetailsPostDto,
        StoreDetailsPutDto,
        StoreDetailsResponseDto,
        StoreDetailsFiltersDto
    > , IStoreService

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public StoreService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<StoreDetailsResponseDto>> AddAsync(StoreDetailsPostDto storeDetailsDto)
    {
        if (storeDetailsDto.MainAccountId == null)
        {
            var storeDetailAccountQuery = await _repositoriesFactory.GetRepository<DetailAccount>()
                .Filter(da => da.ArabicName == "المخازن");
            var storeDetailAccountId = storeDetailAccountQuery.Data?.Select(da => da.Id).FirstOrDefault();
            if (storeDetailAccountId == null || storeDetailAccountId == Guid.Empty)
                throw new Exception("Stores Detail Account not found");
            // Create the store account
            var storeAccount = new MainAccount
            {
                EnglishName = storeDetailsDto.EnglishName,
                ArabicName = storeDetailsDto.ArabicName,
                DetailAccountId = (Guid)storeDetailAccountId,
                CreatedAt = DateTime.UtcNow
            };
            var storeAccountAddResult = await _repositoriesFactory.GetRepository<MainAccount>().Add(storeAccount);
            if (storeAccountAddResult.Data == null)
                throw new Exception("Store Account not added");

            storeDetailsDto.MainAccountId = storeAccountAddResult.Data.Id;
        }

        
        var result = await base.AddAsync(storeDetailsDto);
        return result;
    }

    /// <summary>
    /// Generate PDF report document for stores
    /// </summary>
    public async Task<byte[]> GetStoreDetailsReportDocument(string? printedBy = null)
    {
        var filter = new StoreDetailsFiltersDto();
        var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
        var storesResult = await storeRepository.GetAll();
        var stores = storesResult.Data?.ToList() ?? new List<StoreDetails>();

        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        var document = new StoreDetailsReportDocument(
            stores,
            company,
            "ar",
            printedBy
        );

        return document.GeneratePDF();
    }
    
}