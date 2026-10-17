namespace SmartAndPro.ERP.Domains.Models.InputModel
{
    public class GetSalesVATDetailsInputModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int StoreID { get; set; }
    }
}
