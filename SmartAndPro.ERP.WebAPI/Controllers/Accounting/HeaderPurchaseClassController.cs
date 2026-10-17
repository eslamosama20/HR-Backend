using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderPurchaseClass;
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
public class HeaderPurchaseClassController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly HeaderPurchaseClassService _service;
    public HeaderPurchaseClassController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<HeaderPurchaseClassService>();
    }
    
    
    // HeaderPurchaseClasses get all
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetHeaderPurchaseClasses([FromQuery] HeaderPurchaseClassFiltersDto filters )
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
    
    // HeaderPurchaseClasses get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetHeaderPurchaseClassesById(Guid id)
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
    
    // HeaderPurchaseClasses create
    [HttpPost("")]
    
    public async Task<IActionResult> CreateHeaderPurchaseClasses([FromBody] HeaderPurchaseClassPostDto headerPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(headerPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // HeaderPurchaseClasses update
    [HttpPut]
    
    public async Task<IActionResult> UpdateHeaderPurchaseClasses([FromBody] HeaderPurchaseClassPutDto headerPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(headerPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // HeaderPurchaseClasses delete
    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> DeleteHeaderPurchaseClasses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<HeaderPurchaseClassService>();
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