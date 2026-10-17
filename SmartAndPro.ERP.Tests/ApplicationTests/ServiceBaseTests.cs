using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;
using Moq;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;

namespace SmartAndPro.ERP.Tests.ApplicationTests;
 
public class ServiceBaseTests
{
    private readonly Mock<RepositoriesFactory> _repositoriesFactoryMock;
    private readonly Mock<AppSettings> _appSettingsMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepository<TestEntity>> _repositoryMock;

    public ServiceBaseTests()
    {
        _repositoriesFactoryMock = new Mock<RepositoriesFactory>();
        _appSettingsMock = new Mock<AppSettings>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IRepository<TestEntity>>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var testEntities = new List<TestEntity>
        {
            new() { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test 1" },
            new() { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Test 2" }
        };
        
        var testFilters = new TestFilters { Name = null};

        _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>()))
            .Returns(_repositoryMock.Object);
        
        _repositoryMock.Setup(x => x.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.Success, testEntities.AsQueryable()));
        
        _mapperMock.Setup(x => x.Map<List<TestResponseDto>>(It.IsAny<List<TestEntity>>()))
            .Returns(new List<TestResponseDto>
        {
            new TestResponseDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test 1" },
            new TestResponseDto { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Test 2" }
        });

        var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

        // Act
        var result = await service.GetAllAsync(testFilters);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.Equal(2, result.Data!.Count);
        Assert.Equal("Test 1", result.Data[0].Name);
        Assert.Equal("Test 2", result.Data[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNoContent_WhenNoDataExists()
    {
        var testFilters = new TestFilters { Name = "Test"};

        _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
        // Arrange
        _repositoryMock.Setup(x => x.GetAll()).ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.NoContent, Enumerable.Empty<TestEntity>().AsQueryable()));

        var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

        // Act
        var result = await service.GetAllAsync(testFilters);

        // Assert
        Assert.Equal(ErrorCodes.NoContent, result.ErrorCode);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnGeneralFail_WhenExceptionOccurs()
    {
        // Arrange
        var testFilters = new TestFilters { Name = "Test"};
        _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
        _repositoryMock.Setup(x => x.GetAll()).ThrowsAsync(new Exception());

        var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

        // Act
        var result = await service.GetAllAsync(testFilters);

        // Assert
        Assert.Equal(ErrorCodes.GeneralFail, result.ErrorCode);
        Assert.Null(result.Data);
    }
    

    [Fact]
    public async Task GetAllAsync_Returns_Success_When_FiltersAreApplied()
    {
        // Arrange
        var testEntities = new List<TestEntity>
        {
            new TestEntity { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test1" },
            new TestEntity { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Test2" }
        };
        
        var testFilters = new TestFilters { Name = "Test1"};

        _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>()))
            .Returns(_repositoryMock.Object);
        
        _repositoryMock.Setup(x => x.GetAll())
            .ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.Success, testEntities.AsQueryable()));
        
        _mapperMock.Setup(x => x.Map<List<TestResponseDto>>(It.IsAny<List<TestEntity>>()))
            .Returns(new List<TestResponseDto>
            {
                new TestResponseDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test1" }
            });

        var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

        // Act
        var result = await service.GetAllAsync(testFilters);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.Single(result.Data!);
        Assert.Equal("Test1", result.Data[0].Name);
    }

        
    
    
    [Fact]
    public async Task AddAsync_ShouldReturnSuccess_WhenDataAddedSuccessfully()
    {
        // Arrange
        var testEntity = new TestEntity { Name = "Test Entity" };
        var testPostDto = new TestPostDto{ Name = "Test Entity" };

        _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
        _mapperMock.Setup(x => x.Map<TestEntity>(It.IsAny<TestPostDto>())).Returns(testEntity);
        _repositoryMock.Setup(x => x.Add(It.IsAny<TestEntity>())).ReturnsAsync(new OperationResponse<TestEntity>(ErrorCodes.Success, testEntity));
        _mapperMock.Setup(x => x.Map<TestResponseDto>(It.IsAny<TestEntity>())).Returns(new TestResponseDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test Entity" });

        var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

        // Act
        var result = await service.AddAsync(testPostDto);

        // Assert
        Assert.Equal(ErrorCodes.Success, result.ErrorCode);
        Assert.Equal(new Guid("11111111-1111-1111-1111-111111111111"), result.Data.Id);
        Assert.Equal("Test Entity", result.Data.Name);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnGeneralFail_WhenExceptionOccurs()
    {
        // Arrange
        var testPostDto = new TestPostDto { Name = "Test Entity" };

        _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
        _mapperMock.Setup(x => x.Map<TestEntity>(It.IsAny<TestPostDto>())).Returns(new TestEntity { Name = "Test Entity" });
        _repositoryMock.Setup(x => x.Add(It.IsAny<TestEntity>())).ThrowsAsync(new Exception());

        var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

        // Act
        var result = await service.AddAsync(testPostDto);

        // Assert
        Assert.Equal(ErrorCodes.GeneralFail, result.ErrorCode);
        Assert.Null(result.Data);
    }
    
    [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenDataUpdatedSuccessfully()
        {
            // Arrange
            var testEntity = new TestEntity { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test Entity" };
            var testPutDto = new TestPutDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Updated Test Entity" };

            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _mapperMock.Setup(x => x.Map<TestEntity>(It.IsAny<TestPutDto>())).Returns(testEntity);
            _repositoryMock.Setup(x => x.Update(It.IsAny<TestEntity>())).ReturnsAsync(new OperationResponse<TestEntity>(ErrorCodes.Success, testEntity));
            _mapperMock.Setup(x => x.Map<TestResponseDto>(It.IsAny<TestEntity>())).Returns(new TestResponseDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Updated Test Entity" });

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.UpdateAsync(testPutDto);

            // Assert
            Assert.Equal(ErrorCodes.Success, result.ErrorCode);
            Assert.Equal(new Guid("11111111-1111-1111-1111-111111111111"), result.Data.Id);
            Assert.Equal("Updated Test Entity", result.Data.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnGeneralFail_WhenExceptionOccurs()
        {
            // Arrange
            var testPutDto = new TestPutDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Updated Test Entity" };

            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _mapperMock.Setup(x => x.Map<TestEntity>(It.IsAny<TestPutDto>())).Returns(new TestEntity { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Updated Test Entity" });
            _repositoryMock.Setup(x => x.Update(It.IsAny<TestEntity>())).ThrowsAsync(new Exception());

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.UpdateAsync(testPutDto);

            // Assert
            Assert.Equal(ErrorCodes.GeneralFail, result.ErrorCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnSuccess_WhenDataExists()
        {
            // Arrange
            var testEntity = new TestEntity { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test Entity" };

            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _repositoryMock.Setup(x => x.Filter(It.IsAny<Expression<Func<TestEntity, bool>>>())).ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.Success, new List<TestEntity> { testEntity }.AsQueryable()));
            _mapperMock.Setup(x => x.Map<TestResponseDto>(It.IsAny<TestEntity>())).Returns(new TestResponseDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test Entity" });

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.GetAsync(new Guid("11111111-1111-1111-1111-111111111111"));

            // Assert
            Assert.Equal(ErrorCodes.Success, result.ErrorCode);
            Assert.Equal(new Guid("11111111-1111-1111-1111-111111111111"), result.Data.Id);
            Assert.Equal("Test Entity", result.Data.Name);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNotFound_WhenDataDoesNotExist()
        {
            // Arrange
            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>()))
                .Returns(_repositoryMock.Object);
            
            _repositoryMock.Setup(x => x.Filter(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.Success, Enumerable.Empty<TestEntity>().AsQueryable()));

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.GetAsync(new Guid("11111111-1111-1111-1111-111111111111"));

            // Assert
            Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnGeneralFail_WhenExceptionOccurs()
        {
            // Arrange
            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _repositoryMock.Setup(x => x.Filter(It.IsAny<Expression<Func<TestEntity, bool>>>())).ThrowsAsync(new Exception());

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.GetAsync(new Guid("11111111-1111-1111-1111-111111111111"));

            // Assert
            Assert.Equal(ErrorCodes.GeneralFail, result.ErrorCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNoContent_WhenDataDeletedSuccessfully()
        {
            // Arrange
            var testEntity = new TestEntity { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test Entity" };

            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _repositoryMock.Setup(x => x.Filter(It.IsAny<Expression<Func<TestEntity, bool>>>())).ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.Success, new List<TestEntity> { testEntity }.AsQueryable()));
            _repositoryMock.Setup(x => x.Update(It.IsAny<TestEntity>())).ReturnsAsync(new OperationResponse<TestEntity>(ErrorCodes.Success, testEntity));
            _mapperMock.Setup(x => x.Map<TestResponseDto>(It.IsAny<TestEntity>())).Returns(new TestResponseDto { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Test Entity" });

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.DeleteAsync(new Guid("11111111-1111-1111-1111-111111111111"));

            // Assert
            Assert.Equal(ErrorCodes.NoContent, result.ErrorCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenDataDoesNotExist()
        {
            // Arrange
            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _repositoryMock.Setup(x => x.Filter(It.IsAny<Expression<Func<TestEntity, bool>>>())).ReturnsAsync(new OperationResponse<IQueryable<TestEntity>>(ErrorCodes.Success, Enumerable.Empty<TestEntity>().AsQueryable()));

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.DeleteAsync(new Guid("11111111-1111-1111-1111-111111111111"));

            // Assert
            Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnGeneralFail_WhenExceptionOccurs()
        {
            // Arrange
            _repositoriesFactoryMock.Setup(x => x.GetRepository<TestEntity>(It.IsAny<bool>())).Returns(_repositoryMock.Object);
            _repositoryMock.Setup(x => x.Filter(It.IsAny<Expression<Func<TestEntity, bool>>>())).ThrowsAsync(new Exception());

            var service = new TestService(_repositoriesFactoryMock.Object, _appSettingsMock.Object, _mapperMock.Object);

            // Act
            var result = await service.DeleteAsync(new Guid("11111111-1111-1111-1111-111111111111"));

            // Assert
            Assert.Equal(ErrorCodes.GeneralFail, result.ErrorCode);
            Assert.Null(result.Data);
        }

    public class TestEntity : Entity
    {
        public string Name { get; set; }
        public Guid Id { get;  set; }
    }

    public class TestPostDto
    {
        public string Name { get; set; }
    }
    public class TestPutDto
    {
        public Guid Id { get;  set; }

        public string Name { get; set; }
    }

    public class TestResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    
    public class TestFilters
    {
        public string Name { get; set; }
    }

    public class TestService : ServiceBase<TestEntity, TestPostDto, TestPutDto, TestResponseDto, TestFilters>
    {
        public TestService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) : base(repositoriesFactory, appSettings, mapper)
        {
        }
    }
}