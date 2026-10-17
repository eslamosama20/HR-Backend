using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

public class PublicRoomTypeControllerUploadTests
{
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IRepository<PublicRoomType>> _roomTypeRepoMock;
    private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
    private readonly PublicRoomTypeController _controller;
    private readonly Guid _roomTypeId = Guid.NewGuid();

    public PublicRoomTypeControllerUploadTests()
    {
        _cloudinaryMock = new Mock<ICloudinaryService>();
        _roomTypeRepoMock = new Mock<IRepository<PublicRoomType>>();
        _repositoriesFactoryMock = new Mock<RepositoriesFactory>();

        _repositoriesFactoryMock
            .Setup(f => f.GetRepository<PublicRoomType>(It.IsAny<bool?>()))
            .Returns(_roomTypeRepoMock.Object);
        _roomTypeRepoMock
            .Setup(r => r.Update(It.IsAny<PublicRoomType>()))
            .ReturnsAsync(new OperationResponse<PublicRoomType>(ErrorCodes.Success, null!));
        _cloudinaryMock
            .Setup(c => c.ExtractPublicIdFromUrl(It.IsAny<string>()))
            .Returns<string>(url =>
            {
                if (string.IsNullOrWhiteSpace(url)) return null;
                var uri = new Uri(url);
                var segments = uri.Segments;
                var uploadIdx = Array.FindIndex(segments, s => s.TrimEnd('/') == "upload");
                if (uploadIdx < 0 || uploadIdx + 2 >= segments.Length) return null;
                var path = string.Concat(segments.Skip(uploadIdx + 2));
                var dot = path.LastIndexOf('.');
                return dot > 0 ? path[..dot] : path;
            });

        var realFactory = new ServiceBaseFactory(
            _repositoriesFactoryMock.Object,
            Mock.Of<SmartAndPro.ERP.SharedKernel.Entities.Configurations.AppSettings>(),
            Mock.Of<AutoMapper.IMapper>(),
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>());

        _controller = new PublicRoomTypeController(
            realFactory,
            _repositoriesFactoryMock.Object,
            _cloudinaryMock.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PublicRoomTypeController>>());
    }

