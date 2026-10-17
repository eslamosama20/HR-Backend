using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;

public interface ICompanyStatusService
    : IServiceBase
    <
        CompanyStatus,
        CompanyStatusPostDto,
        CompanyStatusPutDto,
        CompanyStatusResponseDto,
        CompanyStatusFiltersDto
    >
{
}