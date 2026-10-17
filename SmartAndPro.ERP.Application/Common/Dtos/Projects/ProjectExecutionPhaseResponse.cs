namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectExecutionPhaseResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public decimal? GrantAmount { get; set; }
    public decimal? ExecutionAmount { get; set; }
    public decimal? RemainingAmount { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public bool TransferIssued { get; set; }
    public DateTime? TransferDate { get; set; }
    public bool ContractDataSent { get; set; }
    public DateTime? ContractSentDate { get; set; }
    public bool FinalReportSent { get; set; }
    public DateTime? FinalReportDate { get; set; }
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
