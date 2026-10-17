using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.ProductManagement;

[Route("api/inventory/products")]
[ApiController]
[PermissionAuthorize("InventoryManagement.Read")]
public class ProductsController : ControllerBase
{
    private readonly ERPContext _context;

    public ProductsController(ERPContext context)
    {
        _context = context;
    }

    [HttpGet("for-barcode")]
    public async Task<IActionResult> GetProductsForBarcode(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 200);

        var normalizedSearch = search?.Trim().ToLower();

        var purchaseQuery = _context.MainPurchaseClasses
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.ReferenceCode))
            .Select(x => new BarcodeProductListItemDto
            {
                Id = x.Id,
                ArabicName = x.ArabicName ?? string.Empty,
                EnglishName = x.EnglishName ?? string.Empty,
                ReferenceCode = x.ReferenceCode ?? string.Empty
            });

        var salesQuery = _context.MainSalesClasses
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.ReferenceCode))
            .Select(x => new BarcodeProductListItemDto
            {
                Id = x.Id,
                ArabicName = x.ArabicName ?? string.Empty,
                EnglishName = x.EnglishName ?? string.Empty,
                ReferenceCode = x.ReferenceCode ?? string.Empty
            });

        var query = purchaseQuery.Concat(salesQuery);

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(x =>
                x.ReferenceCode.ToLower().Contains(normalizedSearch) ||
                x.ArabicName.ToLower().Contains(normalizedSearch) ||
                x.EnglishName.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(x => x.ReferenceCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            totalCount,
            pageNumber,
            pageSize,
            data
        });
    }

    public class BarcodeProductListItemDto
    {
        public Guid Id { get; set; }
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
    }
}
