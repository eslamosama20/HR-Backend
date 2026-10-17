using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;

public interface ICompanyService : IServiceBase
<
    CompanyDetails,
    CompanyDetailsPostDto,
    CompanyDetailsPutDto,
    CompanyDetailsResponseDto,
    CompanyDetailsFiltersDto
>
{
}