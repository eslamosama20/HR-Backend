using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Linq;

namespace SmartAndPro.ERP.Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceRepository : RepositoryBase<Domains.Entities.Invoice.Purchase.PurchaseInvoice, ERPContext>,
        IPurchaseInvoiceRepository
    {
        private readonly ERPContext _context;

        public PurchaseInvoiceRepository(ERPContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<PurchaseInvoiceDetailsOutputModel> GetPurchaseInvoiceDetails(GetPurchaseInvoiceDetailsInputModel getPurchaseInvoice)
        {
            try
            {
                return
                    _context.PurchaseInvoices
                    .Include(invoice => invoice.SupplierDetails)
                    .Include(invoice => invoice.PurchaseItems)
                        .ThenInclude(item => item.MainPurchaseClass)
                    .Where(invoice => invoice.IssueDate >= getPurchaseInvoice.FromDate && invoice.IssueDate <= getPurchaseInvoice.ToDate) // Filter by IssueDate range
                    .GroupBy(invoice => new
                    {
                        invoice.Id,
                        invoice.InvoiceNo,
                        SupplierID = invoice.SupplierDetails.Id,
                        SuppilerArabicName = invoice.SupplierDetails.ArabicName,
                        SuppilerEnglishName = invoice.SupplierDetails.EnglishName,
                        MainAccountId = invoice.SupplierDetails.MainAccountId,
                        invoice.TotalPrice,
                        invoice.Description,
                        invoice.IssueDate,
                        invoice.TaxAmount
                    })
                    .Select(group => new PurchaseInvoiceDetailsOutputModel
                    {
                        InvoiceId = group.Key.Id,
                        InvoiceNo = group.Key.InvoiceNo,
                        SupplierId = group.Key.SupplierID.ToString(),
                        MainAccountId = group.Key.MainAccountId.ToString(),
                        SuppilerArabicName = group.Key.SuppilerArabicName,
                        SuppilerEnglishName = group.Key.SuppilerEnglishName,
                        TotalPrice = group.Key.TotalPrice,
                        Description = group.Key.Description,
                        IssueDate = group.Key.IssueDate,
                        ItemDetails = group.SelectMany(invoice => invoice.PurchaseItems)
                            .Select(item => new ItemDetails
                            {
                                ReferenceCode = item.MainPurchaseClass.ReferenceCode,
                                ItemArabicName = item.MainPurchaseClass.ArabicName,
                                ItemEnglishName = item.MainPurchaseClass.EnglishName,
                                Price = item.Price,
                                Quantity = item.Quantity,
                                IsTaxExempted = item.IsTaxExempted,
                                TotalPrice = item.Price * item.Quantity,
                                TotalPriceWithTax = item.IsTaxExempted ? (item.Price * item.Quantity) : (item.Price * item.Quantity * (decimal)1.15)
                            }).ToList(),

                        TotalItemPrice = group.SelectMany(invoice => invoice.PurchaseItems).Sum(item => item.Price),
                        TotalItemQuantity = group.SelectMany(invoice => invoice.PurchaseItems).Sum(item => item.Quantity),
                        TotalInvoicePrice = group.SelectMany(invoice => invoice.PurchaseItems).Sum(item => item.Quantity * item.Price),
                        TotalInvoicePriceWithTax = group.SelectMany(invoice => invoice.PurchaseItems)
                            .Sum(item => item.IsTaxExempted 
                                ? (item.Quantity * item.Price) 
                                : (item.Quantity * item.Price * (decimal)1.15))
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                throw;
            }
        }

        public async Task<(decimal TotalAmount, int TotalCount)> GetDashboardSummaryAsync()
        {
            var query = _context.PurchaseInvoices
                .AsNoTracking()
                .Where(x => x.IsActive);

            var totalAmount = await query.SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;
            var totalCount = await query.CountAsync();

            return (totalAmount, totalCount);
        }

        public async Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyTotalsAsync(DateTime fromInclusive, DateTime toExclusive)
        {
            var rows = await _context.PurchaseInvoices
                .AsNoTracking()
                .Where(x => x.IsActive && x.IssueDate >= fromInclusive && x.IssueDate < toExclusive)
                .GroupBy(x => new { x.IssueDate.Year, x.IssueDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.TotalPrice)
                })
                .ToListAsync();

            return rows.ToDictionary(x => (x.Year, x.Month), x => x.Total);
        }

    }
}
