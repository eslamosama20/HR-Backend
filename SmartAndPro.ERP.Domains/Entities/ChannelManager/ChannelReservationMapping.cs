using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelReservationMapping : Entity, IEntityTypeConfiguration<ChannelReservationMapping>
{
    public Guid ChannelConnectionId { get; set; }
    public Guid ErpReservationId { get; set; }
    public string ChannelReservationId { get; set; } = string.Empty; // External reservation ID
    public string OtaReservationNumber { get; set; } = string.Empty; // OTA Booking reference
    public string OtaName { get; set; } = string.Empty; // e.g. "Booking.com"
    public string OtaStatus { get; set; } = string.Empty; // New, Modified, Cancelled
    public int Version { get; set; } = 1; // Used to prevent out-of-order execution
    public string RawPayload { get; set; } = string.Empty;

    // Navigation
    public ChannelConnection ChannelConnection { get; set; } = null!;
    public SmartAndPro.ERP.Domains.Entities.Reservation.Reservation Reservation { get; set; } = null!;
    public ChannelReservationPayment? PaymentDetails { get; set; }

    public void Configure(EntityTypeBuilder<ChannelReservationMapping> builder)
    {
        builder.ToTable("ChannelReservationMappings")
            .HasKey(x => x.Id);

        builder.Property(x => x.ChannelReservationId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OtaReservationNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OtaName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OtaStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Version).HasDefaultValue(1);
        builder.Property(x => x.RawPayload).HasColumnType("nvarchar(max)").IsRequired();

        builder.HasOne(x => x.ChannelConnection)
            .WithMany()
            .HasForeignKey(x => x.ChannelConnectionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Reservation)
            .WithMany()
            .HasForeignKey(x => x.ErpReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ChannelReservationId).IsUnique();
        builder.HasIndex(x => x.OtaReservationNumber);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
