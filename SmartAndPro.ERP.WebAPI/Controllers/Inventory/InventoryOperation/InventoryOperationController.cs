using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.InventoryOperation.InventoryOperation;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryOperationManagement;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.InventoryOperation;
[Route("api/inventory/[controller]")]
[ApiController]
[PermissionAuthorize("InventoryManagement.Read")]
public class InventoryOperationController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly InventoryOperationService _service;
    public InventoryOperationController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<InventoryOperationService>();
    }

    // InventoryOperations get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetInventoryOperations([FromQuery] InventoryOperationFiltersDto filters )
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
    
    // InventoryOperations get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInventoryOperationsById(Guid id)
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
    
    // InventoryOperations create
    [HttpPost("")]
    public async Task<IActionResult> CreateInventoryOperations([FromBody] InventoryOperationPostDto inventoryOperation)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(inventoryOperation);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // InventoryOperations update
    [HttpPut]
    public async Task<IActionResult> UpdateInventoryOperations([FromBody] InventoryOperationPutDto inventoryOperation)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(inventoryOperation);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // InventoryOperations delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteInventoryOperations(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var inventoryOperation =await _service.GetAsync(id);
            var transactionService =  _serviceBaseFactory.GetService<TransactionService>();
            if (inventoryOperation.Data.TransactionId != null)
            {
                await transactionService.DeleteAsync((Guid)inventoryOperation.Data.TransactionId);
            }
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