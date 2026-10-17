using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

public class NetSalesVATReportDocument : IDocument
{
    private readonly object _reportData;
    private readonly CompanyDetails _companyDetails;
    private readonly SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? _pdfTemplateConfig;
    private readonly string _language;

    public NetSalesVATReportDocument(
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
                    var totalSalesVAT = Convert.ToDecimal(dataType.GetProperty("totalSalesVAT")?.GetValue(_reportData) ?? 0m);
                    var totalReturnsVAT = Convert.ToDecimal(dataType.GetProperty("totalReturnsVAT")?.GetValue(_reportData) ?? 0m);
                    var netVAT = Convert.ToDecimal(dataType.GetProperty("netVAT")?.GetValue(_reportData) ?? 0m);

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
                            t.Span("تقرير صافي القيمة المضافة للمبيعات").SemiBold().FontSize(16);
                        });
                    });

                    col.Item().PaddingTop(20);

                    // Summary Table - Horizontal Layout
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1f);  // إجمالي القيمة المضافة للمبيعات
                            columns.RelativeColumn(1f);  // إجمالي القيمة المضافة للمرتجعات
                            columns.RelativeColumn(1f);  // صافي القيمة المضافة
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
                                .Text("إجمالي القيمة المضافة للمبيعات")
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
                                .Text("إجمالي القيمة المضافة للمرتجعات")
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
                                .Text("صافي القيمة المضافة")
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
                            .Text($"{totalSalesVAT:N2}")
                            .FontSize(11);

                        table.Cell()
                            .Background("#f4eaff")
                            .Border(1)
                            .BorderColor(Colors.Black)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"({totalReturnsVAT:N2})")
                            .FontSize(11)
                            .FontColor(Colors.Red.Medium);

                        table.Cell()
                            .Background(netVAT >= 0 ? "#C6EFCE" : "#FFC7CE")
                            .Border(1)
                            .BorderColor(netVAT >= 0 ? Colors.Green.Medium : Colors.Red.Medium)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{netVAT:N2}")
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(netVAT >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                    });

                    // Note
                    col.Item().PaddingTop(20).Text(t =>
                    {
                        t.Span("ملاحظة: ").SemiBold().FontSize(10);
                        t.Span("صافي القيمة المضافة = القيمة المضافة للمبيعات - القيمة المضافة للمرتجعات").FontSize(9).Italic();
                    });
                });
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in NetSalesVATReportDocument.Compose: {ex.Message}");
            throw;
        }
    }
}
