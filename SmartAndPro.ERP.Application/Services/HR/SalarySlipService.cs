using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.SalarySlip;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class SalarySlipService : ServiceBase<
    SalarySlip,
    SalarySlipPostDto,
    SalarySlipPutDto,
    SalarySlipResponseDto,
    ParamFilter>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _mapper;

    public SalarySlipService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<List<SalarySlipResponseDto>>> GetAllAsync(
        ParamFilter filters,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<SalarySlip>();
            var operation = await repo.GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<SalarySlipResponseDto>>(operation.ErrorCode, null);

            var query = operation.Data.Include(x => x.Employee).AsQueryable();

            if (!filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }

            var entities = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            var result = _mapper.Map<List<SalarySlipResponseDto>>(entities);
            return new OperationResponse<List<SalarySlipResponseDto>>(ErrorCodes.Success, result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<SalarySlipResponseDto>>(ErrorCodes.GeneralFail, null);
        }
    }
}
