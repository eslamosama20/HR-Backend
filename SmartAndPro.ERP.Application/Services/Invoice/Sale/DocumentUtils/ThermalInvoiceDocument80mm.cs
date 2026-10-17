using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using QRCoder;
using System;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

/// <summary>
/// Thermal receipt printer format - 80mm width
/// تنسيق إيصال حراري - عرض 80 ملم
/// </summary>
public class ThermalInvoiceDocument80mm : IDocument
{
    private readonly SaleInvoice _invoice;
    private readonly CompanyDetails _companyDetails;
    private readonly string? _sellerName;
    private readonly SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment? _cashierPayment;
    private const float WidthMm = 80f;

    public ThermalInvoiceDocument80mm(
        SaleInvoice invoice,
        CompanyDetails companyDetails,
        string? sellerName = null,
        SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment? cashierPayment = null)
    {
        _invoice = invoice;
        _companyDetails = companyDetails;
        _sellerName = sellerName;
        _cashierPayment = cashierPayment;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.ContinuousSize(WidthMm, Unit.Millimetre);
            page.MarginVertical(3, Unit.Millimetre);
            page.MarginHorizontal(4, Unit.Millimetre);
            page.PageColor(Colors.White);
            page.ContentFromRightToLeft();
            page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(9).DirectionAuto());

            page.Content().Column(column =>
            {
                column.Spacing(2);

                // ===== Company Header =====
                column.Item().AlignCenter().Column(header =>
                {
                    header.Spacing(1);
                    if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                    {
                        header.Item().AlignCenter().Height(35).Image(_companyDetails.Logo).FitHeight();
                    }
                    if (!string.IsNullOrWhiteSpace(_companyDetails?.ArabicName))
                        header.Item().AlignCenter().Text(_companyDetails.ArabicName).Bold().FontSize(12);
                    if (!string.IsNullOrWhiteSpace(_companyDetails?.EnglishName))
                        header.Item().AlignCenter().Text(_companyDetails.EnglishName).FontSize(8);
                    if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                        header.Item().AlignCenter().Text($"الرقم الضريبي: {_companyDetails.TaxNumber}").FontSize(8);
                    var phone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                    if (phone != null && !string.IsNullOrWhiteSpace(phone.PhoneNumber))
                        header.Item().AlignCenter().Text($"هاتف: {phone.PhoneNumber}").FontSize(8);
                });

                var isWalkInCustomer = string.Equals(_invoice.CustomerProfile?.ArabicName?.Trim(), "عميل نقدي", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_invoice.CustomerProfile?.EnglishName?.Trim(), "Walk-in Customer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_invoice.CustomerProfile?.EnglishName?.Trim(), "Cash Customer", StringComparison.OrdinalIgnoreCase);

                var hasCustomerTax = !isWalkInCustomer && !string.IsNullOrWhiteSpace(_invoice.CustomerProfile?.TaxNumber);
                var title = hasCustomerTax ? "فاتورة ضريبية" : "فاتورة ضريبية مبسطة";
                column.Item().AlignCenter().PaddingVertical(3).Text(title).Bold().FontSize(11);

                DashedLine(column);

                // ===== Invoice Info =====
                column.Item().Column(info =>
                {
                    info.Spacing(1);
                    InfoRow(info, "رقم الفاتورة", _invoice.InvoiceNo ?? _invoice.Id.ToString());
                    InfoRow(info, "تاريخ الإصدار", _invoice.IssueDate.ToString("dd/MM/yyyy hh:mm tt"));

                    var customerName = !string.IsNullOrWhiteSpace(_invoice.CustomerProfile?.ArabicName) 
                        ? _invoice.CustomerProfile.ArabicName 
                        : _invoice.CustomerProfile?.EnglishName;

                    if (!string.IsNullOrWhiteSpace(customerName))
                        InfoRow(info, "العميل", customerName);
                    if (hasCustomerTax)
                        InfoRow(info, "الرقم الضريبي للعميل", _invoice.CustomerProfile!.TaxNumber!);
                    if (!string.IsNullOrWhiteSpace(_sellerName))
                        InfoRow(info, "البائع", _sellerName);

                    if (_cashierPayment != null)
                    {
                        string paymentLabel = _cashierPayment.PaymentMethod switch
                        {
                            0 => "نقدي (Cash)",
                            1 => "بطاقة / فيزا (Card)",
                            2 => "آجل (Credit)",
                            3 => "دفع متنوع (Mixed)",
                            _ => "نقدي"
                        };
                        InfoRow(info, "طريقة الدفع", paymentLabel);
                    }
                });

                DashedLine(column);

                // ===== Items Table =====
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3.5f); // الصنف
                        columns.RelativeColumn(1.2f); // الكمية
                        columns.RelativeColumn(1.5f); // السعر
                        columns.RelativeColumn(1.8f); // الإجمالي
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                            .Text("الصنف").Bold().FontSize(8);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                            .AlignCenter().Text("الكمية").Bold().FontSize(8);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                            .AlignCenter().Text("السعر").Bold().FontSize(8);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                            .AlignCenter().Text("الإجمالي").Bold().FontSize(8);
                    });

