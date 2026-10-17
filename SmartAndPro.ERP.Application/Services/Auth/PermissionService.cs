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

public class PermissionService : BaseService , IPermissionService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IRepository<Permission> _permissionRepository;
        private readonly IMapper _mapper;
    
    public PermissionService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) : base(repositoriesFactory, appSettings)
    {
        _repositoriesFactory = repositoriesFactory;
        _permissionRepository = _repositoriesFactory.GetRepository<Permission>();
        _mapper = mapper;
    }

    public async Task<PermissionDto> GetPermissionByIdAsync(Guid id)
    {
        var permissionQuery = await _permissionRepository.Filter(x => x.Id == id);
        if (permissionQuery.Data == null)
        {
            throw new Exception("Permission not found.");
        }

        var permission = await permissionQuery.Data.FirstOrDefaultAsync();
        if (permission == null)
        {
            throw new Exception("Permission not found.");
        }
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description
        };
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissionsQuery = await _permissionRepository.GetAll();
        if (permissionsQuery.Data == null)
        {
            throw new Exception("Permissions not found.");
        }
        var permissions = await permissionsQuery.Data.Include(x => x.DomainDetails).ToListAsync();
        
        return  _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<PermissionDto> AddPermissionAsync(PermissionDto permissionDto)
    {
        // check with permission name already exists
        var permissionExistsQuery = await _permissionRepository.Filter(x => x.Name == permissionDto.Name);
        if (permissionExistsQuery.Data != null)
        {
            throw new Exception("Permission name already exists.");
        }
        var permissionExists = await permissionExistsQuery.Data.FirstOrDefaultAsync();
        if (permissionExists != null)
        {
            throw new Exception("Permission name already exists.");
        }
        var permission =  _mapper.Map<Permission>(permissionDto);
        var permissionResult = await _permissionRepository.Add(permission);
        return _mapper.Map<PermissionDto>(permissionResult.Data);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(PermissionDto permissionDto)
    {
        var permissionQuery = await _permissionRepository.Filter(x => x.Id == permissionDto.Id);
        if (permissionQuery.Data == null)
        {
            throw new Exception("Permission not found.");
        }
        var permission = await permissionQuery.Data.FirstOrDefaultAsync();
        if (permission == null) throw new Exception("Permission not found.");

        permission.Name = permissionDto.Name;
        permission.Description = permissionDto.Description;

        var updatedPermissionQuery = await _permissionRepository.Update(permission);
        var updatedPermission = updatedPermissionQuery.Data;
        if (updatedPermission == null) throw new Exception("Permission not found.");
        return new PermissionDto
        {
            Id = updatedPermission.Id,
            Name = updatedPermission.Name,
            Description = updatedPermission.Description
        };
    }

    public async Task<PermissionDto> DeletePermissionAsync(Guid id)
    {
        var permissionQuery = await _permissionRepository.Filter(x => x.Id == id);
        if (permissionQuery.Data == null)
        {
            throw new Exception("Permission not found.");
        }
        var permission = await permissionQuery.Data.FirstOrDefaultAsync();
        if (permission == null)
        {
            throw new Exception("Permission not found.");
        }
        await _permissionRepository.Delete(permission);
        return new PermissionDto
        {
            Id = id,
            Name = string.Empty,
            Description = string.Empty
        };
    }
}
