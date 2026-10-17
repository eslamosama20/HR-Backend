using SmartAndPro.ERP.Application.Common.Dtos.Reservation.RoomType;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms
{
   
        public class HotelAllotmentRoomPostDto
        {
        public Guid? RoomTypeId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        // السعر لكل يوم من الأسبوع
        public Dictionary<DayOfWeek, decimal> PricePerDay { get; set; } = new();

        // عدد الغرف لكل يوم من الأسبوع
        public Dictionary<DayOfWeek, int> RoomsPerDay { get; set; } = new();

        public Guid? MealPlanId { get; set; }
        public Guid? ViewId { get; set; }
        public decimal MealPlanCost { get; set; }
        public decimal ViewCost { get; set; }

        public int NumberOfRooms { get; set; }

    }

}

