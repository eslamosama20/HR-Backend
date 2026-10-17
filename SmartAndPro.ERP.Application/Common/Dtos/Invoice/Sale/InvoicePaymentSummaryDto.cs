using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class InvoicePaymentSummaryDto
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; }

    public decimal InvoiceTotal { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public InvoicePaymentStatus PaymentStatus { get; set; }
}
