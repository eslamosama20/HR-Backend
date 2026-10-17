namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectAssignmentPhaseResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ExecutingEntityId { get; set; }
    public string? ExecutingEntityName { get; set; }
    public Guid? SupervisingEntityId { get; set; }
    public string? SupervisingEntityName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime? AssignmentDate { get; set; }
    public DateTime? AssignmentApprovalDate { get; set; }
    public DateTime? ExecutionStartDate { get; set; }
    public int? ExecutionDuration { get; set; }
    public string? ExecutionDurationUnit { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
