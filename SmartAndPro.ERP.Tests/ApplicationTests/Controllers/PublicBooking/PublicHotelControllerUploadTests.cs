using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.WebAPI.Controllers.PublicBooking;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers.PublicBooking;

public class PublicHotelControllerUploadTests
{
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IRepository<PublicHotel>> _hotelRepoMock;
    private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
    private readonly PublicHotelController _controller;
    private readonly Guid _hotelId = Guid.NewGuid();

    public PublicHotelControllerUploadTests()
    {
        _cloudinaryMock = new Mock<ICloudinaryService>();
        _hotelRepoMock = new Mock<IRepository<PublicHotel>>();
        _repositoriesFactoryMock = new Mock<RepositoriesFactory>();

        _repositoriesFactoryMock
            .Setup(f => f.GetRepository<PublicHotel>(It.IsAny<bool?>()))
            .Returns(_hotelRepoMock.Object);
        _hotelRepoMock
            .Setup(r => r.Update(It.IsAny<PublicHotel>()))
            .ReturnsAsync(new OperationResponse<PublicHotel>(ErrorCodes.Success, null!));

        var realFactory = new ServiceBaseFactory(
            _repositoriesFactoryMock.Object,
            Mock.Of<SmartAndPro.ERP.SharedKernel.Entities.Configurations.AppSettings>(),
            Mock.Of<AutoMapper.IMapper>(),
            Mock.Of<IHttpContextAccessor>());

        _controller = new PublicHotelController(
            realFactory,
            _repositoriesFactoryMock.Object,
            _cloudinaryMock.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PublicHotelController>>());
    }

    private static PublicHotel CreateTestHotel()
    {
        return new PublicHotel
        {
            Id = Guid.NewGuid(),
            Name = "Test Hotel",
            MainImageUrl = "https://res.cloudinary.com/test/image/upload/v1/publicHotel/main/test.jpg",
            MainImagePublicId = "publicHotel/main/test",
            AdditionalImageUrls = JsonSerializer.Serialize(new[]
            {
                "https://res.cloudinary.com/test/image/upload/v1/publicHotel/additional/img1.jpg",
                "https://res.cloudinary.com/test/image/upload/v1/publicHotel/additional/img2.jpg"
            })
        };
    }

