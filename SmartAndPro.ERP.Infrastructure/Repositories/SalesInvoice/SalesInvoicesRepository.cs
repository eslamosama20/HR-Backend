using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Models.InputModel;
using SmartAndPro.ERP.Domains.Models.OutputModel;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Infrastructure.Repositories.SalesInvoice
{
    public class SalesInvoicesRepository : RepositoryBase<Domains.Entities.Invoice.Sale.SaleInvoice, ERPContext>,
        ISalesInvoicesRepository
    {
        private readonly ERPContext _context;

        public SalesInvoicesRepository(ERPContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<SalesInvoiceDetailsOutputModel> GetSalesInvoiceDetails(GetSalesInvoiceDetailsInputModel getSalesInvoice)
        {
            try
            {
                return
                    _context.SaleInvoices
                    .Include(invoice => invoice.CustomerProfile)
                    .Include(invoice => invoice.SaleItems)
                        .ThenInclude(item => item.MainSalesClass)
                    .Where(invoice => invoice.IssueDate >= getSalesInvoice.FromDate &&
                           invoice.IssueDate <= getSalesInvoice.ToDate && !invoice.IsQuotation)
                    .GroupBy(invoice => new
                    {
                        invoice.Id,
                        invoice.InvoiceNo,
                        SuppilerArabicName = invoice.CustomerProfile.ArabicName,
                        SuppilerEnglishName = invoice.CustomerProfile.EnglishName,
                        SupplierId = invoice.CustomerProfileId,
                        MainAccountId = invoice.CustomerProfile.MainAccountId,
                        invoice.TotalPrice,
                        DiscountAmount = invoice.DiscountAmount,
                        invoice.Description,
                        invoice.IssueDate
                    })
                    .Select(group => new SalesInvoiceDetailsOutputModel
                    {
                        InvoiceId = group.Key.Id,
                        InvoiceNo = group.Key.InvoiceNo,
                        SuppilerArabicName = group.Key.SuppilerArabicName,
                        SuppilerEnglishName = group.Key.SuppilerEnglishName,
                        SupplierId = group.Key.SupplierId.ToString(),
                        MainAccountId = group.Key.MainAccountId.ToString(),
                        TotalPrice = group.Key.TotalPrice,
                        DiscountAmount = group.Key.DiscountAmount,
                        Description = group.Key.Description,
                        IssueDate = group.Key.IssueDate,
                        ItemDetails = group.SelectMany(invoice => invoice.SaleItems)
                            .Select(item => new ItemDetails
                            {
                                ReferenceCode = item.MainSalesClass.ReferenceCode,
                                ItemArabicName = item.MainSalesClass.ArabicName,
                                ItemEnglishName = item.MainSalesClass.EnglishName,
                                Price = item.Price,
                                Quantity = item.Quantity,
                                TotalPrice = item.Price * item.Quantity,
                                TotalPriceWithTax = item.MainSalesClass.IsTaxExempted
                                    ? item.Price * item.Quantity
                                    : (item.Price * item.Quantity * (decimal)0.15) + (item.Price * item.Quantity),
                                IsTaxExempted = item.MainSalesClass.IsTaxExempted
                            }).ToList(),

                        TotalItemPrice = group.SelectMany(invoice => invoice.SaleItems).Sum(item => item.Price),
                        TotalItemQuantity = group.SelectMany(invoice => invoice.SaleItems).Sum(item => item.Quantity),
                        TotalInvoicePrice = group.SelectMany(invoice => invoice.SaleItems).Sum(item => item.Quantity * item.Price) - group.Key.DiscountAmount,
                        TotalInvoicePriceWithTax = group.SelectMany(invoice => invoice.SaleItems)
                            .Sum(item => item.MainSalesClass.IsTaxExempted
                                ? item.Quantity * item.Price
                                : item.Quantity * item.Price * (decimal)0.15 + (item.Quantity * item.Price)) - (group.Key.DiscountAmount * (decimal)1.15)
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
            var query = _context.SaleInvoices
                .AsNoTracking()
                .Where(x => !x.IsQuotation && !x.Reservations.Any());

            var totalAmount = await query.SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;
            var totalCount = await query.CountAsync();

            return (totalAmount, totalCount);
        }

        public async Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyTotalsAsync(DateTime fromInclusive, DateTime toExclusive)
        {
            var rows = await _context.SaleInvoices
                .AsNoTracking()
                .Where(x => !x.IsQuotation && !x.Reservations.Any() && x.IssueDate >= fromInclusive && x.IssueDate < toExclusive)
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
