using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SaleInvoiceFilterDto
    {
        public string? InvoiceNo { get; set; }
        public Guid? CustomerProfileId { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? IsActive { get; set; }
        public SaleInvoiceType? SaleInvoiceType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }


    }
}
