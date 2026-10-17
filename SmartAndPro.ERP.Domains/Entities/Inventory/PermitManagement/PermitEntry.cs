using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;

public class PermitEntry : Entity , IEntityTypeConfiguration<PermitEntry>
{
    
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    
    // أسعار البيع الثلاثة (Three Sales Prices)
    public decimal RetailPrice { get; set; }        // سعر القطاعي
    public decimal WholesalePrice { get; set; }     // سعر الجملة
    public decimal BulkPrice { get; set; }          // سعر جملة الجملة
    
    public Guid MainPurchaseClassId { get; set; }
    public Guid? MainSalesClassId { get; set; }
    
    public string? ShelfNumber { get; set; }
    public string? RowNumber { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    
    public MainPurchaseClass? MainPurchaseClass{ get; set; }
    
    public MainSalesClass? MainSalesClass { get; set; }

    
    public Guid InventoryPermitId { get; set; }

    public void Configure(EntityTypeBuilder<PermitEntry> builder)
    {
        builder
            .ToTable("PermitEntry")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.BatchNo).HasMaxLength(100);

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasColumnType("money");
        
        builder.Property(x => x.Quantity)
            .IsRequired();
        
        builder.Property(x => x.RetailPrice)
            .HasColumnType("money")
            .HasDefaultValue(0m);
        
        builder.Property(x => x.WholesalePrice)
            .HasColumnType("money")
            .HasDefaultValue(0m);
        
        builder.Property(x => x.BulkPrice)
            .HasColumnType("money")
            .HasDefaultValue(0m);
        
        builder.HasOne(x=>x.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(x => x.MainPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x=>x.MainSalesClass)
            .WithMany()
            .HasForeignKey(x => x.MainSalesClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<InventoryPermit>()
            .WithMany(t => t.PermitEntries)
            .HasForeignKey(x => x.InventoryPermitId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.MainPurchaseClass)
            .AutoInclude();
        builder.Navigation(x => x.MainSalesClass)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }

}