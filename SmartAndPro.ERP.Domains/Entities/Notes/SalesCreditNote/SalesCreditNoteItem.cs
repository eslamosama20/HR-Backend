using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

namespace SmartAndPro.ERP.Domains.Entities.Notes.SalesCreditNote

{
    public class SalesCreditNoteItem : Entity, IEntityTypeConfiguration<SalesCreditNoteItem>
    {
        public string? ItemNo { get; set; }

        public Guid MainSalesClassId { get; set; }
        public MainSalesClass MainSalesClass { get; set; }
        public int NoOfNights { get; set; } = 1;
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } = 1;
        
        public Guid SalesCreditNoteId { get; set; }

        public SalesCreditNote SalesCreditNote { get; set; }
        public void Configure(EntityTypeBuilder<SalesCreditNoteItem> builder)
        {
            builder.Property(x => x.NoOfNights).HasDefaultValue(1);
            builder.Navigation(x => x.MainSalesClass).AutoInclude();

            builder.Property(e => e.Price)
                .HasPrecision(18, 2);
        }
    }
}
