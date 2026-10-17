using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelRoomMapping : Entity, IEntityTypeConfiguration<ChannelRoomMapping>
{
    public Guid ChannelConnectionId { get; set; }
    public Guid ErpRoomTypeId { get; set; }
    public string ChannelRoomId { get; set; } = string.Empty; // Room ID in Channel Manager
    public string? RoomName { get; set; }
    public int? MaxAllocationOverride { get; set; }

    // Navigation
    public ChannelConnection ChannelConnection { get; set; } = null!;
    public RoomType RoomType { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelRoomMapping> builder)
    {
        builder.ToTable("ChannelRoomMappings")
            .HasKey(x => x.Id);

        builder.Property(x => x.ChannelRoomId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RoomName).HasMaxLength(200);

        builder.HasOne(x => x.ChannelConnection)
            .WithMany(c => c.RoomMappings)
            .HasForeignKey(x => x.ChannelConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RoomType)
            .WithMany()
            .HasForeignKey(x => x.ErpRoomTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
