using System;
using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfTemplateConfigurationResponseDto
{
    public Guid Id { get; set; }
    public PdfHeaderFooterConfigDto? Header { get; set; }
    public PdfHeaderFooterConfigDto? Footer { get; set; }
}