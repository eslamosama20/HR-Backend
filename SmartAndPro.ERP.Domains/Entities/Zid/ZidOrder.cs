using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums.Zid;

namespace SmartAndPro.ERP.Domains.Entities.Zid;

/// <summary>
/// Zid e-commerce order synced to the ERP
/// </summary>
public class ZidOrder : Entity, IEntityTypeConfiguration<ZidOrder>
{
    /// <summary>Order ID on Zid platform (numeric)</summary>
    public long ZidOrderId { get; set; }

    /// <summary>Human-readable reference (e.g. #1234)</summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    public ZidOrderStatus Status { get; set; } = ZidOrderStatus.New;
    public ZidPaymentStatus PaymentStatus { get; set; } = ZidPaymentStatus.Pending;
    public ZidFulfillmentStatus FulfillmentStatus { get; set; } = ZidFulfillmentStatus.Pending;

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // Customer info (from Zid)
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public long? ZidCustomerId { get; set; }

    // Shipping address
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? ShippingStreet { get; set; }
    public string? ShippingPostalCode { get; set; }
    public double? ShippingLat { get; set; }
    public double? ShippingLng { get; set; }

    // Payment
    public string? PaymentMethod { get; set; }
    public string? ZidPaymentId { get; set; }

    // ERP mapping
    public int? SaleInvoiceId { get; set; }
    public Guid? StoreDetailsId { get; set; }

    // Sync tracking
    public ZidSyncStatus SyncStatus { get; set; } = ZidSyncStatus.Pending;
    public string? SyncError { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime ZidCreatedAt { get; set; }

    // Navigation
    public virtual ICollection<ZidOrderItem> Items { get; set; } = new List<ZidOrderItem>();

    public void Configure(EntityTypeBuilder<ZidOrder> builder)
    {
        builder.ToTable("ZidOrders").HasKey(x => x.Id);

        builder.Property(x => x.ZidOrderId).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerEmail).HasMaxLength(200);
        builder.Property(x => x.CustomerPhone).HasMaxLength(30);
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(50);
        builder.Property(x => x.ZidPaymentId).HasMaxLength(100);
        builder.Property(x => x.SyncError).HasMaxLength(1000);

        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShippingAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.ZidOrderId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SyncStatus);
        builder.HasIndex(x => x.StoreDetailsId);
        builder.HasIndex(x => x.ZidCreatedAt);

        builder.HasMany(x => x.Items)
               .WithOne(i => i.ZidOrder)
               .HasForeignKey(i => i.ZidOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
