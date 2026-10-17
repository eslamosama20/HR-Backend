namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase
{
    public class PurchaseInvoiceFilterDto
    {
        public string? InvoiceNo { get; set; }
        public Guid? SupplierDetailsId { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? IsActive { get; set; }
        public bool IsFrontal { get; set; } = false;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }


    }
}
