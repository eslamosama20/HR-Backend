using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Permits;


public class InventoryPermitEntryService
    : ServiceBase
    <
        PermitEntry,
        InventoryPermitEntryPostDto,
        InventoryPermitEntryPutDto,
        InventoryPermitEntryResponseDto,
        InventoryPermitEntryFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public InventoryPermitEntryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
        
}

