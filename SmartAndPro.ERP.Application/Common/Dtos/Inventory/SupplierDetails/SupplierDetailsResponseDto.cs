using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierStatus;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;

public class SupplierDetailsResponseDto
{
    public Guid Id { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    public Address Address { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    
    public string? BankName { get; set; }
    
    public string? BankAccountNumber { get; set; }
    
    public string? IBAN { get; set; }
    
    public string? TaxNumber { get; set; }
    
    public string? AdditionalContactNumber { get; set; }
    public string? District { get; set; }
    public string? ShortAddress { get; set; }
    public string? BuildingNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? Fax { get; set; }
    public string? Website { get; set; }
    public string? Instructions { get; set; }
    public string? GaztNumber { get; set; }
    public string? OtherId { get; set; }
    
    public Guid? MainAccountId { get; set; }
    public MainAccountResponseDto MainAccount { get; set; }


    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public SupplierStatusResponseDto SupplierStatus { get; set; }
    public Guid SupplierStatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public bool IsFrontal { get; set; } = false;
    
}
