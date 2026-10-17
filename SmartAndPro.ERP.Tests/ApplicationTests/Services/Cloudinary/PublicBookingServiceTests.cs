using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using SmartAndPro.ERP.Tests.ApplicationTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Cloudinary;

public class PublicBookingServiceTests
{
    #region PublicHotelService Tests

    public class PublicHotelServiceTests
    {
        private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
        private readonly Mock<AppSettings> _appSettingsMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepository<PublicHotel>> _hotelRepoMock;

        public PublicHotelServiceTests()
        {
            _repositoriesFactoryMock = new Mock<RepositoriesFactory>();
            _appSettingsMock = new Mock<AppSettings>();
            _mapperMock = new Mock<IMapper>();
            _hotelRepoMock = new Mock<IRepository<PublicHotel>>();

            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<PublicHotel>(It.IsAny<bool?>()))
                .Returns(_hotelRepoMock.Object);
        }

        private static PublicHotel CreateHotel()
        {
            return new PublicHotel
            {
                Id = Guid.NewGuid(),
                Name = "Test Hotel",
                MainImageUrl = "https://res.cloudinary.com/test/image/upload/v1/test/main.jpg",
                MainImagePublicId = "test/main",
                RoomTypes = new List<PublicRoomType>(),
                Bookings = new List<Domains.Entities.PublicBooking.PublicBooking>(),
                HotelPaidServices = new List<PublicHotelPaidService>()
            };
        }

