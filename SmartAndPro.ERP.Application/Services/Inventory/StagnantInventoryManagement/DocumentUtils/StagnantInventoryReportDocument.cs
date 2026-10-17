using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.StagnantInventoryManagement.DocumentUtils;

public class StagnantInventoryReportDocument : IDocument
{
    private readonly List<StagnantInventoryResponseDto> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly string _storeName;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly string _language;
    private readonly bool _isRtl;
    private readonly string? _printedBy;

    public StagnantInventoryReportDocument(
        List<StagnantInventoryResponseDto> items,
        CompanyDetails companyDetails,
        string storeName,
        DateTime? startDate,
        DateTime? endDate,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<StagnantInventoryResponseDto>();
        _companyDetails = companyDetails;
        _storeName = storeName;
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
                // Report info row: Date (Right) - Title (Center) - User (Left)
                var reportTitle = _isRtl ? "تقرير الرواكد" : "Stagnant Inventory Report";
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
                        var storeLabel = _isRtl ? "المخزن" : "Store";
                        var titleText = !string.IsNullOrEmpty(_storeName) 
                            ? $"{reportTitle} - {storeLabel}: {_storeName}" 
                            : reportTitle;
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
                        columns.ConstantColumn(35);   // م
                        columns.RelativeColumn(3);    // اسم الصنف
                        columns.ConstantColumn(70);   // الكود
                        columns.RelativeColumn(2);    // المخزن
                        columns.ConstantColumn(60);   // الكمية
                        columns.ConstantColumn(90);   // آخر حركة
                        columns.ConstantColumn(70);   // مدة الركود
                        columns.RelativeColumn(2);    // السبب
                        columns.RelativeColumn(2);    // الحالة
                    });

                    // Header row
                    table.Header(header =>
                    {
                        var headers = _isRtl 
                            ? new[] { "م", "اسم الصنف", "الكود", "المخزن", "الكمية", "آخر حركة", "مدة الركود", "السبب", "الحالة" }
                            : new[] { "#", "Product", "Code", "Store", "Qty", "Last Move", "Stagnant Days", "Reason", "Status" };
                        
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

                        // م
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text((i + 1).ToString()).FontSize(8);
                        
                        // اسم الصنف
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.ProductName ?? "-").FontSize(8);
                        
                        // الكود
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.ProductCode ?? "-").FontSize(8);
                        
                        // المخزن
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.StoreName ?? "-").FontSize(8);
                        
                        // الكمية
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.Quantity.ToString("N2")).FontSize(8);
                        
                        // آخر حركة
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(item.LastMovementDate?.ToString("dd-MM-yyyy") ?? "-").FontSize(8);
                        
                        // مدة الركود (بالأيام)
                        var stagnantDays = GetStagnantDaysText(item);
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(stagnantDays).FontSize(8);
                        
                        // السبب
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(GetReasonText(item.Reason)).FontSize(8);
                        
                        // الحالة
                        var statusText = item.IsDisposed 
                            ? (_isRtl ? "تم التصرف" : "Disposed") 
                            : (_isRtl ? "راكد" : "Stagnant");
                        var statusColor = item.IsDisposed ? Colors.Green.Darken1 : Colors.Orange.Darken1;
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignCenter().Text(statusText).FontSize(8).FontColor(statusColor);
                    }

                    // Totals row
                    var totalLabel = _isRtl ? "الإجمالي" : "Total";
                    table.Cell().ColumnSpan(4).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(totalLabel).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(_items.Sum(x => x.Quantity).ToString("N2")).Bold().FontSize(10);

                    table.Cell().ColumnSpan(4).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text("").FontSize(10);
                });

                // Summary section
                col.Item().PaddingTop(15).Column(summaryCol =>
                {
                    var recordsLabel = _isRtl ? "إجمالي عدد السجلات" : "Total Records";
                    var quantityLabel = _isRtl ? "إجمالي كميات الرواكد" : "Total Stagnant Quantity";
                    var disposedLabel = _isRtl ? "تم التصرف فيها" : "Disposed Items";
                    var pendingLabel = _isRtl ? "في انتظار التصرف" : "Pending Items";
                    
                    var disposedCount = _items.Count(x => x.IsDisposed);
                    var pendingCount = _items.Count(x => !x.IsDisposed);
                    
                    summaryCol.Item().AlignRight().Text($"{recordsLabel}: {_items.Count}").FontSize(10);
                    summaryCol.Item().AlignRight().Text($"{quantityLabel}: {_items.Sum(x => x.Quantity):N2}").FontSize(10);
                    summaryCol.Item().AlignRight().Text($"{disposedLabel}: {disposedCount}").FontSize(10).FontColor(Colors.Green.Darken1);
                    summaryCol.Item().AlignRight().Text($"{pendingLabel}: {pendingCount}").FontSize(10).FontColor(Colors.Orange.Darken1);
                });
            });
        });
    }

    private string GetStagnantDaysText(StagnantInventoryResponseDto item)
    {
        if (item.LastMovementDate.HasValue)
        {
            var days = (int)(DateTime.UtcNow - item.LastMovementDate.Value).TotalDays;
            return _isRtl ? $"{days} يوم" : $"{days} days";
        }
        return "-";
    }

    private string GetReasonText(StagnantReason reason)
    {
        return reason switch
        {
            StagnantReason.Manual => _isRtl ? "يدوي" : "Manual",
            StagnantReason.AutoByPeriod => _isRtl ? "تلقائي بالفترة" : "Auto by Period",
            StagnantReason.OnEntryPermit => _isRtl ? "عند إذن الدخول" : "On Entry Permit",
            _ => "-"
        };
    }
}
