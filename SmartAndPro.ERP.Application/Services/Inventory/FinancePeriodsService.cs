using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.FinancePeriods;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory;

public class FinancePeriodsService 
    : ServiceBase
    <
        FinancePeriods,
        FinancePeriodsPostDto,
        FinancePeriodsPutDto,
        FinancePeriodsResponseDto,
        FinancePeriodsFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public FinancePeriodsService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
}