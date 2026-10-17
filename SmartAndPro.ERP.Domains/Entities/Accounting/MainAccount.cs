using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class MainAccount : Entity , IEntityTypeConfiguration<MainAccount>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }

    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public Guid? DetailAccountId { get; set; }
    
    public string? Icon { get; set; }
    
    public void Configure(EntityTypeBuilder<MainAccount> builder)
    {
        builder
            .ToTable("MainAccount")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.ReferenceCode)
            .IsRequired()
            .HasMaxLength(100);
        
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
        
        
        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();

        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}