//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
//using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
//using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
//using SmartAndPro.ERP.Domains.ValueObjects;

//namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

//public class CombinedSalesInvoicesDocument : IDocument
//{
//    private readonly List<SaleInvoice> _invoices;
//    private readonly CompanyDetails _companyDetails;
//    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
//    private readonly Dictionary<int, string>? _quotationNumbers;

//    public CombinedSalesInvoicesDocument(
//        IEnumerable<SaleInvoice> invoices,
//        CompanyDetails companyDetails,
//        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
//        Dictionary<int, string>? quotationNumbers = null)
//    {
//        _invoices = invoices.ToList();
//        _companyDetails = companyDetails;
//        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
//        _quotationNumbers = quotationNumbers;
//    }

//    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

//    public void Compose(IDocumentContainer container)
//    {
//        container.Page(page =>
//        {
//            page.Size(PageSizes.A4);
//            page.Margin(20);
//            page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

//            PdfTemplateUtils.ApplyHeaderFooter(
//                page,
//                _pdfTemplateConfig,
//                (pg, header) => pg.Header().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, header, _companyDetails)),
//                (pg, footer) => pg.Footer().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, footer, _companyDetails))
//            );

//            page.Content().Column(col =>
//            {
//                foreach (var invoice in _invoices)
//                {
//                    col.Item().Element(c => RenderInvoice(c, invoice));
//                    if (invoice != _invoices.Last())
//                        col.Item().PageBreak();
//                }
//            });
//        });
//    }

//    private void RenderInvoice(IContainer container, SaleInvoice invoice)
//    {
//        container.Column(col =>
//        {
//            col.Item().Border(1).Padding(6).Row(r =>
//            {
//                r.RelativeItem().Column(c =>
//                {
//                    c.Item().Text(t => { t.Span("رقم الفاتورة: ").SemiBold(); t.Span(invoice.InvoiceNo); });
//                    if (_quotationNumbers != null && _quotationNumbers.ContainsKey(invoice.Id))
//                    {
//                        c.Item().PaddingTop(2).Text(t => 
//                        { 
//                            t.Span("رقم عرض السعر: ").SemiBold().FontColor("#2563eb"); 
//                            t.Span(_quotationNumbers[invoice.Id]).FontColor("#2563eb"); 
//                        });
//                    }
//                });
//                var hasVat = !string.IsNullOrWhiteSpace(invoice.CustomerProfile?.TaxNumber);
//                var title = hasVat ? "فاتورة ضريبية" : "فاتورة ضريبية مبسطة";
//                r.RelativeItem().AlignCenter().Text(title).SemiBold();
//                r.RelativeItem().AlignRight().Text(t => { t.Span("التاريخ: ").SemiBold(); t.Span(invoice.IssueDate.ToString("yyyy-MM-dd")); });
//            });

//            // Client Name and Tax ID Section
//            if (invoice.CustomerProfile != null)
//            {
//                col.Item().PaddingTop(2).PaddingBottom(8).Row(r =>
//                {
//                    // Tax ID on the left corner (from company details)
//                    if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
//                    {
//                        r.ConstantItem(200).AlignLeft().Text(t =>
//                        {
//                            t.Span("الرقم الضريبي: ").SemiBold();
//                            t.Span(_companyDetails.TaxNumber);
//                        });
//                    }

//                    // Spacer to push elements to corners
//                    r.RelativeItem();

//                    // Client Name on the right corner
//                    r.ConstantItem(200).AlignRight().Text(t =>
//                    {
//                        t.Span("اسم العميل: ").SemiBold();
//                        var customerName = invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "";
//                        t.Span(customerName);
//                    });
//                });
//            }

