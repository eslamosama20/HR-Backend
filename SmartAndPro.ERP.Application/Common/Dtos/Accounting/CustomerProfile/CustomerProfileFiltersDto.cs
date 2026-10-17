namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;

public class CustomerProfileFiltersDto
{
    public string? ArabicName { get; set; }
   
    public string? EnglishName { get; set; }
    
    public string? ContactNumber { get; set; }
    
    public bool? IsReservationCompany { get; set; }
    
    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// Case-insensitive substring match. Empty or null returns all records.
    /// </summary>
    public string? SearchKeyword { get; set; }
}