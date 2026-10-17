using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;

public class StagnantInventoryPostDto
{
    public Guid? MainPurchaseClassId { get; set; }
    
    public Guid? MainSalesClassId { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTime? LastMovementDate { get; set; }
    
    public StagnantReason Reason { get; set; }
    
    public int? StagnantPeriodMonths { get; set; }
    
    public bool IsAutoAdded { get; set; }
    
    public string? Notes { get; set; }
    
    public string? AddedBy { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
}
