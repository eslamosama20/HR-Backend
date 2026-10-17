using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement.Interfaces;

public interface ISupplierService : IServiceBase
<
    SupplierDetails,
    SupplierDetailsPostDto,
    SupplierDetailsPutDto,
    SupplierDetailsResponseDto,
    SupplierDetailsFiltersDto
>
{
}