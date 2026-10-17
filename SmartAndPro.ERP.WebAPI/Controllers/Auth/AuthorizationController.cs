using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.WebAPI.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthorizationController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuthorizationPolicyService _authorizationPolicyService;
    private readonly IRoleService _roleService;

    public AuthorizationController(
        IAuthService authService,
        IAuthorizationPolicyService authorizationPolicyService,
        IRoleService roleService)
    {
        _authService = authService;
        _authorizationPolicyService = authorizationPolicyService;
        _roleService = roleService;
    }
    
    // get all policies
    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies()
    {
        try
        {
            var policies = await _authorizationPolicyService.GetAllPoliciesAsync();
            if (policies == null) return NotFound();
            return Ok(policies);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    // get role permissions
    [HttpGet("rolePermissions/{roleName}")]
    public async Task<IActionResult> GetRolePermissions(string roleName)
    {
        try
        {
            var rolePermissions = await _roleService.GetRolePermissionsAsync(roleName);
            if (rolePermissions == null) return NotFound();
            return Ok(rolePermissions);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    // authorize user got from the http context by checking if user has role or permission
    [HttpPost("authorize")]
    [Authorize]
    public async Task<IActionResult> Authorize([FromBody] AuthorizeDto authorizeDto)
    {
        try
        {
            var roleToCheck = authorizeDto.Role;
            var permissionToCheck = authorizeDto.Permission;
            if (string.IsNullOrEmpty(roleToCheck) && string.IsNullOrEmpty(permissionToCheck)) return BadRequest("Role or permission is required");
            var userClaims = HttpContext.User;
            if (userClaims == null) return Unauthorized();
            var userId = Guid.TryParse(userClaims.FindFirst(c => c.Type == ClaimTypes.Name).Value, out Guid userGuid) ? userGuid : Guid.Empty;
            var userRoles = await _authService.GetUserRolesAsync(userId);
            if (roleToCheck != null)
            {
                if (!userRoles.Any(r => r.Name == roleToCheck))
                {
                    return Unauthorized();
                }
            }
            
            // check if user has permission
            if (permissionToCheck != null)
            {
                var rules = 
                    userRoles.SelectMany(ur => ur.RolePermissions).Where(rp => rp.Permission.Name == permissionToCheck);
                if ( rules == null || !rules.Any())
                {
                    return Unauthorized();
                }
                return Ok();
            } 
            return Ok();
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    [HttpPost("assignRole")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
    {
        try
        {
            await _authService.AssignRoleAsync(assignRoleDto.UserId, assignRoleDto.RoleId);
            return Ok();
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    [HttpPost("unassignRole")]
    public async Task<IActionResult> UnassignRole([FromBody] AssignRoleDto unassignRoleDto)
    {
        try
        {
            await _authService.UnassignRoleAsync(unassignRoleDto.UserId, unassignRoleDto.RoleId);
            return Ok();
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    [HttpPost("assignRoles")]
    public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto assignRolesDto)
    {
        try
        {
            await _authService.AssignRolesAsync(assignRolesDto.UserId, assignRolesDto.RoleIds);
            return Ok();
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            throw;
        }
    }
    
    
    
}