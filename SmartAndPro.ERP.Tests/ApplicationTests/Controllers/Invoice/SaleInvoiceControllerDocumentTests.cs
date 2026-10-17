using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.WebAPI.Controllers.Invoice;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers.Invoice;

/// <summary>
/// Contract tests for the batch/merged document endpoints that were extended with
/// print-type support (combined printing format choice feature) and the server-side
/// Print-1 replica (Format1Server) parity params.
/// Only the document-generation actions are exercised; heavy service dependencies
/// are mocked so no database is required.
/// </summary>
public class SaleInvoiceControllerDocumentTests
{
    private readonly Mock<ISalesInvoicesService> _salesServiceMock;
    private readonly SaleInvoiceController _controller;

    public SaleInvoiceControllerDocumentTests()
    {
        _salesServiceMock = new Mock<ISalesInvoicesService>();
        _controller = new SaleInvoiceController(
            serviceBaseFactory: null,
            salesInvoicesService: _salesServiceMock.Object,
            salesInvoiceImportService: null,
            documentShareService: null,
            mapper: null,
            logger: null,
            repositoriesFactory: null,
            memoryCache: null,
            env: null);
    }

    #region GET /documents (selected ZIP)

    [Fact]
    public async Task GetDocuments_ShouldPassPrintTypeToService_AndReturnZip()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetDocuments(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[][] { new byte[] { 1, 2, 3 } });

