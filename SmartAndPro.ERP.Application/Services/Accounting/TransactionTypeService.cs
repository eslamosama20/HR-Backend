using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TransactionType;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class TransactionTypeService
    : ServiceBase
    <
        TransactionType,
        TransactionTypePostDto,
        TransactionTypePutDto,
        TransactionTypeResponseDto,
        TransactionTypeFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public TransactionTypeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
}