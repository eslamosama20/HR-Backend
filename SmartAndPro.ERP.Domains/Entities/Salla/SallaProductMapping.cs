using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums.Salla;

namespace SmartAndPro.ERP.Domains.Entities.Salla;

/// <summary>
/// Maps a Salla product/variant to an ERP ProductUnit
/// </summary>
public class SallaProductMapping : Entity, IEntityTypeConfiguration<SallaProductMapping>
{
    public long SallaProductId { get; set; }
    public string? SallaSku { get; set; }
    public string SallaProductName { get; set; } = string.Empty;
    public decimal? SallaPrice { get; set; }

    /// <summary>Mapped ERP MainPurchaseClass Id (from شجرة أصناف المشتريات)</summary>
    public Guid? MainPurchaseClassId { get; set; }
    public string? MainPurchaseClassName { get; set; }

    /// <summary>The ERP store this mapping belongs to (for stock sync)</summary>
    public Guid? StoreDetailsId { get; set; }

    public bool IsMapped => MainPurchaseClassId.HasValue;
    public DateTime? LastSyncedAt { get; set; }

    public void Configure(EntityTypeBuilder<SallaProductMapping> builder)
    {
        builder.ToTable("SallaProductMappings").HasKey(x => x.Id);

        builder.Property(x => x.SallaProductId).IsRequired();
        builder.Property(x => x.SallaSku).HasMaxLength(100);
        builder.Property(x => x.SallaProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.SallaPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MainPurchaseClassName).HasMaxLength(300);

        builder.HasIndex(x => x.SallaProductId).IsUnique();
        builder.HasIndex(x => x.MainPurchaseClassId);
        builder.HasIndex(x => x.StoreDetailsId);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
