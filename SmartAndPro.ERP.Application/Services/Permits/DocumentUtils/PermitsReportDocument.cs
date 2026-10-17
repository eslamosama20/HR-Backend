using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermit;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Permits.DocumentUtils;

public class PermitsReportDocument : IDocument
{
    private readonly List<InventoryPermitResponseDto> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly string _language;
    private readonly bool _isRtl;
    private readonly string? _printedBy;

    public PermitsReportDocument(
        List<InventoryPermitResponseDto> items,
        CompanyDetails companyDetails,
        DateTime? startDate,
        DateTime? endDate,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<InventoryPermitResponseDto>();
        _companyDetails = companyDetails;
        _startDate = startDate;
        _endDate = endDate;
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
                // Report info row: Date (Right) - Title (Center) - User (Left)
                var reportTitle = _isRtl ? "تقرير الأذون المخزنية" : "Inventory Permits Report";
                var dateLabel = _isRtl ? "التاريخ" : "Date";
                var userLabel = _isRtl ? "المستخدم" : "User";
                
                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(infoRow =>
                {
                    // Date (Right side)
                    infoRow.RelativeItem().Column(dateCol =>
                    {
                        dateCol.Item().AlignRight().Text($"{dateLabel}: {DateTime.Now:dd/MM/yyyy}").FontSize(10).FontColor(Colors.White);
                        
                        // Date range if provided
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
                        titleCol.Item().AlignCenter().Text(reportTitle).FontSize(12).Bold().FontColor(Colors.White);
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
                        columns.RelativeColumn(2);    // رقم الإذن
                        columns.ConstantColumn(80);   // نوع الإذن
                        columns.RelativeColumn(2);    // المخزن
                        columns.RelativeColumn(3);    // الوصف
                        columns.ConstantColumn(100);  // تاريخ الإذن
                        columns.ConstantColumn(100);  // تاريخ الإنشاء
                    });

                    // Header row
                    table.Header(header =>
                    {
                        var headers = _isRtl 
                            ? new[] { "م", "رقم الإذن", "نوع الإذن", "المخزن", "الوصف", "تاريخ الإذن", "تاريخ الإنشاء" }
                            : new[] { "#", "Permit No.", "Type", "Store", "Description", "Permit Date", "Created At" };
                        
                        foreach (var headerText in headers)
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                                .Padding(5).AlignCenter().Text(headerText).Bold().FontSize(10);
                        }
                    });

                    // Data rows
                    for (int i = 0; i < _items.Count; i++)
                    {
                        var item = _items[i];
                        var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        // م
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text((i + 1).ToString()).FontSize(9);
                        
                        // رقم الإذن
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text(item.ReferenceNumber ?? "-").FontSize(9);
                        
                        // نوع الإذن
                        var permitTypeDisplay = item.PermitType switch
                        {
                            "Entry" => _isRtl ? "دخول" : "Entry",
                            "Exit" => _isRtl ? "خروج" : "Exit",
                            _ => item.PermitType ?? "-"
                        };
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text(permitTypeDisplay).FontSize(9);
                        
                        // المخزن
                        var storeName = _isRtl 
                            ? (item.StoreDetails?.ArabicName ?? item.StoreDetails?.EnglishName ?? "-")
                            : (item.StoreDetails?.EnglishName ?? item.StoreDetails?.ArabicName ?? "-");
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text(storeName).FontSize(9);
                        
                        // الوصف
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text(item.Description ?? "-").FontSize(9);
                        
                        // تاريخ الإذن
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text(item.PermitDate.ToString("dd/MM/yyyy")).FontSize(9);
                        
                        // تاريخ الإنشاء
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text(item.CreatedAt.ToString("dd/MM/yyyy")).FontSize(9);
                    }

                    // Summary row
                    var entryCount = _items.Count(x => x.PermitType == "Entry");
                    var exitCount = _items.Count(x => x.PermitType == "Exit");
                    var entryLabel = _isRtl ? "أذون الدخول" : "Entry Permits";
                    var exitLabel = _isRtl ? "أذون الخروج" : "Exit Permits";
                    var totalLabel = _isRtl ? "إجمالي الأذون" : "Total Permits";
                    
                    table.Cell().ColumnSpan(7).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text($"{totalLabel}: {_items.Count}  |  {entryLabel}: {entryCount}  |  {exitLabel}: {exitCount}").Bold().FontSize(10);
                });

                // Summary section
                col.Item().PaddingTop(15).Column(summaryCol =>
                {
                    var recordsLabel = _isRtl ? "عدد الأذون" : "Total Permits";
                    summaryCol.Item().AlignRight().Text($"{recordsLabel}: {_items.Count}").FontSize(10);
                });
            });
        });
    }

    public byte[] GeneratePDF()
    {
        return Document.Create(Compose).GeneratePdf();
    }
}
