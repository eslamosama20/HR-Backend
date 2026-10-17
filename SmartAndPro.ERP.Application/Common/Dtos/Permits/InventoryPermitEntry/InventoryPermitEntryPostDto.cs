using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;

public class InventoryPermitEntryPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "UnitPrice Field is required")]
    public decimal UnitPrice { get; set; }  
    
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; }

    public decimal RetailPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal BulkPrice { get; set; }

    public Guid? MainPurchaseClassId { get; set; }

    public Guid? MainSalesClassId { get; set; }
    
    public string? ShelfNumber { get; set; }
    public string? RowNumber { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    
    public Guid? InventoryPermitId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}