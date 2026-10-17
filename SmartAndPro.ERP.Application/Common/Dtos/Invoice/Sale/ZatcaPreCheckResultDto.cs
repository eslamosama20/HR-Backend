using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class ZatcaPreCheckResultDto
    {
        public bool IsValid { get; set; }
        public string InvoiceType { get; set; } // "standard" or "simplified"
        public List<string> CompanyErrors { get; set; } = new List<string>();
        public List<string> CustomerErrors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
