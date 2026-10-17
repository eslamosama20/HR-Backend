using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations.PurchaseQuotationItems;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations
{
    public class PurchaseQuotationPutDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public string? Currency { get; set; }
        public Guid? SupplierDetailsId { get; set; }
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
        public List<PurchaseQuotationItemPostDto> PurchaseQuotationItems { get; set; }
        public bool IsFrontal { get; set; } = false;
        
    }
}
