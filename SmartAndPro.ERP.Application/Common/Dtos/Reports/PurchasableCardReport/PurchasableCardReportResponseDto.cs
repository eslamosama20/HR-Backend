namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.PurchasableCardReport
{
    public class PurchasableCardReportResponseDto
    {
        public DateTime OperationDate { get; set; }
        public string OperationReferenceNumber { get; set; }
        public string? Description { get; set; }
        public string OperationSource { get; set; }
        public string? SupplierDetailsArabicName { get; set; }
        public string? SupplierDetailsEnglishName { get; set; }
        public List<PurchasableCardReportItemResponseDto> PurchasableCardReportItems { get; set; }   
    }
}
