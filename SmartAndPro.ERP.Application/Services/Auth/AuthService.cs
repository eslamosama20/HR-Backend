using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Abstraction;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Auth;

public class AuthService : BaseService , IAuthService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IMapper _mapper;
    public AuthService(RepositoriesFactory repositoriesFactory , AppSettings appSettings , IMapper mapper) : base(repositoriesFactory, appSettings)
    {
        _repositoriesFactory = repositoriesFactory;
        _userRepository = _repositoriesFactory.GetRepository<User>();
        _roleRepository = _repositoriesFactory.GetRepository<Role>();
        _permissionRepository = _repositoriesFactory.GetRepository<Permission>();
        _userRoleRepository = _repositoriesFactory.GetRepository<UserRole>();
        _mapper = mapper;
    }
    

    public async Task<bool> AuthorizeAsync(Guid userId, string permission)
    {
        try
        {
            var userQuery = await _userRepository.Filter(x => x.Id == userId);
            if (userQuery.Data == null)
            {
                throw new Exception("User not found.");
            }
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var hasPermission = user.UserRoles
                .Any(ur => ur.Role.RolePermissions
                    .Any(rp => rp.Permission.Name == permission));

            return hasPermission;
        }
        catch (Exception ex)
        {
            // Consider using a more specific exception handling strategy
            throw new Exception($"Authorization failed: {ex.Message}");
        }
    }
    public async Task<bool> AuthorizeAsync(Guid userId, string roleName, string permission)
    {
        try
        {
            var userQuery = await _userRepository.Filter(x => x.Id == userId);
            if (userQuery.Data == null)
            {
                throw new Exception("User not found.");
            }
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var hasPermission = user.UserRoles
                .Where(ur => ur.Role.Name == roleName)
                .Any(ur => ur.Role.RolePermissions
                    .Any(rp => rp.Permission.Name == permission));

            return hasPermission;
        }
        catch (Exception ex)
        {
            // Consider using a more specific exception handling strategy
            throw new Exception($"Authorization failed: {ex.Message}");
        }
    }

    public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        var userQuery = await _userRepository.Filter(x => x.Id == userId);
        if (userQuery.Data == null)
        {
            throw new Exception("User not found.");
        }
        var user = await userQuery.Data.FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(user.UserRoles.Select(ur => ur.Role));
        return roleDtos;
    }

    public async Task AssignRolesAsync(Guid userId, List<Guid> roleIds)
    {
        var userQuery = await _userRepository.Filter(x => x.Id == userId);
        if (userQuery.Data == null)
        {
            throw new Exception("User not found.");
        }
        var user = await userQuery.Data.FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var rolesQuery = await _roleRepository.Filter(r => roleIds.Contains(r.Id));
        if (rolesQuery.Data == null)
        {
            throw new Exception("Role not found.");
        }
        var roles = await rolesQuery.Data.ToListAsync();

        var userRoleRepo = _repositoriesFactory.GetRepository<UserRole>(true);
        try
        {
            await userRoleRepo.BeginTransactionAsync();
            foreach (var role in roles)
            {
                if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
                {
                    continue;
                }
                var userRole = new UserRole { UserId = userId, RoleId = role.Id };
                var CreateUserRole = await userRoleRepo.Add(userRole);
                if (CreateUserRole.ErrorCode != ErrorCodes.Success)
                    throw new Exception($"Failed to assign role {role.Id} to user {userId}.");
            }
            await userRoleRepo.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await userRoleRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            throw ;
        }
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId)
    {
        var userQuery = await _userRepository.Filter(x => x.Id == userId);
        if (userQuery.Data == null)
        {
            throw new Exception("User not found.");
        }
        var user = await userQuery.Data.FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var roleQuery = await _roleRepository.Filter(x => x.Id == roleId);
        if (roleQuery.Data == null)
        {
            throw new Exception("Role not found.");
        }
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null)
        {
            throw new Exception("Role not found.");
        }

        if (user.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            throw new Exception("User already has this role.");
        }
        
        var userRole = new UserRole { UserId = userId, RoleId = roleId };
        var createUserRoleQuery = await _userRoleRepository.Add(userRole);
        if (createUserRoleQuery == null)
            throw new Exception("UserRole not created.");
        var createUserRole = createUserRoleQuery.Data;
        if (createUserRole == null)
        {
            throw new Exception("UserRole not created.");
        }
    }
    
    public async Task UnassignRoleAsync(Guid userId, Guid roleId)
    {
        var userQuery = await _userRepository.Filter(x => x.Id == userId);
        if (userQuery.Data == null)
        {
            throw new Exception("User not found.");
        }
        var user = await userQuery.Data.FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }
        
        var roleQuery = await _roleRepository.Filter(x => x.Id == roleId);
        if (roleQuery.Data == null)
        {
            throw new Exception("Role not found.");
        }
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null)
        {
            throw new Exception("Role not found.");
        }
        
        if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            throw new Exception("User does not have this role.");
        }
        
        var userRole = user.UserRoles.First(ur => ur.RoleId == roleId);
        userRole.Role = null;
        userRole.User = null;
        var deleteUserRoleQuery = await _userRoleRepository.Delete(userRole);
        if (deleteUserRoleQuery.Data == null)
            throw new Exception("UserRole not deleted.");
        var deleteUserRole = deleteUserRoleQuery.Data;
        if (deleteUserRole == null)
            throw new Exception("UserRole not deleted.");
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId)
    {
        var roleQuery = await _roleRepository.Filter(x => x.Id == roleId);
        if (roleQuery.Data == null)
        {
            throw new Exception("Role not found.");
        }
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null)
        {
            throw new Exception("Role not found.");
        }

        var permissionDtos = _mapper.Map<IEnumerable<PermissionDto>>(role.RolePermissions.Select(rp => rp.Permission));
        return permissionDtos;
    }

    public async Task<HashSet<string>> GetUserPermissionNamesAsync(Guid userId)
    {
        var userQuery = await _userRepository.Filter(x => x.Id == userId);
        if (userQuery.Data == null)
        {
            throw new Exception("User not found.");
        }
        var user = await userQuery.Data.FirstOrDefaultAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var permissionNames = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .ToHashSet();

        return permissionNames;
    }
    
}