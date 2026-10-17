using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSeason;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Reservation;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("ReservationSeason.Read")]
public class ReservationSeasonController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly ReservationSeasonService _service;
    public ReservationSeasonController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<ReservationSeasonService>();
    }


    // ReservationSeasons get all

    [HttpGet("")]
    public async Task<IActionResult> GetReservationSeasons([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ReservationSeasonFiltersDto(), pageNumber, pageSize);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Search
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int? sortOrder = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.SearchAsync(q, pageNumber, pageSize, sortField, sortOrder);
            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // ReservationSeasons get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReservationSeasonsById(Guid id)
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

    // ReservationSeasons create
    [HttpPost("")]
    [PermissionAuthorize("ReservationSeason.Create")]
    public async Task<IActionResult> CreateReservationSeasons([FromBody] ReservationSeasonPostDto reservationSeasons)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(reservationSeasons);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // ReservationSeasons update
    [HttpPut]
    [PermissionAuthorize("ReservationSeason.Update")]
    public async Task<IActionResult> UpdateReservationSeasons([FromBody] ReservationSeasonPutDto reservationSeasons)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(reservationSeasons);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // ReservationSeasons delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("ReservationSeason.Delete")]
    public async Task<IActionResult> DeleteReservationSeasons(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ReservationSeasonService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // Get all frontal reservation seasons
    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontal()
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.GetAllFrontalAsync();
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // Make reservation season frontal
    [HttpPost("frontal/{id}")]
    public async Task<IActionResult> MakeFrontalAsync(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseData = await _service.MakeFrontalAsync(id);
            result = responseData.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

}