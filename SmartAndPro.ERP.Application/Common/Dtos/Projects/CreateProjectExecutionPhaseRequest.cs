using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateProjectExecutionPhaseRequest
{
    [Required(ErrorMessage = "معرف المشروع مطلوب")]
    public Guid ProjectId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "قيمة المنحة يجب أن تكون أكبر من أو تساوي صفر")]
    public decimal? GrantAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "قيمة التنفيذ يجب أن تكون أكبر من أو تساوي صفر")]
    public decimal? ExecutionAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "المبلغ المتبقي يجب أن يكون أكبر من أو يساوي صفر")]
    public decimal? RemainingAmount { get; set; }
    
    [Range(0, 100, ErrorMessage = "نسبة الإنجاز يجب أن تكون بين 0 و 100")]
    public decimal? CompletionPercentage { get; set; }
    
    public bool? TransferIssued { get; set; }
    public DateTime? TransferDate { get; set; }
    
    public bool? ContractDataSent { get; set; }
    public DateTime? ContractSentDate { get; set; }
    
    public bool? FinalReportSent { get; set; }
    public DateTime? FinalReportDate { get; set; }
    
    public Guid? CompanyId { get; set; }
    
    [MaxLength(200)]
    public string? CompanyName { get; set; }
    
    public string? Notes { get; set; }
}
