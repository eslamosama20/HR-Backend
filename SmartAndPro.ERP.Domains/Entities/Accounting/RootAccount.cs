using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class RootAccount : Entity , IEntityTypeConfiguration<RootAccount>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ReferenceCode { get; set; }
    public string? Icon { get; set; }
    
    public List<HeaderAccount> HeaderAccounts { get; set; }
    public void Configure(EntityTypeBuilder<RootAccount> builder)
    {
        builder
            .ToTable("RootAccount")
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
        
        builder.HasMany(x => x.HeaderAccounts)
            .WithOne()
            .HasForeignKey(x => x.RootAccountId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.HeaderAccounts)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}