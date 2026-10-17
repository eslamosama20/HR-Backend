using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSeason;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class ReservationSeasonService
    : ServiceBase
    <
        ReservationSeason,
        ReservationSeasonPostDto,
        ReservationSeasonPutDto,
        ReservationSeasonResponseDto,
        ReservationSeasonFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public ReservationSeasonService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<ReservationSeasonResponseDto>>> GetAllFrontalAsync()
    {
        var frontalSeasons = await _repositoriesFactory.GetRepository<ReservationSeason>().Filter(rs => rs.IsFrontal);

        if (frontalSeasons.Data == null || !frontalSeasons.Data.Any())
        {
            return new OperationResponse<List<ReservationSeasonResponseDto>>(
                ErrorCodes.Success,
                new List<ReservationSeasonResponseDto>()
            );
        }

        var seasonDtos = _mapper.Map<List<ReservationSeasonResponseDto>>(frontalSeasons.Data);

        return new OperationResponse<List<ReservationSeasonResponseDto>>(
            ErrorCodes.Success,
            seasonDtos
        );
    }

    public async Task<OperationResponse<ReservationSeasonResponseDto>> MakeFrontalAsync(Guid seasonId)
    {
        var seasonQuery = await _repositoriesFactory.GetRepository<ReservationSeason>().Filter(rs => rs.Id == seasonId);
        var season = seasonQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (season == null)
        {
            return new OperationResponse<ReservationSeasonResponseDto>(
                ErrorCodes.NotFound,
                null
            );
        }

        season.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<ReservationSeason>().Update(season);

        if (updateResult.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<ReservationSeasonResponseDto>(
                ErrorCodes.GeneralFail,
                null
            );
        }

        return new OperationResponse<ReservationSeasonResponseDto>(
            ErrorCodes.Success,
            _mapper.Map<ReservationSeasonResponseDto>(updateResult.Data)
        );
    }
}