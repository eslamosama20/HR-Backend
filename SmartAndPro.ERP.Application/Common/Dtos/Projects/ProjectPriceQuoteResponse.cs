namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectPriceQuoteResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime QuoteDate { get; set; }
    public string? QuoteNumber { get; set; }
    public decimal? QuoteAmount { get; set; }
    public DateTime? ValidUntil { get; set; }
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public Guid? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public string? FilePath { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? SaleInvoiceId { get; set; }
    public string? CustomerName { get; set; }
    public string? DepartmentName { get; set; }
}
