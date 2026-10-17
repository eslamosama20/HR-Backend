namespace SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;

public class EmploymentTypeResponseDto
{
    public Guid Id { get; set; }

    public string Code { get; set; }
    
    public string? Description { get; set; }
    
    public string? WeeklyDayOff { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}