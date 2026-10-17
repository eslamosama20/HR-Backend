using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Invoice.Purchase.DocumentUtils;

public class PurchaseVATReportDocument : IDocument
{
    private readonly List<PurchaseVATDetailsResponse> _purchaseVATDetails;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration? _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _language;

    public PurchaseVATReportDocument(
        List<PurchaseVATDetailsResponse> purchaseVATDetails,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default,
        string language = "ar")
    {
        _purchaseVATDetails = purchaseVATDetails;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig;
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
                // Header
                col.Item().PaddingBottom(10).Border(1).Padding(8).Row(r =>
                {
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
                    
                    r.RelativeItem().AlignCenter().Text(t =>
                    {
                        t.Span("تقرير القيمة المضافة للمشتريات").SemiBold().FontSize(16);
                    });
                });

                // Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        if (_language == "ar")
                        {
                            columns.RelativeColumn(1.5f);  // قيمة القيمة المضافة
                            columns.RelativeColumn(0.8f);  // العدد
                            columns.RelativeColumn(1.8f);  // اسم الصنف
                            columns.RelativeColumn(1f);    // كود الصنف
                            columns.RelativeColumn(1f);    // معفاة من الضرائب
                            columns.RelativeColumn(1.5f);  // تاريخ الفاتورة
                            columns.RelativeColumn(1.5f);  // اسم المورد
                            columns.RelativeColumn(1f);    // رقم الفاتورة
                        }
                        else
                        {
                            columns.RelativeColumn(1f);    // Invoice No
                            columns.RelativeColumn(1.5f);  // Supplier Name
                            columns.RelativeColumn(1.5f);  // Invoice Date
                            columns.RelativeColumn(1f);    // Tax Exempted
                            columns.RelativeColumn(1f);    // Item Code
                            columns.RelativeColumn(1.8f);  // Item Name
                            columns.RelativeColumn(0.8f);  // Quantity
                            columns.RelativeColumn(1.5f);  // VAT Amount
                        }
                    });

                    // Header Row
                    table.Header(header =>
                    {
                        void HeaderCell(string text)
                        {
                            header.Cell()
                                .Background(Colors.Orange.Lighten2)
                                .Border(0.5f)
                                .BorderColor(Colors.Orange.Darken1)
                                .Padding(4)
                                .Text(text)
                                .SemiBold()
                                .FontSize(9);
                        }

                        if (_language == "ar")
                        {
                            HeaderCell("قيمة القيمة المضافة");
                            HeaderCell("العدد");
                            HeaderCell("اسم الصنف");
                            HeaderCell("كود الصنف");
                            HeaderCell("معفاة من الضرائب");
                            HeaderCell("تاريخ الفاتورة");
                            HeaderCell("اسم المورد");
                            HeaderCell("رقم الفاتورة");
                        }
                        else
                        {
                            HeaderCell("Invoice No");
                            HeaderCell("Supplier Name");
                            HeaderCell("Invoice Date");
                            HeaderCell("Tax Exempted");
                            HeaderCell("Item Code");
                            HeaderCell("Item Name");
                            HeaderCell("Quantity");
                            HeaderCell("VAT Amount");
                        }
                    });

                    // Data Rows
                    foreach (var invoice in _purchaseVATDetails)
                    {
                        foreach (var item in invoice.Items)
                        {
                            void DataCell(string text, bool isNumeric = false)
                            {
                                table.Cell()
                                    .Border(0.5f)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(4)
                                    .AlignMiddle()
                                    .Text(text)
                                    .FontSize(9);
                            }

                            if (_language == "ar")
                            {
                                DataCell(item.VATAmount.ToString("N2"), true);
                                DataCell(item.Quantity.ToString("N2"), true);
                                DataCell(item.ItemArabicName);
                                DataCell(item.ReferenceCode);
                                DataCell(item.IsTaxExempted ? "نعم" : "لا");
                                DataCell(invoice.IssueDate.ToString("dd/MM/yyyy"));
                                DataCell(invoice.SuppilerArabicName);
                                DataCell(invoice.InvoiceNo);
                            }
                            else
                            {
                                DataCell(invoice.InvoiceNo);
                                DataCell(invoice.SuppilerEnglishName);
                                DataCell(invoice.IssueDate.ToString("dd/MM/yyyy"));
                                DataCell(item.IsTaxExempted ? "Yes" : "No");
                                DataCell(item.ReferenceCode);
                                DataCell(item.ItemEnglishName);
                                DataCell(item.Quantity.ToString("N2"), true);
                                DataCell(item.VATAmount.ToString("N2"), true);
                            }
                        }
                    }
                });

                // Totals
                var totalVAT = _purchaseVATDetails.Sum(inv => inv.Items.Sum(item => item.VATAmount));
                col.Item().PaddingTop(10).AlignRight().Text(t =>
                {
                    t.Span("إجمالي القيمة المضافة: ").SemiBold();
                    t.Span(totalVAT.ToString("N2")).FontColor(Colors.Orange.Darken2).SemiBold();
                });
            });
        });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error composing Purchase VAT report document: {ex.Message}");
            throw;
        }
    }

    public byte[] GeneratePdf()
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            return Document.Create(container => Compose(container)).GeneratePdf();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating Purchase VAT PDF: {ex.Message}");
            throw;
        }
    }
}
