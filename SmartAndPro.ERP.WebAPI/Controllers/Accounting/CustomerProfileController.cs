using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.CustomerProfile.Read)]
public class CustomerProfileController : Controller
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly CustomerProfileService _service;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;
    public CustomerProfileController(ServiceBaseFactory serviceBaseFactory, SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<CustomerProfileService>();
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
    [PermissionAuthorize(Permissions.CustomerProfile.Update)]
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
    

    // CustomerProfiles get all
    [HttpGet("")]
    public async Task<IActionResult> GetCustomerProfiles([FromQuery] CustomerProfileFiltersDto filters, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllPaginatedAsync(filters, pageNumber, pageSize);
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
        [FromQuery] int? sortOrder = null,
        [FromQuery] bool? isReservationCompany = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.SearchAsync(q, pageNumber, pageSize, sortField, sortOrder, isReservationCompany);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }


    // CustomerProfiles get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerProfilesById(Guid id)
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
    
    // CustomerProfiles create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.CustomerProfile.Create)]
    public async Task<IActionResult> CreateCustomerProfiles([FromBody] CustomerProfilePostDto customerProfile)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _quotaService.ValidateCustomerQuotaAsync(User);
            var responseDate = await _service.AddAsync(customerProfile);
            return responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(new { Message = ex.Message });
        }
    }
    
    // CustomerProfiles update
    [HttpPut]
    [PermissionAuthorize(Permissions.CustomerProfile.Update)]
    public async Task<IActionResult> UpdateCustomerProfiles([FromBody] CustomerProfilePutDto customerProfile)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(customerProfile);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // CustomerProfiles delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.CustomerProfile.Delete)]
    public async Task<IActionResult> DeleteCustomerProfiles(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<CustomerProfileService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ── Excel bulk import ─────────────────────────────────────────────────────
    /// <summary>
    /// Upload a .xlsx file and import all rows in a single server-side transaction.
    /// Returns structured counts: imported, updated, and per-row failures.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [PermissionAuthorize(Permissions.CustomerProfile.Create)]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

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

    // CustomerProfiles delete range
    [HttpDelete("delete-range")]
    [PermissionAuthorize(Permissions.CustomerProfile.Delete)]
    public async Task<IActionResult> DeleteRangeCustomerProfiles([FromBody] List<Guid> ids)
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
    [AllowAnonymous]
    public IActionResult DownloadTemplate()
    {
        var fileName = "customers_import_template.xlsx";
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

        if (!System.IO.File.Exists(templatePath))
            return NotFound(new { message = "Template file not found." });

        return PhysicalFile(templatePath, ExcelContentType, fileName);
    }
}
