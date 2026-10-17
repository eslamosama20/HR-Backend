using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;

public class RecipeEntryPostDto
{

    
    [JsonInclude]
    [Required(ErrorMessage = "Quantity Field is required")]
    public decimal Quantity { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "MainPurchaseClassId Field is required")]
    public Guid MainPurchaseClassId { get; set; }
    
    public Guid? RecipeId { get; set; }
    
    public bool? IsActive { get; set; } = true;
}