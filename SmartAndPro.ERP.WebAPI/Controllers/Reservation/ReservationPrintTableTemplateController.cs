using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Reservation.ReservationPrintTableTemplate;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.Infrastructure.Data.Persistence;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[PermissionAuthorize("ReservationPrintTableTemplates.Read")]
public class ReservationPrintTableTemplateController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ReservationPrintTableTemplateService _service;

    public ReservationPrintTableTemplateController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<ReservationPrintTableTemplateService>();
    }

    // Get all templates
    [HttpGet("")]
    public async Task<IActionResult> GetReservationPrintTableTemplates([FromQuery] ReservationPrintTableTemplateFilterDto filters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetAllAsync(filters);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Get template by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReservationPrintTableTemplateById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Create template
    [HttpPost("")]
    [PermissionAuthorize("ReservationPrintTableTemplates.Create")]
    public async Task<IActionResult> CreateReservationPrintTableTemplate([FromBody] ReservationPrintTableTemplatePostDto template)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.AddAsync(template);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Update template
    [HttpPut]
    [PermissionAuthorize("ReservationPrintTableTemplates.Update")]
    public async Task<IActionResult> UpdateReservationPrintTableTemplate([FromBody] ReservationPrintTableTemplatePutDto template)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.UpdateAsync(template);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Delete template
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("ReservationPrintTableTemplates.Delete")]
    public async Task<IActionResult> DeleteReservationPrintTableTemplate(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var template = await _service.GetAsync(id);
            if (template.Data == null)
            {
                return NotFound();
            }

            var responseData = await _service.DeleteAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Seed default templates - Developer only
    [HttpPost("seed-defaults")]
    [PermissionAuthorize(DeveloperPermissions.SeedTemplates)]
    public async Task<IActionResult> SeedDefaultTemplates()
    {
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.SeedDefaultTemplatesAsync();
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

}
