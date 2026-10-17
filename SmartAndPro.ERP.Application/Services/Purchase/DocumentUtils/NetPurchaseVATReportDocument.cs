using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Purchase.DocumentUtils;

public class NetPurchaseVATReportDocument : IDocument
{
    private readonly object _reportData;
    private readonly CompanyDetails _companyDetails;
    private readonly SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? _pdfTemplateConfig;
    private readonly string _language;

    public NetPurchaseVATReportDocument(
        object reportData,
        CompanyDetails companyDetails,
        SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        string language = "ar")
    {
        _reportData = reportData;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig;
        _language = language?.ToLower() ?? "ar";
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20f);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                PdfTemplateUtils.ApplyHeaderFooter(
                page,
                _pdfTemplateConfig,
                (pg, header) => pg.Header().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, header, _companyDetails)),
                (pg, footer) => pg.Footer().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, footer, _companyDetails)),
                new PdfHeaderFooterConfig
                {
                    Columns = new List<PdfColumnGroup>
                    {
                        new PdfColumnGroup
                        {
                            Rows = new List<PdfRowGroup>
                            {
                                new PdfRowGroup
                                {
                                    Items = new List<PdfItem>
                                    {
                                        new PdfItem{ Id = "logo", Type = "field", FontSize = "inherit", Align = "center" },
                                        new PdfItem{ Id = "englishName", Type = "field", FontSize = "14px", Align = "center" }
                                    }
                                }
                            }
                        }
                    }
                },
                new PdfHeaderFooterConfig
                {
                    Columns = new List<PdfColumnGroup>
                    {
                        new PdfColumnGroup
                        {
                            Rows = new List<PdfRowGroup>
                            {
                                new PdfRowGroup
                                {
                                    Items = new List<PdfItem>
                                    {
                                        new PdfItem{ Id = "primaryPhone", Type = "field", FontSize = "inherit", Align = "center" },
                                        new PdfItem{ Id = "email", Type = "field", FontSize = "inherit", Align = "center" },
                                        new PdfItem{ Id = "website", Type = "field", FontSize = "inherit", Align = "right" }
                                    }
                                }
                            }
                        }
                    }
                }
            );

            page.Content().Column(col =>
                {
                    // Get data using reflection
                    var dataType = _reportData.GetType();
                    var startDate = dataType.GetProperty("startDate")?.GetValue(_reportData)?.ToString() ?? "";
                    var endDate = dataType.GetProperty("endDate")?.GetValue(_reportData)?.ToString() ?? "";
                    var totalPurchasesVAT = Convert.ToDecimal(dataType.GetProperty("totalPurchasesVAT")?.GetValue(_reportData) ?? 0m);
                    var totalReturnsVAT = Convert.ToDecimal(dataType.GetProperty("totalReturnsVAT")?.GetValue(_reportData) ?? 0m);
                    var totalVAT = Convert.ToDecimal(dataType.GetProperty("totalVAT")?.GetValue(_reportData) ?? 0m);

                    // Title
                    col.Item().PaddingVertical(10).AlignCenter().Text(text =>
                    {
                        text.Span(_language == "ar" 
                            ? "تقرير صافي القيمة المضافة للمشتريات" 
                            : "Net Purchase VAT Report")
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                    });

                    // Date Range
                    col.Item().PaddingBottom(10).AlignCenter().Text(text =>
                    {
                        var dateText = _language == "ar"
                            ? $"من {startDate} إلى {endDate}"
                            : $"From {startDate} To {endDate}";
                        text.Span(dateText).FontSize(11).FontColor(Colors.Grey.Darken1);
                    });

                    // Spacing
                    col.Item().PaddingBottom(15);

                    // Summary Table - Horizontal Layout (3 columns)
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1f); // إجمالي القيمة المضافة للمشتريات
                            columns.RelativeColumn(1.2f); // إجمالي القيمة المضافة لمرتجعات المشتريات
                            columns.RelativeColumn(1f); // صافي القيمة المضافة للمشتريات
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell()
                                .Background("#42acff")
                                .Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(8)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(_language == "ar" 
                                    ? "إجمالي القيمة المضافة للمشتريات"
                                    : "Total Purchases VAT")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor(Colors.White);

                            header.Cell()
                                .Background("#42acff")
                                .Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(8)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(_language == "ar" 
                                    ? "إجمالي القيمة المضافة لمرتجعات المشتريات"
                                    : "Total Purchase Returns VAT")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor(Colors.White);

                            header.Cell()
                                .Background("#42acff")
                                .Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(8)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(_language == "ar" 
                                    ? "صافي القيمة المضافة للمشتريات"
                                    : "Net Purchase VAT")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor(Colors.White);
                        });

                        // Data Row
                        table.Cell()
                            .Background("#f4eaff")
                            .Border(1)
                            .BorderColor(Colors.Black)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{totalPurchasesVAT:N2}")
                            .FontSize(11);

                        table.Cell()
                            .Background("#f4eaff")
                            .Border(1)
                            .BorderColor(Colors.Black)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{totalReturnsVAT:N2}")
                            .FontSize(11);

                        table.Cell()
                            .Background(totalVAT >= 0 ? "#E6F3FF" : "#FFC7CE")
                            .Border(1)
                            .BorderColor(totalVAT >= 0 ? Colors.Blue.Medium : Colors.Red.Medium)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{totalVAT:N2}")
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(totalVAT >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                    });

                    // Note
                    col.Item().PaddingTop(20).PaddingHorizontal(10).Text(text =>
                    {
                        text.Span(_language == "ar" 
                            ? "ملاحظة: " 
                            : "Note: ")
                            .Bold()
                            .FontSize(9);
                        text.Span(_language == "ar"
                            ? "صافي القيمة المضافة = إجمالي ضريبة المشتريات - إجمالي ضريبة المرتجعات"
                            : "Net VAT = Total Purchases VAT - Total Returns VAT")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    // Generation timestamp
                    col.Item().PaddingTop(30).AlignRight().Text(text =>
                    {
                        var timestamp = _language == "ar"
                            ? $"تم الإنشاء: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"
                            : $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                        text.Span(timestamp).FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error composing Net Purchase VAT Report PDF: {ex.Message}");
            throw;
        }
    }
}
