using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Abstraction;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Auth;

public class RoleService : BaseService, IRoleService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository;
    private readonly IMapper _mapper;

    
    public RoleService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings)
    {
        _repositoriesFactory = repositoriesFactory;
        _roleRepository = _repositoriesFactory.GetRepository<Role>();
        _permissionRepository = _repositoriesFactory.GetRepository<Permission>();
        _rolePermissionRepository = _repositoriesFactory.GetRepository<RolePermission>();
        _mapper = mapper;
    }

    public async Task<RoleDto> GetRoleByIdAsync(Guid id)
    {
        var roleQuery = await _roleRepository.Filter(x => x.Id == id);
        if (roleQuery.Data == null)
        {
            throw new Exception("Role not found.");
        }
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null)
        {
            throw new Exception("Role not found.");
        }

        var permissionsQuery = await _rolePermissionRepository.Filter(x => x.RoleId == id);
        role.RolePermissions = await permissionsQuery.Data.ToListAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync( Expression<Func<Role, bool>> predicate = null)
    {
        List<Role> roles;
        if (predicate == null)
        {
            var rolesQuery = await _roleRepository.Filter(x => x.Name != "System.Developer");
            roles = await rolesQuery.Data.ToListAsync();
        }
        else
        {
            var filteredRolesQuery = await _roleRepository.Filter(predicate);
            roles = await filteredRolesQuery.Data.Where(x => x.Name != "System.Developer").ToListAsync();
        }

        return _mapper.Map<IEnumerable<RoleDto>>(roles);
    }

    public async Task<RoleDto> AddRoleAsync(RoleDto roleDto)
    {
        var incomingRole = _mapper.Map<Role>(roleDto);

        var roleExistsQuery = await _roleRepository.Filter(x => x.Name == incomingRole.Name);
        var roleExists = await roleExistsQuery.Data.FirstOrDefaultAsync();
        if (roleExists != null)
        {
            throw new Exception("Role name already exists.");
        }
        
        var role = await _roleRepository.Add(incomingRole);
        return _mapper.Map<RoleDto>(role.Data);
    }

    public async Task<RoleDto> UpdateRoleAsync(RoleDto roleDto)
    {
        var roleQuery = await _roleRepository.Filter(x => x.Id == roleDto.Id);
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null)
        {
            throw new Exception("Role not found.");
        }
        // delete old role permissions
        var oldRolePermissionsQuery = await _rolePermissionRepository.Filter(x => x.RoleId == roleDto.Id);
        var oldRolePermissions = await oldRolePermissionsQuery.Data.ToListAsync();
        foreach (var oldRolePermission in oldRolePermissions)
        {
            oldRolePermission.Role = null;
            oldRolePermission.Permission = null;
            await _rolePermissionRepository.Delete(oldRolePermission);
        }
        role.Name = roleDto.Name;
        role.IsFrontal = roleDto.IsFrontal;

        role.MaxUsersCount = roleDto.MaxUsersCount;
        role.MaxCompaniesCount = roleDto.MaxCompaniesCount;
        role.MaxBranchesCount = roleDto.MaxBranchesCount;
        role.MaxStoresCount = roleDto.MaxStoresCount;
        role.MaxBanksCount = roleDto.MaxBanksCount;
        role.MaxTreasuriesCount = roleDto.MaxTreasuriesCount;
        role.MaxSuppliersCount = roleDto.MaxSuppliersCount;
        role.MaxCustomersCount = roleDto.MaxCustomersCount;
        var rolePermissions = roleDto.RolePermissions.Select(rp => new RolePermission
        {
            RoleId = role.Id,
            PermissionId = rp.PermissionId
        }).ToList();
        await _rolePermissionRepository.AddRange(rolePermissions);
        role.RolePermissions = null;
        role.UserRoles = null;
        role.DomainDetails = null;
        await _roleRepository.Update(role);
        return roleDto;
    }

    public async Task<RoleDto> DeleteRoleAsync(Guid id)
    {
        var roleQuery = await _roleRepository.Filter(x => x.Id == id);
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null)
        {
            throw new Exception("Role not found.");
        }
        var roleToDelete = role;
        if (roleToDelete == null)
        {
            throw new Exception("Role not found.");
        }
        roleToDelete.IsActive = false;
        await _roleRepository.Update(roleToDelete);
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(string roleName)
    {
        var roleQuery = await _roleRepository.Filter(x => x.Name == roleName);
        if (roleQuery.Data == null)
        {
            throw new Exception("Role not found.");
        }
        var role = await roleQuery.Data.FirstOrDefaultAsync();
        if (role == null) return null;
            
        return role.RolePermissions.Select(rp => rp.Permission).ToList(); 
    }

    public async Task<RoleDto> AddRolePermissionAsync(Guid roleId, Guid permissionId)
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
        var permissionQuery = await _permissionRepository.Filter(x => x.Id == permissionId);
        if (permissionQuery.Data == null)
        {
            throw new Exception("Permission not found.");
        }
        var permission = await permissionQuery.Data.FirstOrDefaultAsync();
        if (permission == null)
        {
            throw new Exception("Permission not found.");
        }
        
        
        if(role.DomainDetailsId != permission.DomainDetailsId)
        {
            throw new Exception("Permission and Role must belong to the same domain.");
        }
        var rolePermissionExists = role.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId && rp.RoleId == roleId);
        if (rolePermissionExists != null)
        {
            throw new Exception("Role permission already exists.");
        }
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        await _rolePermissionRepository.Add(rolePermission);

        role.RolePermissions.Add(rolePermission);
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> RemoveRolePermissionAsync(Guid roleId, Guid permissionId)
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
        var rolePermission = role?.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission == null)
            return null;

        await _rolePermissionRepository.Delete(rolePermission);
        role.RolePermissions.Remove(rolePermission);

        return _mapper.Map<RoleDto>(role);
    }
    
    
    // Get All Frontal Roles
    public async Task<IEnumerable<RoleDto>> GetAllFrontalRolesAsync()
    {
        var frontalRolesQuery = await _roleRepository.Filter(x => x.IsFrontal);
        var frontalRoles = await frontalRolesQuery.Data.ToListAsync();
        return _mapper.Map<IEnumerable<RoleDto>>(frontalRoles);
    }
    
    // Make Role Frontal
    public async Task<RoleDto> MakeRoleFrontalAsync(Guid roleId)
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
        
        role.IsFrontal = true;
        await _roleRepository.Update(role);
        
        return _mapper.Map<RoleDto>(role);
    }
    
}