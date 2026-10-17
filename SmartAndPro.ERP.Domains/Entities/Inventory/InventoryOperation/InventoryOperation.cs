using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.Stocktaking;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;

public class InventoryOperation : Entity , IEntityTypeConfiguration<InventoryOperation>
{
    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public DateTime OperationDate { get; set; } = DateTime.UtcNow;
    
    public Guid? TransactionId { get; set; }

    public OperationSource OperationSource { get; set; }
    
    public int? PurchaseInvoiceId { get; set; }
    public PurchaseInvoice PurchaseInvoice { get; set; }
    
    public Guid? InventoryPermitId { get; set; }
    public InventoryPermit InventoryPermit { get; set; }
    
    public int? PurchaseReturnId { get; set; }
    public PurchaseReturn PurchaseReturn { get; set; }
    
    public List<OperationEntry> OperationEntries { get; set; }

    public Guid? SupplierDetailsId { get; set; }
    public SupplierDetails SupplierDetails { get; set; }
    
    public Guid? InventoryStocktakingId { get; set; }
    public InventoryStocktaking InventoryStocktaking { get; set; }

    public void Configure(EntityTypeBuilder<InventoryOperation> builder)
    {
        builder
            .ToTable("InventoryOperation")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.OperationDate)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);
        
        builder.Property(b => b.OperationSource)
            .IsRequired()
            .HasConversion(
                operationSource => operationSource.ToString(),
                operationSourceString => Enum.Parse<OperationSource>(operationSourceString)
            );

        builder.HasMany(x => x.OperationEntries)
            .WithOne()
            .HasForeignKey(te => te.InventoryOperationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<InventoryOperation>(i => i.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(i => i.PurchaseInvoice)
            .WithOne()
            .HasForeignKey<InventoryOperation>(i => i.PurchaseInvoiceId);
        builder.HasOne(i => i.InventoryPermit)
            .WithOne()
            .HasForeignKey<InventoryOperation>(i => i.InventoryPermitId);
        
        builder.HasOne(i => i.PurchaseReturn)
            .WithOne()
            .HasForeignKey<InventoryOperation>(i => i.PurchaseReturnId);
        
        builder.HasOne(x => x.SupplierDetails)
            .WithMany()
            .HasForeignKey(x => x.SupplierDetailsId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.InventoryStocktaking)
            .WithMany()
            .HasForeignKey(x => x.InventoryStocktakingId)
            .OnDelete(DeleteBehavior.SetNull);
        
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }
}