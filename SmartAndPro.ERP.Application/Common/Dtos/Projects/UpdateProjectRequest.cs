using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class UpdateProjectRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المشروع مطلوب")]
    [MaxLength(500)]
    public string ProjectName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ProjectNumber { get; set; }

    [MaxLength(100)]
    public string? FileNumber { get; set; }

    [Required(ErrorMessage = "حالة المشروع مطلوبة")]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFinancialSolvency { get; set; } = false;
    
    public decimal? TotalAmount { get; set; }
    
    public decimal? ExecutionAmount { get; set; }
    
    public decimal? ProfitPercentage { get; set; }

    public string? Country { get; set; }

    public decimal? CommissionPercentage { get; set; }

    public string? CheckNumber { get; set; }

    public DateTime? CheckReceiptDate { get; set; }

    public decimal? CheckAmount { get; set; }

    public string? ClearingDetails { get; set; }

    public Guid? CompanyDetailsId { get; set; }

    public string? ShareToken { get; set; }

    public string? DepartmentName { get; set; }

    public string? SupervisingEntityName { get; set; }

    [MaxLength(200)]
    public string? ExecutingCompanyName { get; set; }

    public string? QuoteNumber { get; set; }

    public DateTime? QuoteDate { get; set; }

    public string? AssignmentNumber { get; set; }

    public DateTime? AssignmentApprovalDate { get; set; }

    public int? ExecutionDuration { get; set; }

    public string? ExecutionDurationUnit { get; set; }

    public System.Collections.Generic.List<ProjectCheckDto> Checks { get; set; } = new();
    public System.Collections.Generic.List<DeletedCheckDto> DeletedChecks { get; set; } = new();
    public System.Collections.Generic.List<UpdateProjectExecutionMilestoneRequest> Milestones { get; set; } = new();
}

public class DeletedCheckDto
{
    public Guid Id { get; set; }
    public string Reason { get; set; } = string.Empty;
}
