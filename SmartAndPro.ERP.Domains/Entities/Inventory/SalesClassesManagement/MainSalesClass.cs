using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

public class MainSalesClass : Entity , IEntityTypeConfiguration<MainSalesClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }

    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public Guid DetailSalesClassId { get; set; }
    
    public string? Icon { get; set; }
    
    public ClassType ClassType { get; set; } = ClassType.Direct;
    
    public MeasurementUnit MeasurementUnit { get; set; }
    public Guid MeasurementUnitId { get; set; }
    

    public bool IsTaxExempted { get; set; } = false;

    // يميز الأصناف المنشأة أوتو من الحجوزات (غرف/وجبات/إطلالات/خدمات)
    // الأصناف العادية = false، أصناف الحجوزات = true
    public bool IsReservationItem { get; set; } = false;
    
    public void Configure(EntityTypeBuilder<MainSalesClass> builder)
    {
        builder
            .ToTable("MainSalesClass")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.IsTaxExempted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsReservationItem)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(x => x.ReferenceCode)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.ClassType)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        
        builder.HasIndex(x => new { x.ArabicName, x.IsReservationItem })
            .IsUnique();
        
        builder.HasIndex(x => new { x.EnglishName, x.IsReservationItem })
            .IsUnique();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(x => x.ReferenceCode)
            .IsUnique();
        
        builder.Property(x => x.Icon)
            .HasMaxLength(300);
        
        builder.HasOne(x => x.ClientProfile)
            .WithMany()
            .HasForeignKey(x => x.ClientProfileId);
        
        builder.HasOne(p => p.MeasurementUnit)
            .WithMany()
            .HasForeignKey(p => p.MeasurementUnitId);
        
        
        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();

        
        builder.Navigation(x => x.MeasurementUnit)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}