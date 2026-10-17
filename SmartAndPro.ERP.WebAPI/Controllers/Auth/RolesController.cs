using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Auth;

[Route("api/auth/[controller]")]
[ApiController]
[Authorize]
public class RolesController : Controller
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    // Get All Frontal Roles
    [PermissionAuthorize("FrontalRoleManagement.Read")]
    [HttpGet("frontal")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetFrontalRoles()
    {
        var roles = await _roleService.GetAllFrontalRolesAsync();
        return Ok(roles);
    }
    
    // Post Make Frontal Role
    [PermissionAuthorize("RoleManagement.Update")]
    [HttpPost("frontal/{id:guid}")]
    public async Task<ActionResult<RoleDto>> MakeRoleFrontal(Guid id)
    {
        var role = await _roleService.MakeRoleFrontalAsync(id);
        return Ok(role);
    }
    
    [PermissionAuthorize("RoleManagement.Read")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [PermissionAuthorize("RoleManagement.Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return Ok(role);
    }

    [PermissionAuthorize("RoleManagement.Create")]
    [HttpPost]
    public async Task<ActionResult<RoleDto>> AddRole([FromBody] RoleDto roleDto)
    {
        var role = await _roleService.AddRoleAsync(roleDto);
        return Ok(role);
    }

    [PermissionAuthorize("RoleManagement.Update")]
    [HttpPut]
    public async Task<ActionResult> UpdateRole([FromBody] RoleDto roleDto)
    {
        await _roleService.UpdateRoleAsync(roleDto);
        return NoContent();
    }

    [PermissionAuthorize("RoleManagement.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRole(Guid id)
    {
        await _roleService.DeleteRoleAsync(id);
        return NoContent();
    }   

    [HttpGet("/{roleName}/permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetRolePermissions(string roleName)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(roleName);
        return Ok(permissions);
    }
    
    [PermissionAuthorize("RoleManagement.Update")]
    [HttpPost("/{roleId:guid}/permissionsAssign/{permissionId:guid}")]
    public async Task<ActionResult<RoleDto>> AddRolePermission(Guid roleId, Guid permissionId)
    {
        var role = await _roleService.AddRolePermissionAsync(roleId, permissionId);
        return Ok(role);
    }
    
    [PermissionAuthorize("RoleManagement.Update")]
    [HttpDelete("/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<ActionResult> RemoveRolePermission(Guid roleId, Guid permissionId)
    {
        await _roleService.RemoveRolePermissionAsync(roleId, permissionId);
        return NoContent();
    }
    
}
