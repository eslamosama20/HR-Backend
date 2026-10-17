using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;

public class RecipeEntryResponseDto
{
    public Guid Id { get; set; }
    
    public decimal Quantity { get; set; }
    public Guid MainPurchaseClassId { get; set; }
    public MainPurchaseClassResponseDto MainPurchaseClass{ get; set; }
    public Guid RecipeId { get; set; }

        
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}