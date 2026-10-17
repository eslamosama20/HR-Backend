using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Accounting;

public class TransactionType : Entity , IEntityTypeConfiguration<TransactionType>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder
            .ToTable("TransactionType")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(x => x.ArabicName)
            .IsUnique();
        
        builder.HasIndex(x => x.EnglishName)
            .IsUnique();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasQueryFilter(p => p.IsActive == true);

    }
}