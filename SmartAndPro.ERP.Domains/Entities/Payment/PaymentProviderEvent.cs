using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Payment;

public class PaymentProviderEvent : Entity, IEntityTypeConfiguration<PaymentProviderEvent>
{
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;

    public void Configure(EntityTypeBuilder<PaymentProviderEvent> builder)
    {
        builder.ToTable("PaymentProviderEvents").HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();

        // Unique index on Provider + EventId to ensure idempotency (prevent processing same webhook event twice)
        builder.HasIndex(x => new { x.Provider, x.EventId }).IsUnique();
        
        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
