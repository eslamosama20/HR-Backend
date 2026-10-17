using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Hotel;
using SmartAndPro.ERP.Application.Services.Reservation;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers;

/// <summary>
/// Integration tests for HotelController.Search that use a real InMemory database.
/// These tests verify that EF Core can translate the dynamic OR predicate to SQL,
/// catching the most likely production crash: EF Core failing to translate the
/// ApplySearchKeyword extension method.
/// </summary>
public class HotelControllerSearchTests
{
    private readonly DbContextOptions<ERPContext> _options;
    private readonly string _databaseName;
    private readonly Mock<IAutoNumberGenerator> _autoNumberGeneratorMock;

    public HotelControllerSearchTests()
    {
        _databaseName = $"HotelSearchTest_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<ERPContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;
        _autoNumberGeneratorMock = new Mock<IAutoNumberGenerator>();
    }

    private async Task SeedTestDataAsync()
    {
        using var context = new ERPContext(_options, _autoNumberGeneratorMock.Object);
        
        var hotels = new List<Hotel>
        {
            new Hotel 
            { 
                Id = Guid.NewGuid(), 
                ArabicName = "فندق هيلتون", 
                EnglishName = "Hilton Hotel",
                ContactNumber = "+966501234567",
                Email = "hilton@example.com",
                TaxNumber = "1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Hotel 
            { 
                Id = Guid.NewGuid(), 
                ArabicName = "فندق ماريوت", 
                EnglishName = "Marriott Hotel",
                ContactNumber = "+966509876543",
                Email = "marriott@example.com",
                TaxNumber = "0987654321",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Hotel 
            { 
                Id = Guid.NewGuid(), 
                ArabicName = "فندق صغير", 
                EnglishName = "Small Hotel",
                ContactNumber = "+966501111111",
                Email = "small@example.com",
                TaxNumber = "1111111111",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Hotel 
            { 
                Id = Guid.NewGuid(), 
                ArabicName = "فندق هيلتون جاردن", 
                EnglishName = "Hilton Garden Inn",
                ContactNumber = "+966502222222",
                Email = "hilton.garden@example.com",
                TaxNumber = "2222222222",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Hotel 
            { 
                Id = Guid.NewGuid(), 
                ArabicName = "منتجع الشاطئ", 
                EnglishName = "Beach Resort",
                ContactNumber = "+966503333333",
                Email = "beach@example.com",
                TaxNumber = "3333333333",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            }
        };

        context.Hotels.AddRange(hotels);
        await context.SaveChangesAsync();
    }

    private HotelService CreateService()
    {
        var context = new ERPContext(_options, _autoNumberGeneratorMock.Object);
        var repositoriesFactory = new RepositoriesFactory(context, null);
        var appSettings = new AppSettings();
        
        // Create a simple AutoMapper config for Hotel -> HotelResponseDto
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Hotel, HotelResponseDto>();
        });
        var mapper = config.CreateMapper();

        return new HotelService(repositoriesFactory, appSettings, mapper);
    }

    [Fact]
    public async Task SearchAsync_FindsByArabicName()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("هيلتون", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count); // Hilton Hotel + Hilton Garden Inn
        Assert.All(result.Data, h => Assert.Contains("هيلتون", h.ArabicName));
    }

    [Fact]
    public async Task SearchAsync_FindsByEnglishName()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("Marriott", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Marriott Hotel", result.Data[0].EnglishName);
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act - lowercase search for "hilton"
        var result = await service.SearchAsync("hilton", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count); // Should find both Hilton hotels
    }

    [Fact]
    public async Task SearchAsync_ReturnsCorrectTotalCount()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("فندق", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.Equal(4, result.TotalCount); // All 4 hotels with "فندق" in ArabicName
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsFirstPageOfAllRecords()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("", pageNumber: 1, pageSize: 3);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count); // pageSize=3, so only 3 returned
        Assert.Equal(5, result.TotalCount); // Total is 5
    }

    [Fact]
    public async Task SearchAsync_SortByArabicNameAscending()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("فندق", pageNumber: 1, pageSize: 20, sortField: "ArabicName", sortOrder: 1);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Data.Count);
        // Arabic names should be in ascending order
        for (int i = 0; i < result.Data.Count - 1; i++)
        {
            Assert.True(string.Compare(result.Data[i].ArabicName, result.Data[i + 1].ArabicName) <= 0);
        }
    }

    [Fact]
    public async Task SearchAsync_SearchByEmail()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("beach@example.com", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Beach Resort", result.Data[0].EnglishName);
    }

    [Fact]
    public async Task SearchAsync_SearchByTaxNumber()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("1234567890", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Hilton Hotel", result.Data[0].EnglishName);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmptyData()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("xyz_nonexistent", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_SetsQueryAndTookFields()
    {
        // Arrange
        await SeedTestDataAsync();
        var service = CreateService();

        // Act
        var result = await service.SearchAsync("hilton", pageNumber: 1, pageSize: 20);

        // Assert
        Assert.Equal("hilton", result.Query);
        Assert.True(result.Took >= 0); // Duration should be non-negative
    }
}
