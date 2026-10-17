using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Recipes.Recipe;

public class RecipeResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    
    public List<RecipeEntryResponseDto> RecipeEntries { get; set; }

    public Guid MainSalesClassId { get; set; }
    public MainSalesClassResponseDto MainSalesClass { get; set; }
    
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public int? NumberOfPersons { get; set; } = 1;
}