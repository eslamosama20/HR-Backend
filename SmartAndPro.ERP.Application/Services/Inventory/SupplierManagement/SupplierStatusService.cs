using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement;

public class SupplierStatusService
    : ServiceBase
    <
        SupplierStatus,
        SupplierStatusPostDto,
        SupplierStatusPutDto,
        SupplierStatusResponseDto,
        SupplierStatusFiltersDto
    >, ISupplierStatusService

{
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;

    public SupplierStatusService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
}