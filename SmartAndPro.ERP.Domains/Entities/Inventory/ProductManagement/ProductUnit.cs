using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

public class ProductUnit : Entity, IEntityTypeConfiguration<ProductUnit>
{
    public sealed override Guid Id { get; set; }
    public override bool IsActive { get; set; } = true;
    public override Guid CreatedBy { get; set; }
    public decimal? Quantity { get; set; } = 0;
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    public Guid? MainSalesClassId { get; set; }
    public Guid? MainPurchaseClassId { get; set; }

    public MainPurchaseClass MainPurchaseClass { get; set; }
    public MainSalesClass MainSalesClass { get; set; }
    
    public StoreDetails StoreDetails { get; set; }
    public Guid? StoreDetailsId { get; set; }


    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        builder
            .ToTable("ProductUnit")
            .HasKey(x => x.Id);
        
        // unique index on both store and main purchase class or main sales class
        builder.HasIndex(x => new { x.StoreDetailsId, x.MainPurchaseClassId})
            .IsUnique();

        builder.Property(x => x.Quantity)
            .IsRequired();
        

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.HasOne(p => p.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(p => p.MainPurchaseClassId);
        
        builder.HasOne(p => p.MainSalesClass)
            .WithMany()
            .HasForeignKey(p => p.MainSalesClassId);

        builder.HasOne(p => p.StoreDetails)
            .WithMany()
            .HasForeignKey(p => p.StoreDetailsId);
        

        builder.Navigation(e => e.StoreDetails).AutoInclude();
        builder.Navigation(e => e.MainPurchaseClass).AutoInclude();
        builder.Navigation(e => e.MainSalesClass).AutoInclude();

        builder.HasQueryFilter(p => p.IsActive);
    }
}