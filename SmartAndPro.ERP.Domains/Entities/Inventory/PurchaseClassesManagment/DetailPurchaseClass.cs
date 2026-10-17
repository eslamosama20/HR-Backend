using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

public class DetailPurchaseClass : Entity , IEntityTypeConfiguration<DetailPurchaseClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }

    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public Guid HeaderPurchaseClassId { get; set; }
    
    public List<MainPurchaseClass> MainPurchaseClasses { get; set; }
    
    public string? Icon { get; set; }
    
    public void Configure(EntityTypeBuilder<DetailPurchaseClass> builder)
    {
        builder
            .ToTable("DetailPurchaseClass")
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
        
        builder.HasOne(x => x.ClientProfile)
            .WithMany()
            .HasForeignKey(x => x.ClientProfileId);
        
        
        
        
        builder.HasMany(x => x.MainPurchaseClasses)
            .WithOne()
            .HasForeignKey(x => x.DetailPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.MainPurchaseClasses)
            .AutoInclude();

        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}