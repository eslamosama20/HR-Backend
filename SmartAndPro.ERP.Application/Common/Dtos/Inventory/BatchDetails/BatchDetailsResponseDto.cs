using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchStatus;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;

public class BatchDetailsResponseDto
{
    public Guid Id { get; set; }
    public DateTime ExpiryDate { get; set; }
    public BatchStatusResponseDto BatchStatus { get; set; }
    public Guid BatchStatusId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public Guid StoreDetailsId { get; set; }

    public List<ProductBatchToUnitAssignmentsResponseDto> ProductBatchToUnitAssignments { get; set; } = null;
}