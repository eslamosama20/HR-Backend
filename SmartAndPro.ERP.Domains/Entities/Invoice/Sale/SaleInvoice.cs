using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Invoice.Sale
{
    public class SaleInvoice : BaseInvoice<int>, IEntityTypeConfiguration<SaleInvoice>
    {
        public Guid CustomerProfileId { get; set; }
        
        public CustomerProfile CustomerProfile { get; set; }
        public Guid? StoreDetailsId { get; set; }
        
        public Guid? TransactionId { get; set; }
        
        public Guid? ProjectId { get; set; }
        public virtual SmartAndPro.ERP.Domains.Entities.Projects.Project? Project { get; set; }
        
        public Guid? CustomerDepartmentId { get; set; }
        public virtual CustomerDepartment? CustomerDepartment { get; set; }
        
        public bool IsQuotation { get; set; } = false;
        public int? ConvertedToInvoiceId { get; set; } // For quotations: ID of the invoice it was converted to
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        
        public bool? IsCleared { get; set; } = false;
        public bool? IsFrontal { get; set; } = false;        
        public string? InvoiceHash { get; set; }
        
        public string? GeneratedQR { get; set; }

        public List<Attachment> Attachments { get; set; }
        
        public List<SalesCreditNote> SalesCreditNotes { get; set; }
        public List<ReceiptConnection> ReceiptConnections { get; set; } = new();
        public List<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation> Reservations { get; set; } = new();
        public SaleInvoiceType SaleInvoiceType { get; set; } = SaleInvoiceType.SaleInvoice;

        public decimal PaidAmount { get; set; } = 0;
        public decimal RemainingAmount { get; set; } = 0;
        public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.Unpaid;
        public void Configure(EntityTypeBuilder<SaleInvoice> builder)
        {
            builder.ToTable("SaleInvoice");
            builder.HasKey(b => b.Id);
            
            builder.HasMany<SalesCreditNote>()
                .WithOne(scn => scn.SaleInvoice)
                .HasForeignKey(scn => scn.SaleInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(e => e.InvoiceNo)
                .IsUnique();

            builder.HasIndex(e => e.SaleInvoiceType);  

            builder.Property(e => e.IsCleared)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(e => e.InvoiceHash)
                .HasMaxLength(999);
            
            builder.Property(e => e.GeneratedQR)
                .HasMaxLength(Int32.MaxValue);
            
            builder.Property(e => e.IsQuotation)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(e => e.PaidAmount)
                .HasPrecision(18, 2);

            builder.Property(e => e.RemainingAmount)
                .HasPrecision(18, 2);
            
            builder.HasOne<Transaction>()
                .WithOne()
                .HasForeignKey<SaleInvoice>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);


            builder.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.SaleInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Removed AutoInclude to avoid HotelAllotmentInvoiceId column error
            // builder.Navigation(e => e.Attachments).AutoInclude();

            builder.HasMany(x => x.ReceiptConnections)
         .WithOne(x=>x.SaleInvoice)
         .HasForeignKey(x => x.SaleInvoiceId)
         .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(x => x.ReceiptConnections)
                .AutoInclude();

            builder.Navigation(e => e.SaleItems).AutoInclude();
            builder.Navigation(e => e.CustomerProfile).AutoInclude();

            builder.HasIndex(e => e.ReferenceNumber)
                  .IsUnique()
                  .HasFilter("[ReferenceNumber] IS NOT NULL");

            builder.HasIndex(e => e.PaymentStatus);

            builder.HasIndex(e => e.IssueDate);

            builder.HasMany(e => e.Reservations)
               .WithOne(e=>e.SaleInvoice)
               .HasForeignKey(e => e.SaleInvoiceId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(e => e.Reservations).AutoInclude();


            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.CustomerDepartment)
                .WithMany()
                .HasForeignKey(x => x.CustomerDepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(x => x.Project).AutoInclude();
            builder.Navigation(x => x.CustomerDepartment).AutoInclude();
        }
    }
}
