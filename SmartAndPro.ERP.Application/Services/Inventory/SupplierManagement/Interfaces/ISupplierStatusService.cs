using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement.Interfaces;

public interface ISupplierStatusService
    : IServiceBase
    <
        SupplierStatus,
        SupplierStatusPostDto,
        SupplierStatusPutDto,
        SupplierStatusResponseDto,
        SupplierStatusFiltersDto
    >
{
}