namespace Bee.ZatcaHelper.Contracts;

public class InvoiceReportingResponse
{
    public ValidationResults ValidationResults { get; set; }
    
    public string? ReportingStatus { get; set; }
    public string? ClearanceStatus { get; set; }
}
