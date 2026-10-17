using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

public class HeaderSalesClass : Entity , IEntityTypeConfiguration<HeaderSalesClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    
    public Guid RootSalesClassId { get; set; }
    
    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public string? Icon { get; set; }
    
    public List<DetailSalesClass> DetailSalesClasses { get; set; }

    public void Configure(EntityTypeBuilder<HeaderSalesClass> builder)
    {
        builder
            .ToTable("HeaderSalesClass")
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
        
        builder.HasMany(x => x.DetailSalesClasses)
            .WithOne()
            .HasForeignKey(x => x.HeaderSalesClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.DetailSalesClasses)
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