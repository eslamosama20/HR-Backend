using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateProjectAssessmentRequest
{
    [Required(ErrorMessage = "معرف المشروع مطلوب")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "تاريخ التقييم مطلوب")]
    public DateTime AssessmentDate { get; set; }

    [MaxLength(200, ErrorMessage = "اسم المقيّم لا يمكن أن يتجاوز 200 حرف")]
    public string? AssessedBy { get; set; }

    [Required(ErrorMessage = "قيمة التقييم مطلوبة")]
    [Range(0.01, double.MaxValue, ErrorMessage = "قيمة التقييم يجب أن تكون أكبر من صفر")]
    public decimal AssessmentAmount { get; set; }

    public string? AssessmentNotes { get; set; }

    public Guid? DocumentId { get; set; }
}

public class UpdateProjectAssessmentRequest : CreateProjectAssessmentRequest
{
    [Required(ErrorMessage = "معرف التقييم مطلوب")]
    public Guid Id { get; set; }
}
