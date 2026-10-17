using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchUnitStatus;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;

public class ProductBatchToUnitAssignmentsResponseDto
{
    public Guid Id { get; set; }
    public ProductUnitResponseDto ProductUnit { get; set; }
    public Guid ProductUnitId { get; set; }
    public Guid ProductBatchDetailsId { get; set; }
    public BatchUnitStatusResponseDto BatchUnitStatus { get; set; }
    public Guid BatchUnitStatusId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}