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

public class PurchaseInvoiceReportDocument : IDocument
{
    private readonly List<PurchaseInvoiceDetailsResponse> _purchaseInvoiceDetails;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _language;

    public PurchaseInvoiceReportDocument(
        List<PurchaseInvoiceDetailsResponse> purchaseInvoiceDetails,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default,
        string language = "ar")
    {
        _purchaseInvoiceDetails = purchaseInvoiceDetails;
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

                // Apply Footer from PDF Template Configuration
                PdfTemplateUtils.ApplyHeaderFooter(
                    page,
                    _pdfTemplateConfig,
                    (pg, header) => { }, // Skip header - we use custom header
                    (pg, footer) => pg.Footer().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, footer, _companyDetails)),
                    null, // No default header
                    null  // No default footer - use from config only
                );

            page.Content().PaddingTop(10).Column(col =>
            {
                // Header
                col.Item().PaddingBottom(10).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Row(r =>
                {
                    // التاريخ في أقصى الشمال (Left)
                    r.RelativeItem().AlignLeft().Column(c =>
                    {
                        if (_startDate != default && _endDate != default)
                        {
                            c.Item().Text(t =>
                            {
                                t.Span($"من: {_startDate:dd/MM/yyyy}").SemiBold().FontSize(10);
                            });
                            c.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span($"إلى: {_endDate:dd/MM/yyyy}").SemiBold().FontSize(10);
                            });
                        }
                    });
                    
                    // العنوان في المنتصف (Center)
                    r.RelativeItem().AlignCenter().AlignMiddle().Text(t =>
                    {
                        t.Span("تقرير القيمة المضافة لأصناف المشتريات").SemiBold().FontSize(14);
                    });

                    // مساحة فارغة في اليمين للتوازن
                    r.RelativeItem().AlignRight().Text("");
                });

                // Table
                col.Item().Table(table =>
                {
                    // Column definitions: بيانات الفاتورة (4 columns) + الاصناف (6 columns)
                    // RTL order: من اليمين لليسار - نفس ترتيب HTML
                    table.ColumnsDefinition(columns =>
                    {
                        if (_language == "ar")
                        {
                            // Arabic - LTR order with adjusted widths
                            columns.RelativeColumn(1f);  // الاجمالي بالضريبة (Total with Tax)
                            columns.RelativeColumn(1.2f);  // الاجمالي (Total) - زيادة
                            columns.RelativeColumn(0.9f);  // سعر الوحدة (Unit Price) - زيادة
                            columns.RelativeColumn(0.7f);  // العدد (Quantity) - تقليل
                            columns.RelativeColumn(1.5f);  // اسم الصنف (Item Name)
                            columns.RelativeColumn(1f);  // كود الصنف (Item Code)
                            columns.RelativeColumn(1.6f); // تاريخ الفاتورة (Invoice Date) - توسيع أكثر
                            columns.RelativeColumn(1.5f);  // الوصف (Description)
                            columns.RelativeColumn(1.5f);  // اسم المورد (Supplier Name)
                            columns.RelativeColumn(1.2f); // رقم الفاتورة (Invoice No)
                        }
                        else
                        {
                            // English - RTL order: نفس الأعمدة
                            columns.RelativeColumn(1.2f); // رقم الفاتورة (Invoice No) - أقصى اليمين
                            columns.RelativeColumn(1.5f);  // اسم المورد (Supplier Name)
                            columns.RelativeColumn(1.5f);  // الوصف (Description)
                            columns.RelativeColumn(1.2f); // تاريخ الفاتورة (Invoice Date)
                            columns.RelativeColumn(1f);  // كود الصنف (Item Code)
                            columns.RelativeColumn(1.4f);  // اسم الصنف (Item Name)
                            columns.RelativeColumn(1f);  // العدد (Quantity) - تكبير
                            columns.RelativeColumn(0.8f);  // سعر الوحدة (Unit Price)
                            columns.RelativeColumn(1f);  // الاجمالي (Total)
                            columns.RelativeColumn(1f);  // الاجمالي بالضريبة (Total with Tax) - أقصى اليسار
                        }
                    });

                    // Header Row - All columns
                    table.Header(header =>
                    {
                        if (_language == "en")
                        {
                            // English headers - RTL order (من اليمين لليسار) - نفس ترتيب العربي
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Invoice No");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Supplier Name");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Description");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Invoice Date");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Item Code");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Item Name");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Quantity");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Unit Price");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total with Tax");
                        }
                        else
                    {
                            // Arabic headers - LTR order (من اليسار لليمين) - نفس ترتيب الإنجليزي
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الاجمالي بالضريبة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الاجمالي");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("سعر الوحدة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("العدد");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("اسم الصنف");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("كود الصنف");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("تاريخ الفاتورة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الوصف");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("اسم المورد");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("رقم الفاتوره");
                        }
                    });

                    // Group Header Row (بيانات الفاتورة / الاصناف) - صف عادي بعد Header مباشرة
                    if (_language == "en")
                    {
                        // English - RTL order: بيانات الفاتورة أولاً (من اليمين)، ثم الاصناف
                        table.Cell().ColumnSpan((uint)4).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Invoice Data");
                        table.Cell().ColumnSpan((uint)6).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Items");
                    }
                    else
                    {
                        // Arabic - LTR order: الاصناف أولاً (من اليسار)، ثم بيانات الفاتورة
                        table.Cell().ColumnSpan((uint)6).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الاصناف");
                        table.Cell().ColumnSpan((uint)4).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("بيانات الفاتورة");
                    }

                    // Calculate totals
                    decimal totalQuantity = 0;
                    decimal totalPrice = 0;
                    decimal totalInvoicePrice = 0;
                    decimal totalInvoicePriceWithTax = 0;

                    // Data Rows
                    foreach (var invoice in _purchaseInvoiceDetails)
                    {
                        if (invoice.ItemDetails == null || !invoice.ItemDetails.Any())
                            continue;

                        var itemCount = invoice.ItemDetails.Count;
                        var issueDateStr = invoice.IssueDate.ToString("dd/MM/yyyy");

                        // Add rows for each item in the invoice
                        for (int i = 0; i < itemCount; i++)
                        {
                            var item = invoice.ItemDetails[i];
                            var itemName = _language == "en" 
                                ? (!string.IsNullOrWhiteSpace(item.ItemEnglishName) ? item.ItemEnglishName : item.ItemArabicName)
                                : (!string.IsNullOrWhiteSpace(item.ItemArabicName) ? item.ItemArabicName : item.ItemEnglishName);
                            var supplierNameForRow = _language == "en"
                                ? (!string.IsNullOrWhiteSpace(invoice.SuppilerEnglishName) ? invoice.SuppilerEnglishName : invoice.SuppilerArabicName)
                                : (!string.IsNullOrWhiteSpace(invoice.SuppilerArabicName) ? invoice.SuppilerArabicName : invoice.SuppilerEnglishName);

                            if (_language == "en")
                            {
                                // English - RTL order: Invoice No, Supplier Name, Description, Invoice Date, Item Code, Item Name, Quantity, Unit Price, Total, Total with Tax
                                // بيانات الفاتورة (only first row per invoice) - RTL order: من اليمين
                                if (i == 0)
                                {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.InvoiceNo ?? "");
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(supplierNameForRow);
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.Description ?? "");
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(issueDateStr);
                                }
                                // الاصناف (each item row) - RTL order
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.ReferenceCode ?? "");
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(itemName);
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Quantity.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Price.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.TotalPrice.ToString("0.##"));
                                // Total with Tax - only first row per invoice
                                if (i == 0)
                                {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                                }
                            }
                            else
                            {
                                // Arabic - LTR order: Total with Tax, Total, Unit Price, Quantity, Item Name, Item Code, Invoice Date, Description, Supplier Name, Invoice No
                                // Total with Tax - only first row per invoice
                            if (i == 0)
                            {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                                }
                                // Total, Unit Price, Quantity, Item Name, Item Code (each item row)
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.TotalPrice.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Price.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Quantity.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(itemName);
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.ReferenceCode ?? "");
                                // Invoice Date, Description, Supplier Name, Invoice No - only first row per invoice
                            if (i == 0)
                            {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(issueDateStr);
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.Description ?? "");
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(supplierNameForRow);
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.InvoiceNo ?? "");
                                }
                            }

                            // Update totals
                            totalQuantity += item.Quantity;
                            totalPrice += item.TotalPrice;
                        }

                        // Invoice subtotal row (blue text style)
                        if (_language == "en")
                        {
                            // English - RTL order: Invoice No, Supplier Name, Description, Invoice Date, Item Code, Item Name, Quantity, Unit Price, Total, Total with Tax
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("Total");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalItemQuantity.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalItemPrice.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePrice.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                        }
                        else
                        {
                            // Arabic - LTR order: Total with Tax, Total, Unit Price, Quantity, Item Name, Item Code, Invoice Date, Description, Supplier Name, Invoice No
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePrice.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalItemPrice.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalItemQuantity.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("الاجمالي");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                        }

                        totalInvoicePrice += invoice.TotalInvoicePrice;
                        totalInvoicePriceWithTax += invoice.TotalInvoicePriceWithTax;
                    }

                    // Final Total Row (green text style)
                    if (_language == "en")
                    {
                        // English - RTL order: Invoice No, Supplier Name, Description, Invoice Date, Item Code, Item Name, Quantity, Unit Price, Total, Total with Tax
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("Total");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalQuantity.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalPrice.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePrice.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePriceWithTax.ToString("0.##"));
                    }
                    else
                    {
                        // Arabic - LTR order: Total with Tax, Total, Unit Price, Quantity, Item Name, Item Code, Invoice Date, Description, Supplier Name, Invoice No
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePriceWithTax.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePrice.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalPrice.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalQuantity.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("الاجمالي");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                    }
                });
            });
            });
        }
        catch (Exception ex)
        {
            SharedKernel.Utilities.LogService.LogError($"Error in PurchaseInvoiceReportDocument.Compose: {ex.Message}");
            SharedKernel.Utilities.LogService.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                SharedKernel.Utilities.LogService.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    static IContainer HeaderStyle(IContainer c) => c.Background("#FFE4B5").Border(1).BorderColor("#000000").Padding(4).DefaultTextStyle(t => t.SemiBold());
    static IContainer DataStyle(IContainer c) => c.Background("#f4eaff").Border(1).BorderColor("#000000").Padding(2);
    static IContainer SubtotalStyle(IContainer c) => c.Background("#E3F2FD").Border(1).BorderColor("#2196F3").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#1976D2"));
    static IContainer TotalStyle(IContainer c) => c.Background("#E6FFE6").Border(1).BorderColor("#00AA00").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#00AA00"));
}

