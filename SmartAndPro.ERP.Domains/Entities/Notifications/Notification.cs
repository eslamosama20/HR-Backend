using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.Notifications
{
    public class Notification : Entity, IEntityTypeConfiguration<Notification>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; 
        public string? Link { get; set; } 

        public NotificationType NotificationType { get; set; }

        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable(nameof(Notifications));
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(n => n.Content)
                   .IsRequired()
                   .HasMaxLength(2000); 

            builder.Property(n => n.Link)
                   .HasMaxLength(500);

            builder.Property(n => n.NotificationType)
                   .IsRequired()
                   .HasConversion<int>();


            builder.HasMany(n => n.UserNotifications)
                   .WithOne(un => un.Notification)
                   .HasForeignKey(un => un.NotificationId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
