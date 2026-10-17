namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreStatus;

public class StoreStatusFiltersDto 
{
    public string? ArabicName { get; set; }
   
    public string? EnglishName { get; set; }

    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// </summary>
    public string? SearchKeyword { get; set; }
}