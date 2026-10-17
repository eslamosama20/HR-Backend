namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectHistoryResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? ActionBy { get; set; }
    public DateTime ActionDate { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Notes { get; set; }
}
