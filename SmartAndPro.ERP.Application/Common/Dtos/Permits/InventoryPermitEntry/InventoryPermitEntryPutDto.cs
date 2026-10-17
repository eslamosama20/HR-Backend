using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;

public class InventoryPermitEntryPutDto
{
    public Guid? Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "UnitPrice Field is required")]
    public decimal UnitPrice { get; set; }  
    
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; }

    public Guid? MainPurchaseClassId { get; set; }

    public Guid? MainSalesClassId { get; set; }
    
    public string? ShelfNumber { get; set; }
    public string? RowNumber { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "InventoryPermitId Field is required")]
    public Guid? InventoryPermitId { get; set; }
    
    public bool? IsActive { get; set; } = true;

}