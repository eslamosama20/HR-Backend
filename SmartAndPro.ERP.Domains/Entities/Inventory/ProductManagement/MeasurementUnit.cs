using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

public class MeasurementUnit : Entity ,IEntityTypeConfiguration<MeasurementUnit>
{
    public sealed override Guid Id { get; set; }
    [Searchable]
    public string ArabicName { get; set; }
    [Searchable]
    public string EnglishName { get; set; }
    [Searchable]
    public string ArabicSymbol { get; set; }
    [Searchable]
    public string EnglishSymbol { get; set; }
    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }
    


    public void Configure(EntityTypeBuilder<MeasurementUnit> builder)
    {
        builder
            .ToTable("MeasurementUnit")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.ArabicSymbol)
            .IsRequired()
            .HasMaxLength(10);
        builder.Property(x => x.EnglishSymbol)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.CreatedBy)
            .IsRequired();
        
        builder.HasQueryFilter(p => p.IsActive);
        
        
    }
}