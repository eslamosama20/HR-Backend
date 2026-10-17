using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;

public interface IBatchService : IServiceBase
<
    ProductBatchDetails,
    BatchDetailsPostDto,
    BatchDetailsPutDto,
    BatchDetailsResponseDto,
    BatchDetailsFiltersDto
>
{
    // Task<OperationResponse<ProductBatchDetails>> UnassignProductFromBatch(Guid batchId, ProductUnit product);
}