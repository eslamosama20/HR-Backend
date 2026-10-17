using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.OvertimeHours;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.HR;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAndPro.ERP.WebAPI.Controllers.HR;
[Route("api/hr/[controller]/")]
[ApiController]
[Authorize]
public class OvertimeHoursController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly OvertimeHoursService _service;
    public OvertimeHoursController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<OvertimeHoursService>();
    }
    
    
    
    [HttpGet("")]
    public async Task<IActionResult> GetOvertimeHours( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ParamFilter());
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // OvertimeHours get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOvertimeHoursById(Guid id)
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
    
    // OvertimeHours create
    [HttpPost("")]
    public async Task<IActionResult> CreateOvertimeHours([FromBody] OvertimeHoursPostDto overtimeHours)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(overtimeHours);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // OvertimeHours update
    [HttpPut]
    public async Task<IActionResult> UpdateOvertimeHours([FromBody] OvertimeHoursPutDto overtimeHours)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(overtimeHours);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // OvertimeHours delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteOvertimeHours(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<OvertimeHoursService>();
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