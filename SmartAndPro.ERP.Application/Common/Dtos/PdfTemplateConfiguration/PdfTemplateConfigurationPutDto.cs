using System;

namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfTemplateConfigurationPutDto
{
    public Guid Id { get; set; }
    public PdfHeaderFooterConfigDto Header { get; set; } = new();
    public PdfHeaderFooterConfigDto Footer { get; set; } = new();
}