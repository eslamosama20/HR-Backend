using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.WebAPI.Controllers;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers.PublicBooking;

public class CloudinaryMigrationControllerPublicTests
{
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IRepository<PublicHotel>> _hotelRepoMock;
    private readonly Mock<IRepository<PublicRoomType>> _roomTypeRepoMock;
    private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
    private readonly CloudinaryMigrationController _controller;

    public CloudinaryMigrationControllerPublicTests()
    {
        _cloudinaryMock = new Mock<ICloudinaryService>();
        _hotelRepoMock = new Mock<IRepository<PublicHotel>>();
        _roomTypeRepoMock = new Mock<IRepository<PublicRoomType>>();
        _repositoriesFactoryMock = new Mock<RepositoriesFactory>();

        _repositoriesFactoryMock
            .Setup(f => f.GetRepository<PublicHotel>(It.IsAny<bool?>()))
            .Returns(_hotelRepoMock.Object);
        _repositoriesFactoryMock
            .Setup(f => f.GetRepository<PublicRoomType>(It.IsAny<bool?>()))
            .Returns(_roomTypeRepoMock.Object);

        _controller = new CloudinaryMigrationController(
            _repositoriesFactoryMock.Object,
            _cloudinaryMock.Object);
    }

    private static PublicHotel CreateHotelWithBase64(string mainBase64 = "SGVsbG8=", string? additionalBase64 = null)
    {
        return new PublicHotel
        {
            Id = Guid.NewGuid(),
            Name = "Test Hotel",
            MainImage = mainBase64,
            AdditionalImages = additionalBase64
        };
    }

