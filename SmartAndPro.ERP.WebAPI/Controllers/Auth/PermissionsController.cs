using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using System.Linq;
using SmartAndPro.ERP.Infrastructure.Data.Persistence;

namespace SmartAndPro.ERP.WebAPI.Controllers.Auth;


[Route("api/auth/[controller]")]
[ApiController]
[Authorize]
[PermissionAuthorize("System.Read")]
public class PermissionsController : Controller
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(id);
        if (permission == null) return NotFound();
        return Ok(permission);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        return Ok(permissions);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddPermission(PermissionDto permissionDto)
    {
        var permission = await _permissionService.AddPermissionAsync(permissionDto);
        return CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, permission);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdatePermission(Guid id, PermissionDto permissionDto)
    {
        if (id != permissionDto.Id) return BadRequest();
        await _permissionService.UpdatePermissionAsync(permissionDto);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePermission(Guid id)
    {
        await _permissionService.DeletePermissionAsync(id);
        return NoContent();
    }
}