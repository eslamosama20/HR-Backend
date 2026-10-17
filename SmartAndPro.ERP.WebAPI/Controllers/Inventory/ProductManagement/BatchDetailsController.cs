using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;
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

[Route("api/inventory/[controller]")]
[ApiController]
[Authorize]
[PermissionAuthorize("InventoryManagement.Read")]
public class BatchDetailsController : ControllerBase
{
    private readonly BatchService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public BatchDetailsController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<BatchService>();
    }


    [HttpGet]
    public async Task<IActionResult> Get( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new BatchDetailsFiltersDto(), pageNumber, pageSize);
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
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

    [HttpPost]
    
    public async Task<IActionResult> Create([FromBody] BatchDetailsPostDto batch)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(batch);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    [HttpPut]
    
    public async Task<IActionResult> Update([FromBody] BatchDetailsPutDto batch)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(batch);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }


    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> Delete(Guid id)
    {
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


    /*[HttpGet("productUnit/{productUnitId:guid}")]
    
    public async Task<IActionResult> GetByProductUnit(Guid productUnitId)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetBatchByProductUnitId(productUnitId);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
    */

    [HttpPost("productUnit/assign")]
    
    public async Task<IActionResult> AssignProductToBatch([FromBody] ProductBatchToUnitAssignmentsDto batchAssign)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AssignProductToBatch(batchAssign);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    /*[HttpDelete("productUnit/unassign")]
    
    public async Task<IActionResult> UnassignProductFromBatch([FromBody] UnassignProductFromBatchDto data)
    {
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UnassignProductFromBatch(data.productUnitId, data.batchDetailsId);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }*/
}
