using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.OvertimeHours;

public class OvertimeHoursResponseDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public EmployeeResponseDto Employee { get; set; }
    
    public DateOnly Date { get; set; }
    
    public decimal HoursCount { get; set; }
    
    public string? Reason { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}