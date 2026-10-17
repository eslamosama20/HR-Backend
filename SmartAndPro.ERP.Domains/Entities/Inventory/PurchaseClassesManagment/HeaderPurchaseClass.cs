using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

public class HeaderPurchaseClass : Entity , IEntityTypeConfiguration<HeaderPurchaseClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    
    public Guid RootPurchaseClassId { get; set; }
    
    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public string? Icon { get; set; }
    
    public List<DetailPurchaseClass> DetailPurchaseClasses { get; set; }

    public void Configure(EntityTypeBuilder<HeaderPurchaseClass> builder)
    {
        builder
            .ToTable("HeaderPurchaseClass")
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
        
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.Icon)
            .HasMaxLength(300);
        
        builder.HasOne(x => x.ClientProfile)
            .WithMany()
            .HasForeignKey(x => x.ClientProfileId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(x => x.DetailPurchaseClasses)
            .WithOne()
            .HasForeignKey(x => x.HeaderPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.DetailPurchaseClasses)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

        builder.HasIndex(x => x.ArabicName)
            .IsUnique();
        
        builder.HasIndex(x => x.EnglishName)
            .IsUnique();
        
        builder.HasIndex(x => x.ReferenceCode)
            .IsUnique();
        
        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();
        
    }
}