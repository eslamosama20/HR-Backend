using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Salla;

public class SallaOrderItem : Entity, IEntityTypeConfiguration<SallaOrderItem>
{
    public Guid SallaOrderId { get; set; }
    public long SallaProductId { get; set; }
    public string? SallaSku { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? OptionsSummary { get; set; }

    /// <summary>Mapped ERP MainPurchaseClass Id (from شجرة أصناف المشتريات)</summary>
    public Guid? MainPurchaseClassId { get; set; }

    // Navigation
    public virtual SallaOrder SallaOrder { get; set; } = null!;

    public void Configure(EntityTypeBuilder<SallaOrderItem> builder)
    {
        builder.ToTable("SallaOrderItems").HasKey(x => x.Id);

        builder.Property(x => x.SallaProductId).IsRequired();
        builder.Property(x => x.SallaSku).HasMaxLength(100);
        builder.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.OptionsSummary).HasMaxLength(500);

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.SallaOrderId);
        builder.HasIndex(x => x.SallaProductId);
        builder.HasIndex(x => x.MainPurchaseClassId);
    }
}
