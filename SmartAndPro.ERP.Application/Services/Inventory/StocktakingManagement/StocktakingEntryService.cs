using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.Stocktaking.StocktakingEntry;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory.StocktakingManagement;


public class StocktakingEntryService
    : ServiceBase
    <
        StocktakingEntry,
        StocktakingEntryPostDto,
        StocktakingEntryPutDto,
        StocktakingEntryResponseDto,
        StocktakingEntryFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public StocktakingEntryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    
}

