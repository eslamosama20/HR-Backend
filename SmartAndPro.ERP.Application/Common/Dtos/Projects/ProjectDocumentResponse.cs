namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class ProjectDocumentResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentName { get; set; }
    public string? FilePath { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? MetaData { get; set; }
    public bool IsActive { get; set; }
}
