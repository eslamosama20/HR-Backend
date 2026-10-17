using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Payment_Schedules;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices
{
    public class HotelAllotmentInvoicePostDto
    {
        public string? InvoiceNo { get; set; }

        public Guid HotelId { get; set; }

        public Guid? MainAccountId { get; set; }

        //public DateTime FromDate { get; set; }

        //public DateTime ToDate { get; set; }

        [MinLength(1)]
        public List<HotelAllotmentRoomPostDto> HotelAllotmentRooms { get; set; } = new();

        public string? Currency { get; set; }
        public string? Description { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        // دي اللي بتيجي من الفرونت
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }

        // القيم دي أنت بتحسبها في الباك
        // public decimal TaxAmount { get; set; }
        // public decimal DiscountAmount { get; set; }
        // public decimal TotalPrice { get; set; }
        // public decimal NetPrice { get; set; }

        public bool IsFrontal { get; set; } = false;
        public List<PaymentSchedulePostDto>? PaymentSchedules { get; set; }
        public string? InvoiceType { get; set; }
        public bool? IsCanceled { get; set; } = false;



    }

}
