using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.RootPurchaseClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
[Authorize]
[PermissionAuthorize("MainPurchaseClass.Read")]
public class RootPurchaseClassController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly RootPurchaseClassService _service;
    public RootPurchaseClassController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<RootPurchaseClassService>();
    }
    
    
    // RootPurchaseClasses get all
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetRootPurchaseClasses([FromQuery] RootPurchaseClassFiltersDto filters )
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
    
    // RootPurchaseClasses get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetRootPurchaseClassesById(Guid id)
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
    
    // RootPurchaseClasses create
    [HttpPost("")]
    
    public async Task<IActionResult> CreateRootPurchaseClasses([FromBody] RootPurchaseClassPostDto rootPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(rootPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // RootPurchaseClasses update
    [HttpPut]
    
    public async Task<IActionResult> UpdateRootPurchaseClasses([FromBody] RootPurchaseClassPutDto rootPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(rootPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // RootPurchaseClasses delete
    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> DeleteRootPurchaseClasses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<RootPurchaseClassService>();
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