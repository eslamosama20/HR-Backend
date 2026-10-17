namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class DepartmentResponse
{
    public Guid Id { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
