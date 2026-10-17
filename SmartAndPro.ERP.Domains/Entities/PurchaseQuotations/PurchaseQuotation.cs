using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Enums;


namespace SmartAndPro.ERP.Domains.Entities.PurchaseQuotations
{
    public class PurchaseQuotation : BaseInvoice<int>, IEntityTypeConfiguration<PurchaseQuotation>
    {
        public Guid SupplierDetailsId { get; set; }
        public SupplierDetails SupplierDetails { get; set; }
        //public Guid? TransactionId { get; set; }
        //public virtual List<PurchaseQuotationItem> PurchaseQuotationItems { get; set; } = new List<PurchaseQuotationItem>();

        public virtual List<PurchaseQuotationItem> PurchaseQuotationItems { get; set; } = new List<PurchaseQuotationItem>();

        public List<Attachment> Attachments { get; set; } = new ();
        
        public bool IsFrontal { get; set; } = false;

        //public int? PurchaseInvoiceId { get; set; }
        //public PurchaseInvoice? PurchaseInvoice { get; set; }
        public PurchaseQuotationStatus Status { get; set; } = PurchaseQuotationStatus.Draft;
        //public bool IsConvertedToInvoice
        //  => Status == PurchaseQuotationStatus.ConvertedToInvoice;

        //public List<ReceiptConnection> ReceiptConnections { get; set; } = new();

        public void Configure(EntityTypeBuilder<PurchaseQuotation> builder)
        {
            builder.ToTable(nameof(PurchaseQuotation));
            builder.HasKey(b => b.Id);
            //builder.HasOne<Transaction>()
            //    .WithOne()
            //    .HasForeignKey<PurchaseQuotation>(i => i.TransactionId)
            //    .OnDelete(DeleteBehavior.SetNull);


            builder.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.PurchaseQuotationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(e => e.Attachments).AutoInclude();

            //   builder.HasMany(x => x.ReceiptConnections)
            //.WithOne(x=>x.PurchaseQuotation)
            //.HasForeignKey(x => x.PurchaseQuotationId)
            //.OnDelete(DeleteBehavior.NoAction);

            //builder.Navigation(x => x.ReceiptConnections)
            //    .AutoInclude();

            //builder.HasOne(x => x.PurchaseInvoice)
            //    .WithOne()
            //    .HasForeignKey<PurchaseQuotation>(x => x.PurchaseInvoiceId)
            //    .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(e => e.PurchaseQuotationItems).AutoInclude();
            builder.Navigation(e => e.SupplierDetails).AutoInclude();
        }
    }
}
