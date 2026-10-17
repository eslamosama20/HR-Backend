using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.BankDocument;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using AutoMapper;

namespace SmartAndPro.ERP.Application.Services.HR;

public class BankDocumentService : ServiceBase<
    BankDocument,
    BankDocumentPostDto,
    BankDocumentPutDto,
    BankDocumentResponseDto,
    ParamFilter>
{
    public BankDocumentService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
    }
}
