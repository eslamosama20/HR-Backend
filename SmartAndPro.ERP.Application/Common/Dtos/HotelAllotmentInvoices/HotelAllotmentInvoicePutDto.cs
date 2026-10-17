using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices
{
    public class HotelAllotmentInvoicePutDto
    {
        public int Id { get; set; }

        public Guid? HotelId { get; set; }
        public Guid? MainAccountId { get; set; }

        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        // دي القيم اللي بتيجي من الفرونت
        public decimal? Tax { get; set; }
        public decimal? Discount { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }

        public int? TotalNumberOfRooms { get; set; }
        public int? TotalNumberOfDays { get; set; }
        public decimal? TotalCostBeforeVAT { get; set; }

        // الغرف
        public List<HotelAllotmentRoomPutDto>? HotelAllotmentRooms { get; set; } = new();
        //public List<PaymentSchedulePutDto>? PaymentSchedules { get; set; } = new();

        public bool? IsCanceled { get; set; } = false;

    }

}
