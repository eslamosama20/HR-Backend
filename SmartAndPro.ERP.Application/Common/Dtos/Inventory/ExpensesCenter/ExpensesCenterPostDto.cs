using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ExpensesCenter;

public class ExpensesCenterPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    
    public bool? IsActive { get; set; } = true;
}