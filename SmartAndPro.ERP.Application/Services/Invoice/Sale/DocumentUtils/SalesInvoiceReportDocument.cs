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

public class SalesInvoiceReportDocument : IDocument
{
    private readonly List<SalesInvoiceDetailsResponse> _salesInvoiceDetails;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;
    private readonly string _language;

    public SalesInvoiceReportDocument(
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
                        var title = _language == "ar" ? "تقرير القيمة المضافة لأصناف المبيعات" : "Sales VAT Items Report";
                        t.Span(title).SemiBold().FontSize(16);
                    });
                });

                // Table
                col.Item().Table(table =>
                {
                    // Column definitions: بيانات الفاتورة (4 columns) + الاصناف (7 columns) + قيمة الضريبة
                    // RTL order: من اليمين لليسار - نفس ترتيب HTML
                    table.ColumnsDefinition(columns =>
                    {
                        if (_language == "ar")
                        {
                            // Arabic - LTR order with adjusted widths
                            columns.RelativeColumn(1.2f);  // الاجمالي بالضريبة (Total with Tax)
                            columns.RelativeColumn(1.1f);  // قيمة الضريبة (VAT Amount)
                            columns.RelativeColumn(1.1f);  // الاجمالي (Total)
                            columns.RelativeColumn(0.9f);  // سعر الوحدة (Unit Price)
                            columns.RelativeColumn(0.7f);  // العدد (Quantity)
                            columns.RelativeColumn(1.5f);  // اسم الصنف (Item Name)
                            columns.RelativeColumn(1f);  // كود الصنف (Item Code)
                            columns.RelativeColumn(1.4f); // تاريخ الفاتورة (Invoice Date)
                            columns.RelativeColumn(1.5f);  // الوصف (Description)
                            columns.RelativeColumn(1.5f);  // اسم العميل (Customer Name)
                            columns.RelativeColumn(1.2f); // رقم الفاتورة (Invoice No)
                        }
                        else
                        {
                            // English - RTL order: نفس الأعمدة
                            columns.RelativeColumn(1.2f); // رقم الفاتورة (Invoice No) - أقصى اليمين
                            columns.RelativeColumn(1.5f);  // اسم العميل (Customer Name)
                            columns.RelativeColumn(1.5f);  // الوصف (Description)
                            columns.RelativeColumn(1.2f); // تاريخ الفاتورة (Invoice Date)
                            columns.RelativeColumn(1f);  // كود الصنف (Item Code)
                            columns.RelativeColumn(1.4f);  // اسم الصنف (Item Name)
                            columns.RelativeColumn(0.8f);  // العدد (Quantity)
                            columns.RelativeColumn(0.9f);  // سعر الوحدة (Unit Price)
                            columns.RelativeColumn(1f);  // الاجمالي (Total)
                            columns.RelativeColumn(1f);  // قيمة الضريبة (VAT Amount)
                            columns.RelativeColumn(1.2f);  // الاجمالي بالضريبة (Total with Tax) - أقصى اليسار
                        }
                    });

                    // Header Row - All columns
                    table.Header(header =>
                    {
                        if (_language == "en")
                        {
                            // English headers - RTL order (من اليمين لليسار) - نفس ترتيب العربي
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Invoice No");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Customer Name");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Description");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Invoice Date");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Item Code");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Item Name");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Quantity");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Unit Price");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("VAT Amount");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Total with Tax");
                        }
                        else
                        {
                            // Arabic headers - LTR order (من اليسار لليمين) - نفس ترتيب الإنجليزي
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الاجمالي بالضريبة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("قيمة الضريبة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الاجمالي");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("سعر الوحدة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("العدد");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("اسم الصنف");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("كود الصنف");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("تاريخ الفاتورة");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الوصف");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("اسم العميل");
                            header.Cell().Element(HeaderStyle).AlignMiddle().AlignCenter().Text("رقم الفاتوره");
                        }
                    });

                    // Group Header Row (بيانات الفاتورة / الاصناف) - صف عادي بعد Header مباشرة
                    if (_language == "en")
                    {
                        // English - RTL order: بيانات الفاتورة أولاً (من اليمين)، ثم الاصناف
                        table.Cell().ColumnSpan((uint)4).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Invoice Data");
                        table.Cell().ColumnSpan((uint)7).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("Items");
                    }
                    else
                    {
                        // Arabic - LTR order: الاصناف أولاً (من اليسار)، ثم بيانات الفاتورة
                        table.Cell().ColumnSpan((uint)7).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("الاصناف");
                        table.Cell().ColumnSpan((uint)4).Element(HeaderStyle).AlignMiddle().AlignCenter().Text("بيانات الفاتورة");
                    }

                    // Calculate totals
                    decimal totalQuantity = 0;
                    decimal totalPrice = 0;
                    decimal totalInvoicePrice = 0;
                    decimal totalInvoicePriceWithTax = 0;

                    // Data Rows
                    foreach (var invoice in _salesInvoiceDetails)
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
                            var customerNameForRow = _language == "en"
                                ? (!string.IsNullOrWhiteSpace(invoice.SuppilerEnglishName) ? invoice.SuppilerEnglishName : invoice.SuppilerArabicName)
                                : (!string.IsNullOrWhiteSpace(invoice.SuppilerArabicName) ? invoice.SuppilerArabicName : invoice.SuppilerEnglishName);

                            // Calculate VAT amount for this invoice
                            var vatAmount = invoice.TotalInvoicePriceWithTax - invoice.TotalInvoicePrice;

                            if (_language == "en")
                            {
                                // English - RTL order: Invoice No, Customer Name, Description, Invoice Date, Item Code, Item Name, Quantity, Unit Price, Total, VAT Amount, Total with Tax
                                // بيانات الفاتورة (only first row per invoice) - RTL order: من اليمين
                                if (i == 0)
                                {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.InvoiceNo ?? "");
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(customerNameForRow);
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.Description ?? "");
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(issueDateStr);
                                }
                                // الاصناف (each item row) - RTL order
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.ReferenceCode ?? "");
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(itemName);
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Quantity.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Price.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.TotalPrice.ToString("0.##"));
                                // VAT Amount and Total with Tax - only first row per invoice
                                if (i == 0)
                                {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(vatAmount.ToString("0.##"));
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                                }
                            }
                            else
                            {
                                // Arabic - LTR order: Total with Tax, VAT Amount, Total, Unit Price, Quantity, Item Name, Item Code, Invoice Date, Description, Customer Name, Invoice No
                                // Total with Tax and VAT Amount - only first row per invoice
                                if (i == 0)
                                {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(vatAmount.ToString("0.##"));
                                }
                                // Total, Unit Price, Quantity, Item Name, Item Code (each item row)
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.TotalPrice.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Price.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.Quantity.ToString("0.##"));
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(itemName);
                                table.Cell().Element(DataStyle).AlignMiddle().AlignCenter().Text(item.ReferenceCode ?? "");
                                // Invoice Date, Description, Customer Name, Invoice No - only first row per invoice
                                if (i == 0)
                                {
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(issueDateStr);
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.Description ?? "");
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(customerNameForRow);
                                    table.Cell().RowSpan((uint)itemCount).Element(DataStyle).AlignMiddle().AlignCenter().Text(invoice.InvoiceNo ?? "");
                                }
                            }

                            // Update totals
                            totalQuantity += item.Quantity;
                            totalPrice += item.TotalPrice;
                        }

                        // Invoice subtotal row (blue text style)
                        var invoiceVatAmount = invoice.TotalInvoicePriceWithTax - invoice.TotalInvoicePrice;
                        if (_language == "en")
                        {
                            // English - RTL order: Invoice No, Customer Name, Description, Invoice Date, Item Code, Item Name, Quantity, Unit Price, Total, VAT Amount, Total with Tax
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text("Total");
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalItemQuantity.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalItemPrice.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePrice.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoiceVatAmount.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                        }
                        else
                        {
                            // Arabic - LTR order: Total with Tax, VAT Amount, Total, Unit Price, Quantity, Item Name, Item Code, Invoice Date, Description, Customer Name, Invoice No
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoice.TotalInvoicePriceWithTax.ToString("0.##"));
                            table.Cell().Element(SubtotalStyle).AlignMiddle().AlignCenter().Text(invoiceVatAmount.ToString("0.##"));
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
                    var totalVatAmount = totalInvoicePriceWithTax - totalInvoicePrice;
                    if (_language == "en")
                    {
                        // English - RTL order: Invoice No, Customer Name, Description, Invoice Date, Item Code, Item Name, Quantity, Unit Price, Total, VAT Amount, Total with Tax
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("Total");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text("");
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalQuantity.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalPrice.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePrice.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalVatAmount.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePriceWithTax.ToString("0.##"));
                    }
                    else
                    {
                        // Arabic - LTR order: Total with Tax, VAT Amount, Total, Unit Price, Quantity, Item Name, Item Code, Invoice Date, Description, Customer Name, Invoice No
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalInvoicePriceWithTax.ToString("0.##"));
                        table.Cell().Element(TotalStyle).AlignMiddle().AlignCenter().Text(totalVatAmount.ToString("0.##"));
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
            SharedKernel.Utilities.LogService.LogError($"Error in SalesInvoiceReportDocument.Compose: {ex.Message}");
            SharedKernel.Utilities.LogService.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                SharedKernel.Utilities.LogService.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    static IContainer HeaderStyle(IContainer c) => c.Background("#42acff").Border(1).BorderColor("#000000").Padding(4).DefaultTextStyle(t => t.SemiBold());
    static IContainer DataStyle(IContainer c) => c.Background("#f4eaff").Border(1).BorderColor("#000000").Padding(2);
    static IContainer SubtotalStyle(IContainer c) => c.Background("#E3F2FD").Border(1).BorderColor("#2196F3").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#1976D2"));
    static IContainer TotalStyle(IContainer c) => c.Background("#E6FFE6").Border(1).BorderColor("#00AA00").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#00AA00"));
}

