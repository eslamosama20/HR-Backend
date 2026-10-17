using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Permits.DocumentUtils;

public class StoreTransferReportItem
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string FromStore { get; set; } = string.Empty;
    public string ToStore { get; set; } = string.Empty;
    public string Products { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal ShippingCost { get; set; }
    public string Status { get; set; } = "مكتمل";
}

public class StoreTransferReportDocument : IDocument
{
    private readonly List<StoreTransferReportItem> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly string _language;
    private readonly bool _isRtl;
    private readonly string? _printedBy;

    public StoreTransferReportDocument(
        List<StoreTransferReportItem> items,
        CompanyDetails companyDetails,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<StoreTransferReportItem>();
        _companyDetails = companyDetails;
        _language = language ?? "ar";
        _isRtl = _language.ToLower() == "ar";
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

            // Apply RTL content direction if Arabic
            if (_isRtl)
            {
                page.ContentFromRightToLeft();
            }

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
                var pageLabel = _isRtl ? "صفحة" : "Page";
                var ofLabel = _isRtl ? "من" : "of";
                text.Span($"{pageLabel} ").FontSize(9);
                text.CurrentPageNumber().FontSize(9);
                text.Span($" {ofLabel} ").FontSize(9);
                text.TotalPages().FontSize(9);
            });

            // Content
            page.Content().Column(col =>
            {
                // Report info row: Date (Right) - Title (Center) - User (Left) - moved to content to eliminate gap
                var reportTitle = _isRtl ? "تقرير نقل المخزون" : "Store Transfer Report";
                var dateLabel = _isRtl ? "التاريخ" : "Date";
                var userLabel = _isRtl ? "المستخدم" : "User";
                
                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(infoRow =>
                {
                    // Date (Right side)
                    infoRow.RelativeItem().Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text($"{dateLabel}: {DateTime.Now:dd/MM/yyyy}").FontSize(10).FontColor(Colors.White);
                    });
                    
                    // Title (Center)
                    infoRow.RelativeItem(2).Column(titleCol =>
                    {
                        var titleText = reportTitle;
                        titleCol.Item().AlignCenter().Text(titleText).FontSize(12).Bold().FontColor(Colors.White);
                    });
                    
                    // User (Left side)
                    infoRow.RelativeItem().Column(userCol =>
                    {
                        if (!string.IsNullOrEmpty(_printedBy))
                        {
                            userCol.Item().AlignLeft().Text($"{userLabel}: {_printedBy}").FontSize(10).FontColor(Colors.White);
                        }
                    });
                });

                col.Item().Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // م
                        columns.ConstantColumn(80);   // التاريخ
                        columns.ConstantColumn(100);  // رقم المرجع
                        columns.RelativeColumn(2);    // من المخزن
                        columns.RelativeColumn(2);    // إلى المخزن
                        columns.RelativeColumn(3);    // الأصناف
                        columns.ConstantColumn(60);   // الكمية
                        columns.ConstantColumn(80);   // رسوم الشحن
                        columns.ConstantColumn(70);   // الحالة
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("م").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("التاريخ").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("رقم المرجع").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("من المخزن").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("إلى المخزن").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الأصناف").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الكمية").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("رسوم الشحن").Bold().FontSize(9).FontColor(Colors.White);
                        
                        header.Cell().Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الحالة").Bold().FontSize(9).FontColor(Colors.White);
                    });

                    // Data rows
                    int index = 1;
                    foreach (var item in _items)
                    {
                        var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignCenter().Text(index.ToString()).FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignCenter().Text(item.TransferDate.ToString("dd/MM/yyyy")).FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignCenter().Text(item.ReferenceNumber ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignRight().Text(item.FromStore ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignRight().Text(item.ToStore ?? "-").FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignRight().Text(TruncateText(item.Products, 40)).FontSize(8);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignCenter().Text(FormatQuantity(item.TotalQuantity)).FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignCenter().Text(item.ShippingCost > 0 ? $"{FormatQuantity(item.ShippingCost)} ر.س" : "-").FontSize(9);

                        table.Cell().Background(bgColor).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(4).AlignCenter().Text(item.Status ?? "-").FontSize(9);

                        index++;
                    }

                    // Totals row
                    table.Cell().ColumnSpan(6).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text("الإجمالي").Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatQuantity(_items.Sum(x => x.TotalQuantity))).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text($"{FormatQuantity(_items.Sum(x => x.ShippingCost))} ر.س").Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text("").FontSize(10);
                });
                
                // Summary section
                col.Item().PaddingTop(15).Column(summaryCol =>
                {
                    summaryCol.Item().Text($"إجمالي عمليات النقل: {_items.Count}").FontSize(10).Bold();
                    summaryCol.Item().Text($"إجمالي الكميات: {FormatQuantity(_items.Sum(x => x.TotalQuantity))}").FontSize(10);
                    summaryCol.Item().Text($"إجمالي رسوم الشحن: {FormatQuantity(_items.Sum(x => x.ShippingCost))} ر.س").FontSize(10);
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

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "-";
        return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
    }
}
