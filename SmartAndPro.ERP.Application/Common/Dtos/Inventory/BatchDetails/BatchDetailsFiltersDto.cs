namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;

public class BatchDetailsFiltersDto
{
    public DateTime? ExpiryDate { get; set; }

    public Guid? BatchStatusId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? StoreDetailsId { get; set; }

    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// Case-insensitive substring match. Empty or null returns all records.
    /// </summary>
    public string? SearchKeyword { get; set; }
}