using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Domains.ValueObjects;

[Owned]
public class PdfHeaderFooterConfig
{
    public List<PdfColumnGroup> Columns { get; set; } = new();
    public List<CustomText> CustomTexts { get; set; } = new();
    public List<string> AddressSubfields { get; set; } = new();
}

[Owned]
public class PdfColumnGroup
{
    public int Id { get; set; }
    public List<PdfRowGroup> Rows { get; set; } = new();
}

[Owned]
public class PdfRowGroup
{
    public int Id { get; set; }
    public List<PdfItem> Items { get; set; } = new();
}

[Owned]
public class PdfItem
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "field"; // "field" or "custom"
    public string FontSize { get; set; } = "inherit";
    public string Align { get; set; } = "left"; // left, center, right
    public string Label { get; set; } = "";
    
    public bool IsBold { get; set; } = false;
    public bool IsItalic { get; set; } = false;
    public bool IsUnderline { get; set; } = false;
    public bool IsStrikethrough { get; set; } = false;
}

[Owned]
public class CustomText
{
    public string Id { get; set; }
    public string Value { get; set; } = "";
}