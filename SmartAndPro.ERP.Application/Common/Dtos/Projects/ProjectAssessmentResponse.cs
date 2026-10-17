namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectAssessmentResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string? AssessedBy { get; set; }
    public decimal? AssessmentAmount { get; set; }
    public string? AssessmentNotes { get; set; }
    public Guid? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
