namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;

public class CompanyDetailsFiltersDto
{

    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// Case-insensitive substring match. Empty or null returns all records.
    /// </summary>
    public string? SearchKeyword { get; set; }

    public string? ArabicName { get; set; }
    public string? EnglishName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? TaxNumber { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? CreatedBy { get; set; }
}