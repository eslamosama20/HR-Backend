using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;

public interface IBranchService : IServiceBase
<
    BranchDetails,
    BranchDetailsPostDto,
    BranchDetailsPutDto,
    BranchDetailsResponseDto,
    BranchDetailsFiltersDto
>
{
}