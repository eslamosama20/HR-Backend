using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Cashier;

/// <summary>
/// Stores idempotency records for checkout operations.
/// Prevents duplicate invoice creation when the same Idempotency-Key header is sent twice
/// (e.g. double-click, network retry).
/// </summary>
public class CheckoutIdempotencyRecord : Entity, IEntityTypeConfiguration<CheckoutIdempotencyRecord>
{
    /// <summary>Client-supplied idempotency key (from Idempotency-Key HTTP header)</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>The invoice that was created for this checkout</summary>
    public int InvoiceId { get; set; }

    /// <summary>The invoice number for reference</summary>
    public string InvoiceNo { get; set; } = string.Empty;

    /// <summary>Total amount of the checkout</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Serialized CheckoutResponse JSON so duplicates return identical body</summary>
    public string ResponseJson { get; set; } = string.Empty;

    /// <summary>SHA256 Hash of the request body to reject mismatched payload conflicts</summary>
    public string RequestHash { get; set; } = string.Empty;

    /// <summary>When this record was created</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Record expires after 24 hours — after that, the key can be reused</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>The cashier who initiated this checkout</summary>
    public Guid RequestedByUserGuid { get; set; }

    /// <summary>The session under which this checkout occurred</summary>
    public int SessionId { get; set; }

    public void Configure(EntityTypeBuilder<CheckoutIdempotencyRecord> builder)
    {
        builder.ToTable("CheckoutIdempotencyRecords").HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.InvoiceNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ResponseJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(128).HasDefaultValue("").IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.Property(x => x.RequestedByUserGuid).IsRequired();
        builder.Property(x => x.SessionId).IsRequired();

        // Unique index on the key for fast duplicate detection
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        // Index on expiry for cleanup jobs
        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
