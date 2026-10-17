namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class ReservationInvoiceListDto
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal NetPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string CustomerProfileName { get; set; }
    public Guid? TransactionId { get; set; }
    public List<Guid> ReservationIds { get; set; } = new();
}
