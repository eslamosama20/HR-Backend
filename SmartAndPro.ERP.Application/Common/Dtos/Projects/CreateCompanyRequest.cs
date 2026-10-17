using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateCompanyRequest
{
    [Required(ErrorMessage = "اسم الشركة مطلوب")]
    [MaxLength(500)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CompanyType { get; set; }

    [MaxLength(200)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
}
