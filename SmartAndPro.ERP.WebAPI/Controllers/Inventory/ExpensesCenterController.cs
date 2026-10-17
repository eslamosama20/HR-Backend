using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ExpensesCenter;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory;
[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.ExpensesCenter.Read)]
public class ExpensesCenterController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ExpensesCenterService _service;
    public ExpensesCenterController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<ExpensesCenterService>();
    }
    
    // ExpensesCenter get all
    [HttpGet("")]
    public async Task<IActionResult> GetExpensesCenter( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ExpensesCenterFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // ExpensesCenter get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetExpensesCenterById(Guid id)
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
    
    // ExpensesCenter create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.ExpensesCenter.Create)]
    public async Task<IActionResult> CreateExpensesCenter([FromBody] ExpensesCenterPostDto expensesCenter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(expensesCenter);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // ExpensesCenter update
    [HttpPut]
    [PermissionAuthorize(Permissions.ExpensesCenter.Update)]
    public async Task<IActionResult> UpdateExpensesCenter([FromBody] ExpensesCenterPutDto expensesCenter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(expensesCenter);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // ExpensesCenter delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.ExpensesCenter.Delete)]
    public async Task<IActionResult> DeleteExpensesCenter(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ExpensesCenterService>();
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