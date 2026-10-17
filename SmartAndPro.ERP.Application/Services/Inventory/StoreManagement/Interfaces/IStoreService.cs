using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.StoreManagement.Interfaces;

public interface IStoreService : IServiceBase
<
    StoreDetails,
    StoreDetailsPostDto,
    StoreDetailsPutDto,
    StoreDetailsResponseDto,
    StoreDetailsFiltersDto
>
{

}