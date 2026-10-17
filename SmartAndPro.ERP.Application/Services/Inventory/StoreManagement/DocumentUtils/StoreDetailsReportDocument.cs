using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.StoreManagement.DocumentUtils;

public class StoreDetailsReportDocument : IDocument
{
    private readonly List<StoreDetails> _items;
    private readonly CompanyDetails _companyDetails;
    private readonly string _language;
    private readonly bool _isRtl;
    private readonly string? _printedBy;

    public StoreDetailsReportDocument(
        List<StoreDetails> items,
        CompanyDetails companyDetails,
        string language = "ar",
        string? printedBy = null)
    {
        _items = items ?? new List<StoreDetails>();
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
                var reportTitle = _isRtl ? "تقرير المخازن" : "Stores Report";
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
                var totalStores = _items.Count;
                var totalWarehouses = _items.Count(x => x.StoreType == StoreType.Warehouse);
                var totalShops = _items.Count(x => x.StoreType == StoreType.Store);

                var totalLabel = _isRtl ? "إجمالي المخازن" : "Total Stores";
                var warehousesLabel = _isRtl ? "المستودعات" : "Warehouses";
                var shopsLabel = _isRtl ? "المتاجر" : "Shops";

                // Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);   // #
                        columns.RelativeColumn(2);    // Arabic Name
                        columns.RelativeColumn(2);    // English Name
                        columns.RelativeColumn(1);    // Type
                        columns.RelativeColumn(1.5f); // Status
                        columns.RelativeColumn(1);    // Has Account
                        columns.RelativeColumn(1.5f); // Created At
                    });

                    // Header - matching DamagedInventoryReportDocument style
                    table.Header(header =>
                    {
                        var headers = _isRtl 
                            ? new[] { "#", "الاسم بالعربية", "الاسم بالإنجليزية", "النوع", "الحالة", "له حساب", "تاريخ الإنشاء" }
                            : new[] { "#", "Arabic Name", "English Name", "Type", "Status", "Has Account", "Created At" };

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

                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(index.ToString()).FontSize(9);
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.ArabicName ?? "-").FontSize(9);
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.EnglishName ?? "-").FontSize(9);
                        
                        var typeText = item.StoreType == StoreType.Warehouse 
                            ? (_isRtl ? "مستودع" : "Warehouse") 
                            : (_isRtl ? "متجر" : "Shop");
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(typeText).FontSize(9);
                        
                        var statusText = item.StoreStatus != null 
                            ? (_isRtl ? item.StoreStatus.ArabicName : item.StoreStatus.EnglishName) 
                            : "-";
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(statusText ?? "-").FontSize(9);
                        
                        var hasAccountText = item.MainAccountId != null 
                            ? (_isRtl ? "نعم" : "Yes") 
                            : (_isRtl ? "لا" : "No");
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(hasAccountText).FontSize(9);
                        
                        table.Cell().Background(rowColor).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text(item.CreatedAt.ToString("dd-MM-yyyy")).FontSize(9);

                        index++;
                    }
                });

                // Empty message
                if (!_items.Any())
                {
                    var noDataMessage = _isRtl ? "لا توجد مخازن" : "No stores found";
                    col.Item().PaddingTop(20).AlignCenter().Text(noDataMessage).FontSize(12).FontColor(Colors.Grey.Medium);
                }

                // Total at the end (after table) - matching DamagedInventoryReportDocument style
                col.Item().PaddingTop(10).Table(totalsTable =>
                {
                    totalsTable.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    totalsTable.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text($"{totalLabel}: {totalStores}").FontSize(10).Bold();
                    totalsTable.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text($"{warehousesLabel}: {totalWarehouses}").FontSize(10).Bold();
                    totalsTable.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium).Padding(5).AlignCenter().Text($"{shopsLabel}: {totalShops}").FontSize(10).Bold();
                });
            });
        });
    }

    public byte[] GeneratePDF()
    {
        return Document.Create(Compose).GeneratePdf();
    }
}
