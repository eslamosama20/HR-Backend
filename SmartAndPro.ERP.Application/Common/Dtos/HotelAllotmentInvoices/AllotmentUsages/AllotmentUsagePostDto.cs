using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.AllotmentUsages
{
    public class AllotmentUsagePostDto
    {
        public int HotelAllotmentRoomId { get; set; }
        public Guid? ReservationId { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }
        public DateTime UsageDate { get; set; }

        public int ConsumedRooms { get; set; }
        public int OverbookedRooms { get; set; } 

        public decimal PricePerRoom { get; set; }
        public decimal? Discount { get; set; }
        public Guid? MealPlanId { get; set; }
        public Guid? ViewId { get; set; }
        public string? Notes { get; set; }
    }
}
