using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.HRTrustReceipt;

public class HRTrustReceiptPostDto
{
    public Guid EmployeeId { get; set; }
    public string ItemDescription { get; set; }
    public string? ItemSerialNumber { get; set; }
    public decimal? ItemValue { get; set; }
    public DateOnly ReceiveDate { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; } = 7;
    public bool IsCritical { get; set; } = false;
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;
    public Guid? AttachmentId { get; set; }
}
