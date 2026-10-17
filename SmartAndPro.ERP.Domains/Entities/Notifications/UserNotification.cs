using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.Notifications
{
    public class UserNotification : Entity , IEntityTypeConfiguration<UserNotification>
    {
        public Guid UserId { get; set; } 
        public User User { get; set; }
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsIgnored { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public void Configure(EntityTypeBuilder<UserNotification> builder)
        {
            builder.ToTable("UserNotifications");
            builder.HasKey(un => un.Id);

            builder.Property(un => un.IsRead)
                   .HasDefaultValue(false);

            builder.Property(un => un.IsIgnored)
                   .HasDefaultValue(false);

            builder.HasOne(un => un.Notification)
                   .WithMany(n => n.UserNotifications)
                   .HasForeignKey(un => un.NotificationId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(un => un.User)
            .WithMany(u => u.UserNotifications)
            .HasForeignKey(un => un.UserId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(un => new { un.UserId, un.NotificationId }).IsUnique();
        }
    }
}
