using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Return.Purchase.DocumentUtils;

public class PurchaseReturnVATReportDocument : IDocument
{
    private readonly List<object> _purchaseReturnVATDetails;
    private readonly CompanyDetails _companyDetails;
    private readonly SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _language;

    public PurchaseReturnVATReportDocument(
        List<object> purchaseReturnVATDetails,
        CompanyDetails companyDetails,
        SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default,
        string language = "ar")
    {
        _purchaseReturnVATDetails = purchaseReturnVATDetails;
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
                        t.Span("تقرير القيمة المضافة لمرتجعات المشتريات").SemiBold().FontSize(16);
                    });
                });

                // Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1f);    // رقم المرتجع
                        columns.RelativeColumn(1.5f);  // اسم المورد
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
                                .Background(Colors.Orange.Lighten2)
                                .Border(0.5f)
                                .BorderColor(Colors.Orange.Darken1)
                                .Padding(4)
                                .Text(text)
                                .SemiBold()
                                .FontSize(9);
                        }

                        HeaderCell("رقم المرتجع");
                        HeaderCell("اسم المورد");
                        HeaderCell("تاريخ الإصدار");
                        HeaderCell("كود الصنف");
                        HeaderCell("اسم الصنف");
                        HeaderCell("الكمية");
                        HeaderCell("السعر");
                        HeaderCell("قيمة الضريبة");
                    });

                    // Data Rows
                    decimal totalVAT = 0;
                    foreach (var item in _purchaseReturnVATDetails)
                    {
                        var itemType = item.GetType();
                        var returnNo = itemType.GetProperty("returnNo")?.GetValue(item)?.ToString() ?? "";
                        var supplierName = itemType.GetProperty("supplierName")?.GetValue(item)?.ToString() ?? "";
                        var issueDate = itemType.GetProperty("issueDate")?.GetValue(item);
                        var itemNo = itemType.GetProperty("itemNo")?.GetValue(item)?.ToString() ?? "";
                        var itemName = itemType.GetProperty("itemName")?.GetValue(item)?.ToString() ?? "";
                        var quantity = Convert.ToDecimal(itemType.GetProperty("quantity")?.GetValue(item) ?? 0);
                        var price = Convert.ToDecimal(itemType.GetProperty("price")?.GetValue(item) ?? 0);
                        var taxAmount = Convert.ToDecimal(itemType.GetProperty("taxAmount")?.GetValue(item) ?? 0);

                        totalVAT += taxAmount;

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

                        DataCell(returnNo);
                        DataCell(supplierName);
                        DataCell(issueDate != null ? ((DateTime)issueDate).ToString("dd/MM/yyyy") : "");
                        DataCell(itemNo);
                        DataCell(itemName);
                        DataCell(quantity.ToString("N2"), true);
                        DataCell(price.ToString("N2"), true);
                        DataCell(taxAmount.ToString("N2"), true);
                    }
                });

                // Totals
                decimal total = 0;
                foreach (var item in _purchaseReturnVATDetails)
                {
                    var itemType = item.GetType();
                    var taxAmount = Convert.ToDecimal(itemType.GetProperty("taxAmount")?.GetValue(item) ?? 0);
                    total += taxAmount;
                }
                
                col.Item().PaddingTop(10).AlignRight().Text(t =>
                {
                    t.Span("إجمالي القيمة المضافة: ").SemiBold();
                    t.Span(total.ToString("N2")).FontColor(Colors.Orange.Darken2).SemiBold();
                });
            });
        });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error composing Purchase Return VAT report document: {ex.Message}");
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
            Console.WriteLine($"Error generating Purchase Return VAT PDF: {ex.Message}");
            throw;
        }
    }
}
