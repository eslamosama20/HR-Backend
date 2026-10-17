namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.RootAccount;

public class RootAccountFiltersDto 
{
    public string? ArabicName { get; set; }
   
    public string? EnglishName { get; set; }
    
    public string? ReferenceCode { get; set; }

    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// </summary>
    public string? SearchKeyword { get; set; }
}