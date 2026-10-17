using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.HotelAllotmentRooms;
using SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices
{
    public class HotelAllotmentInvoiceResponseDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public Guid HotelId { get; set; }
        public string? HotelName { get; set; }

        public Guid? MainAccountId { get; set; }

        public Guid? TransactionId { get; set; }

        //public int TotalNumberOfRooms { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }

        public List<HotelAllotmentRoomResponseDto> HotelAllotmentRooms { get; set; } = new();

        public string? Currency { get; set; }
        public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        public bool IsFrontal { get; set; } 

        public decimal TotalCostBeforeVAT { get; set; }
        public string? InvoiceType { get; set; } // يظهر كـ String
        //public List<PaymentScheduleResponseDto>? PaymentSchedules { get; set; } = new();

        public decimal TotalPayments { get; set; } // هنا هنحسب مجموع الدفعات

        public bool? IsCanceled { get; set; } = false;



    }

}
