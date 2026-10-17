using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.SystemSetting;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.SystemSetting;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.SystemSetting;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SystemSettingController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly SystemSettingService _service;

    public SystemSettingController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<SystemSettingService>();
    }

    [PermissionAuthorize(Permissions.SystemSetting.Read)]
    [HttpGet("")]
    public async Task<IActionResult> GetRecipeEntries([FromQuery] SystemSettingFiltersDto filters)
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

    [PermissionAuthorize(Permissions.SystemSetting.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSystemSettingById(Guid id)
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

    [HttpPost("")]
    [PermissionAuthorize(Permissions.SystemSetting.Create)]
    public async Task<IActionResult> CreateSystemSetting([FromBody] SystemSettingPostDto systemSetting)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(systemSetting);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpPut]
    [PermissionAuthorize(Permissions.SystemSetting.Update)]
    public async Task<IActionResult> UpdateSystemSetting([FromBody] SystemSettingPutDto systemSetting)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(systemSetting);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.SystemSetting.Delete)]
    public async Task<IActionResult> DeleteSystemSetting(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
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

    [HttpGet("current-reservation-serial")]
    public async Task<IActionResult> GetCurrentReservationSerial()
    {
        try
        {
            var serial = await _service.PreviewReservationSerialAsync();
            return Ok(new
            {
                CurrentSerial = serial
            });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return BadRequest(ex.Message);
        }
    }
}