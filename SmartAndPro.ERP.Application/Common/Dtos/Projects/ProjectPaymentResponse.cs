namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectPaymentResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal PaymentAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
