using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelDeadLetterQueue : Entity, IEntityTypeConfiguration<ChannelDeadLetterQueue>
{
    public string SourceTable { get; set; } = string.Empty; // 'ChannelOutboxEvents' or 'ChannelWebhookQueue'
    public Guid OriginalEventId { get; set; }
    public Guid HotelId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }

    // Navigation
    public Hotel Hotel { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelDeadLetterQueue> builder)
    {
        builder.ToTable("ChannelDeadLetterQueue")
            .HasKey(x => x.Id);

        builder.Property(x => x.SourceTable).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ResolvedBy).HasMaxLength(100);
        builder.Property(x => x.ResolutionNotes).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