//            var itemsList = invoice.SaleItems.ToList();
//            var itemSubtotals = itemsList.Select(si => si.Price * si.Quantity * si.NoOfNights).ToList();
//            var grossTotal = itemSubtotals.Sum();
//            var discount = invoice.DiscountAmount;
//            var perItemValues = new List<(decimal taxableBase, decimal tax, decimal total)>();
//            for (int ic = 0; ic < itemsList.Count; ic++)
//            {
//                var saleItem = itemsList[ic];
//                var itemSubtotal = itemSubtotals[ic];
//                var itemDiscount = grossTotal > 0 ? (itemSubtotal / grossTotal) * discount : 0m;
//                var taxableBase = Math.Max(itemSubtotal - itemDiscount, 0m);
//                var isExempt = saleItem.MainSalesClass?.IsTaxExempted ?? false;
//                var taxRate = isExempt ? 0m : 0.15m;
//                var itemTax = Math.Round(taxableBase * taxRate, 2);
//                var itemTotal = taxableBase + itemTax;
//                perItemValues.Add((Math.Round(taxableBase,2), itemTax, Math.Round(itemTotal,2)));
//            }
//            var netTotal = Math.Round(grossTotal - discount, 2);
//            var taxTotal = Math.Round(perItemValues.Sum(v => v.tax), 2);
//            var grandTotal = Math.Round(netTotal + taxTotal, 2);

//            col.Item().Table(table =>
//            {
//                table.ColumnsDefinition(columns =>
//                {
//                    columns.ConstantColumn(70); // total
//                    columns.ConstantColumn(55); // tax
//                    columns.ConstantColumn(70); // net
//                    columns.ConstantColumn(55); // price
//                    columns.ConstantColumn(55); // qty
//                    columns.RelativeColumn(1f); // unit
//                    columns.RelativeColumn(2f); // name
//                    columns.RelativeColumn(1.2f); // item no
//                    columns.ConstantColumn(30); // index
//                });

//                table.Header(header =>
//                {
//                    header.Cell().Element(Header).AlignCenter().Text("االجمالي");
//                    header.Cell().Element(Header).AlignCenter().Text("الضريبه");
//                    header.Cell().Element(Header).AlignCenter().Text("اجمالي قبل الضريبه");
//                    header.Cell().Element(Header).AlignCenter().Text("السعر");
//                    header.Cell().Element(Header).AlignCenter().Text("الكميه");
//                    header.Cell().Element(Header).AlignCenter().Text("الوحده");
//                    header.Cell().Element(Header).AlignCenter().Text("اسم الصنف");
//                    header.Cell().Element(Header).AlignCenter().Text("رقم الصنف");
//                    header.Cell().Element(Header).AlignCenter().Text("No.");
//                });

//                var index = 1;
//                for (int i = 0; i < itemsList.Count; i++)
//                {
//                    var item = itemsList[i];
//                    var itemNo = item.MainSalesClass?.ReferenceCode ?? item.ItemNo ?? index.ToString();
//                    var name = item.MainSalesClass?.EnglishName ?? item.MainSalesClass?.ArabicName ?? item.Name ?? "";
//                    var unit = item.MainSalesClass?.MeasurementUnit?.EnglishName ?? item.MainSalesClass?.MeasurementUnit?.ArabicName ?? "";
//                    var qty = item.Quantity * item.NoOfNights;
//                    var computed = perItemValues[i];
//                    var total = computed.total;
//                    var tax = computed.tax;
//                    var net = computed.taxableBase;

//                    table.Cell().Element(Data).AlignCenter().Text(total.ToString("0.##"));
//                    table.Cell().Element(Data).AlignCenter().Text(tax.ToString("0.##"));
//                    table.Cell().Element(Data).AlignCenter().Text(net.ToString("0.##"));
//                    table.Cell().Element(Data).AlignCenter().Text(item.Price.ToString("0.##"));
//                    table.Cell().Element(Data).AlignCenter().Text(qty.ToString("0.##"));
//                    table.Cell().Element(Data).AlignCenter().Text(unit);
//                    table.Cell().Element(Data).AlignCenter().Text(name);
//                    table.Cell().Element(Data).AlignCenter().Text(itemNo);
//                    table.Cell().Element(Data).AlignCenter().Text(index.ToString());
//                    index++;
//                }
//            });


