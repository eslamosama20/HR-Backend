using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;

public class BranchDetailsPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string? ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string? EnglishName { get; set; }

    public string? Email { get; set; }


    public string? Address { get; set; }


    public Guid? BranchStatusId { get; set; }
    
    [JsonInclude]
    [Required(ErrorMessage = "CompanyDetailsId Field is required")]
    public Guid? CompanyDetailsId { get; set; }
    
    public string? Phone { get; set; }
    
    public string? BankName { get; set; }
    
    public string? BankAccountNumber { get; set; }
    
    public string? IBAN { get; set; }
    
    public string? TaxNumber { get; set; }
    
    public bool? IsActive { get; set; } = true;
}