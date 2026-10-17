using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfHeaderFooterConfigDto
{
    public List<PdfColumnGroupDto> Columns { get; set; } = new();
    public List<CustomTextDto> CustomTexts { get; set; } = new();
    public List<string> AddressSubfields { get; set; } = new();
}