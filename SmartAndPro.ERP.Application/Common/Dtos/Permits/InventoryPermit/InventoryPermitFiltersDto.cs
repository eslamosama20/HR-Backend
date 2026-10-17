namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;

public class InventoryPermitFiltersDto
{
    public Guid? StoreDetailsId { get; set; }
    public string? PermitType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}