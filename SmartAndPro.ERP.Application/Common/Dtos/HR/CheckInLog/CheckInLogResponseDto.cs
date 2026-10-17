using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.CheckInLog;

public class CheckInLogResponseDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public EmployeeResponseDto Employee { get; set; }
    
    public DateTime Time { get; set; }
    public string LogType { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}