using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.DetailAccount;

public class DetailAccountPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid Id { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]    
    public string ArabicName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string EnglishName { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "ReferenceCode Field is required")]
    public string ReferenceCode { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "HeaderAccountId Field is required")]
    public Guid HeaderAccountId { get; set; }
    

    public string? Icon { get; set; }

    public bool? IsActive { get; set; } = true;

}