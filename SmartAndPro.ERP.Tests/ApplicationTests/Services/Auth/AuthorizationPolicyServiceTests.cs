using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth;
using SmartAndPro.ERP.Domains.Entities.Auth;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.Tests.ApplicationTests.Helpers;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Auth;

public class AuthorizationPolicyServiceTests
{
    private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
    private readonly Mock<IRepository<Permission>> _permissionRepositoryMock;
    private readonly Mock<IRepository<Role>> _roleRepositoryMock;
    private readonly Mock<AppSettings> _appSettingsMock;
    private readonly AuthorizationPolicyService _service;

    public AuthorizationPolicyServiceTests()
    {
        _repositoriesFactoryMock = new Mock<RepositoriesFactory>();
        _permissionRepositoryMock = new Mock<IRepository<Permission>>();
        _roleRepositoryMock = new Mock<IRepository<Role>>();
        _appSettingsMock = new Mock<AppSettings>();

        _repositoriesFactoryMock
            .Setup(f => f.GetRepository<Permission>(It.IsAny<bool?>()))
            .Returns(_permissionRepositoryMock.Object);
        _repositoriesFactoryMock
            .Setup(f => f.GetRepository<Role>(It.IsAny<bool?>()))
            .Returns(_roleRepositoryMock.Object);

        _service = new AuthorizationPolicyService(
            _repositoriesFactoryMock.Object,
            _appSettingsMock.Object);
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldReturnPermissionPolicies()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "Dashboard.Read" },
            new() { Id = Guid.NewGuid(), Name = "Cashier.POS" }
        };
        var roles = new List<Role>();

        SetupPermissions(permissions);
        SetupRoles(roles);

        // Act
        var result = (await _service.GetAllPoliciesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Permission.Dashboard.Read" && p.Type == PolicyType.Permission);
        result.Should().Contain(p => p.Name == "Permission.Cashier.POS" && p.Type == PolicyType.Permission);
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldReturnRolePolicies()
    {
        // Arrange
        var permissions = new List<Permission>();
        var roles = new List<Role>
        {
            new() { Id = Guid.NewGuid(), Name = "System.Admin", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Cashier.User", IsActive = true }
        };

        SetupPermissions(permissions);
        SetupRoles(roles);

        // Act
        var result = (await _service.GetAllPoliciesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Role.System.Admin" && p.Type == PolicyType.Role);
        result.Should().Contain(p => p.Name == "Role.Cashier.User" && p.Type == PolicyType.Role);
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldReturnCombinedPolicies()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "Dashboard.Read" }
        };
        var roles = new List<Role>
        {
            new() { Id = Guid.NewGuid(), Name = "Admin", IsActive = true }
        };

        SetupPermissions(permissions);
        SetupRoles(roles);

        // Act
        var result = (await _service.GetAllPoliciesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Type == PolicyType.Permission);
        result.Should().Contain(p => p.Type == PolicyType.Role);
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldSetPolicyValue_ToEntityName()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "SalesInvoice.Read" }
        };

        SetupPermissions(permissions);
        SetupRoles(new List<Role>());

        // Act
        var result = (await _service.GetAllPoliciesAsync()).ToList();

        // Assert
        var policy = result.First(p => p.Type == PolicyType.Permission);
        policy.Value.Should().Be("SalesInvoice.Read");
        policy.Name.Should().Be("Permission.SalesInvoice.Read");
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldThrow_WhenPermissionsNotFound()
    {
        // Arrange
        _permissionRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<Permission>>
            {
                Data = null,
                ErrorCode = ErrorCodes.NoContent
            });

        SetupRoles(new List<Role>());

        // Act & Assert
        await _service.Invoking(s => s.GetAllPoliciesAsync())
            .Should().ThrowAsync<Exception>()
            .WithMessage("*Permissions not found*");
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldThrow_WhenRolesNotFound()
    {
        // Arrange
        SetupPermissions(new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "Dashboard.Read" }
        });

        _roleRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<Role>>
            {
                Data = null,
                ErrorCode = ErrorCodes.NoContent
            });

        // Act & Assert
        await _service.Invoking(s => s.GetAllPoliciesAsync())
            .Should().ThrowAsync<Exception>()
            .WithMessage("*Roles not found*");
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ShouldReturnEmpty_WhenNoPermissionsOrRoles()
    {
        // Arrange
        SetupPermissions(new List<Permission>());
        SetupRoles(new List<Role>());

        // Act
        var result = (await _service.GetAllPoliciesAsync()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    private void SetupPermissions(List<Permission> permissions)
    {
        var queryable = permissions.AsAsyncQueryable();
        _permissionRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<Permission>>
            {
                Data = queryable,
                ErrorCode = ErrorCodes.Success
            });
    }

    private void SetupRoles(List<Role> roles)
    {
        var queryable = roles.AsAsyncQueryable();
        _roleRepositoryMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<Role>>
            {
                Data = queryable,
                ErrorCode = ErrorCodes.Success
            });
    }
}
