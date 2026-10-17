using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices
{
    public class HotelAllotmentRoom : BaseAllotmentItem<int>, IEntityTypeConfiguration<HotelAllotmentRoom>
    {
        public int HotelAllotmentInvoiceId { get; set; }
        public HotelAllotmentInvoice HotelAllotmentInvoice { get; set; }
        public Guid? RoomTypeId { get; set; }
        public RoomType RoomType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int NumberOfNights { get; set; }
        public int NumberOfRooms { get; set; }

        public Guid? MealPlanId { get; set; }
        public ReservationMeal? MealPlan { get; set; }

        public decimal MealPlanCost { get; set; }

        public Guid? ViewId { get; set; }
        public ReservationView? View { get; set; }

        public decimal ViewCost { get; set; }

       


        // السعر لكل يوم من الأسبوع
        public Dictionary<DayOfWeek, decimal> PricePerDay { get; set; } = new();

        // عدد الغرف لكل يوم من الأسبوع
        public Dictionary<DayOfWeek, int> RoomsPerDay { get; set; } = new();

        // الدالة تحسب الإجمالي ليوم معين
        public decimal GetTotalForDay(DayOfWeek day)
        {
            decimal price = PricePerDay.ContainsKey(day) ? PricePerDay[day] : 0;
            int rooms = RoomsPerDay.ContainsKey(day) ? RoomsPerDay[day] : 0;

            decimal total = (price + MealPlanCost + ViewCost) * rooms;

            if (Discount > 0) total -= total * (Discount / 100);
            if (Tax > 0) total += total * (Tax / 100);

            return total;
        }

        public decimal GetTotalForPeriod(DateTime start, DateTime end)
        {
            decimal total = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                total += GetTotalForDay(date.DayOfWeek);
            }
            return total;
        }

        public decimal GetPriceForDay(DateTime day)
        {
            return PricePerDay.TryGetValue(day.DayOfWeek, out var price)
                ? price
                : 0;
        }

        public List<AllotmentUsage>? AllotmentUsages { get; set; } 
        public void Configure(EntityTypeBuilder<HotelAllotmentRoom> builder)
        {
            builder.ToTable(nameof(HotelAllotmentRoom));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.RoomType)
                 .WithMany()
                 .HasForeignKey(x => x.RoomTypeId);

            builder.HasOne(x => x.HotelAllotmentInvoice)
                .WithMany(x => x.HotelAllotmentRooms)
                .HasForeignKey(x => x.HotelAllotmentInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasMany(x=>x.AllotmentUsages)
                .WithOne(x=>x.HotelAllotmentRoom)
                .HasForeignKey(x => x.HotelAllotmentRoomId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Navigation(x => x.RoomType)
                .AutoInclude();

            var jsonOptions = new JsonSerializerOptions();

            builder.Property(x => x.PricePerDay)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v)
                        ? new Dictionary<DayOfWeek, decimal>()
                        : JsonSerializer.Deserialize<Dictionary<DayOfWeek, decimal>>(v, jsonOptions))
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.RoomsPerDay)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v)
                        ? new Dictionary<DayOfWeek, int>()
                        : JsonSerializer.Deserialize<Dictionary<DayOfWeek, int>>(v, jsonOptions))
                .HasColumnType("nvarchar(max)");
        }
    }
}
