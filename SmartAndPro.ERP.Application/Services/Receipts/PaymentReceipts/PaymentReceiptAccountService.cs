using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaymentReceiptAccountResponse;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Receipts.PaymentReceipts;


public class PaymentReceiptAccountService
    : ServiceBase
    <
        PaymentReceiptAccount,
        PaymentReceiptAccountPostDto,
        PaymentReceiptAccountPutDto,
        PaymentReceiptAccountResponseDto,
        PaymentReceiptAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public PaymentReceiptAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
        
}

