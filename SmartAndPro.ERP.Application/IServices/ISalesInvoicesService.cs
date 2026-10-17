using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.IServices
{
    public interface ISalesInvoicesService
    {
        Task<SaleProductBarcodeResultDto?> GetProductByBarcodeAsync(string barcode);

        Task<string> GetLastInvoiceNo();

        Task<string> GetLastQuotationNo();

        //Task<PaginatedResult<IEnumerable<SaleInvoiceDto>>> GetAllAsync(int? pageNumber = null, int? pageSize = null);

        Task<OperationResponse<object>> GetAllAsync(int? pageNumber = null, int? pageSize = null, bool? isSentToZatca = null, string? keyword = null, bool? hasTransaction = null);

        Task<OperationResponse<object>> GetAllReservationInvoicesAsync(int? pageNumber = null, int? pageSize = null, int? type = null, string keyword = null, string sortField = null, int? sortOrder = null, bool? isSentToZatca = null, bool? hasTransaction = null);

        //Task<PaginatedResult<IEnumerable<SaleInvoiceDto>>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null);
        Task<OperationResponse<object>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null, bool? isSentToZatca = null, string? keyword = null);
        
        Task<SaleInvoice> MakeFrontalAsync(int id);
        Task<IEnumerable<SaleInvoice>> GetAllQuotationsAsync();
        


        Task<OperationResponse<object>> ApplyCreditBalanceAsync(SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale.ApplyCreditBalanceDto dto);
        Task<OperationResponse<object>> GetCustomerCreditBalanceAsync(Guid mainAccountId);

        Task<SaleInvoice> GetFirstAsync();

        Task<SaleInvoice> GetByIdAsync(int id);

        Task<SaleInvoiceDto?> GetCopyTemplateAsync(int id);

        Task<SaleInvoice?> GetByReservationIdAsync(Guid reservationId);
        Task<List<SaleInvoice?>> GetByReservationIdsAsync(List<Guid> reservationIds);

        Task<SaleInvoice> AddAsync(SaleInvoicePostDto invoice);

        //Task<int> CreateFromReservationsAsync(List<Guid> reservationIds);

        Task<SaleInvoice> ConvertQuotationToInvoiceAsync(int quotationId);

        Task<string?> GetOriginalQuotationNoAsync(int invoiceId);
        Task<string?> GetQuotationNoByIdAsync(int quotationId);

        Task<int> UpdateTransaction(int? invoiceId, Guid? transactionId);

        Task<int> CreateTransaction(int id,bool? isFrontal = false);
        //Task<int> CreateBulkTransactions(List<int> ids);
        Task<int> CreateBulkTransactionsOptimized(List<int> ids);


        Task<int> EditAsync(SaleInvoice invoice, List<SaleItem> oldsaleItems);

        Task<int> RemoveAsync(SaleInvoice invoice);

        /// <summary>
        /// Retrieves the details of specified sales invoices, including supplier information, 
        /// issue date, description, and a list of item details.
        /// </summary>
        /// <param name="getPurchaseInvoiceDetails">
        /// An object containing the request parameters to identify the sales invoices to retrieve.
        /// </param>
        /// <returns>
        /// An <see cref="OperationResponse{List{SalesInvoiceDetailsResponse}}"/> containing details about the sales invoices,
        /// including each invoice's number, supplier name, issue date, description, and a list of items with their prices and quantities.
        /// </returns>
        OperationResponse<List<SalesInvoiceDetailsResponse>> GetSalesInvoiceDetails(GetSalesInvoiceDetailsRequest getPurchaseInvoiceDetails);

        byte[] ExportSalesInvoiceDetails(GetSalesInvoiceDetailsRequest getPurchaseInvoiceDetails);

        // VAT Report methods
        OperationResponse<List<SalesVATDetailsResponse>> GetSalesVATDetails(GetSalesVATDetailsRequest getVATDetails);
        byte[] ExportSalesVATDetails(GetSalesVATDetailsRequest getVATDetails);
        Task<byte[]> GetVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
        
        Task ZatcaClearInvoice(int id);
        Task<ZatcaPreCheckResultDto> ValidateZatcaInvoiceAsync(int id);
        Task UpdatePaymentStatusAsync(int invoiceId);
        Task<InvoicePaymentSummaryDto?> GetPaymentSummaryAsync(int invoiceId);
    Task<byte[]> GetDocument(int invoiceId, SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait, string? sellerName = null, SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions? print1 = null, SmartAndPro.ERP.Application.Services.Invoice.Sale.Print2ServerOptions? print2 = null);
    Task<byte[]> GetDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
    Task<byte[]> GetSummaryDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
    Task<byte[][]> GetDocuments(int[] invoiceIds, SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait, SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions? print1 = null, SmartAndPro.ERP.Application.Services.Invoice.Sale.Print2ServerOptions? print2 = null);
    Task<byte[][]> GetAllDocuments();
    Task<byte[]> GetMergedDocument(int[] invoiceIds, SmartAndPro.ERP.Domains.Enums.InvoicePrintType printType = SmartAndPro.ERP.Domains.Enums.InvoicePrintType.A4Portrait, SmartAndPro.ERP.Application.Services.Invoice.Sale.Print1ServerOptions? print1 = null, SmartAndPro.ERP.Application.Services.Invoice.Sale.Print2ServerOptions? print2 = null);
    Task<object> GetNetSalesVATReport(DateOnly startDate, DateOnly endDate, string language = "ar");
    Task<byte[]> GetNetSalesVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
    Task<object> GetNetDueVATReport(DateOnly startDate, DateOnly endDate, string language = "ar");
    Task<byte[]> GetNetDueVATDocument(DateOnly startDate, DateOnly endDate, string language = "ar");
    }
}
