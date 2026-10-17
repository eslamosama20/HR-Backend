using SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;
using SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;

public class EmployeeResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    public string Name { get; set; }
    public string? Phone { get; set; }
    
    public Guid EmploymentTypeId { get; set; }
    public EmploymentTypeResponseDto EmploymentType { get; set; }
    
    public Guid ShiftId { get; set; }
    public ShiftResponseDto Shift { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    
    public decimal? MonthlyInsurance { get; set; }
    
    public int LeaveBalance { get; set; } = 0;
    
    public int? RemainingLeaveBalance { get; set; }
    public decimal MonthlyRate { get; set; }
    public decimal SupplementaryRate { get; set; }
    public decimal OvertimeRateMultiplier { get; set; } = 1.5m;
    public decimal AbsentRateMultiplier { get; set; } = 1.5m;
    public decimal LateRateMultiplier { get; set; } = 1.5m;
    public string? FactoryType { get; set; }
    
    public string? DeviceId { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public List<AttendanceResponseDto>? AttendanceRecords { get; set; }
    
    public DateTime CreatedAt { get; set; }
}