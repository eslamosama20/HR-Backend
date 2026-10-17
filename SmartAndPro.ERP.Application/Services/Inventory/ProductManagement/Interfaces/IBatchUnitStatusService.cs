using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchUnitStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;

public interface IBatchUnitStatusService
    : IServiceBase
    <
        BatchUnitStatus,
        BatchUnitStatusPostDto,
        BatchUnitStatusPutDto,
        BatchUnitStatusResponseDto,
        BatchUnitStatusFiltersDto
    >
{
}