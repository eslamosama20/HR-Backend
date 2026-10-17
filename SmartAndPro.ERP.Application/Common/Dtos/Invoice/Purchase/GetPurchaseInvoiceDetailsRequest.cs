namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase
{
    public class GetPurchaseInvoiceDetailsRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int StoreID { get; set; }
    }

}
