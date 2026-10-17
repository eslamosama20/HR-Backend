using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Notes.CreditNotes.DocumentUtils;

public class SalesCreditNoteVATReportDocument : IDocument
{
    private readonly List<object> _salesCreditNoteVATDetails;
    private readonly CompanyDetails _companyDetails;
    private readonly SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _language;

    public SalesCreditNoteVATReportDocument(
        List<object> salesCreditNoteVATDetails,
        CompanyDetails companyDetails,
        SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default,
        string language = "ar")
    {
        _salesCreditNoteVATDetails = salesCreditNoteVATDetails;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration();
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

                // Custom Header - Arabic details on right, Logo on left
                page.Header().Element(container =>
                {
                    container.PaddingBottom(10).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
                    {
                        // Column 1: Logo (Left)
                        row.RelativeItem(1f).AlignLeft().AlignMiddle().Column(col =>
                        {
                            if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                            {
                                col.Item().AlignLeft().Height(80).Width(80).Image(_companyDetails.Logo).FitArea();
                            }
                        });

                        // Column 2: Arabic Company Details (Right side - AlignRight)
                        row.RelativeItem(3f).AlignRight().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.ArabicName))
                            {
                                col.Item().AlignRight().Text(_companyDetails.ArabicName).SemiBold().FontSize(12);
                            }
                            if (_companyDetails?.ArabicAddress != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                                {
                                    col.Item().PaddingTop(3).AlignRight().Text(_companyDetails.ArabicAddress.Street).FontSize(9);
                                }
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                                {
                                    col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.City).FontSize(9);
                                }
                            }
                            var primaryPhone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                            {
                                col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(9)).Text(t =>
                                {
                                    t.Span("رقم الهاتف: ");
                                    t.Span(primaryPhone.PhoneNumber);
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                            {
                                col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(9)).Text(t =>
                                {
                                    t.Span("الرقم الضريبي: ");
                                    t.Span(_companyDetails.TaxNumber);
                                });
                            }
                        });
                    });
                });

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
                        t.Span("تقرير القيمة المضافة لمرتجعات المبيعات").SemiBold().FontSize(16);
                    });
                });

                // Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1f);    // رقم المرتجع
                        columns.RelativeColumn(1.5f);  // اسم العميل
                        columns.RelativeColumn(1.2f);  // تاريخ الإصدار
                        columns.RelativeColumn(1f);    // كود الصنف
                        columns.RelativeColumn(1.5f);  // اسم الصنف
                        columns.RelativeColumn(0.8f);  // الكمية
                        columns.RelativeColumn(1f);    // السعر
                        columns.RelativeColumn(1.2f);  // قيمة الضريبة
                    });

                    // Header Row
                    table.Header(header =>
                    {
                        void HeaderCell(string text)
                        {
                            header.Cell()
                                .Background(Colors.Blue.Lighten2)
                                .Border(0.5f)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(4)
                                .Text(text)
                                .FontSize(10)
                                .SemiBold()
                                .AlignCenter();
                        }

                        HeaderCell("رقم المرتجع");
                        HeaderCell("اسم العميل");
                        HeaderCell("تاريخ الإصدار");
                        HeaderCell("كود الصنف");
                        HeaderCell("اسم الصنف");
                        HeaderCell("الكمية");
                        HeaderCell("السعر");
                        HeaderCell("قيمة الضريبة");
                    });

                    // Data Rows
                    decimal totalTax = 0;
                    foreach (dynamic detail in _salesCreditNoteVATDetails)
                    {
                        void DataCell(string text, bool isBold = false)
                        {
                            var cell = table.Cell()
                                .Border(0.5f)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(4)
                                .Text(text)
                                .FontSize(9)
                                .AlignCenter();

                            if (isBold)
                                cell.SemiBold();
                        }

                        DataCell(detail.creditNoteNo?.ToString() ?? "");
                        DataCell(detail.customerName?.ToString() ?? "");
                        
                        DateTime issueDate = detail.issueDate;
                        DataCell(issueDate.ToString("dd/MM/yyyy"));
                        
                        DataCell(detail.itemNo?.ToString() ?? "");
                        DataCell(detail.itemName?.ToString() ?? "");
                        
                        decimal quantity = detail.quantity;
                        DataCell(quantity.ToString("N2"));
                        
                        decimal price = detail.price;
                        DataCell(price.ToString("N2"));
                        
                        decimal taxAmount = detail.taxAmount;
                        DataCell(taxAmount.ToString("N2"));
                        
                        totalTax += taxAmount;
                    }

                    // Total Row
                    table.Cell().ColumnSpan(7)
                        .Background(Colors.Grey.Lighten3)
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Medium)
                        .Padding(4)
                        .Text("إجمالي القيمة المضافة")
                        .FontSize(11)
                        .SemiBold()
                        .AlignCenter();

                    table.Cell()
                        .Background(Colors.Green.Lighten3)
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Medium)
                        .Padding(4)
                        .Text(totalTax.ToString("N2"))
                        .FontSize(11)
                        .SemiBold()
                        .AlignCenter();
                });
            });
        });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error composing SalesCreditNoteVATReportDocument: {ex.Message}");
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
            Console.WriteLine($"Error generating Sales Credit Note VAT PDF: {ex.Message}");
            throw;
        }
    }
}
