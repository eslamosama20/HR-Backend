using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.TopProducts;
using SmartAndPro.ERP.Application.Services.Reports.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Infrastructure.Repositories;

namespace SmartAndPro.ERP.Application.Services.Reports;

/// <summary>
/// خدمة المنتجات الأكثر مبيعاً
/// </summary>
public class TopSellingProductsService
{
    private readonly RepositoriesFactory _repositoriesFactory;

    public TopSellingProductsService(RepositoriesFactory repositoriesFactory)
    {
        _repositoriesFactory = repositoriesFactory;
    }

    /// <summary>
    /// الحصول على قائمة المنتجات الأكثر مبيعاً
    /// </summary>
    public async Task<List<TopProductDto>> GetTopSellingProductsAsync(TopProductsFilterDto filters)
    {
        var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
        
        // جلب جميع فواتير المبيعات مع العناصر
        var invoicesResult = await saleInvoiceRepo.Filter(x => x.IsActive && !x.IsQuotation);

        if (invoicesResult.ErrorCode != 0 || invoicesResult.Data == null)
            return new List<TopProductDto>();

        var queryable = invoicesResult.Data;
        
        // تطبيق فلتر التاريخ على الـ queryable
        if (filters.FromDate.HasValue)
        {
            queryable = queryable.Where(x => x.CreatedAt >= filters.FromDate.Value);
        }
        if (filters.ToDate.HasValue)
        {
            var endDate = filters.ToDate.Value.Date.AddDays(1);
            queryable = queryable.Where(x => x.CreatedAt < endDate);
        }
        
        // تحميل البيانات المرتبطة وتنفيذ الاستعلام
        var invoices = queryable
            .Include(x => x.SaleItems)
                .ThenInclude(si => si.MainSalesClass)
                    .ThenInclude(msc => msc.MeasurementUnit)
            .ToList();

        // تجميع العناصر حسب المنتج
        var allSaleItems = invoices
            .SelectMany(inv => inv.SaleItems.Select(item => new
            {
                Item = item,
                InvoiceDate = inv.CreatedAt
            }))
            .ToList();

        var groupedProducts = allSaleItems
            .GroupBy(x => x.Item.MainSalesClassId)
            .Select(g =>
            {
                var firstItem = g.First().Item;
                var mainSalesClass = firstItem.MainSalesClass;
                
                return new TopProductDto
                {
                    ProductId = g.Key,
                    ProductCode = mainSalesClass?.ReferenceCode ?? "-",
                    ProductNameAr = mainSalesClass?.ArabicName ?? firstItem.Name ?? "غير محدد",
                    ProductNameEn = mainSalesClass?.EnglishName ?? "-",
                    UnitName = mainSalesClass?.MeasurementUnit?.ArabicName ?? "-",
                    TransactionCount = g.Count(),
                    TotalQuantity = g.Sum(x => x.Item.Quantity),
                    TotalAmount = g.Sum(x => x.Item.TotalPrice),
                    LastTransactionDate = g.Max(x => x.InvoiceDate),
                    AverageUnitPrice = g.Sum(x => x.Item.Quantity) > 0 
                        ? g.Sum(x => x.Item.TotalPrice) / g.Sum(x => x.Item.Quantity) 
                        : 0
                };
            })
            .ToList();

        // تطبيق فلتر البحث
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            groupedProducts = groupedProducts
                .Where(x => x.ProductNameAr.ToLower().Contains(searchTerm) ||
                           x.ProductNameEn.ToLower().Contains(searchTerm) ||
                           x.ProductCode.ToLower().Contains(searchTerm))
                .ToList();
        }

        // الترتيب من الأكثر للأقل وتحديد العدد
        var result = groupedProducts
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TransactionCount)
            .Take(filters.Top ?? 100)
            .ToList();

        // إضافة الترتيب
        for (int i = 0; i < result.Count; i++)
        {
            result[i].Rank = i + 1;
        }

        return result;
    }

    /// <summary>
    /// الحصول على إحصائيات المبيعات
    /// </summary>
    public async Task<TopProductsStatisticsDto> GetStatisticsAsync(TopProductsFilterDto filters)
    {
        var products = await GetTopSellingProductsAsync(filters);
        
        return new TopProductsStatisticsDto
        {
            TotalProducts = products.Count,
            TotalTransactions = products.Sum(x => x.TransactionCount),
            TotalQuantity = products.Sum(x => x.TotalQuantity),
            TotalAmount = products.Sum(x => x.TotalAmount)
        };
    }

    /// <summary>
    /// إنشاء تقرير المنتجات الأكثر مبيعاً PDF
    /// </summary>
    public async Task<byte[]> GetTopSellingProductsReportDocument(TopProductsFilterDto filters, string? printedBy = null)
    {
        var items = await GetTopSellingProductsAsync(filters);
        
        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        var document = new TopProductsReportDocument(
            items,
            company,
            filters.FromDate,
            filters.ToDate,
            "تقرير المنتجات الأكثر مبيعاً",
            "عدد المبيعات",
            "آخر بيع",
            "ar",
            printedBy
        );

        return document.GeneratePdf();
    }
}

/// <summary>
/// إحصائيات المنتجات
/// </summary>
public class TopProductsStatisticsDto
{
    public int TotalProducts { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}
