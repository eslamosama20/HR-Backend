using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;

namespace SmartAndPro.ERP.Domains.Entities.ChannelManager;

public class ChannelRateMapping : Entity, IEntityTypeConfiguration<ChannelRateMapping>
{
    public Guid ChannelConnectionId { get; set; }
    public Guid ErpSeasonId { get; set; } // ReservationSeason represents rate seasons in the ERP
    public string ChannelRateId { get; set; } = string.Empty; // Rate ID in Channel Manager
    public string? RateName { get; set; }
    public decimal MarkupPercentage { get; set; } = 0.00m;

    // Navigation
    public ChannelConnection ChannelConnection { get; set; } = null!;
    public ReservationSeason ReservationSeason { get; set; } = null!;

    public void Configure(EntityTypeBuilder<ChannelRateMapping> builder)
    {
        builder.ToTable("ChannelRateMappings")
            .HasKey(x => x.Id);

        builder.Property(x => x.ChannelRateId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RateName).HasMaxLength(200);
        builder.Property(x => x.MarkupPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(0.00m);

        builder.HasOne(x => x.ChannelConnection)
            .WithMany(c => c.RateMappings)
            .HasForeignKey(x => x.ChannelConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReservationSeason)
            .WithMany()
            .HasForeignKey(x => x.ErpSeasonId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(x => x.IsActive == true);
    }
}
