using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SaleInvoiceSummaryDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public Guid? CustomerProfileId { get; set; }
        public string CustomerProfileName { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public InvoicePaymentStatus PaymentStatus { get; set; }
        public bool IsFrontal { get; set; }
        public bool IsQuotation { get; set; }
    }
}
