using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierStatus;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.SupplierManagement;

[Route("api/inventory/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("SupplierDetails.Read")]
public class SupplierStatusController : Controller
{
    private readonly SupplierStatusService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public SupplierStatusController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<SupplierStatusService>();
    }


    // SupplierStatuses get all

    [HttpGet("")]
    
    public async Task<IActionResult> GetSupplierStatuses([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new SupplierStatusFiltersDto(), pageNumber, pageSize);
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

    // SupplierStatuses get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetSupplierStatusesById(Guid id)
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

    // SupplierStatuses create
    [HttpPost("")]
    
    public async Task<IActionResult> CreateSupplierStatuses([FromBody] SupplierStatusPostDto supplierStatuses)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(supplierStatuses);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // SupplierStatuses update
    [HttpPut]
    
    public async Task<IActionResult> UpdateSupplierStatuses([FromBody] SupplierStatusPutDto supplierStatuses)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(supplierStatuses);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // SupplierStatuses delete
    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> DeleteSupplierStatuses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<SupplierStatusService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
}