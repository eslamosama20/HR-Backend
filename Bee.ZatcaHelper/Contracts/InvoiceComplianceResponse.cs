namespace Bee.ZatcaHelper.Contracts;

public class InvoiceComplianceResponse
{
    public ValidationResults ValidationResults { get; set; }
    
    public string? ReportingStatus { get; set; }
    public string? ClearanceStatus { get; set; }
}