//            col.Item().PaddingTop(10).Row(row =>
//            {
//                var qrCol = row.RelativeItem();
//                var totalsCol = row.RelativeItem();

//                qrCol.AlignCenter().Column(c =>
//                {
//                    if (!string.IsNullOrWhiteSpace(invoice.GeneratedQR))
//                    {
//                        try
//                        {
//                            var qrGenerator = new QRCoder.QRCodeGenerator();
//                            var qrData = qrGenerator.CreateQrCode(invoice.GeneratedQR, QRCoder.QRCodeGenerator.ECCLevel.L);
//                            var qrPng = new QRCoder.PngByteQRCode(qrData);
//                            var qrBytes = qrPng.GetGraphic(8);
//                            c.Item().Height(150).Width(150).Image(qrBytes).FitArea();
//                        }
//                        catch {}
//                    }
//                });

//                totalsCol.AlignCenter().Table(t =>
//                {
//                    t.ColumnsDefinition(cd =>
//                    {
//                        cd.RelativeColumn(1); // value
//                        cd.RelativeColumn(2); // label
//                    });
//                    void TotRow(string label, decimal value, bool bold=false)
//                    {
//                        var valueCell = t.Cell().Element(TotalValue).AlignCenter().Text(value.ToString("0.##"));
//                        if (bold) valueCell.SemiBold();
//                        t.Cell().Element(TotalLabel).AlignCenter().Text(label);
//                    }
//                    TotRow("االجمالي", netTotal, false);
//                    TotRow("الضريبه المضافه", taxTotal, false);
//                    TotRow("مع الضريبة", grandTotal, true);
//                });
//            });
//        });
//    }

//    static IContainer Header(IContainer c) => c.Background("#F1F5F9").Border(1).Padding(4).DefaultTextStyle(t => t.SemiBold());
//    static IContainer Data(IContainer c) => c.BorderBottom(1).BorderColor("#E2E8F0").Padding(2);
//    static IContainer TotalLabel(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t=>t.SemiBold());
//    static IContainer TotalValue(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(4);
//}


using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using QRCoder;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

public class CombinedSalesInvoicesDocument : IDocument
{
    private readonly List<SaleInvoice> _invoices;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly Dictionary<int, string>? _quotationNumbers;
    private readonly bool _showSignatures;
    private readonly bool _landscape;
    private readonly bool _showTerms;
    private readonly bool _showBanks;
    private readonly IReadOnlyCollection<Guid>? _uncheckedPolicyIds;
    private readonly IReadOnlyCollection<Guid>? _uncheckedBankIds;

