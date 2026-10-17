using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections
{
    public class ReceiptConnectionResponseDto
    {
        public Guid Id { get; set; }  

        public Guid? PaycheckReceiptId { get; set; }

        public Guid? PaymentReceiptId { get; set; }

        public int? SaleInvoiceId { get; set; }

        public int? PurchaseInvoiceId { get; set; }

        public Guid? ReservationId { get; set; }

        public Guid? ProjectId { get; set; }

        public decimal LinkedAmount { get; set; }

        public ReceiptConnectionType ReceiptConnectionType { get; set; }

        // Calculated properties for display
        public decimal InvoiceTotalAmount { get; set; } = 0;
        public decimal InvoicePaidAmount { get; set; } = 0;
        public decimal InvoiceRemainingAmount { get; set; } = 0;
        public string InvoiceReference { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
    }
}
