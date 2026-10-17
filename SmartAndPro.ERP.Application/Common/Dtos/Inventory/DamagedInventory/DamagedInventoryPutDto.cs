namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;

public class DamagedInventoryPutDto
{
    public Guid Id { get; set; }
    
    public Guid? MainPurchaseClassId { get; set; }
    
    public Guid? MainSalesClassId { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTime? DamageDate { get; set; }
    
    public string? Reason { get; set; }
    
    public string? ReportedBy { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    
    public bool? IsActive { get; set; }
}
