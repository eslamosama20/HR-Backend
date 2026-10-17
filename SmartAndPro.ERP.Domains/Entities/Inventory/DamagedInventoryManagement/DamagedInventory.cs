using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.DamagedInventoryManagement;

public class DamagedInventory : Entity, IEntityTypeConfiguration<DamagedInventory>
{
    public Guid? MainPurchaseClassId { get; set; }
    public MainPurchaseClass? MainPurchaseClass { get; set; }
    
    public Guid? MainSalesClassId { get; set; }
    public MainSalesClass? MainSalesClass { get; set; }
    
    public decimal Quantity { get; set; }
    
    public DateTime DamageDate { get; set; } = DateTime.UtcNow;
    
    public string? Reason { get; set; }
    
    public string? ReportedBy { get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public StoreDetails? StoreDetails { get; set; }

    public void Configure(EntityTypeBuilder<DamagedInventory> builder)
    {
        builder
            .ToTable("DamagedInventory")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasColumnType("decimal(18,4)");
        
        builder.Property(x => x.DamageDate)
            .IsRequired();
        
        builder.Property(x => x.Reason)
            .HasMaxLength(500);
        
        builder.Property(x => x.ReportedBy)
            .HasMaxLength(200);

        builder.HasOne(x => x.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(x => x.MainPurchaseClassId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.MainSalesClass)
            .WithMany()
            .HasForeignKey(x => x.MainSalesClassId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.StoreDetails)
            .WithMany()
            .HasForeignKey(x => x.StoreDetailsId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
