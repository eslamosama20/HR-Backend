using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using System;

namespace SmartAndPro.ERP.Domains.Entities.Reservation;

public class ReservationActionLog : Entity, IEntityTypeConfiguration<ReservationActionLog>
{
    public Guid ReservationId { get; set; }
    public string ActionType { get; set; } // Created, Updated, Confirmed, Cancelled, Copied, Reactivated
    public string ExecutedBy { get; set; } // Username or Email
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Guid? ExecutedByUserId { get; set; }
    public User? ExecutedByUser { get; set; }

    public void Configure(EntityTypeBuilder<ReservationActionLog> builder)
    {
        builder.ToTable("ReservationActionLog")
               .HasKey(x => x.Id);

        builder.HasIndex(x => x.ReservationId);

        builder.Property(x => x.ActionType)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.ExecutedBy)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(x => x.OccurredAt)
               .IsRequired();

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasOne(x => x.ExecutedByUser)
               .WithMany()
               .HasForeignKey(x => x.ExecutedByUserId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(p => p.IsActive == true);
    }
}
