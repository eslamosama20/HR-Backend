using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.SupplierManagement;

[Route("api/inventory/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.SupplierDetails.Read)]
public class SupplierDetailsController : Controller
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly SupplierService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;

    public SupplierDetailsController(ServiceBaseFactory serviceBaseFactory, SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<SupplierService>();
        _quotaService = quotaService;
    }
    
    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontal()
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllFrontalAsync();
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPost("frontal/{id:guid}")]
    [PermissionAuthorize(Permissions.SupplierDetails.Update)]
    public async Task<IActionResult> MakeFrontalAsync(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.MakeFrontalAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // SupplierDetails get all
    [HttpGet("")]
    public async Task<IActionResult> GetSupplierDetails([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new SupplierDetailsFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // Search
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int? sortOrder = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.SearchAsync(q, pageNumber, pageSize, sortField, sortOrder);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // SupplierDetails get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplierDetailsById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // SupplierDetails create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.SupplierDetails.Create)]
    public async Task<IActionResult> CreateSupplierDetails([FromBody] SupplierDetailsPostDto supplierDetails)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _quotaService.ValidateSupplierQuotaAsync(User);
            var responseDate = await _service.AddAsync(supplierDetails);
            return responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(new { Message = ex.Message });
        }
    }

    // SupplierDetails update
    [HttpPut]
    [PermissionAuthorize(Permissions.SupplierDetails.Update)]
    public async Task<IActionResult> UpdateSupplierDetails([FromBody] SupplierDetailsPutDto supplierDetails)
    {
        Console.WriteLine($"[UpdateSupplierDetails] ID: {supplierDetails.Id}, Address: '{supplierDetails.Address}'");
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(supplierDetails);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // SupplierDetails delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.SupplierDetails.Delete)]
    public async Task<IActionResult> DeleteSupplierDetails(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ── Excel bulk import ──────────────────────────────────────────────────────
    /// <summary>
    /// Upload a .xlsx file and import all supplier rows in a single server-side transaction.
    /// Returns structured counts: imported, updated, and per-row failures.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [PermissionAuthorize(Permissions.SupplierDetails.Create)]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided or file is empty." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest(new { message = "Only .xlsx and .xls files are supported." });

        try
        {
            await using var stream = file.OpenReadStream();
            var importResult = await _service.ImportFromExcelAsync(stream);
            return Ok(importResult);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new { message = "Import failed unexpectedly.", error = ex.Message });
        }
    }

    [HttpDelete("delete-range")]
    [PermissionAuthorize(Permissions.SupplierDetails.Delete)]
    public async Task<IActionResult> DeleteRangeSupplierDetails([FromBody] List<Guid> ids)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.DeleteRangeAsync(ids);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var fileName = "supplier-details_import.xlsx";
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

        if (!System.IO.File.Exists(templatePath))
            return NotFound(new { message = "Template file not found." });

        return PhysicalFile(templatePath, ExcelContentType, fileName);
    }
}