    private static PublicRoomType CreateRoomTypeWithBase64(string mainBase64 = "SGVsbG8=", List<string>? additionalBase64 = null)
    {
        return new PublicRoomType
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            MainImage = mainBase64,
            AdditionalImages = additionalBase64 is not null ? JsonSerializer.Serialize(additionalBase64) : null
        };
    }

    private void SetupHotelGetAll(List<PublicHotel> hotels)
    {
        _hotelRepoMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<PublicHotel>>(ErrorCodes.Success, hotels.AsQueryable()));
    }

    private void SetupRoomTypeGetAll(List<PublicRoomType> roomTypes)
    {
        _roomTypeRepoMock
            .Setup(r => r.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<PublicRoomType>>(ErrorCodes.Success, roomTypes.AsQueryable()));
    }

    private static byte[] CreateValidImageBytes()
    {
        // Minimal valid PNG (1x1 pixel)
        return Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
    }

    #region MigratePublicHotels Tests

    [Fact]
    public async Task MigratePublicHotels_MigratesMainImage_ReturnsCorrectCounts()
    {
        var hotel = CreateHotelWithBase64();
        SetupHotelGetAll(new List<PublicHotel> { hotel });
        SetupCloudinaryUploadReturns();

        var result = await _controller.MigratePublicHotels(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":1");
        json.Should().Contain("\"migratedAdditionalImages\":0");
        _hotelRepoMock.Verify(r => r.Update(It.IsAny<PublicHotel>()), Times.Once);
    }

    [Fact]
    public async Task MigratePublicHotels_DryRun_DoesNotUploadOrUpdate()
    {
        var hotel = CreateHotelWithBase64();
        SetupHotelGetAll(new List<PublicHotel> { hotel });

        var result = await _controller.MigratePublicHotels(true, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":1");
        _cloudinaryMock.Verify(c => c.UploadImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default, null), Times.Never);
        _hotelRepoMock.Verify(r => r.Update(It.IsAny<PublicHotel>()), Times.Never);
    }

    [Fact]
    public async Task MigratePublicHotels_SkipsAlreadyMigrated()
    {
        var hotel = CreateHotelWithBase64();
        hotel.MainImage = null;
        hotel.AdditionalImages = null;
        SetupHotelGetAll(new List<PublicHotel> { hotel });

        var result = await _controller.MigratePublicHotels(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"skippedNoData\":1");
        json.Should().Contain("\"migratedMainImages\":0");
    }

    [Fact]
    public async Task MigratePublicHotels_MalformedBase64_SkippedAsError()
    {
        var hotel = CreateHotelWithBase64(mainBase64: "!!!invalid base64!!!");
        SetupHotelGetAll(new List<PublicHotel> { hotel });

        var result = await _controller.MigratePublicHotels(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":0");
        json.Should().Contain("\"skippedDueToError\":1");
    }

    [Fact]
    public async Task MigratePublicHotels_MigratesAdditionalImages_CommaSeparated()
    {
        var base64 = Convert.ToBase64String(CreateValidImageBytes());
        var hotel = CreateHotelWithBase64(additionalBase64: $"{base64},{base64}");
        SetupHotelGetAll(new List<PublicHotel> { hotel });
        SetupCloudinaryUploadReturns();

        var result = await _controller.MigratePublicHotels(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedAdditionalImages\":2");
    }

    [Fact]
    public async Task MigratePublicHotels_EmptyHotelList_ReturnsZeroCounts()
    {
        SetupHotelGetAll(new List<PublicHotel>());

        var result = await _controller.MigratePublicHotels(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"total\":0");
    }

    [Fact]
    public async Task MigratePublicHotels_NullBase64_SkippedNoData()
    {
        var hotel = CreateHotelWithBase64(mainBase64: null!);
        SetupHotelGetAll(new List<PublicHotel> { hotel });

        var result = await _controller.MigratePublicHotels(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"skippedNoData\":1");
    }

    #endregion

    #region MigratePublicRoomTypes Tests

    [Fact]
    public async Task MigratePublicRoomTypes_MigratesMainImage_ReturnsCorrectCounts()
    {
        var roomType = CreateRoomTypeWithBase64();
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });
        SetupCloudinaryUploadReturns();

        var result = await _controller.MigratePublicRoomTypes(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":1");
        _roomTypeRepoMock.Verify(r => r.Update(It.IsAny<PublicRoomType>()), Times.Once);
    }

    [Fact]
    public async Task MigratePublicRoomTypes_DryRun_DoesNotUploadOrUpdate()
    {
        var roomType = CreateRoomTypeWithBase64();
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });

        var result = await _controller.MigratePublicRoomTypes(true, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":1");
        _cloudinaryMock.Verify(c => c.UploadImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default, null), Times.Never);
        _roomTypeRepoMock.Verify(r => r.Update(It.IsAny<PublicRoomType>()), Times.Never);
    }

    [Fact]
    public async Task MigratePublicRoomTypes_SkipsAlreadyMigrated()
    {
        var roomType = CreateRoomTypeWithBase64();
        roomType.MainImage = null;
        roomType.AdditionalImages = null;
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });

        var result = await _controller.MigratePublicRoomTypes(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"skippedNoData\":1");
    }

    [Fact]
    public async Task MigratePublicRoomTypes_MalformedBase64_SkippedAsError()
    {
        var roomType = CreateRoomTypeWithBase64(mainBase64: "!!!invalid!!!");
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });

        var result = await _controller.MigratePublicRoomTypes(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":0");
        json.Should().Contain("\"skippedDueToError\":1");
    }

    [Fact]
    public async Task MigratePublicRoomTypes_MigratesAdditionalImages_JsonArray()
    {
        var base64 = Convert.ToBase64String(CreateValidImageBytes());
        var roomType = CreateRoomTypeWithBase64(additionalBase64: new List<string> { base64, base64 });
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });
        SetupCloudinaryUploadReturns();

        var result = await _controller.MigratePublicRoomTypes(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedAdditionalImages\":2");
    }

    #endregion

    #region MigratePublicBooking Tests

    [Fact]
    public async Task MigratePublicBooking_RunsBothMigrations_ReturnsCombinedSummary()
    {
        var hotel = CreateHotelWithBase64();
        var roomType = CreateRoomTypeWithBase64();
        SetupHotelGetAll(new List<PublicHotel> { hotel });
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });
        SetupCloudinaryUploadReturns();

        var result = await _controller.MigratePublicBooking(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("hotels");
        json.Should().Contain("roomTypes");
    }

    [Fact]
    public async Task MigratePublicBooking_EmptyEntities_ReturnsZeroCounts()
    {
        SetupHotelGetAll(new List<PublicHotel>());
        SetupRoomTypeGetAll(new List<PublicRoomType>());

        var result = await _controller.MigratePublicBooking(false, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("\"migratedMainImages\":0");
    }

    [Fact]
    public async Task MigratePublicBooking_DryRun_DoesNotUpload()
    {
        var hotel = CreateHotelWithBase64();
        var roomType = CreateRoomTypeWithBase64();
        SetupHotelGetAll(new List<PublicHotel> { hotel });
        SetupRoomTypeGetAll(new List<PublicRoomType> { roomType });

        var result = await _controller.MigratePublicBooking(true, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        _cloudinaryMock.Verify(c => c.UploadImageAsync(
            It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default, null), Times.Never);
    }

    #endregion

    private void SetupCloudinaryUploadReturns()
    {
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync((byte[] bytes, string fileName, string contentType, string folder, CancellationToken ct, CloudinaryTransformation? t) =>
                new CloudinaryUploadResult(
                    $"https://res.cloudinary.com/test/{folder}/{fileName}",
                    $"{folder}/{fileName.Replace(".png", "")}",
                    null));
    }
}
