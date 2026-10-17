namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class GetSalesInvoiceDetailsRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int StoreID { get; set; }
    }
}
