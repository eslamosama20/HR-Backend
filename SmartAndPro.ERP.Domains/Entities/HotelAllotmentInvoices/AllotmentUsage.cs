using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices
{
    
        public class AllotmentUsage : Entity, IEntityTypeConfiguration<AllotmentUsage>
        {
            public int HotelAllotmentRoomId { get; set; }
            public HotelAllotmentRoom HotelAllotmentRoom { get; set; }

            public Guid? ReservationId { get; set; }  // ممكن يكون null لو استهلاك مستقل
            public Domains.Entities.Reservation.Reservation Reservation { get; set; }

        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }

        public DateTime UsageDate { get; set; }   // ✔ يوم واحد

        public int ConsumedRooms { get; set; }

            public decimal PricePerRoom { get; set; }
            public decimal? Discount { get; set; }

        public int OverbookedRooms { get; set; } = 0;

        public Guid? MealPlanId { get; set; }
            public ReservationMeal? MealPlan { get; set; }

            public Guid? ViewId { get; set; }
            public ReservationView? View { get; set; }

            //public string Status { get; set; } // Pending, Confirmed, Cancelled
            public string? Notes { get; set; }

            public void Configure(EntityTypeBuilder<AllotmentUsage> builder)
            {
                builder.ToTable(nameof(AllotmentUsage));
                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.HotelAllotmentRoom)
                    .WithMany()
                    .HasForeignKey(x => x.HotelAllotmentRoomId)
                    .OnDelete(DeleteBehavior.NoAction);

                builder.HasOne(x => x.Reservation)
                    .WithMany()
                    .HasForeignKey(x => x.ReservationId)
                    .OnDelete(DeleteBehavior.NoAction);

                builder.HasOne(x => x.MealPlan)
                    .WithMany()
                    .HasForeignKey(x => x.MealPlanId)
                    .OnDelete(DeleteBehavior.NoAction);

                builder.HasOne(x => x.View)
                    .WithMany()
                    .HasForeignKey(x => x.ViewId)
                    .OnDelete(DeleteBehavior.NoAction);

                //builder.Property(x => x.Status)
                //    .HasMaxLength(50)
                //    .IsRequired();

                builder.Property(x => x.Notes)
                    .HasMaxLength(500);

            //builder.Property(x => x.FromDate)
            //    .HasColumnType("date")
            //    .IsRequired();

            //builder.Property(x => x.ToDate)
            //  .HasColumnType("date")
            //  .IsRequired();

            builder.Property(x => x.UsageDate)
               .HasColumnType("date")
               .IsRequired();

            builder.HasIndex(x => new
            {
                x.HotelAllotmentRoomId,
                x.UsageDate
            });

        }
    }
    }


