using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;

public class RecipeEntryPutDto
{
    public Guid? Id { get; set; }
    
 
    
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "MainPurchaseClassId Field is required")]
    public Guid MainPurchaseClassId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "RecipeId Field is required")]
    public Guid RecipeId { get; set; }
    
    public bool? IsActive { get; set; } = true;

}