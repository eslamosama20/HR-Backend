using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System;

namespace SmartAndPro.ERP.Infrastructure.Repositories.SalesInvoice
{
    public interface ISalesInvoicesRepository : IRepository<Domains.Entities.Invoice.Sale.SaleInvoice>
    {

        /// <summary>
        /// Retrieves detailed information about sales invoices based on specified filter criteria.
        /// </summary>
        /// <param name="getSalesInvoice">
        /// An object containing filter criteria, such as date range and other search parameters, for retrieving sales invoices.
        /// </param>
        /// <returns>
        /// A collection of <see cref="SalesInvoiceDetailsOutputModel"/> objects, each containing details about a sales invoice,
        /// including invoice number, supplier information, item details, and other relevant information.
        /// </returns>
        IEnumerable<SalesInvoiceDetailsOutputModel> GetSalesInvoiceDetails(GetSalesInvoiceDetailsInputModel getSalesInvoice);

        Task<(decimal TotalAmount, int TotalCount)> GetDashboardSummaryAsync();
        Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyTotalsAsync(DateTime fromInclusive, DateTime toExclusive);
    }
}
