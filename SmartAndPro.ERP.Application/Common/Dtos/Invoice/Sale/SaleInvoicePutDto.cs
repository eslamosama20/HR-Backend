using SmartAndPro.ERP.Domains.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SaleInvoicePutDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string? Currency { get; set; }
        public Guid? CustomerProfileId { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public Guid? StoreDetailsId { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }

        [MinLength(1)]
        public List<SaleItemPostDto> SaleItems { get; set; }
        public bool? IsQuotation { get; set; } = false;
        public bool? IsFrontal { get; set; }
        public List<Guid>? ReservationIds { get; set; }
        public SaleInvoiceType SaleInvoiceType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }




    }
}
