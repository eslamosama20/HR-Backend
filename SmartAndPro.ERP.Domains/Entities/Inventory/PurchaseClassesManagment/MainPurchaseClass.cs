using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

public class MainPurchaseClass : Entity , IEntityTypeConfiguration<MainPurchaseClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }

    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public Guid DetailPurchaseClassId { get; set; }
    
    public string? Icon { get; set; }
    
    public string? ImageUrl { get; set; }
    public MeasurementUnit MeasurementUnit { get; set; }
    public Guid MeasurementUnitId { get; set; }

    // Pharmacy / Item behavior fields (PDF spec §6)
    public ItemType ItemType { get; set; } = ItemType.General;       // general | medication | service
    public bool NeedsBatch { get; set; }
    public bool NeedsExpiry { get; set; }
    public bool IsPharmacyItem { get; set; }
    public string? SfdaCode { get; set; }
    public bool IsControlled { get; set; } = false;

    public void Configure(EntityTypeBuilder<MainPurchaseClass> builder)
    {
        builder.Property(x => x.IsControlled)
            .IsRequired()
            .HasDefaultValue(false);

        builder
            .ToTable("MainPurchaseClass")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.ReferenceCode)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);



        
        builder.HasIndex(x => x.ArabicName)
            .IsUnique();
        
        builder.HasIndex(x => x.EnglishName)
            .IsUnique();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(x => x.ReferenceCode)
            .IsUnique();
        
        builder.Property(x => x.Icon)
            .HasMaxLength(300);

        builder.Property(x => x.ItemType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(SmartAndPro.ERP.Domains.Enums.ItemType.General)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<SmartAndPro.ERP.Domains.Enums.ItemType>(v, true)
            );

        builder.Property(x => x.NeedsBatch)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.NeedsExpiry)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsPharmacyItem)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SfdaCode)
            .HasMaxLength(100);

        builder.HasOne(p => p.MeasurementUnit)
            .WithMany()
            .HasForeignKey(p => p.MeasurementUnitId);

        
        builder.HasOne(x => x.ClientProfile)
            .WithMany()
            .HasForeignKey(x => x.ClientProfileId);
        
        builder.Navigation(x => x.MeasurementUnit)
            .AutoInclude();


        builder.Property(x => x.SfdaCode).HasMaxLength(50);

        builder.HasQueryFilter(p => p.IsActive == true);

    }
}