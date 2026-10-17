using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelConnection : Entity, IEntityTypeConfiguration<ChannelConnection>
{
    public Guid HotelId { get; set; }
    public string ProviderName { get; set; } = string.Empty; // e.g., "Channex"
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ChannelPropertyId { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string ConnectionCode { get; set; } = string.Empty;

    // Navigation
    public Hotel Hotel { get; set; } = null!;

    public ICollection<ChannelRoomMapping> RoomMappings { get; set; } = new List<ChannelRoomMapping>();
    public ICollection<ChannelRateMapping> RateMappings { get; set; } = new List<ChannelRateMapping>();

    public void Configure(EntityTypeBuilder<ChannelConnection> builder)
    {
        builder.ToTable("ChannelConnections")
            .HasKey(x => x.Id);

        builder.Property(x => x.ProviderName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ApiKey).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.WebhookSecret).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ChannelPropertyId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ApiEndpoint).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Salt).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConnectionCode).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Hotel)
            .WithMany()
            .HasForeignKey(x => x.HotelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
