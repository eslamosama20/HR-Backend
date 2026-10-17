using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using SmartAndPro.ERP.Application.Services.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Auth;

public class TokenServiceTests
{
    [Fact]
    public async Task CreateToken_ShouldIncludeRoleClaims_AndCanonicalAliases()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "cashier@example.com",
            UserName = "cashier.user",
            UserRoles = new List<UserRole>()
        };

        var userRoles = new List<UserRole>
        {
            new()
            {
                UserId = userId,
                Role = new Role { Name = "System.Admin" }
            },
            new()
            {
                UserId = userId,
                Role = new Role { Name = "Cashier.Operator" }
            }
        }.AsQueryable();

        var userRoleRepositoryMock = new Mock<IRepository<UserRole>>();
        userRoleRepositoryMock
            .Setup(repository => repository.Filter(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>() ))
            .ReturnsAsync(new OperationResponse<IQueryable<UserRole>>(ErrorCodes.Success, userRoles));

        var repositoriesFactoryMock = new Mock<RepositoriesFactory>();
        repositoriesFactoryMock
            .Setup(factory => factory.GetRepository<UserRole>(It.IsAny<bool?>()))
            .Returns(userRoleRepositoryMock.Object);

        var appSettings = new AppSettings
        {
            Token = new TokenSettings
            {
                Key = "this_is_a_test_signing_key_123456789",
                Issuer = "SmartAndPro.Tests",
                DurationInDays = "7"
            }
        };

        var mapperMock = new Mock<IMapper>();
        var service = new TokenService(repositoriesFactoryMock.Object, appSettings, mapperMock.Object);

        // Act
        var token = await service.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roleClaims = jwt.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToList();

        // Assert
        roleClaims.Should().Contain("System.Admin");
        roleClaims.Should().Contain("Cashier.Operator");
        roleClaims.Should().Contain("Admin");
        roleClaims.Should().Contain("Cashier");
    }
}
