using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class ReservationServiceEntryService 
    : ServiceBase
    <
        ReservationServiceEntry,
        ReservationServiceEntryPostDto,
        ReservationServiceEntryPutDto,
        ReservationServiceEntryResponseDto,
        ReservationServiceEntryFiltersDto
    >

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public ReservationServiceEntryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
}