using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules
{
    public class PaymentScheduleResponseDto
    {
        public Guid Id { get; set; }

        public string InvoiceId { get; set; }
        public string? InvoiceNo { get; set; }
        public InvoiceTypeEnum InvoiceType { get; set; }

        public decimal Amount { get; set; }
        public decimal? VAT { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; } 
        public string? EntryNo { get; set; }
        public string? Notes { get; set; }
        public PaymentMethodType? PaymentMethodType { get; set; }
        public string? ReferenceNumber { get; set; }

        public Guid? TreasuryId { get; set; }
        public Guid? PaymentReceiptId { get; set; }
        public Guid? PaycheckReceiptId { get; set; }
        public string? ReceiptNo { get; set; }



    }
}
