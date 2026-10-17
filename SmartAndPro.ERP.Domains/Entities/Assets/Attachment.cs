using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Assets
{
    public class Attachment : Entity, IEntityTypeConfiguration<Attachment>
    {
        public string ReferenceNumber { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileData { get; set; } // Store the PDF content as binary data
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public Guid? InventoryPermitId { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public int? SaleInvoiceId { get; set; }
        public int? SalesReturnId { get; set; }
        public int? PurchaseReturnId { get; set; }
        public int? HotelAllotmentInvoiceId { get; set; }
        public Guid? PaymentReceiptId { get; set; }
        public Guid? PaycheckReceiptId { get; set; }
        public Guid? DebitNoteId { get; set; }
        public Guid? CreditNoteId { get; set; }
        public Guid? InventoryStocktakingId { get; set; }
        public Guid? ReservationId { get; set; }
        public Guid? TransactionId { get; set; }
        
        public Guid? MealReservationId { get; set; }
        
        public Guid? SalesCreditNoteId { get; set; }
        public int? PurchaseQuotationId { get; set; }


        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasQueryFilter(p => p.IsActive == true);
            
            
        }
    }
}
