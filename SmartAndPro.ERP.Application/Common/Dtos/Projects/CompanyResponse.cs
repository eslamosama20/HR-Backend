namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CompanyResponse
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
