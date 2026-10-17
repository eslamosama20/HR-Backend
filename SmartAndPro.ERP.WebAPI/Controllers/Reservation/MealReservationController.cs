using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.MealReservation.MealReservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.MealReservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.MealReservation;
[Route("api/[controller]/")]
[ApiController]
[Authorize]
public class MealReservationController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly MealReservationService _service;
    public MealReservationController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<MealReservationService>();
    }
    
    
    // MealReservations get all
    [PermissionAuthorize("MealReservation.Read")]
    [HttpGet("")]
    public async Task<IActionResult> GetMealReservations( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new MealReservationFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MealReservations get by id
    [PermissionAuthorize("MealReservation.Read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMealReservationsById(Guid id)
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
    
    // MealReservations create
    [PermissionAuthorize("MealReservation.Create")]
    [HttpPost("")]
    public async Task<IActionResult> CreateMealReservations([FromBody] MealReservationPostDto mealReservations)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(mealReservations);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MealReservations update
    [PermissionAuthorize("MealReservation.Update")]
    [HttpPut]
    public async Task<IActionResult> UpdateMealReservations([FromBody] MealReservationPutDto mealReservations)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(mealReservations);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // MealReservations delete
    [PermissionAuthorize("MealReservation.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMealReservations(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<MealReservationService>();
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