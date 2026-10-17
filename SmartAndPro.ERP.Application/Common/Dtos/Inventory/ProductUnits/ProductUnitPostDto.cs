using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;

public class ProductUnitPostDto
{
    public Guid? CreatedBy { get; set; }

    public Guid? MainSalesClassId { get; set; }
    
    public Guid? MainPurchaseClassId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "StoreDetailsId Field is required")]
    public Guid StoreDetailsId { get; set; }
    
    public decimal? Quantity { get; set; } = 0;
}