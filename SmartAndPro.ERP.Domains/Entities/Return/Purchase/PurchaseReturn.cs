using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;

namespace SmartAndPro.ERP.Domains.Entities.Return.Purchase
{
    public class PurchaseReturn : BaseReturn<int>, IEntityTypeConfiguration<PurchaseReturn>
    {
        public string? PurchaseInvoiceId { get; set; }

        public Guid SupplierDetailsId { get; set; }
        public SupplierDetails SupplierDetails { get; set; }
        public Guid? TransactionId { get; set; }
        public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();

        public List<Attachment> Attachments { get; set; }
        public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
        {
            RelationalEntityTypeBuilderExtensions.ToTable((EntityTypeBuilder)builder, "PurchaseReturn");
            builder.HasKey(b => b.Id);
            builder.HasOne<Transaction>()
                .WithOne()
                .HasForeignKey<PurchaseReturn>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            
            builder.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.PurchaseReturnId)
                .OnDelete(DeleteBehavior.NoAction);
            
            builder.Navigation(e => e.Attachments).AutoInclude();

            builder.Navigation(e => e.PurchaseReturnItems).AutoInclude();
            builder.Navigation(e => e.SupplierDetails).AutoInclude();
        }
    }
}
