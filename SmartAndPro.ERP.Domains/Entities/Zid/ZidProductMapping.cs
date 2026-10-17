using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Zid;

/// <summary>
/// Maps a Zid product to an ERP MainPurchaseClass (product tree)
/// </summary>
public class ZidProductMapping : Entity, IEntityTypeConfiguration<ZidProductMapping>
{
    public long ZidProductId { get; set; }
    public string? ZidSku { get; set; }
    public string ZidProductName { get; set; } = string.Empty;
    public decimal? ZidPrice { get; set; }

    /// <summary>Mapped ERP MainPurchaseClass Id (شجرة أصناف المشتريات)</summary>
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }

    /// <summary>ERP store for stock sync</summary>
    public Guid? StoreDetailsId { get; set; }

    public bool IsMapped => MainPurchaseClassId.HasValue;
    public DateTime? LastSyncedAt { get; set; }

    public void Configure(EntityTypeBuilder<ZidProductMapping> builder)
    {
        builder.ToTable("ZidProductMappings").HasKey(x => x.Id);

        builder.Property(x => x.ZidProductId).IsRequired();
        builder.Property(x => x.ZidSku).HasMaxLength(100);
        builder.Property(x => x.ZidProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ZidPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MainPurchaseClassName).HasMaxLength(300);

        builder.HasIndex(x => x.ZidProductId).IsUnique();
        builder.HasIndex(x => x.MainPurchaseClassId);
        builder.HasIndex(x => x.StoreDetailsId);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
