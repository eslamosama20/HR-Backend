using System.IO;

namespace SmartAndPro.ERP.Application.Common.Dtos.Attachments;

public class AttachmentFileStreamDto
{
    public Stream Stream { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public long FileSize { get; set; }
}
