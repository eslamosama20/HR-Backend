using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;

public class OperationEntry : Entity , IEntityTypeConfiguration<OperationEntry>
{
    
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public Guid MainPurchaseClassId { get; set; }
    public MainPurchaseClass MainPurchaseClass{ get; set; }
    
    public Guid? StoreDetailsId { get; set; }
    public StoreDetails StoreDetails { get; set; }
    

    public Guid InventoryOperationId { get; set; }
    public InventoryOperation InventoryOperation { get; set; }

    public void Configure(EntityTypeBuilder<OperationEntry> builder)
    {
        builder
            .ToTable("OperationEntry")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasColumnType("money");
        
        builder.Property(x => x.Quantity)
            .IsRequired();
        
        builder.HasOne(x=>x.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(x => x.MainPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x=>x.StoreDetails)
            .WithMany()
            .HasForeignKey(x => x.StoreDetailsId)
            .OnDelete(DeleteBehavior.NoAction);


        
        builder.HasOne(x=>x.InventoryOperation)
            .WithMany(t => t.OperationEntries)
            .HasForeignKey(x => x.InventoryOperationId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.InventoryOperation)
            .AutoInclude();
        builder.HasQueryFilter(p => p.IsActive == true);
    }

}