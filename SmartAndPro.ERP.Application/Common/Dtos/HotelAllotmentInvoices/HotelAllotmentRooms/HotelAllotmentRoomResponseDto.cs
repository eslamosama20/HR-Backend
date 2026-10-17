using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms
{
    public class HotelAllotmentRoomResponseDto
    {
        public int Id { get; set; }
        public Guid? RoomTypeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Dictionary<DayOfWeek, decimal> PricePerDay { get; set; } = new();
        public Dictionary<DayOfWeek, int> RoomsPerDay { get; set; } = new();
        public int NumberOfRooms { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal MealPlanCost { get; set; }
        public decimal ViewCost { get; set; }
        public Guid? MealPlanId { get; set; }
        public Guid? ViewId { get; set; }

    }
}
