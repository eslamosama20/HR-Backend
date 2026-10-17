using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelWebhookQueue : Entity, IEntityTypeConfiguration<ChannelWebhookQueue>
{
    public Guid HotelId { get; set; }
    public string ExternalEventId { get; set; } = string.Empty; // From Channel Manager headers/body
    public string Payload { get; set; } = string.Empty;
    public int Status { get; set; } = 0; // 0=Pending, 1=Processing, 2=Completed, 3=Failed
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 5;
    public string? LockedBy { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public Hotel Hotel { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelWebhookQueue> builder)
    {
        builder.ToTable("ChannelWebhookQueue")
            .HasKey(x => x.Id);

        builder.Property(x => x.ExternalEventId).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.LockedBy).HasMaxLength(100);

        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ExternalEventId).IsUnique();
        builder.HasIndex(x => x.Status);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
