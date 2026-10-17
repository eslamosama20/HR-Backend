using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.CompanyDocument;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class CompanyDocumentService : ServiceBase<
    CompanyDocument,
    CompanyDocumentPostDto,
    CompanyDocumentPutDto,
    CompanyDocumentResponseDto,
    ParamFilter>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _mapper;

    public CompanyDocumentService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<CompanyDocumentResponseDto>>> GetExpiringDocumentsAsync(int days = 30)
    {
        var repo = _repositoriesFactory.GetRepository<CompanyDocument>();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var targetDate = today.AddDays(days);

        var query = await repo.Filter(c => c.ExpirationDate <= targetDate && c.ExpirationDate >= today);
        if (query.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<List<CompanyDocumentResponseDto>>(query.ErrorCode, null);

        var docs = await query.Data.OrderBy(c => c.ExpirationDate).ToListAsync();
        var response = _mapper.Map<List<CompanyDocumentResponseDto>>(docs);

        foreach (var doc in response)
        {
            doc.DaysUntilExpiration = (doc.ExpirationDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
        }

        return new OperationResponse<List<CompanyDocumentResponseDto>>(ErrorCodes.Success, response);
    }

    public async Task<OperationResponse<List<CompanyDocumentResponseDto>>> GetCriticalDocumentsAsync()
    {
        var repo = _repositoriesFactory.GetRepository<CompanyDocument>();
        var query = await repo.Filter(c => c.IsCritical);
        if (query.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<List<CompanyDocumentResponseDto>>(query.ErrorCode, null);

        var docs = await query.Data.OrderBy(c => c.ExpirationDate).ToListAsync();
        var response = _mapper.Map<List<CompanyDocumentResponseDto>>(docs);

        foreach (var doc in response)
        {
            doc.DaysUntilExpiration = (doc.ExpirationDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
        }

        return new OperationResponse<List<CompanyDocumentResponseDto>>(ErrorCodes.Success, response);
    }
}
