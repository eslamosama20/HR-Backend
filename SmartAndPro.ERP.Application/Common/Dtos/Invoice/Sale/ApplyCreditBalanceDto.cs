using System;
using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class ApplyCreditBalanceDto
    {
        public Guid MainAccountId { get; set; }
        public List<InvoiceAllocationDto> Allocations { get; set; }
    }

    public class InvoiceAllocationDto
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
    }
}
