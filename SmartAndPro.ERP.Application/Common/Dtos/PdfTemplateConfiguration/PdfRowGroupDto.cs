using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfRowGroupDto
{
    public List<PdfItemDto> Items { get; set; } = new();
}
