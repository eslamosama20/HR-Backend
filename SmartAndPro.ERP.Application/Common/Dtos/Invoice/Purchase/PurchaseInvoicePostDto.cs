using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase
{
    public class PurchaseInvoicePostDto
    {
        public string? InvoiceNo { get; set; }
        public Guid SupplierDetailsId { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        [MinLength(1)]
        public List<PurchaseItemPostDto> PurchaseItems { get; set; }
        public bool IsFrontal { get; set; } = false;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }


    }
}
