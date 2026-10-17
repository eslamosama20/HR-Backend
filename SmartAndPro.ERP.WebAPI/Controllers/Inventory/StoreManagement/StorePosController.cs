using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.StoreManagement;

/// <summary>
/// POS-related store endpoints per PDF contract
/// </summary>
[PermissionAuthorize("StoreDetails.Read")]
[ApiController]
[Route("api/v1/inventory/stores")]
public class StorePosController : ControllerBase
{
    private readonly ERPContext _db;
    private readonly ILogger<StorePosController> _logger;

    public StorePosController(ERPContext db, ILogger<StorePosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// PDF §5: Get stores available for POS (pharmacy stores)
    /// GET /api/v1/inventory/stores/pos-available
    /// Returns [{id, name, code}]
    /// </summary>
    [HttpGet("pos-available")]
    public async Task<IActionResult> GetPosAvailable()
    {
        try
        {
            var rawStores = await _db.StoreDetails
                .AsNoTracking()
                .Where(s => s.IsActive && s.StoreCategory == "pharmacy")
                .OrderBy(s => s.ArabicName)
                .Select(s => new { s.Id, s.ArabicName, s.EnglishName })
                .ToListAsync();

            // Fallback: if no pharmacy stores, return all active stores
            if (rawStores.Count == 0)
            {
                rawStores = await _db.StoreDetails
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.ArabicName)
                    .Select(s => new { s.Id, s.ArabicName, s.EnglishName })
                    .ToListAsync();
            }

            var stores = rawStores.Select(s => new
            {
                id = s.Id,
                name = s.ArabicName ?? s.EnglishName,
                nameEn = s.EnglishName,
                code = s.Id.ToString()[..8].ToUpper()
            }).ToList();

            return Ok(stores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPosAvailable failed");
            return Ok(new List<object>());
        }
    }
}
