using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using SmartAndPro.ERP.Application.Authorization;
using SmartAndPro.ERP.Application.Services.Auth;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _roleServiceMock = new Mock<IRoleService>();
        _authServiceMock = new Mock<IAuthService>();
        _handler = new PermissionAuthorizationHandler(_roleServiceMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task ShouldSucceed_WhenUserHasDirectPermissionClaim()
    {
        // Arrange
        var requirement = new PermissionRequirement("Dashboard.Read");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("Permission", "Dashboard.Read")
        }));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldSucceed_WhenUserRoleHasPermission()
    {
        // Arrange
        var requirement = new PermissionRequirement("Cashier.POS");
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Cashier"),
            new Claim(ClaimTypes.Name, userId.ToString())
        }));

        _roleServiceMock
            .Setup(s => s.GetRolePermissionsAsync("Cashier"))
            .ReturnsAsync(new List<Permission> { new() { Name = "Cashier.POS" } });

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldSucceed_WhenAuthServiceAuthorizesUser()
    {
        // Arrange
        var requirement = new PermissionRequirement("InventoryManagement.Read");
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, userId.ToString())
        }));

        _roleServiceMock
            .Setup(s => s.GetRolePermissionsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Permission>());

        _authServiceMock
            .Setup(s => s.AuthorizeAsync(userId, "InventoryManagement.Read"))
            .ReturnsAsync(true);

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldFail_WhenNoStrategyMatches()
    {
        // Arrange
        var requirement = new PermissionRequirement("Admin.Read");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Cashier"),
            new Claim(ClaimTypes.Name, Guid.NewGuid().ToString())
        }));

        _roleServiceMock
            .Setup(s => s.GetRolePermissionsAsync("Cashier"))
            .ReturnsAsync(new List<Permission> { new() { Name = "Cashier.POS" } });

        _authServiceMock
            .Setup(s => s.AuthorizeAsync(It.IsAny<Guid>(), "Admin.Read"))
            .ReturnsAsync(false);

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldFail_WhenUserIsNull()
    {
        // Arrange
        var requirement = new PermissionRequirement("Dashboard.Read");
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldFail_WhenUserHasNoClaims()
    {
        // Arrange
        var requirement = new PermissionRequirement("Dashboard.Read");
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        _authServiceMock
            .Setup(s => s.AuthorizeAsync(It.IsAny<Guid>(), "Dashboard.Read"))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldCheckDirectClaimFirst_BeforeRoleLookup()
    {
        // Arrange
        var requirement = new PermissionRequirement("Dashboard.Read");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("Permission", "Dashboard.Read"),
            new Claim(ClaimTypes.Role, "Cashier"),
            new Claim(ClaimTypes.Name, Guid.NewGuid().ToString())
        }));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        _roleServiceMock.Verify(
            s => s.GetRolePermissionsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ShouldSkipRoleService_WhenUserHasNoRoleClaims()
    {
        // Arrange
        var requirement = new PermissionRequirement("Dashboard.Read");
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, userId.ToString())
        }));

        _authServiceMock
            .Setup(s => s.AuthorizeAsync(userId, "Dashboard.Read"))
            .ReturnsAsync(true);

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        _roleServiceMock.Verify(
            s => s.GetRolePermissionsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ShouldFail_WhenRoleServiceThrows()
    {
        // Arrange - after bug fix, handler enters role check and exception propagates
        var requirement = new PermissionRequirement("Dashboard.Read");
        var userId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Cashier"),
            new Claim(ClaimTypes.Name, userId.ToString())
        }));

        _roleServiceMock
            .Setup(s => s.GetRolePermissionsAsync("Cashier"))
            .ThrowsAsync(new Exception("DB connection failed"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act & Assert - exception propagates from role service
        await _handler.Invoking(h => h.HandleAsync(context))
            .Should().ThrowAsync<Exception>()
            .WithMessage("DB connection failed");
    }
}
