using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.LeaveApplication;

public class LeaveApprovalDto
{
    public Guid LeaveApplicationId { get; set; }
    public Guid ApproverId { get; set; }
    public LeaveStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
