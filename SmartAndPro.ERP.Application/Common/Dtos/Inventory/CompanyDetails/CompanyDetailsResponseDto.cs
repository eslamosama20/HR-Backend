using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyStatus;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;

public class CompanyDetailsResponseDto
{
    public Guid Id { get; set; }

    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string? Email { get; set; }
    public Address? Address { get; set; }
    public Address? ArabicAddress { get; set; }
    public string? ShortAddress { get; set; }
    public string? BuildingNumber { get; set; }
    public string? SubNumber { get; set; }
    public string? District { get; set; }
    public string? EnglishDistrict { get; set; }
    
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<CompanyPhoneResponseDto> Phones { get; set; } = new();    
    public List<BankAccountResponseDto> BankAccounts { get; set; } = new();    
    
    public string? TaxNumber { get; set; }
    public string? GaztNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? StampUrl { get; set; }

    public string? Website { get; set; }
    public string? UnifiedNumber { get; set; }
    public string? RegistrationNumber { get; set; }

    public bool IsZatcaIntegrated { get; set; }
    
    public string? BusinessCategory { get; set; }
    public bool IsOnBoarded { get; set; }
    public CompanyStatusResponseDto? CompanyStatus { get; set; }
    public Guid? CompanyStatusId { get; set; }
    public List<CompanyPolicyResponseDto> Policies { get; set; } = new();
    public List<BranchDetailsResponseDto> BranchDetails { get; set; } = new();
}
