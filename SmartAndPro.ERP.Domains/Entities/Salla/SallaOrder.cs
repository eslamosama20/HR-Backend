using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums.Salla;

namespace SmartAndPro.ERP.Domains.Entities.Salla;

/// <summary>
/// Salla e-commerce order synced to the ERP
/// </summary>
public class SallaOrder : Entity, IEntityTypeConfiguration<SallaOrder>
{
    /// <summary>Order ID on Salla platform</summary>
    public long SallaOrderId { get; set; }

    /// <summary>Salla reference number (e.g. #1234)</summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    public SallaOrderStatus Status { get; set; } = SallaOrderStatus.New;
    public SallaPaymentStatus PaymentStatus { get; set; } = SallaPaymentStatus.Pending;
    public SallaFulfillmentStatus FulfillmentStatus { get; set; } = SallaFulfillmentStatus.Pending;

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // Customer info (from Salla)
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public long? SallaCustomerId { get; set; }

    // Shipping address
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? ShippingStreet { get; set; }
    public string? ShippingPostalCode { get; set; }
    public double? ShippingLat { get; set; }
    public double? ShippingLng { get; set; }

    // Payment
    public string? PaymentMethod { get; set; }
    public string? SallaPaymentId { get; set; }

    // ERP mapping
    public int? SaleInvoiceId { get; set; }
    public Guid? StoreDetailsId { get; set; }

    // Sync tracking
    public SallaSyncStatus SyncStatus { get; set; } = SallaSyncStatus.Pending;
    public string? SyncError { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime SallaCreatedAt { get; set; }

    // Navigation
    public virtual ICollection<SallaOrderItem> Items { get; set; } = new List<SallaOrderItem>();

    public void Configure(EntityTypeBuilder<SallaOrder> builder)
    {
        builder.ToTable("SallaOrders").HasKey(x => x.Id);

        builder.Property(x => x.SallaOrderId).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerEmail).HasMaxLength(200);
        builder.Property(x => x.CustomerPhone).HasMaxLength(30);
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(50);
        builder.Property(x => x.SallaPaymentId).HasMaxLength(100);
        builder.Property(x => x.SyncError).HasMaxLength(1000);

        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShippingAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.ShippingCity).HasMaxLength(100);
        builder.Property(x => x.ShippingDistrict).HasMaxLength(200);
        builder.Property(x => x.ShippingStreet).HasMaxLength(300);
        builder.Property(x => x.ShippingPostalCode).HasMaxLength(20);

        builder.HasIndex(x => x.SallaOrderId).IsUnique();
        builder.HasIndex(x => x.ReferenceNumber);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SyncStatus);
        builder.HasIndex(x => x.SallaCreatedAt);
        builder.HasIndex(x => x.SaleInvoiceId);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SallaOrder)
            .HasForeignKey(x => x.SallaOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
