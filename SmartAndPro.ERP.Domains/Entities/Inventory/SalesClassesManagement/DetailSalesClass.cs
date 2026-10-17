using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

public class DetailSalesClass : Entity , IEntityTypeConfiguration<DetailSalesClass>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }

    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public Guid HeaderSalesClassId { get; set; }
    
    public List<MainSalesClass> MainSalesClasses { get; set; }
    
    public string? Icon { get; set; }
    public bool IsForProjects { get; set; } = false;
    
    public void Configure(EntityTypeBuilder<DetailSalesClass> builder)
    {
        builder
            .ToTable("DetailSalesClass")
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

        builder.Property(x => x.IsForProjects)
            .IsRequired()
            .HasDefaultValue(false);

        
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
        
        
        
        
        builder.HasMany(x => x.MainSalesClasses)
            .WithOne()
            .HasForeignKey(x => x.DetailSalesClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.MainSalesClasses)
            .AutoInclude();

        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}