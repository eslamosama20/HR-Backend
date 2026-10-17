namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class NetDueVATReportShareRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Method { get; set; } = string.Empty; // "email" or "WhatsApp"
    public string Receiver { get; set; } = string.Empty; // email address or phone number
    public string Language { get; set; } = "ar";
}
