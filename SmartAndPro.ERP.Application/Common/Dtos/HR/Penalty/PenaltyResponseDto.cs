using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Penalty;

public class PenaltyResponseDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public EmployeeResponseDto Employee { get; set; }
    
    public DateOnly Date { get; set; }
    
    public decimal Amount { get; set; }
    
    public string? Reason { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}