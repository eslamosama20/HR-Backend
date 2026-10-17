namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.DamagedInventory;

public class DamagedInventoryFiltersDto
{
    public Guid? StoreDetailsId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Aliases for compatibility
    public DateTime? FromDate { get => StartDate; set => StartDate = value; }
    public DateTime? ToDate { get => EndDate; set => EndDate = value; }
}
