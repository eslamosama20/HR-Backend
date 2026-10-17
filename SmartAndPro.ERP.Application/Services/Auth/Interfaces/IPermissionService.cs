using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

namespace SmartAndPro.ERP.Application.Services.Auth.Interfaces;

public interface IPermissionService
{
    Task<PermissionDto> GetPermissionByIdAsync(Guid id);
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto> AddPermissionAsync(PermissionDto permissionDto);
    Task<PermissionDto> UpdatePermissionAsync(PermissionDto permissionDto);
    Task<PermissionDto> DeletePermissionAsync(Guid id);
    
}