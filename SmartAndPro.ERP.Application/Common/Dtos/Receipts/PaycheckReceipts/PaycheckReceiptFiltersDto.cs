namespace SmartAndPro.ERP.Application.Common.Dtos.Receipts.PaycheckReceipts;

public class PaycheckReceiptFiltersDto
{
    public int? SaleInvoiceId { get; set; }

    /// <summary>
    /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
    /// </summary>
    public string? SearchKeyword { get; set; }
}