using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfTemplateConfigurationPostDto
{
    public PdfHeaderFooterConfigDto Header { get; set; } = new();
    public PdfHeaderFooterConfigDto Footer { get; set; } = new();
}