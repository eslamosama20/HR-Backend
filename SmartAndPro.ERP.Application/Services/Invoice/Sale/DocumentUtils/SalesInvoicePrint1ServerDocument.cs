using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using System.Globalization;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

/// <summary>
/// Server-side replica of the frontend "Print 1" (format 1) sales-invoice template
/// (<c>sales-invoice-list.component.html</c> — logoHeaderTemplate2 / customerSupplierTemplate2 /
/// rowTemplate2 / summaryTemplate2 / footerTemplate2).
/// Auto orientation: items &gt; 8 → A4 landscape, else portrait (mirrors frontend <c>useLandscape</c>).
/// For Arabic the layout mirrors the frontend RTL rendering; for English it renders LTR.
/// </summary>
public class SalesInvoicePrint1ServerDocument : IDocument
{
    /// <summary>
    /// Auto-orientation threshold ONLY (mirrors the frontend useLandscape: more than 8 items →
    /// A4 landscape, else portrait). Pagination is natural flow: the items table splits across
    /// pages automatically and every page fills to its physical capacity.
    /// </summary>
    public const int LandscapeRowThreshold = 8;

    private readonly SaleInvoice _invoice;
    private readonly CompanyDetails _companyDetails;
    private readonly string _language;
    private readonly string? _sellerName;
    private readonly bool _showStamp;
    private readonly bool _showCompanySignature;
    private readonly bool _showCustomerSignature;
    private readonly IReadOnlyCollection<Guid>? _uncheckedPolicyIds;
    private readonly IReadOnlyCollection<Guid>? _uncheckedBankIds;
    private readonly string? _timeZone;
    private readonly string? _orientation;

