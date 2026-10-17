using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;

public class SupplierStatus : Entity, IEntityTypeConfiguration<SupplierStatus>
{
    public sealed override Guid Id { get; set; }
    
    [Searchable]
    public string ArabicName { get; set; }
    
    [Searchable]
    public string EnglishName { get; set; }


    public void Configure(EntityTypeBuilder<SupplierStatus> builder)
    {
        builder
            .ToTable("SupplierStatus")
            .HasKey(x => x.Id);
        builder.Property(x => x.ArabicName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.EnglishName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasQueryFilter(p => p.IsActive);
    }
}