        private PublicHotelService CreateService()
        {
            return new PublicHotelService(
                _repositoriesFactoryMock.Object,
                _appSettingsMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsHotelWithUrlFields()
        {
            var hotel = CreateHotel();
            SetupGetAllReturns(new List<PublicHotel> { hotel });
            _mapperMock.Setup(m => m.Map<PublicHotelResponseDto>(It.IsAny<PublicHotel>()))
                .Returns((PublicHotel h) => new PublicHotelResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    MainImageUrl = h.MainImageUrl,
                    MainImagePublicId = h.MainImagePublicId
                });

            var service = CreateService();
            var result = await service.GetAsync(hotel.Id);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data!.MainImageUrl.Should().Be(hotel.MainImageUrl);
            result.Data.MainImagePublicId.Should().Be(hotel.MainImagePublicId);
        }

        [Fact]
        public async Task GetAsync_NonExistentHotel_ReturnsNotFound()
        {
            SetupGetAllReturns(new List<PublicHotel>());
            var service = CreateService();
            var result = await service.GetAsync(Guid.NewGuid());
            result.ErrorCode.Should().Be(ErrorCodes.NotFound);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsHotelsWithUrlFields()
        {
            var hotels = new List<PublicHotel> { CreateHotel(), CreateHotel() };
            SetupGetAllReturns(hotels);
            _mapperMock.Setup(m => m.Map<List<PublicHotelResponseDto>>(It.IsAny<List<PublicHotel>>()))
                .Returns((List<PublicHotel> h) => h.Select(x => new PublicHotelResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    MainImageUrl = x.MainImageUrl,
                    MainImagePublicId = x.MainImagePublicId
                }).ToList());

            var service = CreateService();
            var result = await service.GetAllAsync(new PublicHotelFiltersDto());

            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().HaveCount(2);
            result.Data!.All(h => h.MainImageUrl is not null).Should().BeTrue();
        }

        [Fact]
        public async Task GetFeaturedHotelsAsync_FiltersByFeatured()
        {
            var hotels = new List<PublicHotel> { CreateHotel(), CreateHotel() };
            SetupGetAllReturns(hotels);
            _mapperMock.Setup(m => m.Map<List<PublicHotelResponseDto>>(It.IsAny<List<PublicHotel>>()))
                .Returns((List<PublicHotel> h) => h.Select(x => new PublicHotelResponseDto
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList());

            var service = CreateService();
            var result = await service.GetFeaturedHotelsAsync();

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        [Fact]
        public async Task SearchHotelsAsync_FiltersBySearchTerm()
        {
            var hotels = new List<PublicHotel> { CreateHotel() };
            SetupGetAllReturns(hotels);
            _mapperMock.Setup(m => m.Map<List<PublicHotelResponseDto>>(It.IsAny<List<PublicHotel>>()))
                .Returns((List<PublicHotel> h) => h.Select(x => new PublicHotelResponseDto { Id = x.Id }).ToList());

            var service = CreateService();
            var result = await service.SearchHotelsAsync("Test", null, null, null);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        [Fact]
        public async Task GetHotelWithRoomsAsync_ReturnsHotelWithRooms()
        {
            var hotel = CreateHotel();
            SetupGetAllReturns(new List<PublicHotel> { hotel });
            _mapperMock.Setup(m => m.Map<PublicHotelResponseDto>(It.IsAny<PublicHotel>()))
                .Returns((PublicHotel h) => new PublicHotelResponseDto { Id = h.Id, Name = h.Name });

            var service = CreateService();
            var result = await service.GetHotelWithRoomsAsync(hotel.Id);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        [Fact]
        public async Task LinkToErpHotelAsync_LinksHotel()
        {
            var hotel = CreateHotel();
            var erpHotelId = Guid.NewGuid();

            SetupFilterReturns(hotel);
            var erpHotelRepoMock = new Mock<IRepository<Domains.Entities.Reservation.Hotel>>();
            erpHotelRepoMock.Setup(r => r.Filter(It.IsAny<Expression<Func<Domains.Entities.Reservation.Hotel, bool>>>()))
                .ReturnsAsync(new OperationResponse<IQueryable<Domains.Entities.Reservation.Hotel>>(
                    ErrorCodes.Success,
                    new List<Domains.Entities.Reservation.Hotel> { new() { Id = erpHotelId } }.AsAsyncQueryable()));
            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<Domains.Entities.Reservation.Hotel>(It.IsAny<bool?>()))
                .Returns(erpHotelRepoMock.Object);

            _hotelRepoMock.Setup(r => r.Update(It.IsAny<PublicHotel>()))
                .ReturnsAsync(new OperationResponse<PublicHotel>(ErrorCodes.Success, hotel));
            _mapperMock.Setup(m => m.Map<PublicHotelResponseDto>(It.IsAny<PublicHotel>()))
                .Returns(new PublicHotelResponseDto { Id = hotel.Id });

            var service = CreateService();
            var result = await service.LinkToErpHotelAsync(hotel.Id, erpHotelId);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        [Fact]
        public async Task UnlinkFromErpHotelAsync_UnlinksHotel()
        {
            var hotel = CreateHotel();
            hotel.ErpHotelId = Guid.NewGuid();

            SetupFilterReturns(hotel);
            _hotelRepoMock.Setup(r => r.Update(It.IsAny<PublicHotel>()))
                .ReturnsAsync(new OperationResponse<PublicHotel>(ErrorCodes.Success, hotel));
            _mapperMock.Setup(m => m.Map<PublicHotelResponseDto>(It.IsAny<PublicHotel>()))
                .Returns(new PublicHotelResponseDto { Id = hotel.Id });

            var service = CreateService();
            var result = await service.UnlinkFromErpHotelAsync(hotel.Id);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        private void SetupGetAllReturns(List<PublicHotel> hotels)
        {
            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<PublicHotel>(It.IsAny<bool?>()))
                .Returns(_hotelRepoMock.Object);
            _hotelRepoMock
                .Setup(r => r.GetAll())
                .ReturnsAsync(new OperationResponse<IQueryable<PublicHotel>>(ErrorCodes.Success, hotels.AsAsyncQueryable()));
        }

        private void SetupFilterReturns(PublicHotel hotel)
        {
            _hotelRepoMock
                .Setup(r => r.Filter(It.IsAny<Expression<Func<PublicHotel, bool>>>()))
                .ReturnsAsync(new OperationResponse<IQueryable<PublicHotel>>(ErrorCodes.Success,
                    new List<PublicHotel> { hotel }.AsAsyncQueryable()));
        }
    }

    #endregion

    #region PublicRoomTypeService Tests

    public class PublicRoomTypeServiceTests
    {
        private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
        private readonly Mock<AppSettings> _appSettingsMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepository<PublicRoomType>> _roomTypeRepoMock;

        public PublicRoomTypeServiceTests()
        {
            _repositoriesFactoryMock = new Mock<RepositoriesFactory>();
            _appSettingsMock = new Mock<AppSettings>();
            _mapperMock = new Mock<IMapper>();
            _roomTypeRepoMock = new Mock<IRepository<PublicRoomType>>();

            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<PublicRoomType>(It.IsAny<bool?>()))
                .Returns(_roomTypeRepoMock.Object);
        }

        private static PublicRoomType CreateRoomType()
        {
            return new PublicRoomType
            {
                Id = Guid.NewGuid(),
                Name = "Test Room",
                MainImageUrl = "https://res.cloudinary.com/test/image/upload/v1/test/room.jpg",
                MainImagePublicId = "test/room",
                BasePrice = 100m,
                AvailableQuantity = 10,
                Hotel = new PublicHotel { Id = Guid.NewGuid(), Name = "Hotel" },
                SeasonalPricing = new List<PublicRoomPricing>(),
                BookingRooms = new List<PublicBookingRoom>()
            };
        }

        private PublicRoomTypeService CreateService()
        {
            return new PublicRoomTypeService(
                _repositoriesFactoryMock.Object,
                _appSettingsMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsRoomTypeWithUrlFields()
        {
            var roomType = CreateRoomType();
            SetupGetAllReturns(new List<PublicRoomType> { roomType });
            _mapperMock.Setup(m => m.Map<PublicRoomTypeResponseDto>(It.IsAny<PublicRoomType>()))
                .Returns((PublicRoomType r) => new PublicRoomTypeResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    MainImageUrl = r.MainImageUrl,
                    MainImagePublicId = r.MainImagePublicId
                });

            var service = CreateService();
            var result = await service.GetAsync(roomType.Id);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data!.MainImageUrl.Should().Be(roomType.MainImageUrl);
            result.Data.MainImagePublicId.Should().Be(roomType.MainImagePublicId);
        }

        [Fact]
        public async Task GetAsync_NonExistentRoomType_ReturnsNotFound()
        {
            SetupGetAllReturns(new List<PublicRoomType>());
            var service = CreateService();
            var result = await service.GetAsync(Guid.NewGuid());
            result.ErrorCode.Should().Be(ErrorCodes.NotFound);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsRoomTypesWithUrlFields()
        {
            var roomTypes = new List<PublicRoomType> { CreateRoomType(), CreateRoomType() };
            SetupGetAllReturns(roomTypes);
            _mapperMock.Setup(m => m.Map<List<PublicRoomTypeResponseDto>>(It.IsAny<List<PublicRoomType>>()))
                .Returns((List<PublicRoomType> r) => r.Select(x => new PublicRoomTypeResponseDto
                {
                    Id = x.Id,
                    MainImageUrl = x.MainImageUrl
                }).ToList());

            var service = CreateService();
            var result = await service.GetAllAsync(new PublicRoomTypeFiltersDto());

            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByHotelAsync_FiltersByHotelId()
        {
            var roomTypes = new List<PublicRoomType> { CreateRoomType() };
            SetupGetAllReturns(roomTypes);
            _mapperMock.Setup(m => m.Map<List<PublicRoomTypeResponseDto>>(It.IsAny<List<PublicRoomType>>()))
                .Returns((List<PublicRoomType> r) => r.Select(x => new PublicRoomTypeResponseDto { Id = x.Id }).ToList());

            var service = CreateService();
            var result = await service.GetByHotelAsync(Guid.NewGuid());

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        [Fact]
        public async Task GetRoomPriceAsync_ReturnsBasePrice_WhenNoSeasonalPricing()
        {
            var roomType = CreateRoomType();
            SetupGetAllReturns(new List<PublicRoomType> { roomType });
            var pricingRepoMock = new Mock<IRepository<PublicRoomPricing>>();
            pricingRepoMock.Setup(r => r.GetAll())
                .ReturnsAsync(new OperationResponse<IQueryable<PublicRoomPricing>>(ErrorCodes.Success,
                    Enumerable.Empty<PublicRoomPricing>().AsAsyncQueryable()));
            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<PublicRoomPricing>(It.IsAny<bool?>()))
                .Returns(pricingRepoMock.Object);

            var service = CreateService();
            var result = await service.GetRoomPriceAsync(roomType.Id, DateTime.Today);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().Be(100m);
        }

        [Fact]
        public async Task GetAvailableRoomsAsync_ReturnsAvailableCount()
        {
            var roomType = CreateRoomType();
            SetupGetAllReturns(new List<PublicRoomType> { roomType });
            var bookingRoomRepoMock = new Mock<IRepository<PublicBookingRoom>>();
            bookingRoomRepoMock.Setup(r => r.GetAll())
                .ReturnsAsync(new OperationResponse<IQueryable<PublicBookingRoom>>(ErrorCodes.Success,
                    Enumerable.Empty<PublicBookingRoom>().AsAsyncQueryable()));
            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<PublicBookingRoom>(It.IsAny<bool?>()))
                .Returns(bookingRoomRepoMock.Object);

            var service = CreateService();
            var result = await service.GetAvailableRoomsAsync(roomType.Id, DateTime.Today, DateTime.Today.AddDays(1));

            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().Be(10);
        }

        [Fact]
        public async Task LinkToErpRoomTypeAsync_LinksRoomType()
        {
            var roomType = CreateRoomType();
            var erpRoomTypeId = Guid.NewGuid();

            SetupFilterReturns(roomType);
            var erpRepoMock = new Mock<IRepository<Domains.Entities.Reservation.RoomType>>();
            erpRepoMock.Setup(r => r.Filter(It.IsAny<Expression<Func<Domains.Entities.Reservation.RoomType, bool>>>()))
                .ReturnsAsync(new OperationResponse<IQueryable<Domains.Entities.Reservation.RoomType>>(
                    ErrorCodes.Success,
                    new List<Domains.Entities.Reservation.RoomType> { new() { Id = erpRoomTypeId } }.AsAsyncQueryable()));
            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<Domains.Entities.Reservation.RoomType>(It.IsAny<bool?>()))
                .Returns(erpRepoMock.Object);

            _roomTypeRepoMock.Setup(r => r.Update(It.IsAny<PublicRoomType>()))
                .ReturnsAsync(new OperationResponse<PublicRoomType>(ErrorCodes.Success, roomType));
            _mapperMock.Setup(m => m.Map<PublicRoomTypeResponseDto>(It.IsAny<PublicRoomType>()))
                .Returns(new PublicRoomTypeResponseDto { Id = roomType.Id });

            var service = CreateService();
            var result = await service.LinkToErpRoomTypeAsync(roomType.Id, erpRoomTypeId);

            result.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        private void SetupGetAllReturns(List<PublicRoomType> roomTypes)
        {
            _repositoriesFactoryMock
                .Setup(f => f.GetRepository<PublicRoomType>(It.IsAny<bool?>()))
                .Returns(_roomTypeRepoMock.Object);
            _roomTypeRepoMock
                .Setup(r => r.GetAll())
                .ReturnsAsync(new OperationResponse<IQueryable<PublicRoomType>>(ErrorCodes.Success, roomTypes.AsAsyncQueryable()));
        }

        private void SetupFilterReturns(PublicRoomType roomType)
        {
            _roomTypeRepoMock
                .Setup(r => r.Filter(It.IsAny<Expression<Func<PublicRoomType, bool>>>()))
                .ReturnsAsync(new OperationResponse<IQueryable<PublicRoomType>>(ErrorCodes.Success,
                    new List<PublicRoomType> { roomType }.AsAsyncQueryable()));
        }
    }

    #endregion
}
