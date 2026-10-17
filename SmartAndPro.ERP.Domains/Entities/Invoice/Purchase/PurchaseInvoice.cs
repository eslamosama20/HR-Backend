using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.Domains.Entities.Receipts;


namespace SmartAndPro.ERP.Domains.Entities.Invoice.Purchase
{
    public class PurchaseInvoice : BaseInvoice<int>, IEntityTypeConfiguration<PurchaseInvoice>
    {
        public Guid SupplierDetailsId { get; set; }
        public SupplierDetails SupplierDetails { get; set; }
        public Guid? TransactionId { get; set; }
        public virtual List<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        
        public List<Attachment> Attachments { get; set; }
        
        public bool IsFrontal { get; set; } = false;

        public List<ReceiptConnection> ReceiptConnections { get; set; } = new();

        public int? PurchaseQuotationId { get; set; }
        public PurchaseQuotation? PurchaseQuotation { get; set; }

        public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
        {
            builder.ToTable("PurchaseInvoice");
            builder.HasKey(b => b.Id);
            builder.HasIndex(e => e.ReferenceNumber)
                  .IsUnique()
                  .HasFilter("[ReferenceNumber] IS NOT NULL");

            builder.HasIndex(e => e.IssueDate);

            builder.HasOne<Transaction>()
                .WithOne()
                .HasForeignKey<PurchaseInvoice>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            
            builder.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);
            
            builder.Navigation(e => e.Attachments).AutoInclude();

            builder.HasMany(x => x.ReceiptConnections)
         .WithOne(x=>x.PurchaseInvoice)
         .HasForeignKey(x => x.PurchaseInvoiceId)
         .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(x => x.ReceiptConnections)
                .AutoInclude();

            builder.Navigation(e => e.PurchaseItems).AutoInclude();
            builder.Navigation(e => e.SupplierDetails).AutoInclude();

            builder.HasOne(x => x.PurchaseQuotation)
                .WithOne()
                .HasForeignKey<PurchaseInvoice>(x => x.PurchaseQuotationId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