    private PublicRoomType CreateTestRoomType()
    {
        return new PublicRoomType
        {
            Id = _roomTypeId,
            Name = "Test Room",
            MainImageUrl = "https://res.cloudinary.com/test/image/upload/v1/publicRoomType/images/main.jpg",
            MainImagePublicId = "publicRoomType/images/main",
            AdditionalImageUrls = JsonSerializer.Serialize(new[]
            {
                "https://res.cloudinary.com/test/image/upload/v1234567/publicRoomType/images/img1.jpg",
                "https://res.cloudinary.com/test/image/upload/v1234567/publicRoomType/images/img2.jpg"
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

    private void SetupRoomTypeFilterReturns(PublicRoomType? roomType)
    {
        var list = roomType is not null ? new List<PublicRoomType> { roomType } : new List<PublicRoomType>();
        _roomTypeRepoMock
            .Setup(r => r.Filter(It.IsAny<System.Linq.Expressions.Expression<Func<PublicRoomType, bool>>>()))
            .ReturnsAsync(new OperationResponse<IQueryable<PublicRoomType>>(ErrorCodes.Success, list.AsQueryable()));
    }

    [Fact]
    public async Task UploadImages_ValidFiles_SetsFirstAsMainIfNoneExists()
    {
        var roomType = CreateTestRoomType();
        roomType.MainImageUrl = null;
        roomType.MainImagePublicId = null;
        roomType.AdditionalImageUrls = null;
        SetupRoomTypeFilterReturns(roomType);

        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicRoomType/images", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/new.jpg", "newPublicId", null));

        var files = new List<IFormFile> { CreateMockFile("img1.jpg"), CreateMockFile("img2.jpg") };
        var result = await _controller.UploadImages(_roomTypeId, files, default);

        result.Should().BeOfType<OkObjectResult>();
        _roomTypeRepoMock.Verify(r => r.Update(It.Is<PublicRoomType>(rt =>
            rt.MainImageUrl == "https://cloudinary.com/new.jpg")), Times.Once);
    }

    [Fact]
    public async Task UploadImages_NoFiles_ReturnsBadRequest()
    {
        var result = await _controller.UploadImages(_roomTypeId, new List<IFormFile>(), default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImages_NullList_ReturnsBadRequest()
    {
        var result = await _controller.UploadImages(_roomTypeId, null!, default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImages_NonExistentRoomType_ReturnsNotFound()
    {
        SetupRoomTypeFilterReturns(null);
        var files = new List<IFormFile> { CreateMockFile() };
        var result = await _controller.UploadImages(_roomTypeId, files, default);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UploadImages_UploadThrows_ReturnsBadRequest()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicRoomType/images", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        var files = new List<IFormFile> { CreateMockFile() };
        var result = await _controller.UploadImages(_roomTypeId, files, default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImagesTemp_ValidFiles_ReturnsOk()
    {
        _cloudinaryMock
            .Setup(c => c.UploadImageAsync(It.IsAny<IFormFile>(), "publicRoomType/images/temp", It.IsAny<CancellationToken>(), It.IsAny<CloudinaryTransformation>()))
            .ReturnsAsync(new CloudinaryUploadResult("https://cloudinary.com/img.jpg", "imgPublicId", null));

        var files = new List<IFormFile> { CreateMockFile(), CreateMockFile() };
        var result = await _controller.UploadImagesTemp(files, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadImagesTemp_NullList_ReturnsBadRequest()
    {
        var result = await _controller.UploadImagesTemp(null!, default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadImagesTemp_EmptyList_ReturnsBadRequest()
    {
        var result = await _controller.UploadImagesTemp(new List<IFormFile>(), default);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_ByIndex_Zerio_DeletesMainImage()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);

        var result = await _controller.DeleteImage(_roomTypeId, 0, default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _roomTypeRepoMock.Verify(r => r.Update(It.Is<PublicRoomType>(rt =>
            rt.MainImageUrl != null && rt.MainImagePublicId != null)), Times.Once);
    }

    [Fact]
    public async Task DeleteImage_ByIndex_One_DeletesFirstAdditionalImage()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);

        var result = await _controller.DeleteImage(_roomTypeId, 1, default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(
            It.Is<string>(id => id.Contains("img1")), default), Times.Once);
    }

    [Fact]
    public async Task DeleteImage_NegativeIndex_ReturnsBadRequest()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);

        var result = await _controller.DeleteImage(_roomTypeId, -1, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_InvalidIndex_ReturnsBadRequest()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);

        var result = await _controller.DeleteImage(_roomTypeId, 999, default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_NonExistentRoomType_ReturnsNotFound()
    {
        SetupRoomTypeFilterReturns(null);
        var result = await _controller.DeleteImage(_roomTypeId, 0, default);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_CloudinaryFailure_StillClearsDatabase()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);
        _cloudinaryMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("Cloudinary error"));

        var result = await _controller.DeleteImage(_roomTypeId, 0, default);

        result.Should().BeOfType<OkObjectResult>();
        _roomTypeRepoMock.Verify(r => r.Update(It.IsAny<PublicRoomType>()), Times.Once);
    }

    [Fact]
    public async Task Delete_EntityDelete_CleansUpCloudinaryImages()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);

        var result = await _controller.Delete(_roomTypeId, default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(
            It.Is<string>(id => id.Contains("publicRoomType/images/img")), default), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Delete_EntityDelete_CloudinaryFailure_StillCompletes()
    {
        var roomType = CreateTestRoomType();
        SetupRoomTypeFilterReturns(roomType);
        _cloudinaryMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("Cloudinary error"));

        var result = await _controller.Delete(_roomTypeId, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_EntityDelete_NoImages_DoesNotCallCloudinary()
    {
        var roomType = CreateTestRoomType();
        roomType.MainImagePublicId = null;
        roomType.MainImageUrl = null;
        roomType.AdditionalImageUrls = null;
        SetupRoomTypeFilterReturns(roomType);

        var result = await _controller.Delete(_roomTypeId, default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), default), Times.Never);
    }
}
