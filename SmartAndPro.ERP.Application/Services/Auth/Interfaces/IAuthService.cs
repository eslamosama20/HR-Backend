using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

namespace SmartAndPro.ERP.Application.Services.Auth.Interfaces;

public interface IAuthService
{
    Task<bool> AuthorizeAsync(Guid userId, string permission);
    
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId);
    
    Task AssignRolesAsync(Guid userId, List<Guid>  roleIds);
    
    Task UnassignRoleAsync(Guid userId, Guid  roleId);
    Task AssignRoleAsync(Guid userId, Guid  roleIds);
    
    Task<HashSet<string>> GetUserPermissionNamesAsync(Guid userId);
}