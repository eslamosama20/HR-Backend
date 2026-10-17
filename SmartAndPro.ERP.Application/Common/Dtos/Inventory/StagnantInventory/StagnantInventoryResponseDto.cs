using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;

public class StagnantInventoryResponseDto
{
    public Guid Id { get; set; }
    
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }
    public string? ProductCode { get; set; }
    
    public Guid? MainSalesClassId { get; set; }
    public string? MainSalesClassName { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTime? LastMovementDate { get; set; }
    
    public DateTime AddedToStagnantDate { get; set; }
    
    public StagnantReason Reason { get; set; }
    public string? ReasonDisplay { get; set; }
    
    public int? StagnantPeriodMonths { get; set; }
    
    public bool IsAutoAdded { get; set; }
    
    public string? Notes { get; set; }
    
    public string? AddedBy { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public string? StoreName { get; set; }
    
    public bool IsDisposed { get; set; }
    
    public DateTime? DisposedDate { get; set; }
    
    public string? DisposalMethod { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public bool IsActive { get; set; }
    
    // Computed properties
    public string? ProductName => MainPurchaseClassName ?? MainSalesClassName;
    
    public int DaysSinceLastMovement => LastMovementDate.HasValue 
        ? (int)(DateTime.UtcNow - LastMovementDate.Value).TotalDays 
        : 0;
}
