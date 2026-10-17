using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;

namespace SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices.DocumentUtils;

public class MergedHotelAllotmentInvoiceDocument : IDocument
{
    private readonly List<HotelAllotmentInvoice> _invoices;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly string? _sellerName;
    private readonly string? _timeZone;
    private readonly bool _isLandscape;

    public MergedHotelAllotmentInvoiceDocument(
        IEnumerable<HotelAllotmentInvoice> invoices,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        string? sellerName = null,
        string? timeZone = null,
        bool isLandscape = false)
    {
        _invoices = invoices.ToList();
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
        _sellerName = sellerName;
        _timeZone = timeZone;
        _isLandscape = isLandscape;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        foreach (var invoice in _invoices)
        {
            container.Page(page =>
            {
                page.Size(_isLandscape ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                page.Header().Element(c => HotelAllotmentInvoiceDocument.ComposeHeader(c, _companyDetails));
                page.Footer().Element(c => HotelAllotmentInvoiceDocument.ComposeFooter(c, _companyDetails));

                page.Content().Column(col =>
                {
                    col.Item().Element(c => HotelAllotmentInvoiceDocument.ComposeInvoiceInfo(c, invoice, _sellerName, _timeZone));
                    col.Item().PaddingTop(4).Element(c => HotelAllotmentInvoiceDocument.ComposeRoomCostTable(c, invoice));
                    col.Item().PaddingTop(4).Element(c => HotelAllotmentInvoiceDocument.ComposeTotals(c, invoice));
                    col.Item().PaddingTop(8).Element(c => HotelAllotmentInvoiceDocument.ComposeSignatures(c, invoice, _companyDetails));
                });
            });
        }
    }
}