    public SalesInvoicePrint1ServerDocument(
        SaleInvoice invoice,
        CompanyDetails companyDetails,
        string language = "ar",
        string? sellerName = null,
        bool showStamp = true,
        bool showCompanySignature = true,
        bool showCustomerSignature = true,
        IReadOnlyCollection<Guid>? uncheckedPolicyIds = null,
        IReadOnlyCollection<Guid>? uncheckedBankIds = null,
        string? timeZone = null,
        string? orientation = null)
    {
        _invoice = invoice;
        _companyDetails = companyDetails;
        _language = language;
        _sellerName = sellerName;
        _showStamp = showStamp;
        _showCompanySignature = showCompanySignature;
        _showCustomerSignature = showCustomerSignature;
        _uncheckedPolicyIds = uncheckedPolicyIds;
        _uncheckedBankIds = uncheckedBankIds;
        _timeZone = timeZone;
        _orientation = orientation;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // User-forced orientation (parity param "orientation=portrait|landscape") wins;
            // otherwise auto-detect mirrors the frontend useLandscape (items > 8 → landscape).
            var isLandscape = _orientation?.Equals("landscape", StringComparison.OrdinalIgnoreCase) == true
                ? true
                : _orientation?.Equals("portrait", StringComparison.OrdinalIgnoreCase) == true
                    ? false
                    : _invoice.SaleItems.Count > LandscapeRowThreshold;
            page.Size(isLandscape ? PageSizes.A4.Landscape() : PageSizes.A4);
            page.Margin(28);
            page.PageColor(Colors.White);
            // RTL paragraph direction for Arabic: the label must sit flush RIGHT with the
            // Latin/digit value to its LEFT ("رقم الفاتوره : Inv-4" reads right-to-left).
            // QuestPDF defaults to LTR, which renders the label LEFT / value RIGHT (reversed
            // for Arabic readers). Table columns are manually mirrored, so the page-level
            // direction only affects intra-line bidi ordering — no column-order regression.
            page.DefaultTextStyle(x =>
            {
                var style = x.FontFamily("Almarai").FontSize(10);
                var isEn = string.Equals(_language, "en", StringComparison.OrdinalIgnoreCase);
                return isEn ? style : style.DirectionFromRightToLeft();
            });
            // The header box is set as the page header so it repeats on EVERY page (natural
            // flow replaces the old manual PageBreak + header re-render per chunk).
            var headerIsEn = string.Equals(_language, "en", StringComparison.OrdinalIgnoreCase);
            var headerLabels = new Print1Labels(headerIsEn);
            page.Header().Element(c => HeaderBox(c, _invoice, _companyDetails, headerIsEn, headerLabels, _sellerName, _timeZone));
            page.Content().Element(c => RenderInvoice(c, _invoice, _companyDetails, _language, _sellerName,
                _showStamp, _showCompanySignature, _showCustomerSignature,
                _uncheckedPolicyIds, _uncheckedBankIds, _timeZone, includeHeaderBox: false));

            // Team-leader note: the address footer stays pinned to the bottom of EVERY page
            // with a thin separator line above it ("الفوتر دايما يبقي في اخر الصفحه").
            var footerIsEn = string.Equals(_language, "en", StringComparison.OrdinalIgnoreCase);
            page.Footer().Element(c => AddressFooter(c, _companyDetails, footerIsEn));
        });
    }

    /// <summary>
    /// Renders the Print-1 layout for one invoice. Shared by the single document and the
    /// merged document (which hosts it inside one page per invoice and keeps the header box
    /// in the flow via <paramref name="includeHeaderBox"/>).
    /// Pagination is NATURAL FLOW: the items table splits across pages automatically, so every
    /// page fills to its physical capacity and no fixed per-page row limits exist.
    /// </summary>
    internal static void RenderInvoice(
        IContainer pageContent,
        SaleInvoice invoice,
        CompanyDetails companyDetails,
        string language,
        string? sellerName,
        bool showStamp,
        bool showCompanySignature,
        bool showCustomerSignature,
        IReadOnlyCollection<Guid>? uncheckedPolicyIds,
        IReadOnlyCollection<Guid>? uncheckedBankIds,
        string timeZone = "Asia/Riyadh",
        bool includeHeaderBox = true)
    {
        var items = invoice.SaleItems.ToList();
        var isEn = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
        var labels = new Print1Labels(isEn);

        pageContent.Column(col =>
        {
            // Section 1 — header box (only when not already provided by page.Header())
            if (includeHeaderBox)
            {
                col.Item().Element(c => HeaderBox(c, invoice, companyDetails, isEn, labels, sellerName, timeZone));
            }

            // Section 2 — customer / supplier box
            col.Item().PaddingTop(6).Element(c => CustomerSupplierBox(c, invoice, companyDetails, isEn, labels, sellerName));

            // Section 3 — items table (ALL rows; QuestPDF splits it across pages at row
            // boundaries, so each page holds as many rows as physically fit — no limits)
            col.Item().PaddingTop(6).Element(c => ItemsTable(c, items, isEn, labels, 0));

            // Section 4 — summary (terms/banks + totals)
            col.Item().PaddingTop(10).EnsureSpace(200).Element(c =>
                Summary(c, invoice, companyDetails, isEn, labels, uncheckedPolicyIds, uncheckedBankIds));

            // Section 5 — signatures (the address footer moved to page.Footer())
            if (showCompanySignature || showCustomerSignature)
            {
                col.Item().PaddingTop(22).EnsureSpace(140).Element(c =>
                    SignatureFooter(c, invoice, companyDetails, isEn, labels, showStamp, showCompanySignature, showCustomerSignature));
            }
        });
    }

    #region Sections

    private static void HeaderBox(IContainer container, SaleInvoice invoice, CompanyDetails company,
        bool isEn, Print1Labels labels, string? sellerName, string timeZone)
    {
        container.Border(1).BorderColor(Colors.Black).Padding(8).Row(row =>
        {
            if (isEn)
            {
                row.RelativeItem(35).AlignMiddle().Element(c => InvoiceDetails(c, invoice, isEn, labels, timeZone, sellerName));
                row.RelativeItem(30).AlignMiddle().Element(c => LogoAndTitle(c, invoice, company, isEn, labels));
                row.RelativeItem(35).AlignMiddle().Element(c => QrAndSeller(c, invoice, isEn, labels, sellerName));
            }
            else
            {
                row.RelativeItem(35).AlignMiddle().Element(c => QrAndSeller(c, invoice, isEn, labels, sellerName));
                row.RelativeItem(30).AlignMiddle().Element(c => LogoAndTitle(c, invoice, company, isEn, labels));
                row.RelativeItem(35).AlignMiddle().Element(c => InvoiceDetails(c, invoice, isEn, labels, timeZone, sellerName));
            }
        });
    }

    private static void InvoiceDetails(IContainer container, SaleInvoice invoice, bool isEn, Print1Labels labels, string timeZone, string? sellerName)
    {
        // P0-3: full-width structured rows — every Arabic header line shares one uniform right
        // edge (the fixed label column's right edge), instead of per-line span widths.
        var seller = !string.IsNullOrWhiteSpace(sellerName) ? sellerName : invoice.CreatedBy;
        container.Column(col =>
        {
            col.Item().Element(c =>
                AddLabelValueRow(c, labels.InvoiceNo, invoice.InvoiceNo, isEn, ltrValue: true, fontSize: 8.5f));
            col.Item().PaddingTop(2).Element(c =>
                AddLabelValueRow(c, labels.Date,
                    TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate)
                        .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    isEn, ltrValue: true, fontSize: 8.5f));
            var hasVat = !string.IsNullOrWhiteSpace(invoice.CustomerProfile?.TaxNumber);
            var localIssueDate = TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate);
            var localDueDate = TimeZoneUtils.ApplyTimeZone(timeZone, invoice.DueDate);
            if (hasVat && invoice.DueDate != default && invoice.DueDate.Year > 1753 && localDueDate.Date != localIssueDate.Date)
            {
                col.Item().PaddingTop(2).Element(c =>
                    AddLabelValueRow(c, labels.DueDate,
                        localDueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        isEn, ltrValue: true, fontSize: 8.5f));
            }
            // Commented out to match frontend parity (seller/createdBy is only shown under QR in top box)
            // if (!string.IsNullOrWhiteSpace(seller))
            // {
            //     col.Item().PaddingTop(2).Element(c =>
            //         AddLabelValueRow(c, labels.Seller, seller, isEn, ltrValue: false, fontSize: 8.5f));
            // }
        });
    }

    private static void LogoAndTitle(IContainer container, SaleInvoice invoice, CompanyDetails company, bool isEn, Print1Labels labels)
    {
        container.AlignCenter().Column(col =>
        {
            if (company?.Logo != null && company.Logo.Length > 0)
            {
                // Frontend parity: the browser renders the logo <img> at natural aspect ratio
                // (global CSS `height: auto`), so constrain only the width — QuestPDF scales the
                // image proportionally. The 210×48 banner therefore renders ~80pt × ~18pt with no
                // white box gap, matching the frontend header. RenderLogo additionally caps the
                // HEIGHT for portrait/huge logos so they can never corrupt the header layout.
                col.Item().AlignCenter().Element(c => RenderLogo(c, company.Logo));
            }
            var hasVat = !string.IsNullOrWhiteSpace(invoice.CustomerProfile?.TaxNumber);
            var title = hasVat ? labels.TaxInvoice : labels.TaxInvoiceSimple;
            col.Item().PaddingTop(2).AlignCenter().Text(title).SemiBold().FontSize(12);
        });
    }

    private static void RenderLogo(IContainer container, byte[] logo)
    {
        var (w, h) = Print1ImageSize.TryGetImageSize(logo);
        // Portrait (h > w) or unknown: hard cap in an 80x80 box (FitArea preserves aspect).
        // Wide banners (h <= w): natural aspect at Width 80 — matches the frontend and avoids
        // the white-gap look inside a fixed box.
        if (w == null || h == null || h > w)
        {
            container.Width(80).Height(80).Image(logo).FitArea();
        }
        else
        {
            container.Width(80).Image(logo);
        }
    }

    private static void QrAndSeller(IContainer container, SaleInvoice invoice, bool isEn, Print1Labels labels, string? sellerName)
    {
        // Alignment is to the CELL END like the frontend's `flex-col items-end`:
        // English/LTR -> right-flush, Arabic/RTL -> left-flush. Note this is the OPPOSITE of
        // TextAlign() (which right-aligns Arabic) — measured against the frontend render, the QR
        // must hug the cell's end edge, not the reading-start edge.
        var seller = !string.IsNullOrWhiteSpace(sellerName) ? sellerName : invoice.CreatedBy;
        (isEn ? container.AlignRight() : container.AlignLeft()).Column(col =>
        {
            if (!string.IsNullOrWhiteSpace(invoice.GeneratedQR))
            {
                col.Item().Element(c => RenderQr(c, invoice.GeneratedQR));
            }
            if (!string.IsNullOrWhiteSpace(seller))
            {
                col.Item().PaddingTop(3).Text(t =>
                {
                    t.Span("Created By : ").SemiBold().FontSize(8.5f);
                    t.Span(seller).FontSize(8.5f);
                });
            }
        });
    }

    private static void CustomerSupplierBox(IContainer container, SaleInvoice invoice, CompanyDetails company,
        bool isEn, Print1Labels labels, string? sellerName)
    {
        container.Border(1).BorderColor(Colors.Black).Table(t =>
        {
            t.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.RelativeColumn();
            });

            if (isEn)
            {
                t.Cell().Element(HeaderRowCell).AlignCenter().Text(labels.CustomerHeader).SemiBold();
                t.Cell().Element(HeaderRowCell).AlignCenter().Text(labels.Supplier).SemiBold();
                t.Cell().Element(DataCell).Element(c => CustomerDetails(c, invoice, isEn, labels));
                t.Cell().Element(DataCell).Element(c => SupplierDetails(c, company, isEn, labels, sellerName, invoice));
            }
            else
            {
                t.Cell().Element(HeaderRowCell).AlignCenter().Text(labels.Supplier).SemiBold();
                t.Cell().Element(HeaderRowCell).AlignCenter().Text(labels.CustomerHeader).SemiBold();
                t.Cell().Element(DataCell).Element(c => SupplierDetails(c, company, isEn, labels, sellerName, invoice));
                t.Cell().Element(DataCell).Element(c => CustomerDetails(c, invoice, isEn, labels));
            }
        });
    }

    private static void CustomerDetails(IContainer container, SaleInvoice invoice, bool isEn, Print1Labels labels)
    {
        var p = invoice.CustomerProfile;
        container.Padding(8).Column(col =>
        {
            AddField(col.Item(), labels.Customer, p != null ? (isEn ? p.EnglishName : p.ArabicName) : null, isEn, boldValue: true);
            AddField(col.Item(), labels.BuildingNumber, p?.BuildingNumber, isEn);
            AddField(col.Item(), labels.Street, p?.Street, isEn);
            AddField(col.Item(), labels.District, p?.District, isEn);
            AddField(col.Item(), labels.City, p?.City, isEn);
            AddField(col.Item(), labels.Country, p?.Country, isEn);
            AddField(col.Item(), labels.PostalCode, p?.PostalCode, isEn);
            AddField(col.Item(), labels.SubNumber, p?.SubNumber, isEn);
            AddField(col.Item(), labels.TaxNumber, p?.TaxNumber, isEn);
            AddField(col.Item(), labels.OtherId, p?.OtherId, isEn);
        });
    }

    private static void SupplierDetails(IContainer container, CompanyDetails company, bool isEn, Print1Labels labels, string? sellerName, SaleInvoice invoice)
    {
        if (company == null) return;
        var seller = !string.IsNullOrWhiteSpace(sellerName) ? sellerName : invoice.CreatedBy;
        container.Padding(8).Column(col =>
        {
            AddField(col.Item(), labels.Name, isEn ? company.EnglishName : company.ArabicName, isEn, boldValue: true);
            AddField(col.Item(), labels.BuildingNumber, company.BuildingNumber, isEn);
            AddField(col.Item(), labels.Street, isEn ? company.Address?.Street : company.ArabicAddress?.Street, isEn);
            AddField(col.Item(), labels.District, company.District, isEn);
            AddField(col.Item(), labels.City, isEn ? company.Address?.City : company.ArabicAddress?.City, isEn);
            AddField(col.Item(), labels.Country, isEn ? company.Address?.Country : company.ArabicAddress?.Country, isEn);
            AddField(col.Item(), labels.PostalCode, isEn ? company.Address?.ZipCode : company.ArabicAddress?.ZipCode, isEn);
            AddField(col.Item(), labels.SubNumber, company.SubNumber, isEn);
            AddField(col.Item(), labels.TaxNumber, company.TaxNumber, isEn);
            AddField(col.Item(), labels.UnifiedNumber, company.UnifiedNumber, isEn);
            // Commented out to match frontend parity (seller/createdBy is only shown under QR in top box)
            // if (!string.IsNullOrWhiteSpace(seller))
            // {
            //     AddField(col.Item(), labels.Seller, seller, isEn);
            // }
        });
    }

    private static void AddField(IContainer container, string label, string? value, bool isEn, bool boldValue = false)
    {
        var v = string.IsNullOrWhiteSpace(value) ? "-" : value;
        // P0-1: three-cell structured row (fixed label column | detached colon | flexible value)
        // instead of one mixed "label : value" span — mirrors the frontend's flex columns.
        container.PaddingTop(4).Element(c =>
            AddLabelValueRow(c, label, v, isEn, boldValue, fontSize: 8.5f));
    }

    private static void ItemsTable(IContainer container, IReadOnlyList<SaleItem> items, bool isEn, Print1Labels labels, int startIndex)
    {
        container.Border(1).BorderColor(Colors.Black).Table(t =>
        {
            t.ColumnsDefinition(cd =>
            {
                // LTR order: # | description | qty | price | total-before-tax | tax% | tax | total
            // Widths tuned (vs the frontend 6/40/7/7/7/7/7/15) so every header label fits on one
            // line in QuestPDF at 7.5pt (Almarai metrics are wider than the browser's).
                if (isEn)
                {
                    cd.RelativeColumn(5);
                    cd.RelativeColumn(35);
                    cd.RelativeColumn(7);
                    cd.RelativeColumn(7);
                    cd.RelativeColumn(12);
                    cd.RelativeColumn(7);
                    cd.RelativeColumn(10);
                    cd.RelativeColumn(17);
                }
                else
                {
                    // RTL mirror of the frontend (columns render right-to-left in the browser).
                    // Arabic labels are wider than English in Almarai, so the tax columns get extra
                    // width (description absorbs the difference) so every header fits on one line.
                    cd.RelativeColumn(16);
                    cd.RelativeColumn(12);
                    cd.RelativeColumn(9);
                    cd.RelativeColumn(15);
                    cd.RelativeColumn(7);
                    cd.RelativeColumn(7);
                    cd.RelativeColumn(30);
                    cd.RelativeColumn(4);
                }
            });

            // Repeat the header row at the top of the table on EVERY page where the table
            // continues (team-leader request) — QuestPDF re-renders the Header section
            // automatically when the table splits across pages.
            t.Header(header => AddItemsHeader(header, isEn, labels));

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var index = startIndex + i + 1;
                var subtotal = Print1ServerMath.ItemSubtotal(item);
                var tax = Print1ServerMath.ItemTax(item);
                var total = Print1ServerMath.ItemTotal(item);
                // P1-8: display the actual rate — 0% for tax-exempt items (frontend hardcodes 15%).
                var taxPercent = Print1ServerMath.TaxRate(item) == 0m ? "0%" : "15%";

                if (isEn)
                {
                    t.Cell().Element(DataCell).AlignCenter().Text(index.ToString(CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignLeft().Element(c => RenderDescription(c, item, isEn));
                    t.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(item.Price.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(subtotal.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(taxPercent).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(tax.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(total.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                }
                else
                {
                    t.Cell().Element(DataCell).AlignCenter().Text(total.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(tax.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(taxPercent).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(subtotal.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(item.Price.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString("0.##", CultureInfo.InvariantCulture)).FontSize(8);
                    t.Cell().Element(DataCell).AlignRight().Element(c => RenderDescription(c, item, isEn));
                    t.Cell().Element(DataCell).AlignCenter().Text(index.ToString(CultureInfo.InvariantCulture)).FontSize(8);
                }
            }
        });
    }

    private static void AddItemsHeader(TableCellDescriptor t, bool isEn, Print1Labels labels)
    {
        // Header cells use a smaller font (7pt) than row cells (8pt) so each label fits on one
        // line — matching the frontend's single-line headers.
        void H(string text)
        {
            t.Cell().Element(HeaderCell).AlignCenter().Text(text).FontSize(7);
        }

        if (isEn)
        {
            H("#");
            H(labels.Description);
            H(labels.Quantity);
            H(labels.Price);
            H(labels.TotalBeforeTax);
            H(labels.TaxPercent);
            H(labels.TaxAmount);
            H(labels.Total);
        }
        else
        {
            H(labels.Total);
            H(labels.TaxAmount);
            H(labels.TaxPercent);
            H(labels.TotalBeforeTax);
            H(labels.Price);
            H(labels.Quantity);
            H(labels.Description);
            H("#");
        }
    }

    private static void Summary(IContainer container, SaleInvoice invoice, CompanyDetails company, bool isEn,
        Print1Labels labels, IReadOnlyCollection<Guid>? uncheckedPolicyIds, IReadOnlyCollection<Guid>? uncheckedBankIds)
    {
        container.Row(row =>
        {
            if (isEn)
            {
                row.RelativeItem().PaddingRight(10).Element(c => TermsAndBanks(c, company, isEn, labels, uncheckedPolicyIds, uncheckedBankIds));
                // Frontend summaryTemplate2 uses width:230px CSS for the totals table — 230 CSS px
                // ≈ 172.5 PDF pt. Team-leader note: bigger table (215pt) so the Arabic labels
                // (الإجمالي قبل الضريبة / ضريبة القيمة المضافة (15%)) stay on ONE line; the label
                // column gets the majority (5:3) — the "تضيق خانات الارقام" part.
                row.ConstantItem(215).AlignTop().Element(c => TotalsTable(c, invoice, isEn, labels));
            }
            else
            {
                row.ConstantItem(215).AlignTop().Element(c => TotalsTable(c, invoice, isEn, labels));
                row.RelativeItem().PaddingLeft(10).Element(c => TermsAndBanks(c, company, isEn, labels, uncheckedPolicyIds, uncheckedBankIds));
            }
        });
    }

    private static void TermsAndBanks(IContainer container, CompanyDetails company, bool isEn, Print1Labels labels,
        IReadOnlyCollection<Guid>? uncheckedPolicyIds, IReadOnlyCollection<Guid>? uncheckedBankIds)
    {
        if (company == null) return;

        var policies = company.Policies
            .Where(p => !string.IsNullOrWhiteSpace(p.PolicyParagraph))
            .Where(p => uncheckedPolicyIds == null || !uncheckedPolicyIds.Contains(p.Id))
            .ToList();
        var banks = company.BankAccounts
            .Where(b => !string.IsNullOrWhiteSpace(b.BankName) || !string.IsNullOrWhiteSpace(b.BankAccountNumber) || !string.IsNullOrWhiteSpace(b.IBAN) || !string.IsNullOrWhiteSpace(b.SwiftCode))
            .Where(b => uncheckedBankIds == null || !uncheckedBankIds.Contains(b.Id))
            .ToList();
        if (policies.Count == 0 && banks.Count == 0) return;

        TextAlign(container, isEn).Column(col =>
        {
            if (policies.Count > 0)
            {
                col.Item().Element(c => TextAlign(c, isEn).Text(labels.Terms).SemiBold().FontSize(9));
                foreach (var p in policies)
                {
                    col.Item().PaddingTop(3).Element(c => TextAlign(c, isEn).Text(p.PolicyParagraph).FontSize(8.5f));
                }
            }
            if (banks.Count > 0)
            {
                col.Item().PaddingTop(6).Element(c => TextAlign(c, isEn).Text(labels.Banks).SemiBold().FontSize(9));
                foreach (var b in banks)
                {
                    col.Item().PaddingTop(3).Element(c => AddBankLine(c, b, isEn));
                }
            }
        });
    }

    /// <summary>
    /// Renders one bank account as ONE inline (wrapping) line matching the frontend's
    /// flex-wrap bank line: "اسم الحساب: name - bank : number (IBAN: …) (SWIFT: …)".
    /// Empty parts are omitted; separators/parentheses are separate spans. For Arabic the
    /// groups are emitted in REVERSE visual order (SWIFT/IBAN first, then bank pair, then
    /// اسم الحساب last) — QuestPDF lays spans in emission order within the RTL paragraph,
    /// so the first-emitted group lands leftmost and the label lands rightmost, mirroring
    /// the frontend's right-to-left reading.
    /// </summary>
    private static void AddBankLine(IContainer container, BankAccount b, bool isEn)
    {
        var accountName = b.AccountName ?? "";
        var bankName = b.BankName ?? "";
        var accountNumber = b.BankAccountNumber ?? "";
        var iban = b.IBAN ?? "";
        var swift = b.SwiftCode ?? "";

        // Two intentional lines (frontend flex-wrap parity, user-verified): the identity line
        // keeps "اسم الحساب"/"Account Name" FIRST (rightmost in RTL) and the IBAN/SWIFT groups
        // live on their own line, so a long account name can never push the label to line 2.
        container.Column(col =>
        {
            var hasIdentity = !string.IsNullOrWhiteSpace(accountName)
                || !string.IsNullOrWhiteSpace(bankName)
                || !string.IsNullOrWhiteSpace(accountNumber);

            if (hasIdentity)
            {
                col.Item().Text(t =>
                {
                    if (isEn)
                    {
                        // LTR logical order: Account Name: {name} - {bank} : {number}
                        if (!string.IsNullOrWhiteSpace(accountName))
                        {
                            t.Span("Account Name: ").SemiBold().FontSize(8.5f);
                            t.Span(accountName).FontSize(8.5f);
                        }
                        if (!string.IsNullOrWhiteSpace(bankName) || !string.IsNullOrWhiteSpace(accountNumber))
                        {
                            if (!string.IsNullOrWhiteSpace(accountName)) t.Span("  -  ").FontSize(8.5f);
                            if (!string.IsNullOrWhiteSpace(bankName)) t.Span(bankName).SemiBold().FontSize(8.5f);
                            if (!string.IsNullOrWhiteSpace(bankName) && !string.IsNullOrWhiteSpace(accountNumber)) t.Span(" : ").FontSize(8.5f);
                            if (!string.IsNullOrWhiteSpace(accountNumber)) t.Span(accountNumber).FontSize(8.5f);
                        }
                    }
                    else
                    {
                        // QuestPDF lays the spans left-to-right IN EMISSION ORDER (measured), so the
                        // label must be emitted LAST to land rightmost. Desired visual (LTR):
                        // {number} | : {bank} | - | {name} | : اسم الحساب.
                        if (!string.IsNullOrWhiteSpace(accountNumber))
                        {
                            t.Span(accountNumber).FontSize(8.5f);
                        }
                        if (!string.IsNullOrWhiteSpace(bankName))
                        {
                            if (!string.IsNullOrWhiteSpace(accountNumber)) t.Span(" : ").FontSize(8.5f);
                            t.Span(bankName).SemiBold().FontSize(8.5f);
                        }
                        if (!string.IsNullOrWhiteSpace(accountName)
                            && (!string.IsNullOrWhiteSpace(bankName) || !string.IsNullOrWhiteSpace(accountNumber)))
                        {
                            t.Span("  -  ").FontSize(8.5f);
                        }
                        if (!string.IsNullOrWhiteSpace(accountName))
                        {
                            t.Span(accountName).FontSize(8.5f);
                            t.Span(" : اسم الحساب").SemiBold().FontSize(8.5f);
                        }
                    }
                });
            }

            var hasIbanSwift = !string.IsNullOrWhiteSpace(iban) || !string.IsNullOrWhiteSpace(swift);
            if (hasIbanSwift)
            {
                col.Item().PaddingTop(2).Text(t =>
                {
                    void Group(string open, string value, string close)
                    {
                        t.Span(open).SemiBold().FontSize(8.5f);
                        t.Span(value).FontSize(8.5f);
                        t.Span(close).FontSize(8.5f);
                    }

                    if (isEn)
                    {
                        if (!string.IsNullOrWhiteSpace(iban))
                        {
                            Group("(IBAN: ", iban, ")");
                        }
                        if (!string.IsNullOrWhiteSpace(swift))
                        {
                            if (!string.IsNullOrWhiteSpace(iban)) t.Span("  -  ").FontSize(8.5f);
                            Group("(SWIFT: ", swift, ")");
                        }
                    }
                    else
                    {
                        // Reversed Latin emission: SWIFT leftmost, IBAN to its right.
                        if (!string.IsNullOrWhiteSpace(swift))
                        {
                            Group("(SWIFT: ", swift, ")");
                        }
                        if (!string.IsNullOrWhiteSpace(iban))
                        {
                            if (!string.IsNullOrWhiteSpace(swift)) t.Span("  -  ").FontSize(8.5f);
                            Group("(IBAN: ", iban, ")");
                        }
                    }
                });
            }
        });
    }

    private static void TotalsTable(IContainer container, SaleInvoice invoice, bool isEn, Print1Labels labels)
    {
        var net = Print1ServerMath.NetTotal(invoice);
        var tax = Print1ServerMath.TaxTotal(invoice);
        var total = Print1ServerMath.TotalPrice(invoice);
        var discount = invoice.DiscountAmount;

        // Slimmer cells for the totals table (Padding 4, 8.5pt) so the longer Arabic labels
        // (الإجمالي قبل الضريبة / ضريبة القيمة المضافة (15%)) stay on ONE line inside the
        // frontend-calibrated 172pt-wide table (230 CSS px).
        IContainer TotalsCell(IContainer c) =>
            c.Border(1).BorderColor(Colors.Black).Padding(4).DefaultTextStyle(t => t.FontSize(8));

        container.Border(1).BorderColor(Colors.Black).Table(t =>
        {
            t.ColumnsDefinition(cd =>
            {
                // Positional columns: QuestPDF lays cells in EMISSION order, and the Arabic
                // branch emits the VALUE cell first (mirror). So the label needs the WIDE
                // column in the position where its cell lands: English = first (label first),
                // Arabic = second (label emitted last). 7:3 keeps the long Arabic totals
                // labels (الإجمالي قبل الضريبة) on ONE line while the value cells stay
                // narrow ("تضيق خانات الارقام").
                if (isEn)
                {
                    cd.RelativeColumn(7); // label
                    cd.RelativeColumn(3); // value
                }
                else
                {
                    cd.RelativeColumn(3); // value (emitted first -> left)
                    cd.RelativeColumn(7); // label (emitted last -> right)
                }
            });

            void Row(string labelText, string value, bool bold = false)
            {
                // The totals labels are PURE Arabic (or pure English) — no mixed-direction
                // content — so they don't need RTL paragraph direction. QuestPDF's
                // DirectionFromRightToLeft line-breaker splits multi-word Arabic labels onto a
                // second line even when the text easily fits (measured 44pt text in a 93pt
                // column). Rendering these cells LTR (glyphs still shape correctly via
                // HarfBuzz; AlignRight places them on the right) avoids the buggy breaker.
                var labelCellStyle = TextStyle.Default.FontSize(8).DirectionFromLeftToRight();
                if (isEn)
                {
                    // LTR frontend: label column LEFT, value column RIGHT.
                    var labelCell = t.Cell().Element(TotalsCell).AlignLeft().DefaultTextStyle(labelCellStyle);
                    labelCell.Text(labelText).SemiBold();
                    var valueCell = t.Cell().Element(TotalsCell).AlignCenter();
                    if (bold) valueCell.Text(value).SemiBold();
                    else valueCell.Text(value);
                }
                else
                {
                    // RTL mirror (frontend summaryTemplate2 renders the label column on the
                    // RIGHT): QuestPDF tables lay cells LTR, so emit the VALUE cell first
                    // (leftmost) and the LABEL cell second (rightmost, right-aligned).
                    var valueCell = t.Cell().Element(TotalsCell).AlignCenter();
                    if (bold) valueCell.Text(value).SemiBold();
                    else valueCell.Text(value);
                    var labelCell = t.Cell().Element(TotalsCell).AlignRight().DefaultTextStyle(labelCellStyle);
                    labelCell.Text(labelText).SemiBold();
                }
            }

            Row(labels.TotalBeforeTax, net.ToString("0.##", CultureInfo.InvariantCulture));
            if (discount > 0) Row(labels.Discount, discount.ToString("0.##", CultureInfo.InvariantCulture));
            if (tax > 0) Row(labels.AddedTax, tax.ToString("0.##", CultureInfo.InvariantCulture));
            Row(labels.TotalWithTax, total.ToString("0.##", CultureInfo.InvariantCulture), bold: true);
        });
    }

    private static void SignatureFooter(IContainer container, SaleInvoice invoice, CompanyDetails company, bool isEn,
        Print1Labels labels, bool showStamp, bool showCompanySignature, bool showCustomerSignature)
    {
        // Frontend parity (footerTemplate2): columns render conditionally — both → 30%|40%|30%,
        // otherwise a single 100% column. The caller omits the box entirely when neither flag is set.
        // User note: remove the signature-section border entirely — no box around the signatures.
        container.Padding(10).Row(row =>
        {
            if (showCompanySignature && showCustomerSignature)
            {
                if (isEn)
                {
                    row.RelativeItem(3).Element(c => CompanySignature(c, company, isEn, labels, showStamp));
                    row.RelativeItem(4);
                    row.RelativeItem(3).Element(c => CustomerSignature(c, invoice, isEn, labels));
                }
                else
                {
                    row.RelativeItem(3).Element(c => CustomerSignature(c, invoice, isEn, labels));
                    row.RelativeItem(4);
                    row.RelativeItem(3).Element(c => CompanySignature(c, company, isEn, labels, showStamp));
                }
            }
            else if (showCompanySignature)
            {
                row.RelativeItem().Element(c => CompanySignature(c, company, isEn, labels, showStamp));
            }
            else
            {
                row.RelativeItem().Element(c => CustomerSignature(c, invoice, isEn, labels));
            }
        });
    }

    private static void CompanySignature(IContainer container, CompanyDetails company, bool isEn, Print1Labels labels, bool showStamp)
    {
        if (company == null) return;
        TextAlign(container, isEn).Column(col =>
        {
            col.Item().AlignCenter().Text(isEn ? company.EnglishName : company.ArabicName).SemiBold().FontSize(10);
            col.Item().PaddingTop(6).Element(c => SignatureLine(c, labels.Signature, isEn));
            if (showStamp && company.Stamp != null && company.Stamp.Length > 0)
            {
                // P0-2: frontend applies margin-right 40px in Arabic (≈30pt) so the stamp shifts
                // left of the right content edge; English keeps margin 0.
                col.Item().PaddingTop(6).PaddingRight(isEn ? 0 : 30).Width(70).Height(70).Image(company.Stamp).FitArea();
            }
        });
    }

    private static void CustomerSignature(IContainer container, SaleInvoice invoice, bool isEn, Print1Labels labels)
    {
        // Frontend getCustomerName(): EnglishName when isEn else ArabicName (profile may be null)
        var p = invoice.CustomerProfile;
        var customerName = p != null
            ? (isEn ? (p.EnglishName ?? p.ArabicName) : (p.ArabicName ?? p.EnglishName))
            : "";
        TextAlign(container, isEn).Column(col =>
        {
            col.Item().AlignCenter().Text(BidiValue(customerName)).SemiBold().FontSize(10);
            col.Item().PaddingTop(6).Element(c => SignatureLine(c, labels.Signature, isEn));
        });
    }

    /// <summary>
    /// Address bar pinned to the bottom of every page (page.Footer) with a thin separator
    /// line above it. Always reads the ARABIC address first (fallback English) per the
    /// team-leader note ("الفوتر يقرا من العربي" — most invoices are Arabic). Slate thin
    /// border like the frontend's rounded-2xl border-slate-900 (no CornerRadius in
    /// QuestPDF 2025.5.1). Internal so the merged document can host it in its own Footer.
    /// </summary>
    internal static void AddressFooter(IContainer container, CompanyDetails company, bool isEn)
    {
        var address = company?.ArabicAddress?.FullAddress;
        if (string.IsNullOrWhiteSpace(address)) address = company?.Address?.FullAddress;
        container.Column(col =>
        {
            // Thin separator line above the footer line (team-leader requirement).
            col.Item().BorderTop(0.75f).BorderColor("#64748b");
            if (!string.IsNullOrWhiteSpace(address))
            {
                // User note: one plain line instead of the bordered box.
                col.Item().PaddingTop(2).AlignCenter().Text(address).FontSize(8.5f);
            }
            // Page numbering in the pinned footer of every page (user request): "صفحة X من Y"
            // (Arabic, RTL paragraph) / "Page X of Y" (English). Always rendered, even when the
            // company has no address. (Text(lambda) returns void in QuestPDF 2025.5.1, so the
            // font size is set via DefaultTextStyle inside the lambda.)
            col.Item().PaddingTop(2).AlignCenter().Text(t =>
            {
                t.DefaultTextStyle(TextStyle.Default.FontSize(8));
                if (isEn) { t.Span("Page "); t.CurrentPageNumber(); t.Span(" of "); t.TotalPages(); }
                else { t.Span("صفحة "); t.CurrentPageNumber(); t.Span(" من "); t.TotalPages(); }
            });
        });
    }

    #endregion

    #region Helpers

    private static IContainer TextAlign(IContainer container, bool isEn) =>
        isEn ? container.AlignLeft() : container.AlignRight();

    /// <summary>
    /// P0-1: three-cell structured label/value row mirroring the frontend's flex columns
    /// (fixed label column | detached colon | flexible value). QuestPDF rows lay cells LTR, so
    /// for Arabic the VALUE cell is emitted first (lands left), the colon centered, and the
    /// LABEL cell last (right-aligned) — every line shares one uniform right edge.
    /// </summary>
    private static void AddLabelValueRow(IContainer container, string label, string value, bool isEn, bool boldValue = false, bool ltrValue = false, float fontSize = 8.5f)
    {
        // Frontend label column is 72 CSS px ≈ 54pt; 64pt keeps the longest header label
        // (تاريخ الاستحقاق) on one line in Almarai 9pt.
        const float labelWidth = 64f;
        const float colonWidth = 12f;

        var v = string.IsNullOrWhiteSpace(value) ? "-" : value;

        // NOTE: the LTR direction for Latin values (dates, invoice no) is applied via
        // TextDescriptor.DefaultTextStyle INSIDE the Text action — calling
        // IContainer.DefaultTextStyle separately consumes the container's single child slot
        // and throws "multiple child elements" when .Text() is added afterwards.
        void ValueText(IContainer valueCell)
        {
            valueCell.Text(t =>
            {
                if (ltrValue) t.DefaultTextStyle(s => s.DirectionFromLeftToRight());
                if (boldValue) t.Span(v).SemiBold().FontSize(fontSize);
                else t.Span(v).FontSize(fontSize);
            });
        }

        container.Row(row =>
        {
            if (isEn)
            {
                row.ConstantItem(labelWidth).AlignLeft().Text(t => t.Span(label).SemiBold().FontSize(fontSize));
                row.ConstantItem(colonWidth).AlignCenter().Text(t => t.Span(":").FontSize(fontSize));
                ValueText(row.RelativeItem().AlignLeft());
            }
            else
            {
                // Visual RTL: [value | colon | label].
                ValueText(row.RelativeItem().AlignRight());
                row.ConstantItem(colonWidth).AlignCenter().Text(t => t.Span(":").FontSize(fontSize));
                row.ConstantItem(labelWidth).AlignRight().Text(t => t.Span(label).SemiBold().FontSize(fontSize));
            }
        });
    }

    /// <summary>
    /// P0-2: structured signature line. Arabic: the dotted line occupies the left/flexible area
    /// and extends LEFT from the label (dots right-aligned so they hug the label); the label sits
    /// at the far right. English is mirrored (label left, dots extending right).
    /// </summary>
    private static void SignatureLine(IContainer container, string label, bool isEn)
    {
        const string dots = "..................................................";
        // A Row inside a column item already fills the available width.
        container.Row(row =>
        {
            if (isEn)
            {
                row.AutoItem().PaddingRight(4).Text(t => t.Span(label + ":").FontSize(9));
                row.RelativeItem().AlignLeft().Text(t => t.Span(dots).FontSize(9));
            }
            else
            {
                row.RelativeItem().AlignRight().Text(t => t.Span(dots).FontSize(9));
                row.AutoItem().PaddingLeft(4).Text(t => t.Span(label + ":").FontSize(9));
            }
        });
    }

    /// <summary>
    /// Returns the value unchanged. The Arabic page already sets paragraph
    /// <c>DirectionFromRightToLeft()</c>; Unicode bidi ISOLATES (LRI/PDI) were previously tried
    /// but QuestPDF mishandles them inside RTL paragraphs — the value must stay plain so span
    /// ordering (see <see cref="AddLabelValueLine"/>) controls placement.
    /// </summary>
    private static string BidiValue(string? value) => value ?? "";

    private static IContainer HeaderRowCell(IContainer c) =>
        c.Background("#e5e7eb").Border(1).BorderColor(Colors.Black).Padding(5);

    private static IContainer HeaderCell(IContainer c) =>
        c.Background("#42acff").Border(1).BorderColor(Colors.Black).Padding(5)
            .DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White));

    private static IContainer DataCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Black).Padding(5);

    private static void RenderQr(IContainer container, string qrData)
    {
        try
        {
            var qrGenerator = new QRCodeGenerator();
            var qrDataObj = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.L);
            var qrPng = new PngByteQRCode(qrDataObj);
            var qrBytes = qrPng.GetGraphic(10);
            // Team-leader note: QR a bit bigger (52pt) with the FitArea box as the hard SIZE
            // LIMIT — the QR can never exceed the box even when ZATCA data grows the module count.
            container.Width(52).Height(52).Image(qrBytes).FitArea();
        }
        catch
        {
            // QR rendering is best-effort — never fail the whole document for a QR.
        }
    }

    private static void RenderDescription(IContainer container, SaleItem item, bool isEn)
    {
        // Frontend parity (mapInvoiceItemsForFrontendPrint): itemName picks the localized
        // MainSalesClass name — English for en (fallback Arabic), Arabic otherwise (fallback English).
        // P1-5: name and description are separate spans (frontend rowTemplate2 splits them).
        var name = item.MainSalesClass != null
            ? (isEn
                ? (string.IsNullOrWhiteSpace(item.MainSalesClass.EnglishName) ? item.MainSalesClass.ArabicName : item.MainSalesClass.EnglishName)
                : (string.IsNullOrWhiteSpace(item.MainSalesClass.ArabicName) ? item.MainSalesClass.EnglishName : item.MainSalesClass.ArabicName))
            : item.Name ?? "";
        container.Text(t =>
        {
            t.Span(name).SemiBold().FontSize(8);
            var description = item.Name;
            if (!string.IsNullOrWhiteSpace(description) && description != "-")
            {
                t.Span(" - ").FontSize(8);
                t.Span(description).FontSize(8);
            }
        });
    }


    #endregion
}

/// <summary>
/// Frontend-exact calculations for the Print-1 document (sales-invoice-list.component.ts:
/// getItemNetPrice / getItemTaxAmount / getItemTotalPrice / getNetPrice / getTaxAmount / getTotalPrice).
/// Public so unit tests can assert parity with the frontend formulas.
/// </summary>
public static class Print1ServerMath
{
    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    public static decimal TaxRate(SaleItem item) => item.MainSalesClass?.IsTaxExempted == true ? 0m : 0.15m;

    /// <summary>Unrounded line subtotal (frontend uses raw value in invoice-level sums).</summary>
    public static decimal Subtotal(SaleItem item) => item.Price * item.Quantity * item.NoOfNights;

    /// <summary>Frontend getItemNetPrice — per-row total before tax (rounded).</summary>
    public static decimal ItemSubtotal(SaleItem item) => Round2(Subtotal(item));

    /// <summary>Frontend getItemTaxAmount — per-row tax on the rounded subtotal (no discount allocation).</summary>
    public static decimal ItemTax(SaleItem item) => Round2(ItemSubtotal(item) * TaxRate(item));

    /// <summary>Frontend getItemTotalPrice — per-row total.</summary>
    public static decimal ItemTotal(SaleItem item) => ItemSubtotal(item) + ItemTax(item);

    /// <summary>Frontend getNetPrice — Σ raw subtotals − discount, rounded.</summary>
    public static decimal NetTotal(SaleInvoice invoice) =>
        Round2(invoice.SaleItems.Sum(Subtotal) - invoice.DiscountAmount);

    /// <summary>Frontend getTaxAmount — proportional discount allocation; early-return 0 when invoice.TaxAmount == 0.</summary>
    public static decimal TaxTotal(SaleInvoice invoice)
    {
        if (invoice.TaxAmount == 0) return 0m;

        var items = invoice.SaleItems.ToList();
        var subtotals = items.Select(Subtotal).ToList();
        var grossTotal = subtotals.Sum();
        var discount = invoice.DiscountAmount;

        decimal totalTax = 0m;
        for (var i = 0; i < items.Count; i++)
        {
            var rate = TaxRate(items[i]);
            var itemDiscount = grossTotal > 0 ? subtotals[i] / grossTotal * discount : 0m;
            var taxableBase = Math.Max(subtotals[i] - itemDiscount, 0m);
            totalTax += taxableBase * rate;
        }
        return Round2(totalTax);
    }

    /// <summary>Frontend getTotalPrice — net + tax.</summary>
    public static decimal TotalPrice(SaleInvoice invoice) => Round2(NetTotal(invoice) + TaxTotal(invoice));
}

/// <summary>
/// Reads PNG/JPEG pixel dimensions from the raw byte header (no System.Drawing
/// dependency). Returns (null, null) for unknown/corrupt data.
/// </summary>
public static class Print1ImageSize
{
    public static (int? Width, int? Height) TryGetImageSize(byte[] data)
    {
        if (data == null || data.Length < 24) return (null, null);

        // PNG: 8-byte signature, then IHDR chunk: width (4 big-endian) @16, height @20.
        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            int w = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
            int h = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
            if (w > 0 && h > 0) return (w, h);
            return (null, null);
        }

        // JPEG: SOI (FF D8), then scan markers; SOF0..SOF15 carry dimensions (skip C4/C8/CC).
        if (data[0] == 0xFF && data[1] == 0xD8)
        {
            int i = 2;
            while (i + 9 < data.Length)
            {
                if (data[i] != 0xFF) { i++; continue; }
                byte marker = data[i + 1];
                if (marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC)
                {
                    int h = (data[i + 5] << 8) | data[i + 6];
                    int w = (data[i + 7] << 8) | data[i + 8];
                    if (w > 0 && h > 0) return (w, h);
                    return (null, null);
                }
                int len = (data[i + 2] << 8) | data[i + 3];
                if (len < 2) return (null, null);
                i += 2 + len;
            }
        }

        return (null, null);
    }
}

/// <summary>Localized labels matching the frontend i18n keys used by the Print-1 template.</summary>
internal sealed class Print1Labels
{
    private readonly bool _en;
    public Print1Labels(bool isEn) => _en = isEn;

    private string L(string ar, string en) => _en ? en : ar;

    public string InvoiceNo => L("رقم الفاتورة", "InvoiceNo.");
    public string Date => L("تاريخ الإصدار", "Issue Date");
    public string DueDate => L("تاريخ الاستحقاق", "Due Date");
    public string TaxInvoice => L("فاتورة ضريبية", "TAX INVOICE");
    public string TaxInvoiceSimple => L("فاتورة ضريبية مبسطة", "SIMPLIFIED TAX INVOICE");
    public string Customer => L("العميل", "Client Name");

    /// <summary>Customer-column HEADER label (team-leader note: "خلي اسم العميل في الهيدر العميل").</summary>
    public string CustomerHeader => L("العميل", "Client Name");
    public string Supplier => L("المورد", "Supplier");
    public string Name => L("الاسم", "Name");
    public string BuildingNumber => L("رقم المبنى", "Building Number");
    public string Street => L("اسم الشارع", "Street");
    public string District => L("الحي", "District");
    public string City => L("المدينة", "City");
    public string Country => L("البلد", "Country");
    public string PostalCode => L("الرمز البريدي", "Postal Code");
    public string SubNumber => L("الرقم الفرعي", "Sub Number");
    public string TaxNumber => L("الرقم الضريبي", "Tax Number");
    public string OtherId => L("معرف آخر", "Other ID");
    public string UnifiedNumber => L("الرقم الموحد", "Unified Number");
    public string Description => L("الوصف", "Description");
    public string Quantity => L("الكمية", "Quantity");
    public string Price => L("السعر", "Price");
    public string TotalBeforeTax => L("الإجمالي قبل الضريبة", "Total Before Tax");
    public string TaxPercent => L("نسبة الضريبة", "Tax %");
    public string TaxAmount => L("قيمة الضريبة", "Tax Amount");
    public string Total => L("الإجمالي", "Total");
    public string Discount => L("الخصم", "Discount");
    public string AddedTax => L("ضريبة القيمة المضافة (15%)", "VAT (15%)");
    public string TotalWithTax => L("الإجمالي شامل الضريبة", "Total (Incl. VAT)");
    public string Seller => L("البائع", "Seller");
    public string Signature => L("التوقيع", "Signature");
    public string Terms => L("الشروط والأحكام:", "Terms & Conditions:");
    public string Banks => L("الحسابات البنكية للشركة", "Company Bank Accounts");
}
