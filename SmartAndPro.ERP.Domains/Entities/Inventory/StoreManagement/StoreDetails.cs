using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

public class StoreDetails : Entity, IAggregateRoot, IEntityTypeConfiguration<StoreDetails>
{
    public sealed override Guid Id { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }

    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }
    public Guid? CompanyId { get; set; }
    public List<ProductBatchDetails> Batches { get; set; }
    public StoreStatus StoreStatus { get; set; }
    public Guid? StoreStatusId { get; set; }

    public StoreType StoreType { get; set; } = StoreType.Store;

    // PDF spec §8 – StoreCategory: general | pharmacy | warehouse | retail
    public string StoreCategory { get; set; } = "general";

    public Guid? MainAccountId { get; set; }
    public MainAccount MainAccount { get; set; }
    public void Configure(EntityTypeBuilder<StoreDetails> builder)
    {
        builder.ToTable("StoreDetails");
        builder.HasKey(b => b.Id);


        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedBy).IsRequired();
        builder.Property(b => b.ArabicName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.EnglishName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.StoreType)
            .IsRequired()
            .HasDefaultValue(StoreType.Store)
            .HasConversion(
                storeType => storeType.ToString(),
                storeTypeString => Enum.Parse<StoreType>(storeTypeString)
            );

        builder.Property(b => b.StoreCategory)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("general");


        builder.HasMany<ProductBatchDetails>(b => b.Batches)
            .WithOne()
            .HasForeignKey(x => x.StoreDetailsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.StoreStatus)
            .WithMany()
            .HasForeignKey(p => p.StoreStatusId);
        
        builder.HasOne(x => x.MainAccount)
            .WithMany()
            .HasForeignKey(x => x.MainAccountId);
        builder.Navigation(x => x.MainAccount).AutoInclude();


        builder.HasQueryFilter(p => p.IsActive);

        builder.Navigation(e => e.StoreStatus).AutoInclude();
    }


    public void AddBatch(ProductBatchDetails batch)
    {
        Batches.Add(batch);
    }

    public void RemoveBatch(ProductBatchDetails batch)
    {
        Batches.Remove(batch);
    }
}