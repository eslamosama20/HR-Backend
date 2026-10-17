using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Notes.SalesCreditNoteItem;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Notes.CreditNotes;


public class SalesCreditNoteItemService
    : ServiceBase
    <
        SalesCreditNoteItem,
        SalesCreditNoteItemPostDto,
        SalesCreditNoteItemPutDto,
        SalesCreditNoteItemResponseDto,
        SalesCreditNoteItemFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public SalesCreditNoteItemService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
        
}

