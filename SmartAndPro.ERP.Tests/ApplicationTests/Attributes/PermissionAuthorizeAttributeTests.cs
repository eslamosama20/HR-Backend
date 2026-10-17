using FluentAssertions;
using SmartAndPro.ERP.WebAPI.Attributes;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Attributes;

public class PermissionAuthorizeAttributeTests
{
    [Theory]
    [InlineData("Dashboard.Read")]
    [InlineData("Cashier.POS")]
    [InlineData("InventoryManagement.Read")]
    [InlineData("Pharmacy.Create")]
    public void Constructor_ShouldSetPermission_WhenGivenValidPermission(string permission)
    {
        // Act
        var attribute = new PermissionAuthorizeAttribute(permission);

        // Assert
        attribute.Permission.Should().Be(permission);
    }

    [Theory]
    [InlineData("Dashboard.Read", "Permission.Dashboard.Read")]
    [InlineData("Cashier.POS", "Permission.Cashier.POS")]
    [InlineData("SalesInvoice.Read", "Permission.SalesInvoice.Read")]
    public void Policy_ShouldReturnPermissionWithPrefix_WhenSetDirectly(string permission, string expectedPolicy)
    {
        // Act
        var attribute = new PermissionAuthorizeAttribute(permission);

        // Assert
        attribute.Policy.Should().Be(expectedPolicy);
    }

    [Theory]
    [InlineData("Permission.Dashboard.Read", "Dashboard.Read")]
    [InlineData("Permission.Cashier.POS", "Cashier.POS")]
    public void Permission_Getter_ShouldStripPermissionPrefix_WhenPolicyIsSet(string policy, string expectedPermission)
    {
        // Arrange
        var attribute = new PermissionAuthorizeAttribute("dummy");
        attribute.Policy = policy;

        // Act
        var result = attribute.Permission;

        // Assert
        result.Should().Be(expectedPermission);
    }

    [Fact]
    public void Permission_Setter_ShouldUpdatePolicy_WithPermissionPrefix()
    {
        // Arrange
        var attribute = new PermissionAuthorizeAttribute("old.permission");

        // Act
        attribute.Permission = "new.permission";

        // Assert
        attribute.Policy.Should().Be("Permission.new.permission");
        attribute.Permission.Should().Be("new.permission");
    }

    [Fact]
    public void ShouldInheritFromAuthorizeAttribute()
    {
        // Act
        var attribute = new PermissionAuthorizeAttribute("test");

        // Assert
        attribute.Should().BeAssignableTo<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
    }

    [Fact]
    public void Policy_ShouldBeNonNull_WhenConstructed()
    {
        // Act
        var attribute = new PermissionAuthorizeAttribute("Dashboard.Read");

        // Assert
        attribute.Policy.Should().NotBeNullOrWhiteSpace();
    }
}
