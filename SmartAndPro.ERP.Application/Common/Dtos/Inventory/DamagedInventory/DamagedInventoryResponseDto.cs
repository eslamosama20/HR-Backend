namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;

public class DamagedInventoryResponseDto
{
    public Guid Id { get; set; }
    
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }
    
    public Guid? MainSalesClassId { get; set; }
    public string? MainSalesClassName { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTime DamageDate { get; set; }
    
    public string? Reason { get; set; }
    
    public string? ReportedBy { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public string? StoreName { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public bool IsActive { get; set; }
    
    // Alias for compatibility with Controller
    public string? ProductName { get => MainPurchaseClassName ?? MainSalesClassName; }
}
