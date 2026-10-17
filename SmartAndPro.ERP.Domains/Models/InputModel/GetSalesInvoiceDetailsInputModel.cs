namespace SmartAndPro.ERP.Domains.Models.InputModel
{
    public class GetSalesInvoiceDetailsInputModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int StoreId { get; set; }
    }
}
