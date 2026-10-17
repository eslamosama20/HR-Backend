using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.StoreManagement.Interfaces;

public interface IStoreStatusService
    : IServiceBase
    <
        StoreStatus,
        StoreStatusPostDto,
        StoreStatusPutDto,
        StoreStatusResponseDto,
        StoreStatusFiltersDto
    >
{
}