namespace SmartAndPro.ERP.Domains.Models.InputModel.Report
{
    public class GetSupplierDetailsInputModel
    {
        public Guid SupplierId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int ReportType { get; set; }
    }
}
