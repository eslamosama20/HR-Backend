using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;

public interface IBranchStatusService
    : IServiceBase
    <
        BranchStatus,
        BranchStatusPostDto,
        BranchStatusPutDto,
        BranchStatusResponseDto,
        BranchStatusFiltersDto
    >
{
}