using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateProjectAssignmentPhaseRequest
{
    [Required(ErrorMessage = "معرف المشروع مطلوب")]
    public Guid ProjectId { get; set; }

    public Guid? ExecutingEntityId { get; set; }
    
    [MaxLength(200)]
    public string? ExecutingEntityName { get; set; }
    
    public Guid? SupervisingEntityId { get; set; }
    
    [MaxLength(200)]
    public string? SupervisingEntityName { get; set; }
    
    public Guid? DepartmentId { get; set; }
    
    [MaxLength(200)]
    public string? DepartmentName { get; set; }
    
    public DateTime? AssignmentDate { get; set; }
    public DateTime? AssignmentApprovalDate { get; set; }
    public DateTime? ExecutionStartDate { get; set; }
    public int? ExecutionDuration { get; set; }
    
    [MaxLength(50)]
    public string? ExecutionDurationUnit { get; set; }
    
    public string? Notes { get; set; }
}
