namespace SmartAndPro.ERP.Application.Common.Dtos.Attachments
{
    public class AttachmentPostDto
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string File { get; set; } // Handles file upload
        
        
        public Guid? InventoryPermitId { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public int? SaleInvoiceId { get; set; }
        public int? SalesReturnId { get; set; }
        public int? PurchaseReturnId { get; set; }
        public Guid? PaymentReceiptId { get; set; }
        public Guid? PaycheckReceiptId { get; set; }
        public Guid? DebitNoteId { get; set; }
        public Guid? CreditNoteId { get; set; }
        public Guid? InventoryStocktakingId { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