    public CombinedSalesInvoicesDocument(
        IEnumerable<SaleInvoice> invoices,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        Dictionary<int, string>? quotationNumbers = null,
        bool showSignatures = true,
        bool landscape = false,
        bool showTerms = false,
        bool showBanks = false,
        IReadOnlyCollection<Guid>? uncheckedPolicyIds = null,
        IReadOnlyCollection<Guid>? uncheckedBankIds = null)
    {
        _invoices = invoices.OrderByDescending(x => x.IssueDate).ToList();
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
        _quotationNumbers = quotationNumbers;
        _showSignatures = showSignatures;
        _landscape = landscape;
        _showTerms = showTerms;
        _showBanks = showBanks;
        _uncheckedPolicyIds = uncheckedPolicyIds;
        _uncheckedBankIds = uncheckedBankIds;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        foreach (var invoice in _invoices)
        {
            container.Page(page =>
            {
                page.Size(_landscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                // Header - same as SalesInvoiceDocument
                page.Header().Element(headerContainer =>
                {
                    headerContainer.PaddingBottom(8).Row(row =>
                    {
                        // Column 1: English Company Details (Left)
                        row.RelativeItem(1.5f).AlignLeft().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.EnglishName))
                                col.Item().AlignLeft().Text(_companyDetails.EnglishName).SemiBold().FontSize(12);

                            if (_companyDetails?.Address != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.Address.Street))
                                    col.Item().PaddingTop(2).AlignLeft().Text(_companyDetails.Address.Street).FontSize(10);
                                if (!string.IsNullOrWhiteSpace(_companyDetails.Address.City))
                                    col.Item().PaddingTop(2).AlignLeft().Text(_companyDetails.Address.City).FontSize(10);
                            }

                            var primaryPhoneEng = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhoneEng != null && !string.IsNullOrWhiteSpace(primaryPhoneEng.PhoneNumber))
                            {
                                col.Item().PaddingTop(2).AlignLeft().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("Phone Number: ");
                                    t.Span(primaryPhoneEng.PhoneNumber);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                            {
                                col.Item().PaddingTop(2).AlignLeft().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("Tax ID: ");
                                    t.Span(_companyDetails.TaxNumber);
                                });
                            }
                        });

                        // Column 2: Logo (Center)
                        row.RelativeItem(1).AlignCenter().AlignMiddle().Column(col =>
                        {
                            if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                                col.Item().AlignCenter().MaxHeight(80).MaxWidth(120).Image(_companyDetails.Logo).FitArea();
                        });

                        // Column 3: Arabic Company Details (Right)
                        row.RelativeItem(1.5f).AlignRight().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.ArabicName))
                                col.Item().AlignRight().Text(_companyDetails.ArabicName).SemiBold().FontSize(12);

                            if (_companyDetails?.ArabicAddress != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                                    col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.Street).FontSize(10);
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                                    col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.City).FontSize(10);
                            }

                            var primaryPhone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                            {
                                col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("رقم الهاتف: ");
                                    t.Span(primaryPhone.PhoneNumber);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                            {
                                col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("الرقم الضريبي: ");
                                    t.Span(_companyDetails.TaxNumber);
                                });
                            }
                        });
                    });
                });

                // Footer - same as SalesInvoiceDocument
                page.Footer().Element(footerContainer =>
                {
                    footerContainer.BorderTop(1).BorderColor("#E2E8F0").PaddingTop(5).AlignCenter().DefaultTextStyle(x => x.FontSize(9)).Text(t =>
                    {
                        var parts = new List<string>();

                        if (_companyDetails?.ArabicAddress != null)
                        {
                            var addressParts = new List<string>();
                            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Country))
                                addressParts.Add(_companyDetails.ArabicAddress.Country);
                            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                                addressParts.Add(_companyDetails.ArabicAddress.City);
                            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                                addressParts.Add(_companyDetails.ArabicAddress.Street);
                            if (addressParts.Count > 0)
                                parts.Add(string.Join(" - ", addressParts));
                        }

                        if (_companyDetails?.ArabicAddress != null && !string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.ZipCode))
                            parts.Add($"الرقم البريدي: {_companyDetails.ArabicAddress.ZipCode}");

                        var primaryPhone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                        if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                            parts.Add($"رقم الهاتف: {primaryPhone.PhoneNumber}");

                        t.Span(string.Join(" | ", parts));
                    });
                });

                page.Content().Element(c => RenderInvoice(c, invoice));
            });
        }
    }

    private void RenderInvoice(IContainer container, SaleInvoice invoice)
    {
        container.Column(col =>
        {
            // Invoice Header Row
            col.Item().PaddingTop(0).PaddingBottom(2).Border(1).BorderColor("#E2E8F0").Padding(8).Row(r =>
            {
                r.RelativeItem().AlignLeft().Column(c =>
                {
                    c.Item().Text(t =>
                    {
                        t.Span("رقم الفاتورة ").SemiBold();
                        t.Span(invoice.InvoiceNo);
                    });
                    if (_quotationNumbers != null && _quotationNumbers.ContainsKey(invoice.Id))
                    {
                        c.Item().PaddingTop(2).Text(t =>
                        {
                            t.Span("رقم عرض السعر ").SemiBold().FontColor("#2563eb");
                            t.Span(_quotationNumbers[invoice.Id]).FontColor("#2563eb");
                        });
                    }
                    var sellerName = invoice.CreatedBy;
                    if (!string.IsNullOrWhiteSpace(sellerName))
                    {
                        c.Item().PaddingTop(4).Text(t =>
                        {
                            t.Span("Created By : ").SemiBold();
                            t.Span(sellerName);
                        });
                    }
                });

                var hasVat = !string.IsNullOrWhiteSpace(invoice.CustomerProfile?.TaxNumber);
                var title = hasVat ? "فاتورة ضريبية" : "فاتورة ضريبية مبسطة";
                r.RelativeItem().AlignCenter().Text(title).SemiBold().FontSize(16);

                r.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Row(dateRow =>
                    {
                        dateRow.AutoItem().Text($"\u200E{invoice.IssueDate:dd/MM/yyyy}\u200E");
                        dateRow.ConstantItem(8).AlignCenter().Text(":").SemiBold();
                        dateRow.ConstantItem(75).AlignRight().Text("تاريخ الإصدار").SemiBold();
                    });

                    if (hasVat && invoice.DueDate != default && invoice.DueDate.Year > 1753 && invoice.DueDate.Date != invoice.IssueDate.Date)
                    {
                        c.Item().PaddingTop(2).Row(dateRow =>
                        {
                            dateRow.AutoItem().Text($"\u200E{invoice.DueDate:dd/MM/yyyy}\u200E");
                            dateRow.ConstantItem(8).AlignCenter().Text(":").SemiBold();
                            dateRow.ConstantItem(75).AlignRight().Text("تاريخ الاستحقاق").SemiBold();
                        });
                    }
                });
            });

            // Client Name and Tax ID Section
            if (invoice.CustomerProfile != null)
            {
                col.Item().PaddingTop(2).PaddingBottom(8).Row(r =>
                {
                    if (!string.IsNullOrWhiteSpace(invoice.CustomerProfile?.TaxNumber))
                    {
                        r.AutoItem().Row(taxRow =>
                        {
                            taxRow.AutoItem().Text(invoice.CustomerProfile.TaxNumber);
                            taxRow.AutoItem().PaddingLeft(3).Text(":").SemiBold();
                            taxRow.AutoItem().PaddingLeft(3).Text("الرقم الضريبي").SemiBold();
                        });
                    }

                    r.RelativeItem();

                    r.AutoItem().Row(clientRow =>
                    {
                        var customerName = invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "";
                        clientRow.AutoItem().Text(customerName);
                        clientRow.AutoItem().PaddingLeft(3).Text(":").SemiBold();
                        clientRow.AutoItem().PaddingLeft(3).Text("اسم العميل").SemiBold();
                    });
                });
            }

            // Compute per-item values
            var itemsList = invoice.SaleItems.ToList();
            var itemSubtotals = itemsList.Select(si => si.Price * si.Quantity * si.NoOfNights).ToList();
            var grossTotal = itemSubtotals.Sum();
            var discount = invoice.DiscountAmount;
            var perItemValues = new List<(decimal taxableBase, decimal tax, decimal total)>();
            for (int i = 0; i < itemsList.Count; i++)
            {
                var saleItem = itemsList[i];
                var itemSubtotal = itemSubtotals[i];
                var itemDiscount = grossTotal > 0 ? (itemSubtotal / grossTotal) * discount : 0m;
                var taxableBase = Math.Max(itemSubtotal - itemDiscount, 0m);
                var isExempt = saleItem.MainSalesClass?.IsTaxExempted ?? false;
                var taxRate = isExempt ? 0m : 0.15m;
                var itemTax = Math.Round(taxableBase * taxRate, 2);
                var itemTotal = taxableBase + itemTax;
                perItemValues.Add((Math.Round(taxableBase, 2), itemTax, Math.Round(itemTotal, 2)));
            }
            var netTotal = Math.Round(grossTotal - discount, 2);
            var taxTotal = Math.Round(perItemValues.Sum(v => v.tax), 2);
            var grandTotal = Math.Round(netTotal + taxTotal, 2);

            // Table
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(45);
                    columns.RelativeColumn(1f);
                    columns.RelativeColumn(3f);
                    if (invoice.SaleInvoiceType != SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation)
                    {
                        columns.RelativeColumn(0.8f);
                    }
                    columns.ConstantColumn(30);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("الإجمالي");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("الضريبه");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("اجمالي قبل الضريبه");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("السعر");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("الكميه");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("الوحده");
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("اسم الصنف");
                    if (invoice.SaleInvoiceType != SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation)
                    {
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("رقم الصنف");
                    }
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("No.");
                });

                var index = 1;
                for (int i = 0; i < itemsList.Count; i++)
                {
                    var item = itemsList[i];
                    var itemNo = item.MainSalesClass?.ReferenceCode ?? item.ItemNo ?? index.ToString();
                    var name = item.MainSalesClass?.ArabicName ?? item.MainSalesClass?.EnglishName ?? item.Name ?? "";
                    var unit = item.MainSalesClass?.MeasurementUnit?.ArabicName ?? item.MainSalesClass?.MeasurementUnit?.EnglishName ?? "";
                    var qty = item.Quantity * item.NoOfNights;
                    var computed = perItemValues[i];

                    table.Cell().Element(DataStyle).AlignCenter().Text(computed.total.ToString("0.##"));
                    table.Cell().Element(DataStyle).AlignCenter().Text(computed.tax.ToString("0.##"));
                    table.Cell().Element(DataStyle).AlignCenter().Text(computed.taxableBase.ToString("0.##"));
                    table.Cell().Element(DataStyle).AlignCenter().Text(item.Price.ToString("0.##"));
                    table.Cell().Element(DataStyle).AlignCenter().Text(qty.ToString("0.##"));
                    table.Cell().Element(DataStyle).AlignCenter().Text(unit);

                    var hasArabicName = !string.IsNullOrWhiteSpace(item.MainSalesClass?.ArabicName);
                    if (hasArabicName)
                        table.Cell().Element(DataStyle).AlignRight().Text(name);
                    else
                        table.Cell().Element(DataStyle).AlignLeft().Text(name);

                    if (invoice.SaleInvoiceType != SmartAndPro.ERP.Domains.Enums.SaleInvoiceType.ComeFromReservation)
                    {
                        table.Cell().Element(DataStyle).AlignCenter().Text(itemNo);
                    }
                    table.Cell().Element(DataStyle).AlignCenter().Text(index.ToString());
                    index++;
                }
            });

            // Totals + QR + Terms & Banks
            col.Item().PaddingTop(4).Row(row =>
            {
                var totalsCol = row.ConstantItem(160).PaddingTop(6).PaddingRight(8);

                totalsCol.AlignTop().Table(t =>
                {
                    t.ColumnsDefinition(cd =>
                    {
                        cd.ConstantColumn(60);
                        cd.RelativeColumn(1);
                    });
                    void TotRow(string label, decimal value, bool bold = false)
                    {
                        var valueCell = t.Cell().Element(TotalValueStyle).AlignCenter().Text(value.ToString("0.##"));
                        if (bold) valueCell.SemiBold();
                        t.Cell().Element(TotalLabelStyle).AlignRight().Text(label);
                    }
                    TotRow("الإجمالي", netTotal);
                    TotRow("ضريبة القيمة المضافة", taxTotal);
                    TotRow("الإجمالي العام", grandTotal, true);
                });

                if (_showTerms || _showBanks)
                {
                    row.ConstantItem(75).PaddingLeft(0).AlignTop().AlignLeft().Column(c =>
                    {
                        if (!string.IsNullOrWhiteSpace(invoice.GeneratedQR))
                        {
                            try
                            {
                                var qrGenerator = new QRCodeGenerator();
                                var qrData = qrGenerator.CreateQrCode(invoice.GeneratedQR, QRCodeGenerator.ECCLevel.L);
                                var qrPng = new PngByteQRCode(qrData);
                                var qrBytes = qrPng.GetGraphic(10);
                                c.Item().AlignCenter().Height(65).Width(65).Image(qrBytes).FitArea();
                            }
                            catch { }
                        }
                    });

                    row.RelativeItem().PaddingRight(12).PaddingTop(6).AlignTop().Element(c => ComposeTermsAndBanks(c));
                }
                else
                {
                    var qrCol = row.RelativeItem();
                    qrCol.PaddingLeft(0).AlignTop().AlignLeft().Column(c =>
                    {
                        if (!string.IsNullOrWhiteSpace(invoice.GeneratedQR))
                        {
                            try
                            {
                                var qrGenerator = new QRCodeGenerator();
                                var qrData = qrGenerator.CreateQrCode(invoice.GeneratedQR, QRCodeGenerator.ECCLevel.L);
                                var qrPng = new PngByteQRCode(qrData);
                                var qrBytes = qrPng.GetGraphic(10);
                                c.Item().AlignCenter().Height(65).Width(65).Image(qrBytes).FitArea();
                            }
                            catch { }
                        }
                    });
                }
            });

            // Signatures Section
            if (_showSignatures)
            {
                col.Item().PaddingTop(30).Row(r =>
                {
                    // Customer signature - left
                    r.RelativeItem(3).PaddingLeft(0).PaddingRight(80).Column(c =>
                    {
                        var customerName = invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "";
                        if (!string.IsNullOrWhiteSpace(customerName))
                        {
                            var isArabic = !string.IsNullOrWhiteSpace(invoice.CustomerProfile?.ArabicName);
                            var alignment = isArabic ? (Action<IContainer, string>)((cont, text) => { }) : null;

                            if (isArabic)
                            {
                                c.Item().AlignRight().Text(t => t.Span(customerName).FontSize(10));
                                c.Item().PaddingTop(5).AlignRight().Text(t => t.Span("................................................................................ :التوقيع").FontSize(8));
                            }
                            else
                            {
                                c.Item().AlignLeft().Text(t => t.Span(customerName).FontSize(10));
                                c.Item().PaddingTop(5).AlignLeft().Text(t => t.Span("................................................................................ :التوقيع").FontSize(8));
                            }
                        }
                    });

                    // Company signature - right
                    r.RelativeItem(3).PaddingRight(0).PaddingLeft(80).Column(c =>
                    {
                        var companyName = _companyDetails.ArabicName ?? _companyDetails.EnglishName ?? "";
                        c.Item().AlignRight().Text(t => t.Span(companyName).FontSize(10));
                        c.Item().PaddingTop(5).AlignRight().Text(t => t.Span("................................................................................ :التوقيع").FontSize(8));

                        if (_companyDetails.Stamp != null && _companyDetails.Stamp.Length > 0)
                            c.Item().PaddingTop(10).AlignRight().Height(80).Width(80).Image(_companyDetails.Stamp).FitArea();
                    });
                });
            }
        });
    }

    private void ComposeTermsAndBanks(IContainer container)
    {
        if (_companyDetails == null) return;

        var policies = _showTerms && _companyDetails.Policies != null
            ? _companyDetails.Policies
                .Where(p => !string.IsNullOrWhiteSpace(p.PolicyParagraph))
                .Where(p => _uncheckedPolicyIds == null || !_uncheckedPolicyIds.Contains(p.Id))
                .ToList()
            : new List<CompanyPolicy>();

        var banks = _showBanks && _companyDetails.BankAccounts != null
            ? _companyDetails.BankAccounts
                .Where(b => !string.IsNullOrWhiteSpace(b.BankName) || !string.IsNullOrWhiteSpace(b.BankAccountNumber) || !string.IsNullOrWhiteSpace(b.IBAN) || !string.IsNullOrWhiteSpace(b.SwiftCode))
                .Where(b => _uncheckedBankIds == null || !_uncheckedBankIds.Contains(b.Id))
                .ToList()
            : new List<BankAccount>();

        if (policies.Count == 0 && banks.Count == 0) return;

        container.Column(col =>
        {
            if (policies.Count > 0)
            {
                col.Item().AlignRight().Text(": الشروط والأحكام").SemiBold().FontSize(9);
                foreach (var p in policies)
                {
                    if (string.IsNullOrWhiteSpace(p.PolicyParagraph)) continue;
                    var lines = p.PolicyParagraph.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed)) continue;
                        if (IsArabicText(trimmed))
                        {
                            col.Item().PaddingTop(2).AlignRight().Text(trimmed).FontSize(8.5f);
                        }
                        else
                        {
                            col.Item().PaddingTop(2).AlignLeft().Text(trimmed).FontSize(8.5f);
                        }
                    }
                }
            }

            if (banks.Count > 0)
            {
                col.Item().PaddingTop(policies.Count > 0 ? 6 : 0).AlignRight().Text(": الحسابات البنكية").SemiBold().FontSize(9);
                foreach (var b in banks)
                {
                    col.Item().PaddingTop(3).AlignRight().Element(c => AddBankLine(c, b));
                }
            }
        });
    }

    private static void AddBankLine(IContainer container, BankAccount b)
    {
        var accountName = b.AccountName?.Trim() ?? "";
        var bankName = b.BankName?.Trim() ?? "";
        var accountNumber = b.BankAccountNumber?.Trim() ?? "";
        var iban = b.IBAN?.Trim() ?? "";
        var swift = b.SwiftCode?.Trim() ?? "";

        container.Column(col =>
        {
            // 1. اسم البنك واسم الحساب
            var hasBankOrName = !string.IsNullOrWhiteSpace(bankName) || !string.IsNullOrWhiteSpace(accountName);
            if (hasBankOrName)
            {
                col.Item().AlignRight().Text(t =>
                {
                    if (!string.IsNullOrWhiteSpace(bankName))
                    {
                        t.Span(bankName).SemiBold().FontSize(8.5f);
                    }
                    if (!string.IsNullOrWhiteSpace(accountName))
                    {
                        if (!string.IsNullOrWhiteSpace(bankName)) t.Span("  -  ").FontSize(8.5f);
                        t.Span("اسم الحساب: ").SemiBold().FontSize(8.5f);
                        t.Span(accountName).FontSize(8.5f);
                    }
                });
            }

            // 2. رقم الحساب
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                col.Item().PaddingTop(1).AlignRight().Text(t =>
                {
                    t.Span("رقم الحساب: ").SemiBold().FontSize(8.5f);
                    t.Span(accountNumber).FontSize(8.5f);
                });
            }

            // 3. الآيبان
            if (!string.IsNullOrWhiteSpace(iban))
            {
                col.Item().PaddingTop(1).AlignRight().Text(t =>
                {
                    t.Span("الآيبان: ").SemiBold().FontSize(8.5f);
                    t.Span(iban).FontSize(8.5f);
                });
            }

            // 4. السويفت
            if (!string.IsNullOrWhiteSpace(swift))
            {
                col.Item().PaddingTop(1).AlignRight().Text(t =>
                {
                    t.Span("السويفت: ").SemiBold().FontSize(8.5f);
                    t.Span(swift).FontSize(8.5f);
                });
            }
        });
    }

    static IContainer HeaderStyle(IContainer c) => c.Background("#42acff").Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#ffffff"));
    static IContainer DataStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(2);
    static IContainer TotalLabelStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.FontSize(9));
    static IContainer TotalValueStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.FontSize(9));

    private static bool IsArabicText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        foreach (var c in text)
        {
            if (c >= 0x0600 && c <= 0x06FF || c >= 0x0750 && c <= 0x077F || c >= 0x08A0 && c <= 0x08FF || c >= 0xFB50 && c <= 0xFDFF || c >= 0xFE70 && c <= 0xFEFF)
                return true;
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                return false;
        }
        return true;
    }
}