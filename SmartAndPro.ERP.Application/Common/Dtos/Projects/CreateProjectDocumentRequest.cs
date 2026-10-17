using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateProjectDocumentRequest
{
    [Required(ErrorMessage = "معرف المشروع مطلوب")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "نوع المستند مطلوب")]
    [MaxLength(100)]
    public string DocumentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? DocumentName { get; set; }

    [MaxLength(1000)]
    public string? FilePath { get; set; }

    public long? FileSize { get; set; }

    [MaxLength(100)]
    public string? MimeType { get; set; }

    [MaxLength(100)]
    public string? UploadedBy { get; set; }

    public string? MetaData { get; set; }
}
