using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.TopProducts;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Reports.DocumentUtils;

public class TopProductsReportDocument : IDocument
{
    private readonly List<TopProductDto> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly string _language;
    private readonly bool _isRtl;
    private readonly string? _printedBy;
    private readonly string _reportTitle;
    private readonly string _transactionColumnName;
    private readonly string _lastTransactionColumnName;

    public TopProductsReportDocument(
        List<TopProductDto> items,
        CompanyDetails companyDetails,
        DateTime? startDate,
        DateTime? endDate,
        string reportTitle,
        string transactionColumnName,
        string lastTransactionColumnName,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<TopProductDto>();
        _companyDetails = companyDetails;
        _startDate = startDate;
        _endDate = endDate;
        _language = language ?? "ar";
        _isRtl = _language.ToLower() == "ar";
        _printedBy = printedBy;
        _reportTitle = reportTitle;
        _transactionColumnName = transactionColumnName;
        _lastTransactionColumnName = lastTransactionColumnName;
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

            if (_isRtl)
            {
                page.ContentFromRightToLeft();
            }

            // Header
            page.Header().Column(col =>
            {
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
                var dateLabel = _isRtl ? "التاريخ" : "Date";
                var userLabel = _isRtl ? "المستخدم" : "User";
                
                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(infoRow =>
                {
                    // Date (Right side)
                    infoRow.RelativeItem().Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text($"{dateLabel}: {DateTime.Now:dd/MM/yyyy}").FontSize(10).FontColor(Colors.White);
                        
                        if (_startDate.HasValue || _endDate.HasValue)
                        {
                            var dateRange = "";
                            if (_startDate.HasValue && _endDate.HasValue)
                            {
                                dateRange = $"من {_startDate.Value:dd/MM/yyyy} إلى {_endDate.Value:dd/MM/yyyy}";
                            }
                            else if (_startDate.HasValue)
                            {
                                dateRange = $"من {_startDate.Value:dd/MM/yyyy}";
                            }
                            else if (_endDate.HasValue)
                            {
                                dateRange = $"إلى {_endDate.Value:dd/MM/yyyy}";
                            }
                            dateCol.Item().AlignRight().Text(dateRange).FontSize(9).FontColor(Colors.White);
                        }
                    });
                    
                    // Title (Center)
                    infoRow.RelativeItem(2).Column(titleCol =>
                    {
                        titleCol.Item().AlignCenter().Text(_reportTitle).FontSize(12).Bold().FontColor(Colors.White);
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
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);   // م
                        columns.ConstantColumn(70);   // الكود
                        columns.RelativeColumn(3);    // اسم المنتج
                        columns.ConstantColumn(60);   // الوحدة
                        columns.ConstantColumn(70);   // عدد المبيعات/الطلبات
                        columns.ConstantColumn(80);   // إجمالي الكمية
                        columns.ConstantColumn(90);   // إجمالي المبلغ
                        columns.ConstantColumn(70);   // متوسط السعر
                        columns.ConstantColumn(90);   // آخر بيع/طلب
                    });

                    // Header row
                    table.Header(header =>
                    {
                        var headers = _isRtl 
                            ? new[] { "م", "الكود", "اسم المنتج", "الوحدة", _transactionColumnName, "إجمالي الكمية", "إجمالي المبلغ", "متوسط السعر", _lastTransactionColumnName }
                            : new[] { "#", "Code", "Product", "Unit", "Count", "Total Qty", "Total Amount", "Avg Price", "Last" };
                        
                        foreach (var headerText in headers)
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                                .Padding(5).AlignCenter().Text(headerText).Bold().FontSize(9);
                        }
                    });

                    // Data rows
                    for (int i = 0; i < _items.Count; i++)
                    {
                        var item = _items[i];
                        var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.Rank.ToString()).FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.ProductCode ?? "-").FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.ProductNameAr ?? "-").FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.UnitName ?? "-").FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.TransactionCount.ToString()).FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.TotalQuantity.ToString("N2")).FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.TotalAmount.ToString("N2")).FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.AverageUnitPrice.ToString("N2")).FontSize(8);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.LastTransactionDate?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                    }

                    // Totals row
                    var totalLabel = _isRtl ? "الإجمالي" : "Total";
                    table.Cell().ColumnSpan(4).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(totalLabel).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(_items.Sum(x => x.TransactionCount).ToString()).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(_items.Sum(x => x.TotalQuantity).ToString("N2")).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(_items.Sum(x => x.TotalAmount).ToString("N2")).Bold().FontSize(10);

                    table.Cell().ColumnSpan(2).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text("").FontSize(10);
                });

                // Summary section
                col.Item().PaddingTop(15).Column(summaryCol =>
                {
                    var recordsLabel = _isRtl ? "إجمالي عدد المنتجات" : "Total Products";
                    var quantityLabel = _isRtl ? "إجمالي الكميات" : "Total Quantity";
                    var amountLabel = _isRtl ? "إجمالي المبلغ" : "Total Amount";
                    
                    summaryCol.Item().AlignRight().Text($"{recordsLabel}: {_items.Count}").FontSize(10);
                    summaryCol.Item().AlignRight().Text($"{quantityLabel}: {_items.Sum(x => x.TotalQuantity):N2}").FontSize(10);
                    summaryCol.Item().AlignRight().Text($"{amountLabel}: {_items.Sum(x => x.TotalAmount):N2}").FontSize(10).FontColor(Colors.Green.Darken1);
                });
            });
        });
    }
}