        // Act
        var result = await _controller.GetDocuments(new[] { 42 }, InvoicePrintType.A4Landscape);

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/zip");
        file.FileDownloadName.Should().Be("invoices.zip");
        _salesServiceMock.Verify(
            s => s.GetDocuments(new[] { 42 }, InvoicePrintType.A4Landscape, null, null),
            Times.Once);
    }

    [Fact]
    public async Task GetDocuments_WhenPrintTypeOmitted_ShouldDefaultToA4Portrait()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetDocuments(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[][] { new byte[] { 1, 2, 3 } });

        // Act
        await _controller.GetDocuments(new[] { 42 });

        // Assert
        _salesServiceMock.Verify(
            s => s.GetDocuments(new[] { 42 }, InvoicePrintType.A4Portrait, null, null),
            Times.Once);
    }

    [Fact]
    public async Task GetDocuments_WithThermalPrintType_ShouldPassThermalToService()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetDocuments(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[][] { new byte[] { 1, 2, 3 } });

        // Act
        await _controller.GetDocuments(new[] { 42 }, InvoicePrintType.Thermal80mm);

        // Assert
        _salesServiceMock.Verify(
            s => s.GetDocuments(new[] { 42 }, InvoicePrintType.Thermal80mm, null, null),
            Times.Once);
    }

    [Fact]
    public async Task GetDocuments_WithFormat1Server_ShouldPassPrint1Options_AndReturnZip()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetDocuments(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[][] { new byte[] { 1, 2, 3 } });
        var validGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _controller.GetDocuments(
            new[] { 42 },
            InvoicePrintType.Format1Server,
            language: "en",
            showStamp: false,
            showCompanySignature: false,
            uncheckedPolicies: $"{validGuid},not-a-guid",
            uncheckedBanks: "22222222-2222-2222-2222-222222222222");

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/zip");
        _salesServiceMock.Verify(
            s => s.GetDocuments(
                new[] { 42 },
                InvoicePrintType.Format1Server,
                It.Is<Print1ServerOptions>(o =>
                    o.Language == "en" &&
                    o.ShowStamp == false &&
                    o.ShowCompanySignature == false &&
                    o.ShowCustomerSignature &&
                    o.UncheckedPolicyIds != null &&
                    o.UncheckedPolicyIds.Count == 1 &&
                    o.UncheckedPolicyIds.Contains(validGuid) &&
                    o.UncheckedBankIds != null &&
                    o.UncheckedBankIds.Count == 1),
                null),
            Times.Once);
    }

    #endregion

    #region POST /documents/merged

    [Fact]
    public void MergeRequest_ShouldDefaultToA4Portrait()
    {
        // Assert — old clients posting only { ids } must keep the current portrait behavior.
        var request = new SaleInvoiceController.MergeRequest { ids = new[] { 1, 2 } };
        request.printType.Should().Be(InvoicePrintType.A4Portrait);
        request.Language.Should().Be("ar");
        request.ShowStamp.Should().BeTrue();
        request.ShowCompanySignature.Should().BeTrue();
        request.ShowCustomerSignature.Should().BeTrue();
        request.UncheckedPolicies.Should().BeNull();
        request.UncheckedBanks.Should().BeNull();
    }

    [Fact]
    public async Task GetMerged_WithA4Landscape_ShouldPassPrintType_AndReturnPdf()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetMergedDocument(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });
        var request = new SaleInvoiceController.MergeRequest
        {
            ids = new[] { 7, 8 },
            printType = InvoicePrintType.A4Landscape
        };

        // Act
        var result = await _controller.GetMerged(request);

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        file.FileDownloadName.Should().Be("invoices-merged.pdf");
        _salesServiceMock.Verify(
            s => s.GetMergedDocument(new[] { 7, 8 }, InvoicePrintType.A4Landscape, null, null),
            Times.Once);
    }

    [Theory]
    [InlineData(InvoicePrintType.Thermal80mm)]
    [InlineData(InvoicePrintType.Thermal58mm)]
    public async Task GetMerged_WithThermalPrintType_ShouldReturnBadRequest_AndNotCallService(InvoicePrintType thermal)
    {
        // Arrange
        var request = new SaleInvoiceController.MergeRequest
        {
            ids = new[] { 7, 8 },
            printType = thermal
        };

        // Act
        var result = await _controller.GetMerged(request);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Thermal print types are not supported for merged documents");
        _salesServiceMock.Verify(
            s => s.GetMergedDocument(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMerged_WithFormat1Server_ShouldPassPrint1Options_AndReturnPdf()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetMergedDocument(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });
        var request = new SaleInvoiceController.MergeRequest
        {
            ids = new[] { 7, 8 },
            printType = InvoicePrintType.Format1Server,
            Language = "ar",
            ShowStamp = true,
            ShowCompanySignature = false,
            UncheckedBanks = "33333333-3333-3333-3333-333333333333"
        };

        // Act
        var result = await _controller.GetMerged(request);

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        _salesServiceMock.Verify(
            s => s.GetMergedDocument(
                new[] { 7, 8 },
                InvoicePrintType.Format1Server,
                It.Is<Print1ServerOptions>(o =>
                    o.Language == "ar" &&
                    o.ShowCompanySignature == false &&
                    o.ShowCustomerSignature &&
                    o.UncheckedBankIds != null &&
                    o.UncheckedBankIds.Count == 1),
                null),
            Times.Once);
    }

    #endregion

    #region GET /documents/merged/all

    private void SetupGetAllAsync()
    {
        _salesServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(new OperationResponse<object>(
                ErrorCodes.Success,
                new List<SaleInvoiceDto> { new SaleInvoiceDto { Id = 1 }, new SaleInvoiceDto { Id = 2 } }));
    }

    [Fact]
    public async Task GetMergedAll_WithA4Landscape_ShouldPassPrintType_AndReturnPdf()
    {
        // Arrange
        SetupGetAllAsync();
        _salesServiceMock
            .Setup(s => s.GetMergedDocument(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });

        // Act
        var result = await _controller.GetMergedAll(InvoicePrintType.A4Landscape);

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        file.FileDownloadName.Should().Be("invoices-merged-all.pdf");
        _salesServiceMock.Verify(
            s => s.GetMergedDocument(new[] { 1, 2 }, InvoicePrintType.A4Landscape, null, null),
            Times.Once);
    }

    [Fact]
    public async Task GetMergedAll_WithThermalPrintType_ShouldReturnBadRequest_AndNotCallService()
    {
        // Arrange
        SetupGetAllAsync();

        // Act
        var result = await _controller.GetMergedAll(InvoicePrintType.Thermal58mm);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Thermal print types are not supported for merged documents");
        _salesServiceMock.Verify(
            s => s.GetMergedDocument(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMergedAll_WithFormat1Server_ShouldPassPrint1Options_AndReturnPdf()
    {
        // Arrange
        SetupGetAllAsync();
        _salesServiceMock
            .Setup(s => s.GetMergedDocument(It.IsAny<int[]>(), It.IsAny<InvoicePrintType>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });

        // Act
        var result = await _controller.GetMergedAll(
            InvoicePrintType.Format1Server,
            language: "en",
            showCustomerSignature: false,
            uncheckedPolicies: "44444444-4444-4444-4444-444444444444");

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        file.FileDownloadName.Should().Be("invoices-merged-all.pdf");
        _salesServiceMock.Verify(
            s => s.GetMergedDocument(
                new[] { 1, 2 },
                InvoicePrintType.Format1Server,
                It.Is<Print1ServerOptions>(o =>
                    o.Language == "en" &&
                    o.ShowCustomerSignature == false &&
                    o.UncheckedPolicyIds != null &&
                    o.UncheckedPolicyIds.Count == 1),
                null),
            Times.Once);
    }

    #endregion

    #region GET /document/{invoiceId} (single)

    [Fact]
    public async Task GetDocument_WithFormat1Server_ShouldPassPrint1Options_AndReturnPdf()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetDocument(It.IsAny<int>(), It.IsAny<InvoicePrintType>(), It.IsAny<string?>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });
        var validGuid = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // Act
        var result = await _controller.GetDocument(
            42,
            InvoicePrintType.Format1Server,
            language: "en",
            showStamp: false,
            showCompanySignature: false,
            uncheckedPolicies: $"{validGuid},not-a-guid",
            uncheckedBanks: "66666666-6666-6666-6666-666666666666");

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        file.FileDownloadName.Should().Be("invoice-42.pdf");
        _salesServiceMock.Verify(
            s => s.GetDocument(
                42,
                InvoicePrintType.Format1Server,
                It.IsAny<string?>(),
                It.Is<Print1ServerOptions>(o =>
                    o.Language == "en" &&
                    o.ShowStamp == false &&
                    o.ShowCompanySignature == false &&
                    o.ShowCustomerSignature &&
                    o.UncheckedPolicyIds != null &&
                    o.UncheckedPolicyIds.Count == 1 &&
                    o.UncheckedPolicyIds.Contains(validGuid) &&
                    o.UncheckedBankIds != null &&
                    o.UncheckedBankIds.Count == 1),
                null),
            Times.Once);
    }

    [Fact]
    public async Task GetDocument_WhenPrintTypeOmitted_ShouldPassNullOptions_AndReturnPdf()
    {
        // Arrange
        _salesServiceMock
            .Setup(s => s.GetDocument(It.IsAny<int>(), It.IsAny<InvoicePrintType>(), It.IsAny<string?>(), It.IsAny<Print1ServerOptions?>(), It.IsAny<Print2ServerOptions?>()))
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });

        // Act
        var result = await _controller.GetDocument(42);

        // Assert
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("application/pdf");
        _salesServiceMock.Verify(
            s => s.GetDocument(42, InvoicePrintType.A4Portrait, It.IsAny<string?>(), null, null),
            Times.Once);
    }

    #endregion
}
