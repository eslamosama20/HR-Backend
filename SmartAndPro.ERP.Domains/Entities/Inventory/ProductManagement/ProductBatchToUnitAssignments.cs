using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

public class ProductBatchToUnitAssignments : Entity, IEntityTypeConfiguration<ProductBatchToUnitAssignments>
{
    public sealed override Guid Id { get; set; }

    public Guid ProductUnitId { get; set; }
    public Guid ProductBatchDetailsId { get; set; }
    public Guid BatchUnitStatusId { get; set; }

    public ProductBatchDetails ProductBatchDetails { get; set; }
    public BatchUnitStatus BatchUnitStatus { get; set; }
    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }


    public void Configure(EntityTypeBuilder<ProductBatchToUnitAssignments> builder)
    {
        builder
            .ToTable("ProductBatchToUnitAssignments")
            .HasKey(x => x.Id);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.HasOne(p => p.BatchUnitStatus)
            .WithMany()
            .HasForeignKey(p => p.BatchUnitStatusId);

        builder.Property(x => x.ProductUnitId).IsRequired();
        builder.Property(x => x.BatchUnitStatusId).IsRequired();
        builder.Property(x => x.ProductBatchDetailsId).IsRequired();
        


        builder.HasQueryFilter(p => p.IsActive);

        builder.Navigation(e => e.BatchUnitStatus).AutoInclude();
    }
}