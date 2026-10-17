using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules
{
    public class PaymentSchedulePutDto
    {
        public Guid? Id { get; set; }


        public decimal Amount { get; set; }
        public decimal? VAT { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? EntryNo { get; set; }
        public string? Notes { get; set; }
        public PaymentMethodType? PaymentMethodType { get; set; }
        public string? ReferenceNumber { get; set; }
        public Guid? PaymentReceiptId { get; set; }
    }
}
