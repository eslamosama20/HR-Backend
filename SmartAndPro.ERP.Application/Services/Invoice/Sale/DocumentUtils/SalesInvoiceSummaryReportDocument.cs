using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

public class SalesInvoiceSummaryReportDocument : IDocument
{
    private readonly List<SalesInvoiceDetailsResponse> _salesInvoiceDetails;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _language;

    public SalesInvoiceSummaryReportDocument(
        List<SalesInvoiceDetailsResponse> salesInvoiceDetails,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default,
        string language = "ar")
    {
        _salesInvoiceDetails = salesInvoiceDetails;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
        _startDate = startDate;
        _endDate = endDate;
        _language = language?.ToLower() ?? "ar";
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        try
        {
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
                                        new PdfItem{ Id = "arabicName", Type = "field", FontSize = "14px", Align = "center" }
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
                // Header
                col.Item().PaddingBottom(10).Border(1).Padding(8).Row(r =>
                {
                    // التاريخ في أقصى الشمال (Left)
                    r.RelativeItem().AlignLeft().Column(c =>
                    {
                        if (_startDate != default && _endDate != default)
                        {
                            c.Item().Text(t =>
                            {
                                t.Span($"من: {_startDate:dd/MM/yyyy}").SemiBold();
                            });
                            c.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span($"إلى: {_endDate:dd/MM/yyyy}").SemiBold();
                            });
                        }
                    });
                    
                    // العنوان في المنتصف (Center)
                    r.RelativeItem().AlignCenter().Text(t =>
                    {
                        var title = _language == "ar" ? "تقرير القيمة المضافة للمبيعات المختصر" : "Sales VAT Summary Report";
                        t.Span(title).SemiBold().FontSize(16);
                    });
                });

                // Calculate totals
                decimal totalQuantity = 0;
                decimal totalUnitPrice = 0;
                decimal totalInvoicePrice = 0;
                decimal totalInvoicePriceWithTax = 0;

                foreach (var invoice in _salesInvoiceDetails)
                {
                    if (invoice.ItemDetails == null || !invoice.ItemDetails.Any())
                        continue;

                    foreach (var item in invoice.ItemDetails)
                    {
                        totalQuantity += item.Quantity;
                        totalUnitPrice += item.TotalPrice; // إجمالي سعر الوحدة
                    }

                    totalInvoicePrice += invoice.TotalInvoicePrice;
                    totalInvoicePriceWithTax += invoice.TotalInvoicePriceWithTax;
                }

                // Calculate VAT Amount
                decimal totalVatAmount = totalInvoicePriceWithTax - totalInvoicePrice;

                // Summary Table - Only totals
                col.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1f);  // الكمية / Quantity
                        columns.RelativeColumn(1.2f);  // إجمالي سعر الوحدة / Total Unit Price
                        columns.RelativeColumn(1f);  // الإجمالي / Total
                        columns.RelativeColumn(1.2f);  // إجمالي القيمة المضافة / Total VAT
                        columns.RelativeColumn(1.2f);  // الإجمالي بالضريبة / Total with Tax
                    });

                    // Header Row
                    table.Header(header =>
                    {
                        if (_language == "en")
                        {
                            // English order: Quantity, Total Unit Price, Total, Total with Tax, Total VAT
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Quantity");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total Unit Price");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total with Tax");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total VAT");
                        }
                        else
                        {
                            // Arabic order (reversed): إجمالي القيمة المضافة، الإجمالي بالضريبة، الإجمالي، إجمالي سعر الوحدة، إجمالي العدد
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("إجمالي القيمة المضافة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الإجمالي بالضريبة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الإجمالي");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("إجمالي سعر الوحدة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("إجمالي العدد");
                        }
                    });

                    // Data Row
                    if (_language == "en")
                    {
                        // English order: Quantity, Total Unit Price, Total, Total with Tax, Total VAT
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalQuantity.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalUnitPrice.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalInvoicePrice.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalInvoicePriceWithTax.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalVatAmount.ToString("0.##"));
                    }
                    else
                    {
                        // Arabic order (reversed): إجمالي القيمة المضافة، الإجمالي بالضريبة، الإجمالي، إجمالي سعر الوحدة، إجمالي العدد
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalVatAmount.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalInvoicePriceWithTax.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalInvoicePrice.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalUnitPrice.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(totalQuantity.ToString("0.##"));
                    }
                });
            });
            });
        }
        catch (Exception ex)
        {
            SharedKernel.Utilities.LogService.LogError($"Error in SalesInvoiceSummaryReportDocument.Compose: {ex.Message}");
            SharedKernel.Utilities.LogService.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                SharedKernel.Utilities.LogService.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    static IContainer HeaderStyle(IContainer c) => c.Background("#42acff").Border(1).BorderColor("#000000").Padding(4).DefaultTextStyle(t => t.SemiBold());
    static IContainer DataStyle(IContainer c) => c.Background("#f4eaff").Border(1).BorderColor("#000000").Padding(4);
}

