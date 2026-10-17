using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices
{
    public interface IHotelAllotmentInvoiceService
    {
        Task<byte[]?> GetDocument(int invoiceId, InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null);
        Task<byte[][]> GetDocuments(int[] invoiceIds, InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null);
        Task<byte[][]> GetAllDocuments(InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null);
        Task<byte[]> GetMergedDocument(int[] invoiceIds, InvoicePrintType printType = InvoicePrintType.A4Portrait, string? sellerName = null);
    }
}
