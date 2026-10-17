using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

/// <summary>
/// Tracks every inventory movement triggered by a cashier checkout or refund.
/// Provides a cashier-level audit trail for stock changes, linked to invoices and sessions.
/// </summary>
public class CashierInventoryMovement : Entity, IEntityTypeConfiguration<CashierInventoryMovement>
{
    /// <summary>Reference to the sale invoice that triggered this movement</summary>
    public int InvoiceId { get; set; }

    /// <summary>The invoice number for human readability</summary>
    public string InvoiceNo { get; set; } = string.Empty;

    /// <summary>The cashier session during which this movement occurred</summary>
    public int SessionId { get; set; }

    /// <summary>The store/warehouse where stock was deducted or returned</summary>
    public Guid StoreDetailsId { get; set; }

    /// <summary>The purchase class (product/batch) whose stock was affected</summary>
    public Guid MainPurchaseClassId { get; set; }

    /// <summary>Product name for human readability</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Movement direction: 1=Exit (sale), -1=Entry (refund/return)</summary>
    public int Direction { get; set; }

    /// <summary>Quantity moved (always positive; Direction indicates in/out)</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit cost at time of movement (for COGS calculation)</summary>
    public decimal UnitCost { get; set; }

    /// <summary>Unit sale price at time of movement</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Reference to the InventoryPermit created for this movement</summary>
    public Guid? InventoryPermitId { get; set; }

    /// <summary>Cashier who triggered this movement</summary>
    public Guid CashierUserGuid { get; set; }

    /// <summary>Movement type: SALE, REFUND, VOID</summary>
    public string MovementType { get; set; } = "SALE";

    /// <summary>When the movement occurred (UTC)</summary>
    public DateTime MovedAtUtc { get; set; }

    public void Configure(EntityTypeBuilder<CashierInventoryMovement> builder)
    {
        builder.ToTable("CashierInventoryMovements").HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MovementType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(x => x.MovedAtUtc).IsRequired();

        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.MainPurchaseClassId);
        builder.HasIndex(x => x.StoreDetailsId);
        builder.HasIndex(x => x.MovedAtUtc);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
