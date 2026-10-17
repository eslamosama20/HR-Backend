using System.Linq.Expressions;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

namespace SmartAndPro.ERP.Application.Services.Auth.Interfaces;

public interface IRoleService
{
    Task<RoleDto> GetRoleByIdAsync(Guid id);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync(Expression<Func<Role, bool>> predicate = null);
    Task<RoleDto> AddRoleAsync(RoleDto roleDto);
    Task<RoleDto> UpdateRoleAsync(RoleDto roleDto);
    Task<RoleDto> DeleteRoleAsync(Guid id);
    
    Task<List<Permission>> GetRolePermissionsAsync(string roleName);
    
    Task<RoleDto> AddRolePermissionAsync(Guid roleId, Guid permissionId);
    Task<RoleDto> RemoveRolePermissionAsync(Guid roleId, Guid permissionId);
    
    Task<RoleDto> MakeRoleFrontalAsync(Guid id);
    Task<IEnumerable<RoleDto>> GetAllFrontalRolesAsync();
    
}