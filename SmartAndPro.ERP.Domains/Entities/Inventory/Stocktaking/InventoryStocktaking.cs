using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;

public class InventoryStocktaking : Entity , IEntityTypeConfiguration<InventoryStocktaking>
{
    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public DateTime StocktakingDate { get; set; } = DateTime.UtcNow;
    
    public Guid? TransactionId { get; set; }
    

    public List<StocktakingEntry> StocktakingEntries { get; set; }
    
    
    public Guid? StoreDetailsId { get; set; }
    public StoreDetails StoreDetails { get; set; }
    
    public List<Attachment> Attachments { get; set; }

    public void Configure(EntityTypeBuilder<InventoryStocktaking> builder)
    {
        builder
            .ToTable("InventoryStocktaking")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.StocktakingDate)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.HasMany(x => x.StocktakingEntries)
            .WithOne()
            .HasForeignKey(te => te.InventoryStocktakingId)
            .OnDelete(DeleteBehavior.NoAction);
        
        
        builder.HasOne(x=>x.StoreDetails)
            .WithMany()
            .HasForeignKey(x => x.StoreDetailsId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<InventoryStocktaking>(x => x.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        
        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.InventoryStocktakingId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.Attachments)
            .AutoInclude();

        builder.Navigation(x => x.StocktakingEntries)
            .AutoInclude();
        
        builder.Navigation(x => x.StoreDetails)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}