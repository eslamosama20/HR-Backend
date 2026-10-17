using Microsoft.AspNetCore.Http;

namespace SmartAndPro.ERP.Application.Common.Dtos.Attachments;

public class AttachmentUploadDto
{
    public IFormFile File { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? AccountingOrderId { get; set; }
    public Guid? PaymentReceiptId { get; set; }
    public Guid? PaycheckReceiptId { get; set; }
}
