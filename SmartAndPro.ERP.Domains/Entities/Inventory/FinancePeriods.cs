using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Inventory;

public class FinancePeriods : Entity , IEntityTypeConfiguration<FinancePeriods>
{
    public sealed override Guid Id { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }


    public void Configure(EntityTypeBuilder<FinancePeriods> builder)
    {
        builder
            .ToTable("FinancePeriods")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100)
            ;
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasQueryFilter(p => p.IsActive);
    }
}