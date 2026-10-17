using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnitStatus;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Inventory.ProductManagement;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.ProductManagement;
[Route("api/inventory/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("InventoryManagement.Read")]
public class ProductUnitStatusController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ProductUnitStatusService _service;
    public ProductUnitStatusController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<ProductUnitStatusService>();
    }
    
    
    // ProductUnitStatuses get all
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetProductUnitStatuses( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ProductUnitStatusFiltersDto(), pageNumber, pageSize);
                
                
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
    
    // ProductUnitStatuses get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetProductUnitStatusesById(Guid id)
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
    
    // ProductUnitStatuses create
    [HttpPost("")]
    
    public async Task<IActionResult> CreateProductUnitStatuses([FromBody] ProductUnitStatusPostDto productUnitStatuses)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(productUnitStatuses);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // ProductUnitStatuses update
    [HttpPut]
    
    public async Task<IActionResult> UpdateProductUnitStatuses([FromBody] ProductUnitStatusPutDto productUnitStatuses)
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
    
    // ProductUnitStatuses delete
    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> DeleteProductUnitStatuses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ProductUnitStatusService>();
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