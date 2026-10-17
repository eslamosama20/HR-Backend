namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class SaleInvoiceImportFailureDto
{
    public int Row { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
