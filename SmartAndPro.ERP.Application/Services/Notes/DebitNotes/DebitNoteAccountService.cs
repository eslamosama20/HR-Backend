using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.DebitNoteAccount;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Notes.DebitNotes;


public class DebitNoteAccountService
    : ServiceBase
    <
        DebitNoteAccount,
        DebitNoteAccountPostDto,
        DebitNoteAccountPutDto,
        DebitNoteAccountResponseDto,
        DebitNoteAccountFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public DebitNoteAccountService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
        
}

