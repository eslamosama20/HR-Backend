using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchUnitStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement;

public class BatchUnitStatusService 
    : ServiceBase
    <
        BatchUnitStatus,
        BatchUnitStatusPostDto,
        BatchUnitStatusPutDto,
        BatchUnitStatusResponseDto,
        BatchUnitStatusFiltersDto
    > , IBatchUnitStatusService

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public BatchUnitStatusService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
}