using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfColumnGroupDto
{
    public List<PdfRowGroupDto> Rows { get; set; } = new();
}