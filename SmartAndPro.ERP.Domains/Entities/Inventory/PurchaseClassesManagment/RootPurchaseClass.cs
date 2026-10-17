using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

public class RootPurchaseClass : Entity , IEntityTypeConfiguration<RootPurchaseClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }
    public string? Icon { get; set; }
    
    public List<HeaderPurchaseClass> HeaderPurchaseClasses { get; set; }
    public void Configure(EntityTypeBuilder<RootPurchaseClass> builder)
    {
        builder
            .ToTable("RootPurchaseClass")
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
        
        builder.Property(x => x.Icon)
            .HasMaxLength(300);
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(x => x.ArabicName)
            .IsUnique();
        
        builder.HasIndex(x => x.EnglishName)
            .IsUnique();
        
        builder.HasIndex(x => x.ReferenceCode)
            .IsUnique();
        
        builder.HasMany(x => x.HeaderPurchaseClasses)
            .WithOne()
            .HasForeignKey(x => x.RootPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.HeaderPurchaseClasses)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}