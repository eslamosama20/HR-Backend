using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.PublicBooking;

namespace SmartAndPro.ERP.WebAPI.Controllers.PublicBooking;

[Route("api/[controller]")]
[ApiController]
public class PublicPaidServiceController : ControllerBase
{
    private readonly PublicPaidServiceService _service;

    public PublicPaidServiceController(ServiceBaseFactory serviceBaseFactory)
    {
        _service = serviceBaseFactory.GetService<PublicPaidServiceService>();
    }

    /// <summary>
    /// Get all paid services
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get active paid services only
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var result = await _service.GetActiveServicesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get paid service by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _service.GetAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Create new paid service
    /// </summary>
    [HttpPost]
    // [Authorize] // Temporarily disabled for seeding
    public async Task<IActionResult> Create([FromBody] PublicPaidServicePostDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Update existing paid service
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] PublicPaidServicePutDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.UpdateAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Delete paid service
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
