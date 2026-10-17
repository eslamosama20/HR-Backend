using AutoMapper;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Services.Attachments;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ZXing;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Attachments
{
    public class AttachmentServiceTests
    {
        private readonly Mock<RepositoriesFactory> _mockFactory;
        private readonly Mock<IRepository<Attachment>> _mockRepo;
        private readonly Mock<AppSettings> _mockAppSettings;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AttachmentsService _attachmentService;
       
        public AttachmentServiceTests()
        {
            _mockFactory = new Mock<RepositoriesFactory>();
            _mockRepo = new Mock<IRepository<Attachment>>();
            _mockAppSettings = new Mock<AppSettings>();
            _mockMapper = new Mock<IMapper>();

            _attachmentService = new AttachmentsService(
                _mockFactory.Object,
                _mockAppSettings.Object,
                _mockMapper.Object);
        }


        //MethodName_ExpectedBehavior_WhenCondition

        // Test GetAllAsync

        [Fact]
        public async Task GetAllAsync_ShouldReturnSuccessAndDtoList_WhenDataExists()
        {
            // Arrange

            var now = DateTime.UtcNow;

            var attachments = new List<Attachment>
            {
                new Attachment
                {
                    Id = Guid.NewGuid(),
                    FileName = "File1.pdf",
                    FileType = "pdf",
                    UploadedAt = now,
                    ReferenceNumber = "REF1"
                },
                new Attachment
                {
                    Id = Guid.NewGuid(),
                    FileName = "File2.docx",
                    FileType = "docx",
                    UploadedAt = now,
                    ReferenceNumber = "REF2"
                }
            };


            // Use AsQueryable() directly to support expression tree operations (ApplySort, ApplySearchKeyword)
            var queryable = attachments.AsQueryable();

            _mockRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new OperationResponse<IQueryable<Attachment>>(ErrorCodes.Success,
                    queryable));

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                .Returns(_mockRepo.Object);

            var expectedList = attachments.Select(a => new AttachmentResponseDto
            {
                Id= a.Id,
                FileName= a.FileName,
                FileType= a.FileType,
                UploadedAt= a.UploadedAt,
                ReferenceNumber= a.ReferenceNumber
            });

            // Act 

            var result = await _attachmentService.GetAllAsync(new AttachmentFiltersDto());


            // Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(attachments.Count);
            result.Data.Should().BeEquivalentTo(expectedList,options=>options.Excluding(x=>x.UploadedAt)
            .ExcludingMissingMembers());

        }


        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyListWithSuccess_WhenNoAttachmentsExist()
        {
            // Arrange
            var attachments = new List<Attachment>();
            var queryable = attachments.AsQueryable();

            _mockRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new OperationResponse<IQueryable<Attachment>>
                (ErrorCodes.Success, queryable));

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                .Returns(_mockRepo.Object);

            // Act
            var result = await _attachmentService.GetAllAsync(new AttachmentFiltersDto());

            // Assert
            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }


        [Fact]
        public async Task GetAllAsync_ShouldReturnGeneralFail_WhenExceptionIsThrown()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Db error");

            _mockRepo.Setup(r => r.GetAll())
                .ThrowsAsync(expectedException);


            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                .Returns(_mockRepo.Object);


            // Act 

            var result = await _attachmentService.GetAllAsync(new AttachmentFiltersDto());


            // Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.GeneralFail);
            result.Data.Should().BeNull();

            _mockRepo.Verify(r => r.GetAll(), Times.Once);

        }


        // AddAsync


        [Fact]
        public async Task AddAsync_ShouldReturnSuccess_WhenAttachmentAddedSuccessfully()


        {
            //Arrange 

            var attachmentPostDto = new AttachmentPostDto
            {
                File = Convert.ToBase64String(Encoding.UTF8.GetBytes("fake data")),
                 FileName = "Test.pdf",
                 FileType = "application/pdf"

            };

            var expectedAttachmentFromRepo = new Attachment
            {
                FileName = "Test.pdf"

            };

            _mockRepo.Setup(r => r.Add(It.IsAny<Attachment>()))
                .ReturnsAsync(new OperationResponse<Attachment>
                    (
                      ErrorCodes.Success, expectedAttachmentFromRepo
                    ));

            _mockFactory.Setup(f=>f.GetRepository<Attachment>(It.IsAny<bool>()))
                .Returns(_mockRepo.Object);
            
            //Act 

            var result = await _attachmentService.AddAsync(attachmentPostDto);


            //Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().NotBeNull();


            _mockRepo.Verify(r => r.Add(It.IsAny<Attachment>()), Times.Once);

        }

        [Fact]
        public async Task AddAsync_ShouldReturnGeneralFail_WhenAddAttachmentFails()
        {
            //Arrange 

            var attachmentPostDto = new AttachmentPostDto
            {
                File = Convert.ToBase64String(Encoding.UTF8.GetBytes("invalid file")),
                FileName = "file.pdf",
                FileType = "application/pdf"

            };

            var failedResponse = new OperationResponse<Attachment>
            {
                ErrorCode = ErrorCodes.GeneralFail,
                Data = null
            };

            _mockRepo.Setup(r => r.Add(It.IsAny<Attachment>()))
                .ReturnsAsync(failedResponse);


            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                .Returns(_mockRepo.Object);


            //Act 

            var result = await _attachmentService.AddAsync(attachmentPostDto);

            // Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.GeneralFail);
            result.Data.Should().BeNull();

            _mockRepo.Verify(r => r.Add(It.IsAny<Attachment>()), Times.Once);

        }

        [Fact]
        public async Task AddAsync_ShouldReturnBadRequestError_WhenFileIsNotValidBase64()
        {
            //Arrange 

            var attachmentPostDto = new AttachmentPostDto
            {
                File = "this file is not base64",
                FileName = "badfile.pdf",
                FileType = "application/pdf"

            };

            //Act 

            var result = await _attachmentService.AddAsync(attachmentPostDto);

            // Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.BadRequest);
            result.Data.Should().BeNull();

        }

        [Fact]
        public async Task AddAsync_ShouldReturnGeneralFail_WhenExceptionIsThrown()
        {
            // Arrange
            var attachmentPostDto = new AttachmentPostDto
            {
                File = Convert.ToBase64String(Encoding.UTF8.GetBytes("fake data")),
                FileName = "file.pdf",
                FileType = "application/pdf"
            };

            var expectedException = new InvalidOperationException("Db error");

            _mockRepo.Setup(r => r.Add(It.IsAny<Attachment>()))
                     .ThrowsAsync(expectedException);

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                        .Returns(_mockRepo.Object);

            // Act
            var result = await _attachmentService.AddAsync(attachmentPostDto);

            // Assert
            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.GeneralFail);
            result.Data.Should().BeNull();

            _mockRepo.Verify(r => r.Add(It.IsAny<Attachment>()), Times.Once);
        }


        // UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldReturnGeneralFail_WhenAttachmentExistsButUpdateFails()
        {
            //Arrange
            var attachmentPutDto = new AttachmentPutDto
            {
                Id = Guid.NewGuid(),
                File = Convert.ToBase64String(Encoding.UTF8.GetBytes("new data")),
                FileName = "updated.pdf",
                FileType = "application/pdf"
            };
            var existingAttachment = new Attachment
            {
                Id = attachmentPutDto.Id,
                FileName = "old.pdf",
                FileType = "application/pdf",
                FileData = Encoding.UTF8.GetBytes("old data"),
                UploadedAt = DateTime.UtcNow
            };

            var filterResponse = new OperationResponse<IQueryable<Attachment>>(ErrorCodes.Success, new List<Attachment> { existingAttachment }.AsQueryable());

            var updateResponse = new OperationResponse<Attachment>(ErrorCodes.GeneralFail, null);


            _mockRepo.Setup(r => r.Filter(It.IsAny<Expression<Func<Attachment, bool>>>()))
                 .ReturnsAsync(filterResponse);

            _mockRepo.Setup(r => r.Update(It.IsAny<Attachment>()))
                 .ReturnsAsync(updateResponse);

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                  .Returns(_mockRepo.Object);

            //Act 
            var result = await _attachmentService.UpdateAsync(attachmentPutDto);

            //Assert
            
            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.GeneralFail);
            result.Data.Should().BeNull();
            
        }

            [Fact]

            public async Task UpdateAsync_ShouldReturnSuccess_WhenAttachmentExistsAndUpdateSucceeds()
            {
                //Arrange 

                var attachmentPutDto = new AttachmentPutDto
                {
                    Id = Guid.NewGuid(),
                    File = Convert.ToBase64String(Encoding.UTF8.GetBytes("updated file")),
                    FileName = "updated.pdf",
                    FileType = "application/pdf"
                };

                var existingAttachment = new Attachment
                {
                    Id = attachmentPutDto.Id,
                    FileName = "old.pdf",
                    FileType = "application/pdf",
                    FileData = Encoding.UTF8.GetBytes("old file"),
                    UploadedAt = DateTime.UtcNow.AddDays(-1)
                };


                var filteredResponse = new OperationResponse<IQueryable<Attachment>>
                    (ErrorCodes.Success, new List<Attachment> { existingAttachment }.AsQueryable());

                var updatedResponse = new OperationResponse<Attachment>(ErrorCodes.Success, existingAttachment);

                _mockRepo.Setup(r=>r.Filter(It.IsAny<Expression<Func<Attachment,bool>>>()))
                .ReturnsAsync(filteredResponse);

                _mockRepo.Setup(r=>r.Update(It.IsAny<Attachment>()))
                    .ReturnsAsync(updatedResponse);


                _mockFactory.Setup(f=>f.GetRepository<Attachment>(It.IsAny<bool>()))
                    .Returns(_mockRepo.Object);
                //Act 

                var result = await _attachmentService.UpdateAsync(attachmentPutDto);

                //Assert
                result.Should().NotBeNull();
                result.ErrorCode.Should().Be(ErrorCodes.Success);
                result.Data.Should().NotBeNull();
                result.Data.FileName.Should().Be("updated.pdf");

        }


        [Fact]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenAttachmentDoesNotExist()
        {
            //Arrange
            var attachmentPutDto = new AttachmentPutDto
            {
                Id = Guid.NewGuid(),
                File = Convert.ToBase64String(Encoding.UTF8.GetBytes("test"))

            };

            var emptyResponse = new OperationResponse<IQueryable<Attachment>>
                (ErrorCodes.Success, new List<Attachment>().AsQueryable());

            _mockRepo.Setup(r=>r.Filter(It.IsAny<Expression<Func<Attachment,bool>>>()))
                .ReturnsAsync(emptyResponse);

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                 .Returns(_mockRepo.Object);


            // Act 
            var result = await _attachmentService.UpdateAsync(attachmentPutDto);
       
            //Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.NotFound);
            result.Data.Should().BeNull();

        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnGeneralFail_WhenExceptionIsThrown()
        {
            // Arrange
            var attachmentPutDto = new AttachmentPutDto
            {
                Id = Guid.NewGuid(),
                File = Convert.ToBase64String(Encoding.UTF8.GetBytes("test")),
                FileName = "file.pdf",
                FileType = "application/pdf"
            };

            var existingAttachment = new Attachment
            {
                Id = attachmentPutDto.Id,
                FileName = "old.pdf",
                FileType = "application/pdf",
                FileData = Encoding.UTF8.GetBytes("old data"),
                UploadedAt = DateTime.UtcNow
            };

            var filterResponse = new OperationResponse<IQueryable<Attachment>>
                (ErrorCodes.Success, new List<Attachment> { existingAttachment }.AsQueryable());

            _mockRepo.Setup(r => r.Filter(It.IsAny<Expression<Func<Attachment, bool>>>()))
                     .ReturnsAsync(filterResponse);
        }

        // DownloadAsync
        [Fact]
        public async Task DownloadAsync_ShouldReturnSuccess_WhenAttachmentExists()
        {
            // Arrange
            var attachmentId = Guid.NewGuid();
            var attachment = new Attachment
            {
                Id = attachmentId,
                FileName = "test.pdf",
                FileType = "application/pdf",
                FileData = Encoding.UTF8.GetBytes("dummy file"),
                UploadedAt = DateTime.UtcNow,
                ReferenceNumber = "REF123",
                InventoryPermitId = Guid.NewGuid(),
                PurchaseInvoiceId = 1,
                SaleInvoiceId = 2,
                SalesReturnId = 3,
                PurchaseReturnId = 4,
                PaymentReceiptId = Guid.NewGuid(),
                PaycheckReceiptId = Guid.NewGuid(),
                DebitNoteId = Guid.NewGuid(),
                CreditNoteId = Guid.NewGuid(),
                InventoryStocktakingId = Guid.NewGuid(),
                ReservationId = Guid.NewGuid(),
                TransactionId = Guid.NewGuid()
            };

            var operationResponse = new OperationResponse<IQueryable<Attachment>>
                (ErrorCodes.Success, new List<Attachment> { attachment }.AsQueryable());


            _mockRepo.Setup(r => r.Filter(It.IsAny<Expression<Func<Attachment, bool>>>()))
             .ReturnsAsync(operationResponse);

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
             .Returns(_mockRepo.Object);

            // Act
            var result = await _attachmentService.DownloadAsync(attachmentId);

            // Assert
            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.Success);
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(attachment.Id);
            result.Data.FileName.Should().Be(attachment.FileName);
            result.Data.FileData.Should().Be(Convert.ToBase64String(attachment.FileData));
        }
        [Fact]
        public async Task DownloadAsync_ShouldReturnNotFound_WhenAttachmentDoesNotExist()
        {
            var attachmentId = Guid.NewGuid();

            var operationResponse = new OperationResponse<IQueryable<Attachment>>
                (ErrorCodes.Success, new List<Attachment>().AsQueryable());

            _mockRepo.Setup(r => r.Filter(It.IsAny<Expression<Func<Attachment, bool>>>()))
                  .ReturnsAsync(operationResponse);


            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                   .Returns(_mockRepo.Object);

            // Act

            var result = await _attachmentService.DownloadAsync(attachmentId);

            //Assert

            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.NotFound);
            result.Data.Should().BeNull();

        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnGeneralFail_WhenFilterFails()
        {
            // Arrange
            var attachmentId = Guid.NewGuid();
            var operationResponse = new OperationResponse<IQueryable<Attachment>>
                (ErrorCodes.GeneralFail, null);

            _mockRepo.Setup(r => r.Filter(It.IsAny<Expression<Func<Attachment, bool>>>()))
                     .ReturnsAsync(operationResponse);

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                        .Returns(_mockRepo.Object);

            // Act
            var result = await _attachmentService.DownloadAsync(attachmentId);

            // Assert
            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.GeneralFail);
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task DownloadAsync_ShouldReturnGeneralFail_WhenExceptionIsThrown()
        {
            // Arrange
            var attachmentId = Guid.NewGuid();

            var expectedException = new Exception("Database failure");

                _mockRepo.Setup(r => r.Filter(It.IsAny<Expression<Func<Attachment, bool>>>()))
                     .ThrowsAsync(expectedException);

            _mockFactory.Setup(f => f.GetRepository<Attachment>(It.IsAny<bool>()))
                        .Returns(_mockRepo.Object);

            // Act
            var result = await _attachmentService.DownloadAsync(attachmentId);

            // Assert
            result.Should().NotBeNull();
            result.ErrorCode.Should().Be(ErrorCodes.GeneralFail); 
            result.Data.Should().BeNull();
        }

    }
}
