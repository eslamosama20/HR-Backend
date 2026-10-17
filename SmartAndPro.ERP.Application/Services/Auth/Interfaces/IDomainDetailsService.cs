using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

namespace SmartAndPro.ERP.Application.Services.Auth.Interfaces;

public interface IDomainDetailsService
{
    Task<DomainDetailsDto> GetDomainDetailsByIdAsync(Guid id);
    Task<IEnumerable<DomainDetailsDto>> GetAllDomainDetailsAsync();
    Task<DomainDetailsDto> AddDomainDetailsAsync(DomainDetailsDto permissionDto);
    Task<DomainDetailsDto> UpdateDomainDetailsAsync(DomainDetailsDto permissionDto);
    Task<DomainDetailsDto> DeleteDomainDetailsAsync(Guid id);
    
}