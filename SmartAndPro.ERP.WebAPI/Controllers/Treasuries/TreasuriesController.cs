using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ProductManagement;
using SmartAndPro.ERP.Application.Services.Treasuries;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Treasuries;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.Treasury.Read)]
public class TreasuriesController : Controller
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly TreasuryService _service;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;

    public TreasuriesController(ServiceBaseFactory serviceBaseFactory, SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<TreasuryService>();
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
    [PermissionAuthorize(Permissions.Treasury.Update)]
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

    // Treasuries get all
    [HttpGet("")]
    public async Task<IActionResult> GetTreasuries([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new TreasuryFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Treasuries get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTreasuriesById(Guid id)
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

    // Treasuries create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.Treasury.Create)]
    public async Task<IActionResult> CreateTreasuries([FromBody] TreasuryPostDto productUnitStatuses)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _quotaService.ValidateTreasuryQuotaAsync(User);
            var responseDate = await _service.AddAsync(productUnitStatuses);
            return responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(new { Message = ex.Message });
        }
    }

    // Treasuries update
    [HttpPut]
    [PermissionAuthorize(Permissions.Treasury.Update)]
    public async Task<IActionResult> UpdateTreasuries([FromBody] TreasuryPutDto productUnitStatuses)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(productUnitStatuses);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Treasuries delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Treasury.Delete)]
    public async Task<IActionResult> DeleteTreasuries(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<TreasuryService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ── Excel bulk import ──────────────────────────────────────────────────────
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [PermissionAuthorize(Permissions.Treasury.Create)]
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

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult DownloadTemplate()
    {
        var fileName = "treasuries-template.xlsx";
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

        if (!System.IO.File.Exists(templatePath))
            return NotFound(new { message = "Template file not found." });

        return PhysicalFile(templatePath, ExcelContentType, fileName);
    }
}
