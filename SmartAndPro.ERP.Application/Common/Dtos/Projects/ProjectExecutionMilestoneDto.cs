using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectExecutionMilestoneResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string DurationUnit { get; set; } = "Month";
    public int SequenceOrder { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? TargetStartDate { get; set; }
    public DateTime? TargetEndDate { get; set; }
}

public class CreateProjectExecutionMilestoneRequest
{
    [Required(ErrorMessage = "اسم المرحلة مطلوب")]
    [MaxLength(500)]
    public string MilestoneName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "المدة يجب أن تكون أكبر من 0")]
    public int Duration { get; set; }

    [MaxLength(50)]
    public string DurationUnit { get; set; } = "Month";

    public int SequenceOrder { get; set; }
}

public class UpdateProjectExecutionMilestoneRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المرحلة مطلوب")]
    [MaxLength(500)]
    public string MilestoneName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "المدة يجب أن تكون أكبر من 0")]
    public int Duration { get; set; }

    [MaxLength(50)]
    public string DurationUnit { get; set; } = "Month";

    public int SequenceOrder { get; set; }
}
