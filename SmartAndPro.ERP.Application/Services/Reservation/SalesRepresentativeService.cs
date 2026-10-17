using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.SalesRepresentative;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class SalesRepresentativeService
    : ServiceBase
    <
        SalesRepresentative,
        SalesRepresentativePostDto,
        SalesRepresentativePutDto,
        SalesRepresentativeResponseDto,
        SalesRepresentativeFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public SalesRepresentativeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<SalesRepresentativeResponseDto>>> GetAllFrontalAsync()
    {
        var frontalSalesReps = await _repositoriesFactory.GetRepository<SalesRepresentative>().Filter(sr => sr.IsFrontal);

        if (frontalSalesReps.Data == null || !frontalSalesReps.Data.Any())
        {
            return new OperationResponse<List<SalesRepresentativeResponseDto>>(
                ErrorCodes.Success,
                new List<SalesRepresentativeResponseDto>()
            );
        }

        var salesRepDtos = _mapper.Map<List<SalesRepresentativeResponseDto>>(frontalSalesReps.Data);

        return new OperationResponse<List<SalesRepresentativeResponseDto>>(
            ErrorCodes.Success,
            salesRepDtos
        );
    }

    public async Task<OperationResponse<SalesRepresentativeResponseDto>> MakeFrontalAsync(Guid salesRepId)
    {
        var salesRepQuery = await _repositoriesFactory.GetRepository<SalesRepresentative>().Filter(sr => sr.Id == salesRepId);
        var salesRep = salesRepQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (salesRep == null)
        {
            return new OperationResponse<SalesRepresentativeResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        salesRep.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<SalesRepresentative>().Update(salesRep);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<SalesRepresentativeResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<SalesRepresentativeResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<SalesRepresentativeResponseDto>(updateResult.Data)
        );
    }
}