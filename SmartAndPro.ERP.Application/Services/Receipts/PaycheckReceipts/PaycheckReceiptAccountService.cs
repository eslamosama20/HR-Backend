using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceiptAccountResponse;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Receipts.PaycheckReceipts;


public class PaycheckReceiptAccountService
    : ServiceBase
    <
        PaycheckReceiptAccount,
        PaycheckReceiptAccountPostDto,
        PaycheckReceiptAccountPutDto,
        PaycheckReceiptAccountResponseDto,
        PaycheckReceiptAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public PaycheckReceiptAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
        
}

