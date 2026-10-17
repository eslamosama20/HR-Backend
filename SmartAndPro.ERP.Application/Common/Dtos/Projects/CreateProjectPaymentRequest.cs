using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateProjectPaymentRequest
{
    [Required(ErrorMessage = "معرف المشروع مطلوب")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "تاريخ الدفع مطلوب")]
    public DateTime PaymentDate { get; set; }

    [Required(ErrorMessage = "قيمة الدفع مطلوبة")]
    [Range(0.01, double.MaxValue, ErrorMessage = "قيمة الدفع يجب أن تكون أكبر من صفر")]
    public decimal PaymentAmount { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(200)]
    public string? PaymentReference { get; set; }

    public string? Notes { get; set; }
}
