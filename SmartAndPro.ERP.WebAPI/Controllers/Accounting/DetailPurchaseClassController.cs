using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailPurchaseClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Accounting;
[Route("api/accounting/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.MainPurchaseClass.Read)]
public class DetailPurchaseClassController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly DetailPurchaseClassService _service;
    public DetailPurchaseClassController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<DetailPurchaseClassService>();
    }
    
    // DetailPurchaseClasses get all
    [HttpGet("")]
    public async Task<IActionResult> GetDetailPurchaseClasses([FromQuery] DetailPurchaseClassFiltersDto filters )
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
    
    // DetailPurchaseClasses get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetailPurchaseClassesById(Guid id)
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
    
    // DetailPurchaseClasses create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.MainPurchaseClass.Create)]
    public async Task<IActionResult> CreateDetailPurchaseClasses([FromBody] DetailPurchaseClassPostDto detailPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(detailPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // DetailPurchaseClasses update
    [HttpPut]
    [PermissionAuthorize(Permissions.MainPurchaseClass.Update)]
    public async Task<IActionResult> UpdateDetailPurchaseClasses([FromBody] DetailPurchaseClassPutDto detailPurchaseClass)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(detailPurchaseClass);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // DetailPurchaseClasses delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.MainPurchaseClass.Delete)]
    public async Task<IActionResult> DeleteDetailPurchaseClasses(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<DetailPurchaseClassService>();
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