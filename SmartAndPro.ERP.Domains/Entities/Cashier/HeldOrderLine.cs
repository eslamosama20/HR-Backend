using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

public class HeldOrderLine : Entity, IEntityTypeConfiguration<HeldOrderLine>
{
    public Guid HeldOrderId { get; set; }
    public virtual HeldOrder HeldOrder { get; set; } = null!;

    public Guid ProductId { get; set; } // MainPurchaseClassId (resolved to Guid)
    public int? ProductUnitId { get; set; } // MeasurementUnitId / UnitId if exists
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }

    public void Configure(EntityTypeBuilder<HeldOrderLine> builder)
    {
        builder.ToTable("HeldOrderLines").HasKey(x => x.Id);

        builder.Property(x => x.HeldOrderId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.VatRate).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.VatAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(200);
        builder.Property(x => x.ItemCode).HasMaxLength(100);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
