using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;

public class CompanyDetailsPostDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ArabicName Field is required")]
    public string? ArabicName { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "EnglishName Field is required")]
    public string? EnglishName { get; set; }


    public string? Email { get; set; }


    public string? Address { get; set; }
    
    public string? ArabicAddress { get; set; }

    public string? ShortAddress { get; set; }
    public string? BuildingNumber { get; set; }
    public string? SubNumber { get; set; }
    public string? District { get; set; }
    public string? EnglishDistrict { get; set; }
    


    public Guid? CreatedBy { get; set; }

    public Guid? CompanyStatusId { get; set; }


    public List<CompanyPhonePostDto> Phones { get; set; } = new();

    public List<BankAccountPostDto> BankAccounts { get; set; } = new();
    
    [JsonInclude]
    [Required(ErrorMessage = "TaxNumber Field is required")]
    public string TaxNumber { get; set; }

    [JsonInclude]
    public string? GaztNumber { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool? IsZatcaIntegrated { get; set; } = false;

    public string? BusinessCategory { get; set; }

    public string? Website { get; set; }
    public string? UnifiedNumber { get; set; }
    public string? RegistrationNumber { get; set; }
   public List<CompanyPolicyPostDto> Policies { get; set; } = new();
    public string? Logo { get; set; }
    public string? Stamp { get; set; }
    public string? LogoUrl { get; set; }
    public string? StampUrl { get; set; }

}
