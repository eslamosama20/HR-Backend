
namespace SmartAndPro.ERP.Domains.Models.OutputModel
{
    public class SupplierDetailsByProductOutputModel : ISupplierDetails
    {
        public DateTime CreateDate { get; set; }
        public string ReceiptNo { get; set; }
        public string ProductNameAr { get; set; }
        public string ProductNameEn { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal TotalAmount { get; set; } 

    }
}
