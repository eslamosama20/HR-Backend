using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;

public class BranchService
    : ServiceBase
    <
        BranchDetails,
        BranchDetailsPostDto,
        BranchDetailsPutDto,
        BranchDetailsResponseDto,
        BranchDetailsFiltersDto
    >, IBranchService

{
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;

    public BranchService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
}