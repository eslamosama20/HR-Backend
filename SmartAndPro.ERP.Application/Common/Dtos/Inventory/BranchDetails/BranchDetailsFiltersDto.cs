namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BranchDetails;

public class BranchDetailsFiltersDto
{
    public string? ArabicName { get; set; }
    public string? EnglishName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? TaxNumber { get; set; }
    
    public Guid? BranchId { get; set; }
    public Guid? CreatedBy { get; set; }
}