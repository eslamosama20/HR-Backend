using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Auth;


[Route("api/auth/[controller]")]
[ApiController]
[Authorize]
[PermissionAuthorize("System.Read")]
public class DomainDetailsController : Controller
{
    private readonly IDomainDetailsService _domainDetailsService;

    public DomainDetailsController(IDomainDetailsService domainDetailsService)
    {
        _domainDetailsService = domainDetailsService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDomainDetailsById(Guid id)
    {
        var domainDetails = await _domainDetailsService.GetDomainDetailsByIdAsync(id);
        if (domainDetails == null) return NotFound();
        return Ok(domainDetails);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DomainDetailsDto>>> GetAllDomainDetails()
    {
        var domainDetails = await _domainDetailsService.GetAllDomainDetailsAsync();
        return Ok(domainDetails);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddDomainDetails(DomainDetailsDto domainDetailsDto)
    {
        await _domainDetailsService.AddDomainDetailsAsync(domainDetailsDto);
        return CreatedAtAction(nameof(GetDomainDetailsById), new { id = domainDetailsDto.Id }, domainDetailsDto);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateDomainDetails(Guid id, DomainDetailsDto domainDetailsDto)
    {
        if (id != domainDetailsDto.Id) return BadRequest();
        await _domainDetailsService.UpdateDomainDetailsAsync(domainDetailsDto);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteDomainDetails(Guid id)
    {
        await _domainDetailsService.DeleteDomainDetailsAsync(id);
        return NoContent();
    }
}