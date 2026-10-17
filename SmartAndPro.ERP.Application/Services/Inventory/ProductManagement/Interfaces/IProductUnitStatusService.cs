using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnitStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;

public interface IProductUnitStatusService 
    : IServiceBase
        <
            ProductUnitStatus,
            ProductUnitStatusPostDto,
            ProductUnitStatusPutDto,
            ProductUnitStatusResponseDto,
            ProductUnitStatusFiltersDto
        >
{
}