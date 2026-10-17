using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;

public class BranchStatusService
    : ServiceBase
    <
        BranchStatus,
        BranchStatusPostDto,
        BranchStatusPutDto,
        BranchStatusResponseDto,
        BranchStatusFiltersDto
    >, IBranchStatusService

{
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;

    public BranchStatusService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
}