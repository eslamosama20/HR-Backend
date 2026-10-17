using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.TopProducts;
using SmartAndPro.ERP.Application.Services.Reports.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Infrastructure.Repositories;

namespace SmartAndPro.ERP.Application.Services.Reports;

/// <summary>
/// خدمة المنتجات الأكثر طلباً (من فواتير المشتريات)
/// </summary>
public class TopOrderedProductsService
{
    private readonly RepositoriesFactory _repositoriesFactory;

    public TopOrderedProductsService(RepositoriesFactory repositoriesFactory)
    {
        _repositoriesFactory = repositoriesFactory;
    }

    /// <summary>
    /// الحصول على قائمة المنتجات الأكثر طلباً
    /// </summary>
    public async Task<List<TopProductDto>> GetTopOrderedProductsAsync(TopProductsFilterDto filters)
    {
        var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
        
        // جلب جميع فواتير المشتريات مع العناصر
        var invoicesResult = await purchaseInvoiceRepo.Filter(x => x.IsActive);

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

        // تطبيق فلتر المخزن
        if (filters.StoreId.HasValue)
        {
            queryable = queryable.Where(x => x.PurchaseItems.Any(p => p.StoreDetailsId == filters.StoreId.Value));
        }
        
        // تحميل البيانات المرتبطة وتنفيذ الاستعلام
        var invoices = queryable
            .Include(x => x.PurchaseItems)
                .ThenInclude(pi => pi.MainPurchaseClass)
                    .ThenInclude(mpc => mpc.MeasurementUnit)
            .ToList();

        // تجميع العناصر حسب المنتج
        var allPurchaseItems = invoices
            .SelectMany(inv => inv.PurchaseItems.Select(item => new
            {
                Item = item,
                InvoiceDate = inv.CreatedAt
            }))
            .ToList();

        var groupedProducts = allPurchaseItems
            .GroupBy(x => x.Item.MainPurchaseClassId)
            .Select(g =>
            {
                var firstItem = g.First().Item;
                var mainPurchaseClass = firstItem.MainPurchaseClass;
                
                return new TopProductDto
                {
                    ProductId = g.Key,
                    ProductCode = mainPurchaseClass?.ReferenceCode ?? "-",
                    ProductNameAr = mainPurchaseClass?.ArabicName ?? firstItem.Name ?? firstItem.ItemArabicName ?? "غير محدد",
                    ProductNameEn = mainPurchaseClass?.EnglishName ?? firstItem.ItemEnglishName ?? "-",
                    UnitName = mainPurchaseClass?.MeasurementUnit?.ArabicName ?? "-",
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
    /// الحصول على إحصائيات المشتريات
    /// </summary>
    public async Task<TopProductsStatisticsDto> GetStatisticsAsync(TopProductsFilterDto filters)
    {
        var products = await GetTopOrderedProductsAsync(filters);
        
        return new TopProductsStatisticsDto
        {
            TotalProducts = products.Count,
            TotalTransactions = products.Sum(x => x.TransactionCount),
            TotalQuantity = products.Sum(x => x.TotalQuantity),
            TotalAmount = products.Sum(x => x.TotalAmount)
        };
    }

    /// <summary>
    /// إنشاء تقرير المنتجات الأكثر طلباً PDF
    /// </summary>
    public async Task<byte[]> GetTopOrderedProductsReportDocument(TopProductsFilterDto filters, string? printedBy = null)
    {
        var items = await GetTopOrderedProductsAsync(filters);
        
        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        var document = new TopProductsReportDocument(
            items,
            company,
            filters.FromDate,
            filters.ToDate,
            "تقرير المنتجات الأكثر طلباً",
            "عدد الطلبات",
            "آخر طلب",
            "ar",
            printedBy
        );

        return document.GeneratePdf();
    }
}
