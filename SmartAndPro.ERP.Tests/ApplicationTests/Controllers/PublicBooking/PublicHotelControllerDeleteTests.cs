using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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

public class PublicHotelControllerDeleteTests
{
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IRepository<PublicHotel>> _hotelRepoMock;
    private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
    private readonly PublicHotelController _controller;
    private readonly Guid _hotelId = Guid.NewGuid();

    public PublicHotelControllerDeleteTests()
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

        _controller = new PublicHotelController(
            realFactory,
            _repositoriesFactoryMock.Object,
            _cloudinaryMock.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PublicHotelController>>());
    }

    private PublicHotel CreateHotelWithImages()
    {
        return new PublicHotel
        {
            Id = _hotelId,
            Name = "Test Hotel",
            MainImageUrl = "https://res.cloudinary.com/test/image/upload/v1/publicHotel/main/test.jpg",
            MainImagePublicId = "publicHotel/main/test",
            AdditionalImageUrls = JsonSerializer.Serialize(new[]
            {
                "https://res.cloudinary.com/test/image/upload/v1234567/publicHotel/additional/img1.jpg",
                "https://res.cloudinary.com/test/image/upload/v1234567/publicHotel/additional/img2.jpg"
            })
        };
    }

    private void SetupHotelFilterReturns(PublicHotel? hotel)
    {
        var list = hotel is not null ? new List<PublicHotel> { hotel } : new List<PublicHotel>();
        _hotelRepoMock
            .Setup(r => r.Filter(It.IsAny<System.Linq.Expressions.Expression<Func<PublicHotel, bool>>>()))
            .ReturnsAsync(new OperationResponse<IQueryable<PublicHotel>>(ErrorCodes.Success, list.AsQueryable()));
    }

    [Fact]
    public async Task DeleteImage_MainImage_DeletesCloudinaryAndClearsFields()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);

        var result = await _controller.DeleteImage(_hotelId, "main", default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _hotelRepoMock.Verify(r => r.Update(It.Is<PublicHotel>(h =>
            h.MainImageUrl == null && h.MainImagePublicId == null)), Times.Once);
    }

    [Fact]
    public async Task DeleteImage_ByIndex_DeletesCloudinaryAndUpdatesArray()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);

        var result = await _controller.DeleteImage(_hotelId, "1", default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(
            It.Is<string>(id => id.Contains("publicHotel/additional/img2")), default), Times.Once);
        _hotelRepoMock.Verify(r => r.Update(It.IsAny<PublicHotel>()), Times.Once);
    }

    [Fact]
    public async Task DeleteImage_InvalidIndex_ReturnsBadRequest()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);

        var result = await _controller.DeleteImage(_hotelId, "999", default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_NonNegativeIndex_ReturnsBadRequest()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);

        var result = await _controller.DeleteImage(_hotelId, "-1", default);

        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteImage_NonExistentHotel_ReturnsNotFound()
    {
        SetupHotelFilterReturns(null);

        var result = await _controller.DeleteImage(_hotelId, "main", default);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_CloudinaryFailure_StillClearsDatabase()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("Cloudinary error"));

        var result = await _controller.DeleteImage(_hotelId, "main", default);

        result.Should().BeOfType<OkObjectResult>();
        _hotelRepoMock.Verify(r => r.Update(It.IsAny<PublicHotel>()), Times.Once);
    }

    [Fact]
    public async Task DeleteImage_NoMainImage_DoesNotCallCloudinary()
    {
        var hotel = CreateHotelWithImages();
        hotel.MainImagePublicId = null;
        hotel.MainImageUrl = null;
        SetupHotelFilterReturns(hotel);

        var result = await _controller.DeleteImage(_hotelId, "main", default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Delete_EntityDelete_CleansUpCloudinaryImages()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);

        var result = await _controller.Delete(_hotelId, default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(
            It.Is<string>(id => id.Contains("publicHotel/additional/img")), default), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Delete_EntityDelete_CloudinaryFailure_StillCompletes()
    {
        var hotel = CreateHotelWithImages();
        SetupHotelFilterReturns(hotel);
        _cloudinaryMock
            .Setup(c => c.DeleteImageAsync(It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("Cloudinary error"));

        var result = await _controller.Delete(_hotelId, default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_EntityDelete_NoImages_DoesNotCallCloudinary()
    {
        var hotel = CreateHotelWithImages();
        hotel.MainImagePublicId = null;
        hotel.MainImageUrl = null;
        hotel.AdditionalImageUrls = null;
        SetupHotelFilterReturns(hotel);

        var result = await _controller.Delete(_hotelId, default);

        result.Should().BeOfType<OkObjectResult>();
        _cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), default), Times.Never);
    }
}
