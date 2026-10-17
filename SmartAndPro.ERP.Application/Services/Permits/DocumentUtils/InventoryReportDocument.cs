using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Permits.DocumentUtils;

public class InventoryReportItem
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal AverageCost { get; set; }
    public decimal TotalValue { get; set; }
}

public class InventoryReportDocument : IDocument
{
    private readonly List<InventoryReportItem> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly string? _storeName;
    private readonly string _language;
    private readonly string? _printedBy;

    public InventoryReportDocument(
        List<InventoryReportItem> items,
        CompanyDetails companyDetails,
        string? storeName = null,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<InventoryReportItem>();
        _companyDetails = companyDetails;
        _storeName = storeName;
        _language = language;
        _printedBy = printedBy;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.PageColor(Colors.White);
            page.MarginVertical(10);
            page.MarginHorizontal(20);
            page.Size(PageSizes.A4.Landscape());
            page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));
            
            // Enable RTL layout for Arabic
            page.ContentFromRightToLeft();

            // Header
            page.Header().Column(col =>
            {
                // Company Header - English Left, Logo Center, Arabic Right
                // Use ContentFromLeftToRight to prevent RTL from affecting the header layout
                col.Item().ContentFromLeftToRight().Row(row =>
                {
                    // English Info (LEFT side)
                    row.RelativeItem().Column(englishCol =>
                    {
                        englishCol.Item().PaddingBottom(3).AlignLeft().Text(_companyDetails?.EnglishName ?? "").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        
                        var englishAddress = _companyDetails?.Address?.FullAddress;
                        if (!string.IsNullOrEmpty(englishAddress))
                        {
                            englishCol.Item().PaddingBottom(2).AlignLeft().Text(englishAddress).FontSize(9).FontColor(Colors.Blue.Darken2);
                        }
                        
                        if (!string.IsNullOrEmpty(_companyDetails?.TaxNumber))
                        {
                            englishCol.Item().AlignLeft().Text($"Tax ID: {_companyDetails.TaxNumber}").FontSize(9).FontColor(Colors.Blue.Darken2);
                        }
                    });

                    // Logo (CENTER)
                    row.ConstantItem(180).AlignMiddle().AlignCenter().Column(logoCol =>
                    {
                        if (_companyDetails?.Logo != null)
                        {
                            logoCol.Item().AlignCenter().Width(120).Height(80).Image(_companyDetails.Logo);
                        }
                    });

                    // Arabic Info (RIGHT side)
                    row.RelativeItem().Column(arabicCol =>
                    {
                        arabicCol.Item().PaddingBottom(3).AlignRight().Text(_companyDetails?.ArabicName ?? "").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        
                        var arabicAddress = _companyDetails?.ArabicAddress?.FullAddress;
                        if (!string.IsNullOrEmpty(arabicAddress))
                        {
                            arabicCol.Item().PaddingBottom(2).AlignRight().Text(arabicAddress).FontSize(9).FontColor(Colors.Blue.Darken2);
                        }
                        
                        if (!string.IsNullOrEmpty(_companyDetails?.TaxNumber))
                        {
                            arabicCol.Item().AlignRight().Text($"الرقم الضريبي: {_companyDetails.TaxNumber}").FontSize(9).FontColor(Colors.Blue.Darken2);
                        }
                    });
                });
            });

            // Footer
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("صفحة ").FontSize(9);
                text.CurrentPageNumber().FontSize(9);
                text.Span(" من ").FontSize(9);
                text.TotalPages().FontSize(9);
            });

            // Content
            page.Content().Column(col =>
            {
                // Report info row: Date (Right) - Title (Center) - User (Left) - moved to content to eliminate gap
                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(infoRow =>
                {
                    // Date (Right side)
                    infoRow.RelativeItem().Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text($"التاريخ: {DateTime.Now:dd/MM/yyyy}").FontSize(10).FontColor(Colors.White);
                    });
                    
                    // Title (Center)
                    infoRow.RelativeItem(2).Column(titleCol =>
                    {
                        var titleText = !string.IsNullOrEmpty(_storeName) 
                            ? $"تقرير المخزون - المخزن: {_storeName}" 
                            : "تقرير المخزون";
                        titleCol.Item().AlignCenter().Text(titleText).FontSize(12).Bold().FontColor(Colors.White);
                    });
                    
                    // User (Left side)
                    infoRow.RelativeItem().Column(userCol =>
                    {
                        if (!string.IsNullOrEmpty(_printedBy))
                        {
                            userCol.Item().AlignLeft().Text($"المستخدم: {_printedBy}").FontSize(10).FontColor(Colors.White);
                        }
                    });
                });

                col.Item().Table(table =>
                {
                    // Define columns in logical order (RTL mode will flip them automatically)
                    // Logical order: م، كود الصنف، اسم الصنف، الوحدة، الكمية
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);   // م
                        columns.ConstantColumn(120);  // كود الصنف
                        columns.RelativeColumn(4);    // اسم الصنف
                        columns.ConstantColumn(100);  // الوحدة
                        columns.ConstantColumn(100);  // الكمية
                    });

                    // Header row in logical order (RTL mode will flip)
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("م").Bold().FontSize(10);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("كود الصنف").Bold().FontSize(10);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("اسم الصنف").Bold().FontSize(10);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الوحدة").Bold().FontSize(10);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الكمية").Bold().FontSize(10);
                    });

                    // Data rows in logical order (RTL mode will flip)
                    int index = 1;
                    foreach (var item in _items)
                    {
                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(index.ToString()).FontSize(10);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(item.ProductCode ?? "-").FontSize(10);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignRight().Text(item.ProductName ?? "-").FontSize(10);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(item.UnitName ?? "-").FontSize(10);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(FormatQuantity(item.Quantity)).FontSize(10);

                        index++;
                    }

                    // Totals row (RTL mode will flip)
                    table.Cell().ColumnSpan(4).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text("إجمالي الكمية").Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatQuantity(_items.Sum(x => x.Quantity))).Bold().FontSize(10);
                });
                
                // Summary section
                col.Item().PaddingTop(15).Column(summaryCol =>
                {
                    summaryCol.Item().AlignRight().Text($"عدد الأصناف: {_items.Count}").FontSize(10);
                });
            });
        });
    }

    public byte[] GeneratePDF()
    {
        return this.GeneratePdf();
    }

    private static string FormatQuantity(decimal quantity)
    {
        return quantity.ToString("N2");
    }

    private static string FormatCurrency(decimal amount)
    {
        return amount.ToString("N2");
    }
}
