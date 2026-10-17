namespace SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;

public class ShiftResponseDto
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }
    
    public string Name { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int? GracePeriodInMinutes { get; set; } = 0;

    public bool? IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}