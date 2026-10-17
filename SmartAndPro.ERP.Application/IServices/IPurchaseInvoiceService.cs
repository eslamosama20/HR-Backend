using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.IServices
{
    public interface IPurchaseInvoiceService
    {
        Task<ProductBarcodeResultDto?> GetProductByBarcodeAsync(string barcode);

        /// <summary>
        /// Retrieves the details of a specified purchase invoice, including supplier information, 
        /// issue date, description, and a list of item details.
        /// </summary>
        /// <param name="getPurchaseInvoiceDetailsRequest">
        /// An object containing the request parameters to identify the purchase invoice to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="PurchaseInvoiceDetailsResponse"/> containing details about the invoice, 
        /// including the invoice number, supplier name, issue date, description, and a list of items with their prices and quantities.
        /// </returns>
        OperationResponse<List<PurchaseInvoiceDetailsResponse>> GetPurchaseInvoiceDetails(GetPurchaseInvoiceDetailsRequest getPurchaseInvoiceDetails);
        
        /// <summary>
        /// Retrieves VAT details for purchase invoices within the specified date range.
        /// </summary>
        /// <param name="request">Request containing date range and optional store filter</param>
        /// <returns>List of purchase VAT details with item-level breakdown</returns>
        OperationResponse<List<PurchaseVATDetailsResponse>> GetPurchaseVATDetails(GetPurchaseVATDetailsRequest request);
        
        /// <summary>
        /// Generates a PDF document for purchase invoice report within the specified date range.
        /// </summary>
        /// <param name="startDate">Start date for the report</param>
        /// <param name="endDate">End date for the report</param>
        /// <param name="language">Language for the report (ar or en). Default is ar (RTL). en uses LTR order.</param>
        /// <returns>PDF document as byte array</returns>
        Task<byte[]> GetDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
        
        /// <summary>
        /// Generates a summary PDF document for purchase invoice report with totals only.
        /// </summary>
        /// <param name="startDate">Start date for the report</param>
        /// <param name="endDate">End date for the report</param>
        /// <param name="language">Language for the report (ar or en). Default is ar.</param>
        /// <returns>Summary PDF document as byte array</returns>
        Task<byte[]> GetSummaryDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
        
        /// <summary>
        /// Exports purchase VAT details to Excel format
        /// </summary>
        byte[] ExportPurchaseVATDetails(GetPurchaseVATDetailsRequest getVATDetails);
        
        /// <summary>
        /// Generates a PDF document for VAT report
        /// </summary>
        Task<byte[]> GetVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar");

        /// <summary>
        /// Generates a PDF document for Purchase Invoice Summary report
        /// </summary>
        Task<byte[]> GetPurchaseInvoiceSummaryDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
        
        /// <summary>
        /// Gets Net Purchase VAT Report (Purchases VAT - Returns VAT)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="language">Language (ar/en)</param>
        /// <returns>Object with totalPurchasesVAT, totalReturnsVAT, totalVAT</returns>
        Task<object> GetNetPurchaseVATReport(DateOnly startDate, DateOnly endDate, string language = "ar");
        
        /// <summary>
        /// Generates PDF document for Net Purchase VAT Report
        /// </summary>
        Task<byte[]> GetNetPurchaseVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
    }
}
