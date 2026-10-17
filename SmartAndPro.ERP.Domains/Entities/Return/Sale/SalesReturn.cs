using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;

namespace SmartAndPro.ERP.Domains.Entities.Return.Sale
{
    public class SalesReturn : BaseReturn<int>, IEntityTypeConfiguration<SalesReturn>
    {
        public string? SalesInvoiceId { get; set; }

        public Guid CustomerProfileId { get; set; }
        public virtual CustomerProfile CustomerProfile { get; set; }
        
        public Guid? TransactionId { get; set; }

        public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; } = new List<SalesReturnItem>();

        public List<Attachment> Attachments { get; set; }
        public void Configure(EntityTypeBuilder<SalesReturn> builder)
        {
            RelationalEntityTypeBuilderExtensions.ToTable((EntityTypeBuilder)builder, "SalesReturn");
            builder.HasKey(b => b.Id);
            
            builder.HasOne<Transaction>()
                .WithOne()
                .HasForeignKey<SalesReturn>(i => i.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.SalesReturnId)
                .OnDelete(DeleteBehavior.NoAction);
            
            builder.Navigation(e => e.Attachments).AutoInclude();
            
            builder.Navigation(e => e.SalesReturnItems).AutoInclude();
            builder.Navigation(e => e.CustomerProfile).AutoInclude();

        }
    }
}
