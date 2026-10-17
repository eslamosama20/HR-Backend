namespace SmartAndPro.ERP.Application.Common.Dtos.PdfTemplateConfiguration;

public class PdfItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "field";
    public string FontSize { get; set; } = "inherit";
    public string Align { get; set; } = "left";
    public string Label { get; set; } = string.Empty;
    
    public bool IsBold { get; set; } = false;
    public bool IsItalic { get; set; } = false;
    public bool IsUnderline { get; set; } = false;
    public bool IsStrikethrough { get; set; } = false;
}