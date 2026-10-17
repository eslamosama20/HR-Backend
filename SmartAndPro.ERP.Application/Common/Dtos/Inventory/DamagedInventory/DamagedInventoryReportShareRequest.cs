namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;

public class DamagedInventoryReportShareRequest
{
    public Guid? StoreDetailsId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ShareType { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    // Aliases for compatibility with Controller
    public Guid? StoreId { get => StoreDetailsId; set => StoreDetailsId = value; }
    public string? Method { get => ShareType; set => ShareType = value; }
    public string? Receiver { get => Email ?? PhoneNumber; set => Email = value; }
}
