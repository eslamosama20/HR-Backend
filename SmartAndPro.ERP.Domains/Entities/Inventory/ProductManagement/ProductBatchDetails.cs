using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

public class ProductBatchDetails : Entity, IAggregateRoot, IEntityTypeConfiguration<ProductBatchDetails>
{
    public sealed override Guid Id { get; set; }
    public DateTime ExpiryDate { get; set; }

    public Guid BatchStatusId { get; set; }
    public BatchStatus BatchStatus { get; set; }
    public override bool IsActive { get; set; }
    public override Guid CreatedBy { get; set; }

    public List<ProductBatchToUnitAssignments> ProductBatchToUnitAssignments { get; set; }
    public Guid StoreDetailsId { get; set; }


    public void Configure(EntityTypeBuilder<ProductBatchDetails> builder)
    {
        builder.ToTable("ProductBatchDetails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExpiryDate).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.Property(x => x.StoreDetailsId).IsRequired();
        builder.Property(x => x.BatchStatusId).IsRequired();

        // Many-to-many relationship with ProductUnits
        builder.HasMany(x => x.ProductBatchToUnitAssignments)
            .WithOne(p => p.ProductBatchDetails)
            .HasForeignKey(p => p.ProductBatchDetailsId);

        builder.HasOne(p => p.BatchStatus)
            .WithMany()
            .HasForeignKey(p => p.BatchStatusId);


        builder.HasQueryFilter(p => p.IsActive);

        builder.Navigation(e => e.ProductBatchToUnitAssignments).AutoInclude();
        builder.Navigation(e => e.BatchStatus).AutoInclude();
    }

    public void UpdateBatchStatus(BatchStatus batchStatus)
    {
        BatchStatus = batchStatus;
    }
}