using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Abstraction;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Auth;

public class AuthorizationPolicyService : BaseService , IAuthorizationPolicyService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    
    public AuthorizationPolicyService(RepositoriesFactory repositoriesFactory, AppSettings appSettings) : base(repositoriesFactory, appSettings)
    {
        _repositoriesFactory = repositoriesFactory;
        _roleRepository = _repositoriesFactory.GetRepository<Role>();
        _permissionRepository = _repositoriesFactory.GetRepository<Permission>();
    }

    public async Task<IEnumerable<AuthorizationPolicyDto>> GetAllPoliciesAsync()
    {
        var policies = new List<AuthorizationPolicyDto>();
        var permissionsQuery = await _permissionRepository.GetAll();
        if (permissionsQuery.Data == null)
            throw new Exception("Permissions not found.");
        var permissions = await permissionsQuery.Data.ToListAsync();
        foreach (var permission in permissions)
        {
            policies.Add(new AuthorizationPolicyDto
            {
                Name = $"{PolicyType.Permission.ToString()}.{permission.Name}",
                Value = permission.Name,
                Type = PolicyType.Permission
            });
        }

        var rolesQuery = await _roleRepository.GetAll();
        if (rolesQuery.Data == null)
            throw new Exception("Roles not found.");
        var roles = await rolesQuery.Data.ToListAsync();
        foreach (var role in roles)
        {
            policies.Add(new AuthorizationPolicyDto
            {
                Name = $"{PolicyType.Role.ToString()}.{role.Name}",
                Value = role.Name,
                Type = PolicyType.Role
            });
        }

        return policies;
    }
}