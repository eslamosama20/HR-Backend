using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Invoice.Sale

{
    public class SaleItem : BaseSaleItem<int?>, IEntityTypeConfiguration<SaleItem>
    {
        public int SaleInvoiceId { get; set; }
        public int NoOfNights { get; set; } = 1;

        //[JsonIgnore]
        public virtual SaleInvoice SaleInvoice { get; set; }
        public void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            builder.Property(x => x.NoOfNights).HasDefaultValue(1);
            builder.Navigation(x => x.MainSalesClass).AutoInclude();

            builder.Property(e => e.Price)
                .HasPrecision(18, 2);
            
            builder.Property(e => e.TaxAmount)
                .HasPrecision(18, 2);

            builder.Property(e => e.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalPrice)
                .HasPrecision(18, 2);
            
            builder.Property(e => e.NetPrice)
                .HasPrecision(18, 2);
            
            

        }
    }
}
