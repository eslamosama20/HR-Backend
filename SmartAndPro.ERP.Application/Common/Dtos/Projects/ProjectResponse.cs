namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectNumber { get; set; }
    public string? FileNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsFinancialSolvency { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? ExecutionAmount { get; set; }
    public decimal? ProfitPercentage { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? Country { get; set; }
    public decimal? CommissionPercentage { get; set; }
    public string? CheckNumber { get; set; }
    public DateTime? CheckReceiptDate { get; set; }
    public decimal? CheckAmount { get; set; }
    public string? ClearingDetails { get; set; }
    public Guid? CompanyDetailsId { get; set; }
    public string? CompanyName { get; set; }
    public string? ShareToken { get; set; }
    
    public string? DepartmentName { get; set; }
    public string? SupervisingEntityName { get; set; }
    public string? ExecutingCompanyName { get; set; }
    public string? QuoteNumber { get; set; }
    public DateTime? QuoteDate { get; set; }
    public string? AssignmentNumber { get; set; }
    public DateTime? AssignmentApprovalDate { get; set; }
    public int? ExecutionDuration { get; set; }
    public string? ExecutionDurationUnit { get; set; }
    
    // Navigation properties counts
    public int DocumentsCount { get; set; }
    public int PaymentsCount { get; set; }
    public int HistoryCount { get; set; }
    public bool HasAssignmentPhase { get; set; }
    public bool HasExecutionPhase { get; set; }
    public System.Collections.Generic.List<ProjectExecutionMilestoneResponse> Milestones { get; set; } = new();
}