                    foreach (var item in _invoice.SaleItems ?? Enumerable.Empty<SaleItem>())
                    {
                        var itemName = item.MainSalesClass?.ArabicName ?? item.MainSalesClass?.EnglishName ?? item.Name ?? "";
                        var quantity = item.Quantity;
                        var price = item.Price;
                        var total = quantity * price;

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                            .Text(itemName).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                            .AlignCenter().Text($"{quantity:0.##}").FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                            .AlignCenter().Text($"{price:0.00}").FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2)
                            .AlignCenter().Text($"{total:0.00}").FontSize(8);
                    }
                });

                DashedLine(column);

                // ===== Totals =====
                column.Item().Column(totals =>
                {
                    var subtotal = _invoice.SaleItems?.Sum(x => x.Quantity * x.Price) ?? 0m;
                    var discount = _invoice.Discount;
                    var taxAmount = _invoice.SaleItems?.Sum(x =>
                    {
                        var itemTotal = x.Quantity * x.Price;
                        var isExempt = x.MainSalesClass?.IsTaxExempted ?? false;
                        return isExempt ? 0m : itemTotal * 0.15m;
                    }) ?? 0m;
                    var netPrice = _invoice.NetPrice;

                    TotalRow(totals, "المجموع", $"{subtotal:0.00} ر.س", false, 9);
                    if (discount > 0)
                        TotalRow(totals, "الخصم", $"{discount:0.00} ر.س", false, 9);
                    TotalRow(totals, "ضريبة القيمة المضافة (15%)", $"{taxAmount:0.00} ر.س", false, 9);

                    TotalRow(totals, "الإجمالي شامل الضريبة", $"{netPrice:0.00} ر.س", false, 9);

                    // ===== تفاصيل الدفع في الفاتورة =====
                    if (_cashierPayment != null)
                    {
                        totals.Item().PaddingTop(2).Column(payCol =>
                        {
                            if (_cashierPayment.CashAmount > 0)
                                TotalRow(payCol, "• نقدي", $"{_cashierPayment.CashAmount:0.00} ر.س", false, 8.5f);
                            if (_cashierPayment.CardAmount > 0)
                                TotalRow(payCol, "• بطاقة / فيزا", $"{_cashierPayment.CardAmount:0.00} ر.س", false, 8.5f);
                            if (_cashierPayment.CreditAmount > 0)
                                TotalRow(payCol, "• آجل", $"{_cashierPayment.CreditAmount:0.00} ر.س", false, 8.5f);

                            if (_cashierPayment.ChangeAmount > 0)
                                TotalRow(payCol, "• المتبقي للعميل", $"{_cashierPayment.ChangeAmount:0.00} ر.س", false, 8.5f);
                        });
                    }
                    else if (_invoice.PaidAmount > 0)
                    {
                        TotalRow(totals, "المدفوع", $"{_invoice.PaidAmount:0.00} ر.س", false, 8.5f);
                    }
                });

                DashedLine(column);

                // ===== QR Code =====
                if (!string.IsNullOrWhiteSpace(_invoice.GeneratedQR))
                {
                    column.Item().AlignCenter().Column(qr =>
                    {
                        try
                        {
                            using var qrGenerator = new QRCodeGenerator();
                            using var qrCodeData = qrGenerator.CreateQrCode(_invoice.GeneratedQR, QRCodeGenerator.ECCLevel.Q);
                            using var qrCode = new PngByteQRCode(qrCodeData);
                            var qrBytes = qrCode.GetGraphic(4);
                            qr.Item().AlignCenter().Width(55).Height(55).Image(qrBytes).FitArea();
                        }
                        catch { }
                    });
                }

                // ===== Footer =====
                column.Item().PaddingTop(4).AlignCenter().Column(footer =>
                {
                    footer.Item().AlignCenter().Text("شكراً لزيارتكم").FontSize(10);
                    footer.Item().AlignCenter().Text("Thank you for your visit").FontSize(8);
                });
            });
        });
    }

    private static void DashedLine(ColumnDescriptor column)
    {
        column.Item().PaddingVertical(2).AlignCenter()
            .Text("- - - - - - - - - - - - - - - - - - - - - - - - - -")
            .FontSize(6).FontColor(Colors.Grey.Medium);
    }

    private static void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Text($"{label}:").SemiBold().FontSize(8);
            row.RelativeItem().AlignLeft().Text(value).FontSize(8);
        });
    }

    private static void TotalRow(ColumnDescriptor col, string label, string value, bool bold, float fontSize)
    {
        col.Item().PaddingVertical(1).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                var span = text.Span(label);
                if (bold) span.Bold();
                span.FontSize(fontSize);
            });
            row.AutoItem().AlignLeft().Text(text =>
            {
                var span = text.Span(value);
                if (bold) span.Bold();
                span.FontSize(fontSize);
            });
        });
    }
}