    private static IFormFile CreateMockFile(string fileName = "test.jpg", string contentType = "image/jpeg", long size = 1024)
    {
        var stream = new MemoryStream(new byte[size]);
        return new FormFile(stream, 0, size, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task UploadMainImage_ValidFile_ReturnsOkWithUrl()
    {
        var hotel = CreateTestHotel();
        var uploadResult = new CloudinaryUploadResult("https://cloudinary.com/new.jpg", "newPublicId", "https://cloudinary.com/thumb.jpg");

        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/main", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(uploadResult);

        var file = CreateMockFile();
        var result = await _controller.UploadMainImage(hotel.Id, file, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = JsonSerializer.Serialize(ok.Value);
        value.Should().Contain(uploadResult.Url);
        value.Should().Contain(uploadResult.ThumbnailUrl);
        value.Should().Contain(uploadResult.PublicId);
        _hotelRepoMock.Verify(r => r.Update(It.IsAny<PublicHotel>()), Times.Once);
    }

    [Fact]
    public async Task UploadMainImage_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.UploadMainImage(_hotelId, null!, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadMainImage_EmptyFile_ReturnsBadRequest()
    {
        var file = CreateMockFile(size: 0);
        var result = await _controller.UploadMainImage(_hotelId, file, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadMainImage_NonExistentHotel_ReturnsNotFound()
    {
        SetupHotelFilterReturns(null);
        var file = CreateMockFile();
        var result = await _controller.UploadMainImage(_hotelId, file, default);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UploadMainImage_ReplacesPreviousMainImage()
    {
        var hotel = CreateTestHotel();
        hotel.MainImagePublicId = "old-public-id";
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/main", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/new.jpg", "newPublicId", "https://cloudinary.com/thumb.jpg"));

        var file = CreateMockFile();
        var result = await _controller.UploadMainImage(hotel.Id, file, default);

        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadMainImage_CloudinaryDeleteFailure_StillContinues()
    {
        var hotel = CreateTestHotel();
        hotel.MainImagePublicId = "old-public-id";
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cloudinary error"));
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/main", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/new.jpg", "newPublicId", null));

        var file = CreateMockFile();
        var result = await _controller.UploadMainImage(hotel.Id, file, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadMainImage_CloudinaryUploadThrows_ReturnsBadRequest()
    {
        var hotel = CreateTestHotel();
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/main", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        var file = CreateMockFile();
        var result = await _controller.UploadMainImage(hotel.Id, file, default);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var json = System.Text.Json.JsonSerializer.Serialize(badRequest.Value);
        json.Should().Contain("Upload failed");
    }

    [Fact]
    public async Task UploadAdditionalImages_ValidFiles_ReturnsOk()
    {
        var hotel = CreateTestHotel();
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/additional", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/img.jpg", "imgPublicId", null));

        var files = new List<IFormFile> { CreateMockFile("img1.jpg"), CreateMockFile("img2.jpg") };
        var result = await _controller.UploadAdditionalImages(hotel.Id, files, default);

        result.Should().BeOfType<OkObjectResult>();
        _hotelRepoMock.Verify(r => r.Update(It.IsAny<PublicHotel>()), Times.Once);
    }

    [Fact]
    public async Task UploadAdditionalImages_NoFiles_ReturnsBadRequest()
    {
        var result = await _controller.UploadAdditionalImages(_hotelId, new List<IFormFile>(), default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAdditionalImages_NullList_ReturnsBadRequest()
    {
        var result = await _controller.UploadAdditionalImages(_hotelId, null!, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAdditionalImages_NonExistentHotel_ReturnsNotFound()
    {
        SetupHotelFilterReturns(null);
        var files = new List<IFormFile> { CreateMockFile() };
        var result = await _controller.UploadAdditionalImages(_hotelId, files, default);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UploadAdditionalImages_UploadThrows_ReturnsBadRequest()
    {
        var hotel = CreateTestHotel();
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/additional", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        var files = new List<IFormFile> { CreateMockFile() };
        var result = await _controller.UploadAdditionalImages(hotel.Id, files, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadMainImageTemp_ValidFile_ReturnsOk()
    {
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/main/temp", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/temp.jpg", "tempPublicId", "https://cloudinary.com/thumb.jpg"));

        var file = CreateMockFile();
        var result = await _controller.UploadMainImageTemp(file, default);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        json.Should().Contain("temp.jpg");
        json.Should().Contain("tempPublicId");
    }

    [Fact]
    public async Task UploadMainImageTemp_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.UploadMainImageTemp(null!, default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadMainImageTemp_UploadThrows_ReturnsBadRequest()
    {
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/main/temp", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        var file = CreateMockFile();
        var result = await _controller.UploadMainImageTemp(file, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAdditionalImagesTemp_ValidFiles_ReturnsOk()
    {
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicHotel/additional/temp", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/img.jpg", "imgPublicId", null));

        var files = new List<IFormFile> { CreateMockFile(), CreateMockFile() };
        var result = await _controller.UploadAdditionalImagesTemp(files, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadAdditionalImagesTemp_NullList_ReturnsBadRequest()
    {
        var result = await _controller.UploadAdditionalImagesTemp(null!, default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAdditionalImagesTemp_EmptyList_ReturnsBadRequest()
    {
        var result = await _controller.UploadAdditionalImagesTemp(new List<IFormFile>(), default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private void SetupHotelFilterReturns(PublicHotel? hotel)
    {
        var list = hotel is not null ? new List<PublicHotel> { hotel } : new List<PublicHotel>();
        _hotelRepoMock
            .Setup(r => r.Filter(It.IsAny<System.Linq.Expressions.Expression<Func<PublicHotel, bool>>>()))
            .ReturnsAsync(new OperationResponse<IQueryable<PublicHotel>>(ErrorCodes.Success, list.AsQueryable()));
    }
}
