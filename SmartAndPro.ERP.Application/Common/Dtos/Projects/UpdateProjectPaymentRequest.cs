using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class UpdateProjectPaymentRequest : CreateProjectPaymentRequest
{
    [Required(ErrorMessage = "معرف الدفعة مطلوب")]
    public Guid Id { get; set; }
}
