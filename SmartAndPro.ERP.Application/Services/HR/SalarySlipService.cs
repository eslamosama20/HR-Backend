using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.SalarySlip;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using AutoMapper;

namespace SmartAndPro.ERP.Application.Services.HR;

public class SalarySlipService : ServiceBase<
    SalarySlip,
    SalarySlipPostDto,
    SalarySlipPutDto,
    SalarySlipResponseDto,
    ParamFilter>
{
    public SalarySlipService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
    }
}
