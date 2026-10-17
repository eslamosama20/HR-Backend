using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Permits.InventoryPermitEntry;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Permits;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Permits;
[Route("api/[controller]")]
[ApiController]
[PermissionAuthorize("InventoryPermit.Read")]
public class InventoryPermitEntryController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly InventoryPermitEntryService _service;
    public InventoryPermitEntryController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<InventoryPermitEntryService>();
    }

    // inventoryPermitEntries get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetInventoryPermitEntries([FromQuery] InventoryPermitEntryFiltersDto filters )
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(filters);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // inventoryPermitEntries get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInventoryPermitEntryById(Guid id)
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
    
    // inventoryPermitEntries create
    [HttpPost("")]
    public async Task<IActionResult> CreateInventoryPermitEntry([FromBody] InventoryPermitEntryPostDto inventoryPermitEntry)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(inventoryPermitEntry);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // inventoryPermitEntries update
    [HttpPut]
    public async Task<IActionResult> UpdateInventoryPermitEntry([FromBody] InventoryPermitEntryPutDto inventoryPermitEntry)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(inventoryPermitEntry);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // inventoryPermitEntries delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteInventoryPermitEntry(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            /*var inventoryPermit =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (inventoryPermit.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)inventoryPermit.Data.TransactionId);
            }*/
            var responseDate = await _service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

}