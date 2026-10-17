using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Recipes;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Recipes;


public class RecipeEntryService
    : ServiceBase
    <
        RecipeEntry,
        RecipeEntryPostDto,
        RecipeEntryPutDto,
        RecipeEntryResponseDto,
        RecipeEntryFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public RecipeEntryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    
}

