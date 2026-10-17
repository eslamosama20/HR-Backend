using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;

/// <summary>
/// Merged (single-file) variant of the server-side Print 1 replica — one page per invoice,
/// each page sized by that invoice's item count (&gt; 8 → landscape), reusing
/// <see cref="SalesInvoicePrint1ServerDocument.RenderInvoice"/> for the per-invoice layout.
/// </summary>
public class CombinedSalesInvoicesPrint1ServerDocument : IDocument
{
    private readonly List<SaleInvoice> _invoices;
    private readonly CompanyDetails _companyDetails;
    private readonly string _language;
    private readonly string? _sellerName;
    private readonly bool _showStamp;
    private readonly bool _showCompanySignature;
    private readonly bool _showCustomerSignature;
    private readonly IReadOnlyCollection<Guid>? _uncheckedPolicyIds;
    private readonly IReadOnlyCollection<Guid>? _uncheckedBankIds;
    private readonly string _timeZone;
    private readonly string? _orientation;

    public CombinedSalesInvoicesPrint1ServerDocument(
        IEnumerable<SaleInvoice> invoices,
        CompanyDetails companyDetails,
        string language = "ar",
        string? sellerName = null,
        bool showStamp = true,
        bool showCompanySignature = true,
        bool showCustomerSignature = true,
        IReadOnlyCollection<Guid>? uncheckedPolicyIds = null,
        IReadOnlyCollection<Guid>? uncheckedBankIds = null,
        string timeZone = "Asia/Riyadh",
        string? orientation = null)
    {
        _invoices = invoices?.ToList() ?? new List<SaleInvoice>();
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
        foreach (var invoice in _invoices.OrderByDescending(i => i.IssueDate))
        {
            container.Page(page =>
            {
                // User-forced orientation wins; otherwise per-invoice auto-detect mirrors
                // the frontend useLandscape (items > 8 → landscape).
                var isLandscape = _orientation?.Equals("landscape", StringComparison.OrdinalIgnoreCase) == true
                    ? true
                    : _orientation?.Equals("portrait", StringComparison.OrdinalIgnoreCase) == true
                        ? false
                        : invoice.SaleItems.Count > SalesInvoicePrint1ServerDocument.LandscapeRowThreshold;
                page.Size(isLandscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(28);
                page.PageColor(Colors.White);
                // RTL paragraph direction for Arabic (same rationale as the single document).
                page.DefaultTextStyle(x =>
                {
                    var style = x.FontFamily("Almarai").FontSize(10);
                    var isEn = string.Equals(_language, "en", StringComparison.OrdinalIgnoreCase);
                    return isEn ? style : style.DirectionFromRightToLeft();
                });
                page.Content().Element(c => SalesInvoicePrint1ServerDocument.RenderInvoice(
                    c, invoice, _companyDetails, _language, _sellerName,
                    _showStamp, _showCompanySignature, _showCustomerSignature,
                    _uncheckedPolicyIds, _uncheckedBankIds, _timeZone));

                // Same pinned address footer as the single document (team-leader note).
                var footerIsEn = string.Equals(_language, "en", StringComparison.OrdinalIgnoreCase);
                page.Footer().Element(c => SalesInvoicePrint1ServerDocument.AddressFooter(c, _companyDetails, footerIsEn));
            });
        }
    }
}
