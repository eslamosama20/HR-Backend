using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

public class BatchUnitStatus :Entity ,IEntityTypeConfiguration<BatchUnitStatus>
{
    
    public sealed override Guid Id { get; set; }
    
    [Searchable]
    public string ArabicName { get; set; }
    
    [Searchable]
    public string EnglishName { get; set; }

    
    public void Configure(EntityTypeBuilder<BatchUnitStatus> builder)
    {
        builder
            .ToTable("BatchUnitStatuses")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.EnglishName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasQueryFilter(p => p.IsActive);
    }
}