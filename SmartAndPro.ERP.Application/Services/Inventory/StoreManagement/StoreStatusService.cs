using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.StoreManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory.StoreManagement;

public class StoreStatusService 
    : ServiceBase
    <
        StoreStatus,
        StoreStatusPostDto,
        StoreStatusPutDto,
        StoreStatusResponseDto,
        StoreStatusFiltersDto
    > , IStoreStatusService

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public StoreStatusService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
}