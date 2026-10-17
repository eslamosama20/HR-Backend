using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.InsuranceRecord;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class InsuranceRecordService : ServiceBase<
    InsuranceRecord,
    InsuranceRecordPostDto,
    InsuranceRecordPutDto,
    InsuranceRecordResponseDto,
    ParamFilter>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _mapper;

    public InsuranceRecordService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<InsuranceRecordResponseDto>>> GetExpiringRecordsAsync(int days = 30)
    {
        var repo = _repositoriesFactory.GetRepository<InsuranceRecord>();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var targetDate = today.AddDays(days);

        var query = await repo.Filter(c => c.ExpirationDate <= targetDate && c.ExpirationDate >= today);
        if (query.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<List<InsuranceRecordResponseDto>>(query.ErrorCode, null);

        var records = await query.Data
            .Include(x => x.Employee)
            .OrderBy(c => c.ExpirationDate)
            .ToListAsync();

        var response = _mapper.Map<List<InsuranceRecordResponseDto>>(records);

        foreach (var record in response)
        {
            record.DaysUntilExpiration = (record.ExpirationDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
        }

        return new OperationResponse<List<InsuranceRecordResponseDto>>(ErrorCodes.Success, response);
    }
}
