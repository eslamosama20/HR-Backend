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
/// Thermal receipt printer format - 58mm width (compact)
/// تنسيق إيصال حراري - عرض 58 ملم (مضغوط)
/// </summary>
public class ThermalInvoiceDocument58mm : IDocument
{
    private readonly SaleInvoice _invoice;
    private readonly CompanyDetails _companyDetails;
    private readonly string? _sellerName;
    private const float WidthMm = 58f;

    public ThermalInvoiceDocument58mm(
        SaleInvoice invoice,
        CompanyDetails companyDetails,
        string? sellerName = null)
    {
        _invoice = invoice;
        _companyDetails = companyDetails;
        _sellerName = sellerName;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.ContinuousSize(WidthMm, Unit.Millimetre);
            page.MarginVertical(2, Unit.Millimetre);
            page.MarginHorizontal(2, Unit.Millimetre);
            page.PageColor(Colors.White);
            page.ContentFromRightToLeft();
            page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(7).DirectionAuto());

            page.Content().Column(column =>
            {
                column.Spacing(1);

                // ===== Company Header (compact) =====
                column.Item().AlignCenter().Column(header =>
                {
                    header.Spacing(1);
                    if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                    {
                        header.Item().AlignCenter().Height(25).Image(_companyDetails.Logo).FitHeight();
                    }
                    if (!string.IsNullOrWhiteSpace(_companyDetails?.ArabicName))
                        header.Item().AlignCenter().Text(_companyDetails.ArabicName).Bold().FontSize(9);
                    if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                        header.Item().AlignCenter().Text($"ض: {_companyDetails.TaxNumber}").FontSize(6);
                    var phone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                    if (phone != null && !string.IsNullOrWhiteSpace(phone.PhoneNumber))
                        header.Item().AlignCenter().Text(phone.PhoneNumber).FontSize(6);
                });

                var isWalkInCustomer = string.Equals(_invoice.CustomerProfile?.ArabicName?.Trim(), "عميل نقدي", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_invoice.CustomerProfile?.EnglishName?.Trim(), "Walk-in Customer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(_invoice.CustomerProfile?.EnglishName?.Trim(), "Cash Customer", StringComparison.OrdinalIgnoreCase);

                var hasCustomerTax = !isWalkInCustomer && !string.IsNullOrWhiteSpace(_invoice.CustomerProfile?.TaxNumber);
                var title = hasCustomerTax ? "فاتورة ضريبية" : "فاتورة ضريبية مبسطة";
                column.Item().AlignCenter().PaddingVertical(2).Text(title).Bold().FontSize(8);

                DashedLine(column);

                // ===== Invoice Info (stacked, no table) =====
                column.Item().Column(info =>
                {
                    info.Item().Text($"# {_invoice.InvoiceNo ?? _invoice.Id.ToString()}").SemiBold().FontSize(8);
                    info.Item().Text($"{_invoice.IssueDate:dd/MM/yyyy hh:mm tt}").FontSize(6);

                    var customerName = !string.IsNullOrWhiteSpace(_invoice.CustomerProfile?.ArabicName) 
                        ? _invoice.CustomerProfile.ArabicName 
                        : _invoice.CustomerProfile?.EnglishName;

                    if (!string.IsNullOrWhiteSpace(customerName))
                        info.Item().Text(customerName).FontSize(7);
                    if (hasCustomerTax)
                        info.Item().Text($"الرقم الضريبي: {_invoice.CustomerProfile!.TaxNumber}").FontSize(6);
                    if (!string.IsNullOrWhiteSpace(_sellerName))
                        info.Item().Text($"البائع: {_sellerName}").FontSize(6);
                });

                DashedLine(column);

                // ===== Items (stacked layout - no table for 58mm) =====
                column.Item().Column(items =>
                {
                    foreach (var item in _invoice.SaleItems ?? Enumerable.Empty<SaleItem>())
                    {
                        var itemName = item.MainSalesClass?.ArabicName ?? item.MainSalesClass?.EnglishName ?? item.Name ?? "";
                        var quantity = item.Quantity;
                        var price = item.Price;
                        var total = quantity * price;

                        items.Item().PaddingBottom(2).Column(itemBlock =>
                        {
                            itemBlock.Item().Text(itemName).SemiBold().FontSize(7);
                            itemBlock.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"{quantity:0.##} × {price:0.00}").FontSize(6);
                                row.AutoItem().Text($"{total:0.00}").FontSize(7);
                            });
                        });
                    }
                });

                DashedLine(column);

                // ===== Totals (compact) =====
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

                    TotalRow(totals, "المجموع", $"{subtotal:0.00}", false);
                    if (discount > 0)
                        TotalRow(totals, "خصم", $"{discount:0.00}", false);
                    TotalRow(totals, "ضريبة 15%", $"{taxAmount:0.00}", false);

                    column.Item().PaddingVertical(1).LineHorizontal(1).LineColor(Colors.Black);

                    totals.Item().PaddingVertical(2).Row(row =>
                    {
                        row.RelativeItem().Text("الإجمالي").Bold().FontSize(9);
                        row.AutoItem().Text($"{netPrice:0.00} ر.س").Bold().FontSize(9);
                    });
                });

                DashedLine(column);

                // ===== QR Code (smaller) =====
                if (!string.IsNullOrWhiteSpace(_invoice.GeneratedQR))
                {
                    column.Item().AlignCenter().Column(qr =>
                    {
                        try
                        {
                            using var qrGenerator = new QRCodeGenerator();
                            using var qrCodeData = qrGenerator.CreateQrCode(_invoice.GeneratedQR, QRCodeGenerator.ECCLevel.Q);
                            using var qrCode = new PngByteQRCode(qrCodeData);
                            var qrBytes = qrCode.GetGraphic(3);
                            qr.Item().AlignCenter().Width(38).Height(38).Image(qrBytes).FitArea();
                        }
                        catch { }
                    });
                }

                // ===== Footer =====
                column.Item().PaddingTop(3).AlignCenter().Text("شكراً لزيارتكم").FontSize(7);
            });
        });
    }

    private static void DashedLine(ColumnDescriptor column)
    {
        column.Item().PaddingVertical(1).AlignCenter()
            .Text("- - - - - - - - - - - - - - - - - -")
            .FontSize(5).FontColor(Colors.Grey.Medium);
    }

    private static void TotalRow(ColumnDescriptor col, string label, string value, bool bold)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                var span = text.Span(label);
                if (bold) span.Bold();
                span.FontSize(7);
            });
            row.AutoItem().Text(text =>
            {
                var span = text.Span(value);
                if (bold) span.Bold();
                span.FontSize(7);
            });
        });
    }
}
