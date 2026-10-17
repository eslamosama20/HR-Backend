using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailSalesClass;
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
[PermissionAuthorize("MainSalesClass.Read")]
public class DetailSalesClassController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly DetailSalesClassService _service;
    public DetailSalesClassController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<DetailSalesClassService>();
    }
    
    
    // DetailSalesClasses get all
    
    [HttpGet("")]
    
    public async Task<IActionResult> GetDetailSalesClasses([FromQuery] DetailSalesClassFiltersDto filters )
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
    
    // DetailSalesClasses get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetDetailSalesClassesById(Guid id)
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
    
    // DetailSalesClasses create
    [HttpPost("")]
    
    public async Task<IActionResult> CreateDetailSalesClasses([FromBody] DetailSalesClassPostDto detailSalesClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(detailSalesClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // DetailSalesClasses update
    [HttpPut]
    
    public async Task<IActionResult> UpdateDetailSalesClasses([FromBody] DetailSalesClassPutDto detailSalesClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(detailSalesClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // DetailSalesClasses delete
    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> DeleteDetailSalesClasses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<DetailSalesClassService>();
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