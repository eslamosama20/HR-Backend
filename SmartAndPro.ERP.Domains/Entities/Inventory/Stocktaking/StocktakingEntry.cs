using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;

public class StocktakingEntry : Entity , IEntityTypeConfiguration<StocktakingEntry>
{
    
    public decimal ReportedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public Guid MainPurchaseClassId { get; set; }
    public MainPurchaseClass MainPurchaseClass{ get; set; }
    
    public Guid InventoryStocktakingId { get; set; }

    public void Configure(EntityTypeBuilder<StocktakingEntry> builder)
    {
        builder
            .ToTable("StocktakingEntry")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ReportedQuantity)
            .IsRequired();
        
        builder.HasOne(x=>x.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(x => x.MainPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.MainPurchaseClass)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}