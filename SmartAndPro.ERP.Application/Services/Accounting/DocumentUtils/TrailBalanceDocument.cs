using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TrailBalance;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Accounting.DocumentUtils;

public class TrailBalanceDocument : IDocument
{
    private readonly List<TrailBalanceResponseRootDto> _trailBalanceData;
    private readonly CompanyDetails _companyDetails;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _level;

    public TrailBalanceDocument(
        List<TrailBalanceResponseRootDto> trailBalanceData,
        CompanyDetails companyDetails,
        DateOnly startDate,
        DateOnly endDate,
        string level)
    {
        _trailBalanceData = trailBalanceData;
        _companyDetails = companyDetails;
        _startDate = startDate;
        _endDate = endDate;
        _level = level;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.PageColor(Colors.White);
            page.Margin(20);
            page.Size(PageSizes.A4.Landscape());
            page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

            // Header
            page.Header().Column(col =>
            {
                // Company Logo and Name
                if (_companyDetails.Logo != null)
                {
                    col.Item().AlignCenter().Width(80).Height(80).Image(_companyDetails.Logo);
                }
                col.Item().AlignCenter().Text(_companyDetails.ArabicName).FontSize(16).Bold();
                col.Item().AlignCenter().Text(_companyDetails.EnglishName).FontSize(12);
                
                col.Item().PaddingTop(10).AlignCenter().Text("ميزان المراجعة").FontSize(14).Bold();
                col.Item().AlignCenter().Text("Trail Balance").FontSize(12).Bold();
                
                col.Item().PaddingTop(5).AlignCenter().Text($"من {_startDate:dd/MM/yyyy} إلى {_endDate:dd/MM/yyyy}").FontSize(10);
                col.Item().AlignCenter().Text($"From {_startDate:dd/MM/yyyy} To {_endDate:dd/MM/yyyy}").FontSize(9);
                col.Item().AlignCenter().Text($"المستوى: {GetLevelText(_level)}").FontSize(9);
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
                col.Item().PaddingTop(10).Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80);  // Account Code
                        columns.RelativeColumn(3);   // Account Name
                        columns.ConstantColumn(90);  // Previous Debit
                        columns.ConstantColumn(90);  // Previous Credit
                        columns.ConstantColumn(90);  // Period Debit
                        columns.ConstantColumn(90);  // Period Credit
                        columns.ConstantColumn(90);  // Balance
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("رقم الحساب").Bold().FontSize(9);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("اسم الحساب").Bold().FontSize(9);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("السابق مدين").Bold().FontSize(9);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("السابق دائن").Bold().FontSize(9);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الفترة مدين").Bold().FontSize(9);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الفترة دائن").Bold().FontSize(9);
                        
                        header.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium)
                            .Padding(5).AlignCenter().Text("الرصيد").Bold().FontSize(9);
                    });

                    // Flatten the hierarchical data based on level
                    var flattenedData = FlattenTrailBalanceData(_trailBalanceData, _level);

                    // Data rows
                    foreach (var item in flattenedData)
                    {
                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(item.AccountNumber ?? "-").FontSize(9);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignRight().Text(item.AccountArabicName ?? "-").FontSize(9);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(FormatCurrency(item.PreviousDebit)).FontSize(9);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(FormatCurrency(item.PreviousCredit)).FontSize(9);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(FormatCurrency(item.PeriodDebit)).FontSize(9);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(FormatCurrency(item.PeriodCredit)).FontSize(9);

                        table.Cell().Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).AlignCenter().Text(FormatCurrency(item.Balance)).FontSize(9);
                    }

                    // Totals row
                    var totalPreviousDebit = flattenedData.Sum(x => x.PreviousDebit);
                    var totalPreviousCredit = flattenedData.Sum(x => x.PreviousCredit);
                    var totalPeriodDebit = flattenedData.Sum(x => x.PeriodDebit);
                    var totalPeriodCredit = flattenedData.Sum(x => x.PeriodCredit);
                    var totalBalance = flattenedData.Sum(x => x.Balance);

                    table.Cell().ColumnSpan(2).Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text("الإجمالي / Total").Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatCurrency(totalPreviousDebit)).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatCurrency(totalPreviousCredit)).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatCurrency(totalPeriodDebit)).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatCurrency(totalPeriodCredit)).Bold().FontSize(10);

                    table.Cell().Background(Colors.Blue.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(5).AlignCenter().Text(FormatCurrency(totalBalance)).Bold().FontSize(10);
                });

                // Summary section
                col.Item().PaddingTop(20).Column(summary =>
                {
                    var flattenedData = FlattenTrailBalanceData(_trailBalanceData, _level);
                    summary.Item().Text($"إجمالي عدد الحسابات: {flattenedData.Count}").FontSize(10);
                    summary.Item().Text($"Total Accounts: {flattenedData.Count}").FontSize(9);
                });
            });
        });
    }

    private string FormatCurrency(decimal amount)
    {
        return amount == 0 ? "-" : amount.ToString("N2");
    }

    private string GetLevelText(string level)
    {
        return level?.ToLower() switch
        {
            "root" or "first" => "المستوى الأول (Root)",
            "header" or "second" => "المستوى الثاني (Header)",
            "detail" or "third" => "المستوى الثالث (Detail)",
            "main" or "fourth" => "المستوى الرابع (Main)",
            "all" => "جميع المستويات (All)",
            _ => level ?? "جميع المستويات"
        };
    }

    private class FlatAccountData
    {
        public string AccountNumber { get; set; }
        public string AccountArabicName { get; set; }
        public string AccountEnglishName { get; set; }
        public decimal PreviousDebit { get; set; }
        public decimal PreviousCredit { get; set; }
        public decimal PeriodDebit { get; set; }
        public decimal PeriodCredit { get; set; }
        public decimal Balance { get; set; }
    }

    private List<FlatAccountData> FlattenTrailBalanceData(List<TrailBalanceResponseRootDto> data, string level)
    {
        var result = new List<FlatAccountData>();
        
        var levelLower = level?.ToLower() ?? "all";
        
        // Normalize frontend values to backend values
        var normalizedLevel = levelLower switch
        {
            "first" => "root",
            "second" => "header",
            "third" => "detail",
            "fourth" => "main",
            _ => levelLower
        };
        
        foreach (var root in data)
        {
            // Add root level if requested
            if (normalizedLevel == "root" || normalizedLevel == "all")
            {
                result.Add(new FlatAccountData
                {
                    AccountNumber = root.AccountNumber,
                    AccountArabicName = root.AccountArabicName,
                    AccountEnglishName = root.AccountEnglishName,
                    PreviousDebit = root.PreviousDebit,
                    PreviousCredit = root.PreviousCredit,
                    PeriodDebit = root.PeriodDebit,
                    PeriodCredit = root.PeriodCredit,
                    Balance = root.Balance
                });
            }

            // Process headers
            if (root.Headers != null && (normalizedLevel == "header" || normalizedLevel == "detail" || normalizedLevel == "main" || normalizedLevel == "all"))
            {
                foreach (var header in root.Headers)
                {
                    if (normalizedLevel == "header" || normalizedLevel == "all")
                    {
                        result.Add(new FlatAccountData
                        {
                            AccountNumber = header.AccountNumber,
                            AccountArabicName = header.AccountArabicName,
                            AccountEnglishName = header.AccountEnglishName,
                            PreviousDebit = header.PreviousDebit,
                            PreviousCredit = header.PreviousCredit,
                            PeriodDebit = header.PeriodDebit,
                            PeriodCredit = header.PeriodCredit,
                            Balance = header.Balance
                        });
                    }

                    // Process details
                    if (header.Details != null && (normalizedLevel == "detail" || normalizedLevel == "main" || normalizedLevel == "all"))
                    {
                        foreach (var detail in header.Details)
                        {
                            if (normalizedLevel == "detail" || normalizedLevel == "all")
                            {
                                result.Add(new FlatAccountData
                                {
                                    AccountNumber = detail.AccountNumber,
                                    AccountArabicName = detail.AccountArabicName,
                                    AccountEnglishName = detail.AccountEnglishName,
                                    PreviousDebit = detail.PreviousDebit,
                                    PreviousCredit = detail.PreviousCredit,
                                    PeriodDebit = detail.PeriodDebit,
                                    PeriodCredit = detail.PeriodCredit,
                                    Balance = detail.Balance
                                });
                            }

                            // Process mains (Fourth level)
                            if (detail.Mains != null && (normalizedLevel == "main" || normalizedLevel == "all"))
                            {
                                foreach (var main in detail.Mains)
                                {
                                    result.Add(new FlatAccountData
                                    {
                                        AccountNumber = main.AccountNumber,
                                        AccountArabicName = main.AccountArabicName,
                                        AccountEnglishName = main.AccountEnglishName,
                                        PreviousDebit = main.PreviousDebit,
                                        PreviousCredit = main.PreviousCredit,
                                        PeriodDebit = main.PeriodDebit,
                                        PeriodCredit = main.PeriodCredit,
                                        Balance = main.Balance
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
}
