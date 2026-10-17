using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelSyncLog : Entity, IEntityTypeConfiguration<ChannelSyncLog>
{
    public Guid HotelId { get; set; }
    public string Direction { get; set; } = string.Empty; // 'Outgoing' or 'Incoming'
    public string EventType { get; set; } = string.Empty; // e.g. "ari.update", "webhook.booking"
    public string Payload { get; set; } = string.Empty;
    public int? ResponseCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation
    public Hotel Hotel { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelSyncLog> builder)
    {
        builder.ToTable("ChannelSyncLogs")
            .HasKey(x => x.Id);

        builder.Property(x => x.Direction).HasMaxLength(20).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();

        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
