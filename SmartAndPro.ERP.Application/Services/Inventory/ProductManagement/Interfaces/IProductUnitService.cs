using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;

public interface IProductUnitService : IServiceBase
<
    ProductUnit,
    ProductUnitPostDto,
    ProductUnitPutDto,
    ProductUnitResponseDto,
    ProductUnitFiltersDto
>
{
}