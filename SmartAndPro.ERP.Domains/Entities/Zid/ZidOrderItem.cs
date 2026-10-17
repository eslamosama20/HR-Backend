using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Zid;

public class ZidOrderItem : Entity, IEntityTypeConfiguration<ZidOrderItem>
{
    public Guid ZidOrderId { get; set; }
    public long ZidProductId { get; set; }
    public string? ZidSku { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? OptionsSummary { get; set; }

    /// <summary>Mapped ERP MainPurchaseClass Id</summary>
    public Guid? MainPurchaseClassId { get; set; }

    // Navigation
    public virtual ZidOrder ZidOrder { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ZidOrderItem> builder)
    {
        builder.ToTable("ZidOrderItems").HasKey(x => x.Id);

        builder.Property(x => x.ZidProductId).IsRequired();
        builder.Property(x => x.ZidSku).HasMaxLength(100);
        builder.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.OptionsSummary).HasMaxLength(500);

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.ZidOrderId);
        builder.HasIndex(x => x.ZidProductId);
        builder.HasIndex(x => x.MainPurchaseClassId);
    }
}
