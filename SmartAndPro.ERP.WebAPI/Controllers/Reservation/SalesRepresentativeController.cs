using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.SalesRepresentative;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Reservation;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;

namespace SmartAndPro.ERP.WebAPI.Controllers.Reservation;

[Route("api/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("SalesRepresentative.Read")]
public class SalesRepresentativeController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly SalesRepresentativeService _service;
    public SalesRepresentativeController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<SalesRepresentativeService>();
    }


    // SalesRepresentatives get all

    [HttpGet("")]
    public async Task<IActionResult> GetSalesRepresentatives([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new SalesRepresentativeFiltersDto(), pageNumber, pageSize);
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

    // SalesRepresentatives get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSalesRepresentativesById(Guid id)
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

    // SalesRepresentatives create
    [HttpPost("")]
    [PermissionAuthorize(Permissions.SalesRepresentative.Create)]
    public async Task<IActionResult> CreateSalesRepresentatives([FromBody] SalesRepresentativePostDto salesRepresentatives)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(salesRepresentatives);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // SalesRepresentatives update
    [HttpPut]
    [PermissionAuthorize(Permissions.SalesRepresentative.Update)]
    public async Task<IActionResult> UpdateSalesRepresentatives([FromBody] SalesRepresentativePutDto salesRepresentatives)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(salesRepresentatives);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    // SalesRepresentatives delete
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.SalesRepresentative.Delete)]
    public async Task<IActionResult> DeleteSalesRepresentatives(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<SalesRepresentativeService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // Get all frontal sales representatives
    [HttpGet("frontal")]
    [PermissionAuthorize(Permissions.FrontalSalesRepresentative.Read)]
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

    // Make sales representative frontal
    [HttpPost("frontal/{id}")]
    [PermissionAuthorize(Permissions.FrontalSalesRepresentative.Update)]
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