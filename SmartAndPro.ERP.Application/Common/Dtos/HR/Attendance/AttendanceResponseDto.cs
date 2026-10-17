using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;

public class AttendanceResponseDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public EmployeeResponseDto Employee { get; set; }
    
    public DateOnly AttendanceDate { get; set; }
    public string AttendanceFlag { get; set; }
    public string? Reason { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}