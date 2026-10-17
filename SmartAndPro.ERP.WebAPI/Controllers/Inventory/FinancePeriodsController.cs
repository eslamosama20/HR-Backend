using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.FinancePeriods;
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
[PermissionAuthorize(Permissions.FinancePeriods.Read)]
public class FinancePeriodsController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly FinancePeriodsService _service;
    public FinancePeriodsController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<FinancePeriodsService>();
    }
    
    // FinancePeriods get all
    [HttpGet("")]
    public async Task<IActionResult> GetFinancePeriods( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new FinancePeriodsFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FinancePeriods get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFinancePeriodsById(Guid id)
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
    
    // FinancePeriods create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.FinancePeriods.Create)]
    public async Task<IActionResult> CreateFinancePeriods([FromBody] FinancePeriodsPostDto financePeriods)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(financePeriods);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FinancePeriods update
    [HttpPut]
    [PermissionAuthorize(Permissions.FinancePeriods.Update)]
    public async Task<IActionResult> UpdateFinancePeriods([FromBody] FinancePeriodsPutDto financePeriods)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(financePeriods);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FinancePeriods delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.FinancePeriods.Delete)]
    public async Task<IActionResult> DeleteFinancePeriods(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<FinancePeriodsService>();
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