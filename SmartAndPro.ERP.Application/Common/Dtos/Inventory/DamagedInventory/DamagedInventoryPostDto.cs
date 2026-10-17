using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;

public class DamagedInventoryPostDto
{
    public Guid? MainPurchaseClassId { get; set; }
    
    // Alias for MainPurchaseClassId (from frontend)
    [JsonPropertyName("productId")]
    public Guid? ProductId 
    { 
        get => MainPurchaseClassId; 
        set => MainPurchaseClassId = value; 
    }
    
    public Guid? MainSalesClassId { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTime? DamageDate { get; set; }
    
    public string? Reason { get; set; }
    
    public string? ReportedBy { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}
