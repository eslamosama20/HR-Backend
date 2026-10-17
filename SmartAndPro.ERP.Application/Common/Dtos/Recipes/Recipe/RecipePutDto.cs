using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;

namespace SmartAndPro.ERP.Application.Common.Dtos.Recipes.Recipe;

public class RecipePutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    
    public string? ReferenceNumber { get; set; }
    
    public string? Description { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "RecipeEntries Field is required")]
    public List<RecipeEntryPutDto>? RecipeEntries { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "MainSalesClassId Field is required")]
    public Guid MainSalesClassId { get; set; }
    
    public bool? IsActive { get; set; } = true;
    public int? NumberOfPersons { get; set; } = 1;


}