using SmartAndPro.ERP.Application.Common.Dtos.Attachments;

namespace SmartAndPro.ERP.Application.Common.Dtos.Return.Sale
{
    internal class SalesReturnResponseDto
    {
        public int Id { get; set; }
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
        public Guid? TransactionId { get; set; }
        public string? SalesInvoiceId { get; set; }

        public List<SalesReturnItemDto> SalesReturnItems { get; set; } = new List<SalesReturnItemDto>();
        public List<AttachmentResponseDto> Attachments { get; set; }
    }
}
