using SmartAndPro.ERP.Application.Common.Dtos.Attachments;

namespace SmartAndPro.ERP.Application.Common.Dtos.Return.Purchase
{
    public class PurchaseReturnResponseDto
    {
        public int Id { get; set; }
        public string? ReturnNo { get; set; }
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
        public Guid? TransactionId { get; set; }
        public string? PurchaseInvoiceId { get; set; }

        public List<PurchaseReturnItemDto> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItemDto>();
        public List<AttachmentResponseDto> Attachments { get; set; }
    }
}

