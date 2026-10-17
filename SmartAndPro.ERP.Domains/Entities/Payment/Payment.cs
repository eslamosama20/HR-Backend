using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Payment;

public class Payment : Entity, IEntityTypeConfiguration<Payment>
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";

    /// <summary>card | mada | apple_pay | tabby | tamara | stc_pay</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>paymob | tabby | tamara | hyperpay</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Pending | Paid | Failed | Refunded</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>ID returned by the external payment provider</summary>
    public string? ExternalId { get; set; }

    /// <summary>Checkout / redirect URL returned by provider</summary>
    public string? PaymentUrl { get; set; }

    public string? OrderReference { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Description { get; set; }

    /// <summary>Raw webhook payload stored for audit</summary>
    public string? WebhookPayload { get; set; }
    public DateTime? PaidAt { get; set; }

    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments").HasKey(x => x.Id);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.Method).HasMaxLength(50).IsRequired();
        b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ExternalId).HasMaxLength(200);
        b.Property(x => x.PaymentUrl).HasMaxLength(1000);
        b.Property(x => x.OrderReference).HasMaxLength(100);
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.CustomerEmail).HasMaxLength(200);
        b.Property(x => x.CustomerPhone).HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.Method);
        b.HasIndex(x => x.ExternalId);
        b.HasQueryFilter(x => x.IsActive);
    }
}
