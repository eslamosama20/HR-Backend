using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchStatus;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;

public interface IBatchStatusService
    : IServiceBase
    <
        BatchStatus,
        BatchStatusPostDto,
        BatchStatusPutDto,
        BatchStatusResponseDto,
        BatchStatusFiltersDto
    >
{
}