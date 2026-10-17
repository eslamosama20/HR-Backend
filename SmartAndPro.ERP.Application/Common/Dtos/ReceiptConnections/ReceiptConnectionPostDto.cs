using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections
{
    public class ReceiptConnectionPostDto
    {
        public Guid? PaycheckReceiptId { get; set; }

        public Guid? PaymentReceiptId { get; set; }

        public int? SaleInvoiceId { get; set; }

        public int? PurchaseInvoiceId { get; set; }

        public Guid? ReservationId { get; set; }
        public Guid? ProjectId { get; set; }
        public decimal LinkedAmount { get; set; }

        public ReceiptConnectionType ReceiptConnectionType { get; set; }



    }
}
