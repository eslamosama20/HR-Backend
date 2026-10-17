using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class DetailAccount : Entity , IEntityTypeConfiguration<DetailAccount>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }

    public Guid? ClientProfileId { get; set; }
    public ClientProfile ClientProfile { get; set; }
    
    public Guid HeaderAccountId { get; set; }
    
    public List<MainAccount> MainAccounts { get; set; }
    
    public string? Icon { get; set; }
    
    public void Configure(EntityTypeBuilder<DetailAccount> builder)
    {
        builder
            .ToTable("DetailAccount")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.ReferenceCode)
            .IsRequired()
            .HasMaxLength(50);
        
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
        
        
        
        
        builder.HasMany(x => x.MainAccounts)
            .WithOne()
            .HasForeignKey(x => x.DetailAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.MainAccounts)
            .AutoInclude();

        builder.Navigation(x => x.ClientProfile)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}