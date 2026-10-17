using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Abstraction;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Auth;

public class DomainDetailsService : BaseService , IDomainDetailsService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IRepository<DomainDetails> _domainDetailsRepository;
    private readonly IMapper _mapper;

    public DomainDetailsService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) : base(repositoriesFactory, appSettings)
    {
        _repositoriesFactory = repositoriesFactory;
        _domainDetailsRepository = _repositoriesFactory.GetRepository<DomainDetails>();
        _mapper = mapper;
    }

    public async Task<DomainDetailsDto> GetDomainDetailsByIdAsync(Guid id)
    {
        var domainDetailsQuery = await _domainDetailsRepository.Filter(x => x.Id == id);
        if (domainDetailsQuery.Data == null)
        {
            throw new Exception("DomainDetails not found.");
        }
        var domainDetails = await domainDetailsQuery.Data.FirstOrDefaultAsync();
        if (domainDetails == null)
        {
            throw new Exception("DomainDetails not found.");
        }
        return new DomainDetailsDto
        {
            Id = domainDetails.Id,
            Name = domainDetails.Name,
        };
    }

    public async Task<IEnumerable<DomainDetailsDto>> GetAllDomainDetailsAsync()
    {
        var domainDetailsQuery = await _domainDetailsRepository.GetAll();
        if (domainDetailsQuery.Data == null)
            throw new Exception("DomainDetails not found.");
        var domainDetails = await domainDetailsQuery.Data.ToListAsync();
        if (domainDetails == null)
            throw new Exception("DomainDetails not found.");
        return  _mapper.Map<IEnumerable<DomainDetailsDto>>(domainDetails);
    }

    public async Task<DomainDetailsDto> AddDomainDetailsAsync(DomainDetailsDto domainDetailsDto)
    {
        // check with domainDetails name already exists
        var domainDetailsExistsQuery = await _domainDetailsRepository.Filter(x => x.Name == domainDetailsDto.Name);
        if (domainDetailsExistsQuery.Data != null)
            throw new Exception("DomainDetails name already exists.");
        var domainDetailsExists = await domainDetailsExistsQuery.Data.FirstOrDefaultAsync();
        if (domainDetailsExists != null)
        {
            throw new Exception("DomainDetails name already exists.");
        }
        var domainDetails =  _mapper.Map<DomainDetails>(domainDetailsDto);
        var domainDetailsResult = await _domainDetailsRepository.Add(domainDetails);
        
        
        return _mapper.Map<DomainDetailsDto>(domainDetailsResult.Data);
    }

    public async Task<DomainDetailsDto> UpdateDomainDetailsAsync(DomainDetailsDto domainDetailsDto)
    {
        var domainDetailsQuery = await _domainDetailsRepository.Filter(x => x.Id == domainDetailsDto.Id);
        if (domainDetailsQuery.Data == null)
        {
            throw new Exception("DomainDetails not found.");
        }
        var domainDetails = await domainDetailsQuery.Data.FirstOrDefaultAsync();
        if (domainDetails == null) throw new Exception("DomainDetails not found.");

        domainDetails.Name = domainDetailsDto.Name;

        var updatedDomainDetailsQuery = await _domainDetailsRepository.Update(domainDetails);
        var updatedDomainDetails = updatedDomainDetailsQuery.Data;
        if (updatedDomainDetails == null) throw new Exception("DomainDetails not found.");
        return new DomainDetailsDto
        {
            Id = updatedDomainDetails.Id,
            Name = updatedDomainDetails.Name,
        };
    }

    public async Task<DomainDetailsDto> DeleteDomainDetailsAsync(Guid id)
    {
        var domainDetailsQuery = await _domainDetailsRepository.Filter(x => x.Id == id);
        if (domainDetailsQuery.Data == null)
        {
            throw new Exception("DomainDetails not found.");
        }
        var domainDetails = await domainDetailsQuery.Data.FirstOrDefaultAsync();
        if (domainDetails == null)
        {
            throw new Exception("DomainDetails not found.");
        }
        await _domainDetailsRepository.Delete(domainDetails);
        return _mapper.Map<DomainDetailsDto>(domainDetails);
    }
}
