
namespace SmartAndPro.ERP.Domains.Models.OutputModel
{
    public class SupplierDetailsByReceiptOutputModel : ISupplierDetails
    {
        public DateTime CreateDate { get; set; }
        public decimal ReceiptAmountBeforeVAT { get; set; }
        public string ReceiptNo { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalAmount { get; set; } 

    }
}
