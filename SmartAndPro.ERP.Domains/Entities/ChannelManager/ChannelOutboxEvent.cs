using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelOutboxEvent : Entity, IEntityTypeConfiguration<ChannelOutboxEvent>
{
    public Guid HotelId { get; set; }
    public string EventName { get; set; } = string.Empty; // e.g. "availability.update"
    public string Payload { get; set; } = string.Empty; // JSON Serialized
    public int Status { get; set; } = 0; // 0=Pending, 1=Processing, 2=Completed, 3=Failed
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 5;
    public string? LockedBy { get; set; } // Worker name
    public DateTime? LockedUntil { get; set; }
    public string? ErrorMessage { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public Hotel Hotel { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelOutboxEvent> builder)
    {
        builder.ToTable("ChannelOutboxEvents")
            .HasKey(x => x.Id);

        builder.Property(x => x.EventName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.LockedBy).HasMaxLength(100);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(250).IsRequired();

        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IdempotencyKey);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
