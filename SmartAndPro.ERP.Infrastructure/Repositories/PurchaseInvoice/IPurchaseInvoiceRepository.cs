using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;

namespace SmartAndPro.ERP.Infrastructure.Repositories.PurchaseInvoice
{
    public interface IPurchaseInvoiceRepository : IRepository<Domains.Entities.Invoice.Purchase.PurchaseInvoice>
    {
        /// <summary>
        /// Retrieves detailed information about purchase invoices based on specified filter criteria.
        /// </summary>
        /// <param name="getPurchaseInvoice">
        /// An object containing filter criteria, such as date range and other search parameters, for retrieving purchase invoices.
        /// </param>
        /// <returns>
        /// A collection of <see cref="PurchaseInvoiceDetailsOutputModel"/> objects, each containing details about a purchase invoice,
        /// including invoice number, supplier information, item details, and other relevant information.
        /// </returns>
        IEnumerable<PurchaseInvoiceDetailsOutputModel> GetPurchaseInvoiceDetails(GetPurchaseInvoiceDetailsInputModel getPurchaseInvoice);

        Task<(decimal TotalAmount, int TotalCount)> GetDashboardSummaryAsync();
        Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyTotalsAsync(DateTime fromInclusive, DateTime toExclusive);
    }
}
