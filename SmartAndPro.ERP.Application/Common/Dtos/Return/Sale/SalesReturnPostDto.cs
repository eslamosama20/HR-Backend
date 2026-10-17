using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Return.Sale
{
    public class SalesReturnPostDto
    {
        public string? ReturnNo { get; set; }
        public string? Currency { get; set; }
        public Guid? CustomerProfileId { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        public string? SalesInvoiceId { get; set; }

        [MinLength(1)]
        public List<SalesReturnItemPostDto> SalesReturnItems { get; set; }
    }
}
