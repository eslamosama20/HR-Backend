using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.InventoryManagement.DocumentUtils;

/// <summary>
/// Generic inventory classes report document for purchase/sales classes
/// Uses a common DTO to handle both MainPurchaseClass and MainSalesClass
/// </summary>
public class InventoryClassesReportDocument : IDocument
{
    private readonly List<InventoryClassReportItem> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly string _language;
    private readonly bool _isRtl;
    private readonly string? _printedBy;
    private readonly string _classType; // "purchase" or "sales"

    public InventoryClassesReportDocument(
        List<InventoryClassReportItem> items,
        CompanyDetails companyDetails,
        string classType,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<InventoryClassReportItem>();
        _companyDetails = companyDetails;
        _classType = classType ?? "purchase";
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
                var isPurchase = _classType.ToLower() == "purchase";
                var reportTitle = _isRtl 
                    ? (isPurchase ? "تقرير أصناف المشتريات" : "تقرير أصناف المبيعات")
                    : (isPurchase ? "Purchase Classes Report" : "Sales Classes Report");
                var dateLabel = _isRtl ? "التاريخ" : "Date";
                var userLabel = _isRtl ? "المستخدم" : "User";
                
                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(infoRow =>
                {
                    infoRow.RelativeItem().Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text($"{dateLabel}: {DateTime.Now:dd/MM/yyyy}").FontSize(10).FontColor(Colors.White);
                    });

                    infoRow.RelativeItem().AlignCenter().Text(reportTitle).FontSize(16).Bold().FontColor(Colors.White);

                    infoRow.RelativeItem().Column(userCol =>
                    {
                        if (!string.IsNullOrEmpty(_printedBy))
                        {
                            userCol.Item().AlignLeft().Text($"{userLabel}: {_printedBy}").FontSize(10).FontColor(Colors.White);
                        }
                    });
                });

                col.Item().PaddingTop(10);

                // Summary data (will be displayed at the end)
                var totalClasses = _items.Count;
                var totalQuantity = _items.Sum(x => x.ProductUnitsCount);

                var totalLabel = _isRtl ? "إجمالي الأصناف" : "Total Classes";
                var totalQuantityLabel = _isRtl ? "إجمالي الكمية" : "Total Quantity";

                // Table - different columns for purchase vs sales
                var isPurchaseClass = _classType.ToLower() == "purchase";
                
                col.Item().Table(table =>
                {
                    if (isPurchaseClass)
                    {
                        // Purchase classes: #, Class, Measurement Unit, Quantity, Created At
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);   // #
                            columns.RelativeColumn(3);    // Class (Code + Name)
                            columns.RelativeColumn(2);    // Measurement Unit
                            columns.RelativeColumn(1);    // Quantity
                            columns.RelativeColumn(1.5f); // Created At
                        });

                        // Header
                        table.Header(header =>
                        {
                            var headers = _isRtl 
                                ? new[] { "م", "الصنف", "وحدة القياس", "الكمية", "تاريخ الإنشاء" }
                                : new[] { "#", "Class", "Measurement Unit", "Quantity", "Created At" };

                            foreach (var h in headers)
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(h).FontSize(10).Bold();
                            }
                        });

                        // Data rows
                        var index = 1;
                        foreach (var item in _items)
                        {
                            var rowColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            // #
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(index.ToString()).FontSize(9);
                            
                            // Class (Code | Name)
                            var className = _isRtl 
                                ? $"{item.ReferenceCode} | {item.ArabicName}"
                                : $"{item.ReferenceCode} | {item.EnglishName}";
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(className ?? "-").FontSize(9);
                            
                            // Measurement Unit
                            var measurementUnit = _isRtl 
                                ? $"{item.MeasurementUnitArabicName} ({item.MeasurementUnitArabicSymbol})"
                                : $"{item.MeasurementUnitEnglishName} ({item.MeasurementUnitEnglishSymbol})";
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(measurementUnit ?? "-").FontSize(9);
                            
                            // Quantity
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.ProductUnitsCount.ToString("N0")).FontSize(9);
                            
                            // Created At
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.CreatedAt?.ToString("dd/MM/yyyy") ?? "-").FontSize(9);

                            index++;
                        }
                    }
                    else
                    {
                        // Sales classes: #, Class, Measurement Unit, Quantity, Class Type, Created At
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);   // #
                            columns.RelativeColumn(3);    // Class (Code + Name)
                            columns.RelativeColumn(2);    // Measurement Unit
                            columns.RelativeColumn(1);    // Quantity
                            columns.RelativeColumn(1);    // Class Type
                            columns.RelativeColumn(1.5f); // Created At
                        });

                        // Header
                        table.Header(header =>
                        {
                            var headers = _isRtl 
                                ? new[] { "م", "الصنف", "وحدة القياس", "الكمية", "نوع الصنف", "تاريخ الإنشاء" }
                                : new[] { "#", "Class", "Measurement Unit", "Quantity", "Class Type", "Created At" };

                            foreach (var h in headers)
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(h).FontSize(10).Bold();
                            }
                        });

                        // Data rows
                        var index = 1;
                        foreach (var item in _items)
                        {
                            var rowColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            // #
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(index.ToString()).FontSize(9);
                            
                            // Class (Code | Name)
                            var className = _isRtl 
                                ? $"{item.ReferenceCode} | {item.ArabicName}"
                                : $"{item.ReferenceCode} | {item.EnglishName}";
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(className ?? "-").FontSize(9);
                            
                            // Measurement Unit
                            var measurementUnit = _isRtl 
                                ? $"{item.MeasurementUnitArabicName} ({item.MeasurementUnitArabicSymbol})"
                                : $"{item.MeasurementUnitEnglishName} ({item.MeasurementUnitEnglishSymbol})";
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(measurementUnit ?? "-").FontSize(9);
                            
                            // Quantity
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.ProductUnitsCount.ToString("N0")).FontSize(9);
                            
                            // Class Type
                            var classTypeText = item.ClassType == "Direct" 
                                ? (_isRtl ? "مباشر" : "Direct") 
                                : (_isRtl ? "مركب" : "Compound");
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(classTypeText).FontSize(9);
                            
                            // Created At
                            table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.CreatedAt?.ToString("dd/MM/yyyy") ?? "-").FontSize(9);

                            index++;
                        }
                    }
                });

                // Empty message
                if (!_items.Any())
                {
                    var noDataMessage = _isRtl ? "لا توجد أصناف" : "No classes found";
                    col.Item().PaddingTop(20).AlignCenter().Text(noDataMessage).FontSize(12).FontColor(Colors.Grey.Medium);
                }

                // Total at the end (after table) - matching DamagedInventoryReportDocument style
                col.Item().PaddingTop(10).Table(totalsTable =>
                {
                    totalsTable.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    totalsTable.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text($"{totalLabel}: {totalClasses}").FontSize(10).Bold();
                    totalsTable.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text($"{totalQuantityLabel}: {totalQuantity:N0}").FontSize(10).Bold();
                });
            });
        });
    }

    public byte[] GeneratePDF()
    {
        return Document.Create(Compose).GeneratePdf();
    }
}

/// <summary>
/// Common DTO for both Purchase and Sales classes
/// </summary>
public class InventoryClassReportItem
{
    public Guid Id { get; set; }
    public string? ArabicName { get; set; }
    public string? EnglishName { get; set; }
    public string? ReferenceCode { get; set; }
    public string? MeasurementUnitArabicName { get; set; }
    public string? MeasurementUnitEnglishName { get; set; }
    public string? MeasurementUnitArabicSymbol { get; set; }
    public string? MeasurementUnitEnglishSymbol { get; set; }
    public decimal ProductUnitsCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    // For sales classes only
    public string? ClassType { get; set; }
}
