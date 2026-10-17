using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateDepartmentRequest
{
    [Required(ErrorMessage = "اسم الإدارة مطلوب")]
    [MaxLength(300)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DepartmentCode { get; set; }
}
