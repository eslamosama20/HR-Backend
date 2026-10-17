using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Invoice.DocumentUtils;

public class NetDueVATReportDocument : IDocument
{
    private readonly object _reportData;
    private readonly CompanyDetails _companyDetails;
    private readonly SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? _pdfTemplateConfig;
    private readonly string _language;

    public NetDueVATReportDocument(
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
                    var netSalesVAT = Convert.ToDecimal(dataType.GetProperty("netSalesVAT")?.GetValue(_reportData) ?? 0m);
                    var netPurchasesVAT = Convert.ToDecimal(dataType.GetProperty("netPurchasesVAT")?.GetValue(_reportData) ?? 0m);
                    var dueVAT = Convert.ToDecimal(dataType.GetProperty("dueVAT")?.GetValue(_reportData) ?? 0m);

                    // Header
                    col.Item().PaddingBottom(10).Border(1).Padding(8).Row(r =>
                    {
                        r.RelativeItem().AlignLeft().Column(c =>
                        {
                            c.Item().Text(t =>
                            {
                                t.Span($"من: {startDate}").SemiBold();
                            });
                            c.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span($"إلى: {endDate}").SemiBold();
                            });
                        });
                        
                        r.RelativeItem().AlignCenter().Text(t =>
                        {
                            t.Span("تقرير القيمة المضافة المستحقة").SemiBold().FontSize(16);
                        });
                    });

                    col.Item().PaddingTop(20);

                    // Summary Table - Horizontal Layout (3 columns)
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1f);  // صافي القيمة المضافة للمبيعات
                            columns.RelativeColumn(1f);  // صافي القيمة المضافة للمشتريات
                            columns.RelativeColumn(1f);  // القيمة المضافة المستحقة
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
                                    ? "صافي القيمة المضافة للمبيعات"
                                    : "Net Sales VAT")
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
                                    : "Net Purchases VAT")
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
                                    ? "القيمة المضافة المستحقة"
                                    : "Due VAT")
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
                            .Text($"{netSalesVAT:N2}")
                            .FontSize(11);

                        table.Cell()
                            .Background("#f4eaff")
                            .Border(1)
                            .BorderColor(Colors.Black)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{netPurchasesVAT:N2}")
                            .FontSize(11);

                        table.Cell()
                            .Background(dueVAT >= 0 ? "#E6F3FF" : "#FFC7CE")
                            .Border(1)
                            .BorderColor(dueVAT >= 0 ? Colors.Blue.Medium : Colors.Red.Medium)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{dueVAT:N2}")
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(dueVAT >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                    });

                    // Note
                    col.Item().PaddingTop(20).Text(t =>
                    {
                        t.Span("ملاحظة: ").SemiBold().FontSize(10);
                        t.Span("القيمة المضافة المستحقة = صافي القيمة المضافة للمبيعات - صافي القيمة المضافة للمشتريات").FontSize(9).Italic();
                    });
                });
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in NetDueVATReportDocument.Compose: {ex.Message}");
            throw;
        }
    }
}
