#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartAndPro.ERP.Application.Common.AutoMapper;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.SalesInvoice;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Invoice.Sale;

/// <summary>
/// Unit tests specifically validating the backend optimizations and helper queries in SaleInvoiceService:
/// - Fast quotation lookup queries (GetOriginalQuotationNoAsync / GetQuotationNoByIdAsync).
/// - Net amount calculations.
/// - Include / Projection data integrity.
/// </summary>
public class SaleInvoiceServiceBackendOptimizationTests
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

    private static SaleInvoiceService CreateService(ERPContext context)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() })
            .BuildServiceProvider();

        var repositoriesFactory = new RepositoriesFactory(context, serviceProvider);
        var salesInvoicesRepo = new Mock<ISalesInvoicesRepository>();
        var notificationService = new Mock<INotificationService>();
        var purchaseInvoiceService = new Mock<IPurchaseInvoiceService>();
        var appSettings = new AppSettings();
        var mapper = CreateMapper();

        var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var serviceBaseFactory = new ServiceBaseFactory(repositoriesFactory, appSettings, mapper, httpContextAccessor);

        return new SaleInvoiceService(
            repositoriesFactory,
            salesInvoicesRepo.Object,
            appSettings,
            mapper,
            serviceBaseFactory,
            notificationService.Object,
            purchaseInvoiceService.Object);
    }

    [Fact]
    public async Task GetOriginalQuotationNoAsync_WhenConvertedQuotationExists_ReturnsQuotationInvoiceNo()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var quotation = new SaleInvoice
        {
            Id = 10,
            InvoiceNo = "Q-2026-001",
            Currency = "SAR",
            IsQuotation = true,
            ConvertedToInvoiceId = 55
        };
        var targetInvoice = new SaleInvoice
        {
            Id = 55,
            InvoiceNo = "FINV-100",
            Currency = "SAR",
            IsQuotation = false
        };

        context.Set<SaleInvoice>().AddRange(quotation, targetInvoice);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetOriginalQuotationNoAsync(55);

        // Assert
        result.Should().Be("Q-2026-001");
    }

    [Fact]
    public async Task GetOriginalQuotationNoAsync_WhenNoQuotationMatches_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var targetInvoice = new SaleInvoice
        {
            Id = 99,
            InvoiceNo = "FINV-200",
            Currency = "SAR",
            IsQuotation = false
        };

        context.Set<SaleInvoice>().Add(targetInvoice);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetOriginalQuotationNoAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetQuotationNoByIdAsync_WhenIdIsQuotation_ReturnsQuotationInvoiceNo()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var quotation = new SaleInvoice
        {
            Id = 77,
            InvoiceNo = "Q-777",
            Currency = "SAR",
            IsQuotation = true
        };

        context.Set<SaleInvoice>().Add(quotation);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetQuotationNoByIdAsync(77);

        // Assert
        result.Should().Be("Q-777");
    }

    [Fact]
    public async Task GetQuotationNoByIdAsync_WhenInvoiceIsNotQuotation_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var regularInvoice = new SaleInvoice
        {
            Id = 88,
            InvoiceNo = "INV-888",
            Currency = "SAR",
            IsQuotation = false
        };

        context.Set<SaleInvoice>().Add(regularInvoice);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetQuotationNoByIdAsync(88);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetNetAmount_WithItemsAndDiscount_ComputesCorrectNetAmount()
    {
        // Arrange
        var invoice = new SaleInvoice
        {
            DiscountAmount = 50m,
            SaleItems = new List<SaleItem>
            {
                new() { Price = 100m, Quantity = 2m, NoOfNights = 1 }, // 200
                new() { Price = 150m, Quantity = 1m, NoOfNights = 2 }  // 300
            }
        };

        // Act
        var net = SaleInvoiceService.GetNetAmount(invoice);

        // Assert: Total (500) - Discount (50) = 450
        net.Should().Be(450m);
    }

    [Fact]
    public void GetNetAmount_WhenNoItems_ReturnsZeroOrNegativeDiscount()
    {
        // Arrange
        var invoice = new SaleInvoice
        {
            DiscountAmount = 0m,
            SaleItems = new List<SaleItem>()
        };

        // Act
        var net = SaleInvoiceService.GetNetAmount(invoice);

        // Assert
        net.Should().Be(0m);
    }
}
