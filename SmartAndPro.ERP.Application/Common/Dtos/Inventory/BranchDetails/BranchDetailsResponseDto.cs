using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchStatus;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;

public class BranchDetailsResponseDto
{
    public Guid Id { get; set; }

    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string? Email { get; set; }
    public Address? Address { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string? Phone { get; set; }
    
    public string? BankName { get; set; }
    
    public string? BankAccountNumber { get; set; }
    
    public string? IBAN { get; set; }
    
    public string? TaxNumber { get; set; }
    public BranchStatusResponseDto? BranchStatus { get; set; }
    public Guid? BranchStatusId { get; set; }
    public Guid? CompanyDetailsId { get; set; }
}