using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;

namespace SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote
{
    public class SalesCreditNote : Entity, IEntityTypeConfiguration<SalesCreditNote>
    {
        public string AdjustmentReason { get; set; }
        
        public int SaleInvoiceId { get; set; }
        public SaleInvoice SaleInvoice { get; set; }
        
        public string? Description { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }
        public string SalesCreditNoteNo { get; set; }
        public decimal DiscountAmount { get; set; }

        public Guid CustomerProfileId { get; set; }
        
        public CustomerProfile CustomerProfile { get; set; }
        
        public Guid? TransactionId { get; set; }
        
        public List<SalesCreditNoteItem> SalesCreditNoteItems { get; set; } 
        
        public bool? IsCleared { get; set; } = false;
        
        public string? InvoiceHash { get; set; }
        
        public string? GeneratedQR { get; set; }

        public List<Attachment> Attachments { get; set; }
        public void Configure(EntityTypeBuilder<SalesCreditNote> builder)
        {
            builder.ToTable("SalesCreditNote");
            builder.HasKey(b => b.Id);
            
            builder.Property(e => e.IsCleared)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(e => e.InvoiceHash)
                .HasMaxLength(999);
            
            builder.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2);
            
            builder.HasOne<Transaction>()
                .WithOne()
                .HasForeignKey<SalesCreditNote>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);



            builder.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.SalesCreditNoteId)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Removed AutoInclude to avoid HotelAllotmentInvoiceId column error
            builder.Navigation(e => e.SaleInvoice).AutoInclude();

            builder.Navigation(e => e.SalesCreditNoteItems).AutoInclude();
            builder.Navigation(e => e.CustomerProfile).AutoInclude();

        }
    }
}
