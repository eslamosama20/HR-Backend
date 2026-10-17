using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartAndPro.ERP.Application.Common.AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Inventory;

public class CompanyServiceLogoUpdateTests
{
    private static IMapper CreateMapper()
        => new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>()).CreateMapper();

    private static Mock<IAutoNumberGenerator> CreateGenerator()
    {
        var generator = new Mock<IAutoNumberGenerator>();
        generator
            .Setup(g => g.GenerateAsync<It.IsAnyType>(
                It.IsAny<DbContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("1");
        return generator;
    }

    private static ERPContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ERPContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ERPContext(options, CreateGenerator().Object);
    }

    private static CompanyService CreateService(ERPContext context, IMapper mapper)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() })
            .BuildServiceProvider();

        var repositoriesFactory = new RepositoriesFactory(context, serviceProvider);
        var appSettings = new AppSettings();

        return new CompanyService(repositoriesFactory, appSettings, mapper);
    }

    [Fact]
    public async Task UpdateAsync_WithNewLogoUrl_UpdatesLogoUrlAndClearsLegacyLogo()
    {
        var dbName = Guid.NewGuid().ToString();
        var mapper = CreateMapper();
        using var context = CreateContext(dbName);
        var service = CreateService(context, mapper);

        var companyId = Guid.NewGuid();
        var existingCompany = new CompanyDetails
        {
            Id = companyId,
            ArabicName = "شركة تجريبية",
            EnglishName = "Test Company",
            TaxNumber = "300000000000003",
            Logo = new byte[] { 1, 2, 3, 4 },
            LogoUrl = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        context.CompanyDetails.Add(existingCompany);
        await context.SaveChangesAsync();

        var updateDto = new CompanyDetailsPutDto
        {
            Id = companyId,
            ArabicName = "شركة تجريبية معدلة",
            EnglishName = "Updated Test Company",
            TaxNumber = "300000000000003",
            LogoUrl = "https://res.cloudinary.com/test/image/upload/v1/company/logo/logo123.png",
            Logo = null
        };

        var response = await service.UpdateAsync(updateDto);

        response.ErrorCode.Should().Be(ErrorCodes.Success);
        response.Data.Should().NotBeNull();
        response.Data!.LogoUrl.Should().Be("https://res.cloudinary.com/test/image/upload/v1/company/logo/logo123.png");

        var updatedFromDb = await context.CompanyDetails.FindAsync(companyId);
        updatedFromDb.Should().NotBeNull();
        updatedFromDb!.LogoUrl.Should().Be("https://res.cloudinary.com/test/image/upload/v1/company/logo/logo123.png");
        updatedFromDb.Logo.Should().BeNull();
        updatedFromDb.ArabicName.Should().Be("شركة تجريبية معدلة");
    }

    [Fact]
    public async Task UpdateAsync_WithoutChangingLogo_PreservesExistingLogoUrl()
    {
        var dbName = Guid.NewGuid().ToString();
        var mapper = CreateMapper();
        using var context = CreateContext(dbName);
        var service = CreateService(context, mapper);

        var companyId = Guid.NewGuid();
        const string existingLogoUrl = "https://res.cloudinary.com/test/image/upload/v1/company/logo/existing.png";
        const string existingStampUrl = "https://res.cloudinary.com/test/image/upload/v1/company/stamp/existing_stamp.png";

        var existingCompany = new CompanyDetails
        {
            Id = companyId,
            ArabicName = "شركة الأمل",
            EnglishName = "Al Amal Company",
            TaxNumber = "300000000000003",
            LogoUrl = existingLogoUrl,
            StampUrl = existingStampUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        context.CompanyDetails.Add(existingCompany);
        await context.SaveChangesAsync();

        var updateDto = new CompanyDetailsPutDto
        {
            Id = companyId,
            ArabicName = "شركة الأمل المتطورة",
            EnglishName = "Al Amal Advanced Company",
            TaxNumber = "300000000000003",
            LogoUrl = existingLogoUrl,
            StampUrl = existingStampUrl
        };

        var response = await service.UpdateAsync(updateDto);

        response.ErrorCode.Should().Be(ErrorCodes.Success);
        response.Data!.LogoUrl.Should().Be(existingLogoUrl);
        response.Data.StampUrl.Should().Be(existingStampUrl);

        var updatedFromDb = await context.CompanyDetails.FindAsync(companyId);
        updatedFromDb!.LogoUrl.Should().Be(existingLogoUrl);
        updatedFromDb.StampUrl.Should().Be(existingStampUrl);
        updatedFromDb.ArabicName.Should().Be("شركة الأمل المتطورة");
    }

    [Fact]
    public void AutoMapper_CompanyDetailsPutDto_WithUrlInLogoOrLogoUrl_MapsSafelyWithoutThrowing()
    {
        var mapper = CreateMapper();

        var putDto = new CompanyDetailsPutDto
        {
            Id = Guid.NewGuid(),
            ArabicName = "الشركة",
            EnglishName = "Company",
            TaxNumber = "300000000000003",
            Logo = "https://res.cloudinary.com/test/image/upload/v1/company/logo/logo.png",
            Stamp = "https://res.cloudinary.com/test/image/upload/v1/company/stamp/stamp.png"
        };

        var entity = mapper.Map<CompanyDetails>(putDto);

        entity.Should().NotBeNull();
        entity.LogoUrl.Should().Be("https://res.cloudinary.com/test/image/upload/v1/company/logo/logo.png");
        entity.StampUrl.Should().Be("https://res.cloudinary.com/test/image/upload/v1/company/stamp/stamp.png");
        entity.Logo.Should().BeNullOrEmpty();
        entity.Stamp.Should().BeNullOrEmpty();
    }
}
