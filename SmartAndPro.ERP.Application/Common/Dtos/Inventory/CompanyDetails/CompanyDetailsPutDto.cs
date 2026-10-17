using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;

public class CompanyDetailsPutDto
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
    public string? ArabicAddress { get; set; }
    public string? ShortAddress { get; set; }
    public string? BuildingNumber { get; set; }
    public string? SubNumber { get; set; }
    public string? District { get; set; }
    public string? EnglishDistrict { get; set; }



    public Guid? CompanyStatusId { get; set; }

    public List<CompanyPhonePutDto> Phones { get; set; } = new();
    public List<BankAccountPutDto> BankAccounts { get; set; } = new();
    
    [JsonInclude]
    [Required(ErrorMessage = "TaxNumber Field is required")]
    public string TaxNumber { get; set; }

    [JsonInclude]
    public string? GaztNumber { get; set; }

    public bool? IsActive { get; set; } = true;

    public bool? IsZatcaIntegrated { get; set; } = false;

    public string? BusinessCategory { get; set; }

    public string? Logo { get; set; }
    public string? Stamp { get; set; }
    public string? LogoUrl { get; set; }
    public string? StampUrl { get; set; }


    public string? Csr { get; set; }
    public string? PrivateKey { get; set; }


    public long? RequestId { get; set; }
    public string? DispositionMessage { get; set; }
    public string? BinarySecurityToken { get; set; }
    public string? Secret { get; set; }

    public long? ProdRequestId { get; set; }
    public string? ProdDispositionMessage { get; set; }
    public string? ProdBinarySecurityToken { get; set; }
    public string? ProdSecret { get; set; }
    
    public string? Website { get; set; }
    public string? UnifiedNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public List<CompanyPolicyPutDto> Policies { get; set; } = new();
    public bool? IsOnBoarded { get; set; } = false;
}
