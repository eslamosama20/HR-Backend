using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;

public class PdfTemplateConfigurationService
    : ServiceBase<
        Domains.Entities.PdfTemplateConfiguration,
        PdfTemplateConfigurationPostDto,
        PdfTemplateConfigurationPutDto,
        PdfTemplateConfigurationResponseDto,
        object // Replace with a filter DTO if needed
    >
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public PdfTemplateConfigurationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<PdfTemplateConfigurationResponseDto>> AddAsync(PdfTemplateConfigurationPostDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>();
        var existsQuery = await repo.GetAll();
        var exists = existsQuery.Data.Any();
        if (exists)
            throw new InvalidOperationException("A PDF template configuration already exists.");

        return await base.AddAsync(dto);
    }

    public override async Task<OperationResponse<PdfTemplateConfigurationResponseDto>> UpdateAsync(PdfTemplateConfigurationPutDto dto)
    {
        var repo = _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>();
        var existingResult = await repo.Filter(x => x.Id == dto.Id);
        var existing = existingResult.Data.FirstOrDefault();
        if (existing == null)
            throw new InvalidOperationException("PDF template configuration not found.");

        await repo.Delete(existing);
        // Optionally: await _repositoriesFactory.SaveChangesAsync();
        
        var postDto = new PdfTemplateConfigurationPostDto
        {
            Header = dto.Header,
            Footer = dto.Footer
        };
        return await base.AddAsync(postDto);
    }
}