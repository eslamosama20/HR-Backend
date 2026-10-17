using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.HRTrustReceipt;

public class HRTrustReceiptResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string ItemDescription { get; set; }
    public string? ItemSerialNumber { get; set; }
    public decimal? ItemValue { get; set; }
    public DateOnly ReceiveDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public int PreReminderDays { get; set; }
    public bool IsCritical { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid? AttachmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
