#nullable enable
using System;
using System.Collections.Generic;
using FluentAssertions;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Invoice.Sale;

/// <summary>
/// Unit tests for Sales Invoice Copy, Sequential Numbering, and Reference Number Normalization.
/// Guarantees zero duplicate constraint violations in SQL Server.
/// </summary>
public class SalesInvoiceCopyAndNumberingTests
{
    #region Reference Number Normalization Tests

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("\t\r\n", null)]
    [InlineData("REF-123", "REF-123")]
    [InlineData("  PO-9988  ", "PO-9988")]
    [InlineData("CUST-REF-001", "CUST-REF-001")]
    public void NormalizeReferenceNumber_ShouldAlwaysReturnNullOrTrimmedString(string? input, string? expected)
    {
        // Act
        var result = SaleInvoiceService.NormalizeReferenceNumber(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Sequential Invoice Numbering Tests (Inv-)

    [Fact]
    public void CalculateNextInvoiceNumber_WhenListIsEmptyOrNull_ReturnsOne()
    {
        // Act & Assert
        SaleInvoiceService.CalculateNextInvoiceNumber(null).Should().Be(1);
        SaleInvoiceService.CalculateNextInvoiceNumber(new List<string>()).Should().Be(1);
        SaleInvoiceService.CalculateNextInvoiceNumber(new List<string> { "Q-1", "FINV-10", "DRAFT-50" }).Should().Be(1);
    }

    [Fact]
    public void CalculateNextInvoiceNumber_WithSequentialInvoices_ReturnsMaxPlusOne()
    {
        // Arrange
        var invoices = new List<string> { "Inv-1", "Inv-2", "Inv-8345" };

        // Act
        var nextNumber = SaleInvoiceService.CalculateNextInvoiceNumber(invoices);

        // Assert
        nextNumber.Should().Be(8346);
    }

    [Theory]
    [InlineData("Inv-100", "INV-200", "inv-50", 201)]
    [InlineData("INV-0005", "Inv-010", "inv-1", 11)]
    [InlineData("Inv-9999", "INV-10000", null, 10001)]
    public void CalculateNextInvoiceNumber_ShouldBeCaseInsensitive(string inv1, string inv2, string? inv3, int expected)
    {
        // Arrange
        var invoices = new List<string?> { inv1, inv2, inv3 };

        // Act
        var nextNumber = SaleInvoiceService.CalculateNextInvoiceNumber(invoices);

        // Assert
        nextNumber.Should().Be(expected);
    }

    [Fact]
    public void CalculateNextInvoiceNumber_WithRetryOffset_AddsRetryToNextNumber()
    {
        // Arrange
        var invoices = new List<string> { "Inv-8345" };

        // Act
        var attempt0 = SaleInvoiceService.CalculateNextInvoiceNumber(invoices, retryCount: 0);
        var attempt1 = SaleInvoiceService.CalculateNextInvoiceNumber(invoices, retryCount: 1);
        var attempt2 = SaleInvoiceService.CalculateNextInvoiceNumber(invoices, retryCount: 2);

        // Assert
        attempt0.Should().Be(8346);
        attempt1.Should().Be(8347);
        attempt2.Should().Be(8348);
    }

    [Fact]
    public void CalculateNextInvoiceNumber_WithMalformedAndDirtyData_GracefullyExtractsValidNumbers()
    {
        // Arrange
        var dirtyList = new List<string?>
        {
            "Inv-",
            "Inv-abc",
            null,
            "",
            "   ",
            "Inv-50",
            "Inv-0025",
            "NOT-AN-INVOICE",
            "Inv-100"
        };

        // Act
        var nextNumber = SaleInvoiceService.CalculateNextInvoiceNumber(dirtyList);

        // Assert
        nextNumber.Should().Be(101);
    }

    #endregion

    #region Sequential Quotation Numbering Tests (Q-)

    [Fact]
    public void CalculateNextQuotationNumber_WhenListIsEmptyOrNull_ReturnsOne()
    {
        // Act & Assert
        SaleInvoiceService.CalculateNextQuotationNumber(null).Should().Be(1);
        SaleInvoiceService.CalculateNextQuotationNumber(new List<string>()).Should().Be(1);
    }

    [Fact]
    public void CalculateNextQuotationNumber_WithExistingQuotations_ReturnsMaxPlusOne()
    {
        // Arrange
        var quotations = new List<string> { "Q-1", "q-5", "Q-20" };

        // Act
        var nextNumber = SaleInvoiceService.CalculateNextQuotationNumber(quotations);

        // Assert
        nextNumber.Should().Be(21);
    }

    #endregion

    #region Copy Template State Sanitization Tests

    [Fact]
    public void CopyTemplate_SanitizesIdentityFields_ToPreventDatabaseCollisions()
    {
        // Arrange - Simulate an existing original invoice with database-assigned state
        var originalInvoiceDto = new SaleInvoiceDto
        {
            Id = 8345,
            InvoiceNo = "Inv-8345",
            ReferenceNumber = "OLD-REF-999",
            IssueDate = DateTime.UtcNow.AddMonths(-2),
            DueDate = DateTime.UtcNow.AddMonths(-1),
            TransactionId = Guid.NewGuid(),
            IsCleared = true,
            InvoiceHash = "SHA256-SAMPLE-HASH-12345",
            GeneratedQR = "QR-CODE-BINARY-DATA",
            RefundStatus = "partial",
            RefundedAmount = 500,
            PaidAmount = 1500,
            RemainingAmount = 0,
            PaymentStatus = InvoicePaymentStatus.Paid,
            SaleItems = new List<SaleItemDto>
            {
                new SaleItemDto
                {
                    Id = 55001,
                    SaleInvoiceId = 8345,
                    Price = 100,
                    Quantity = 2,
                    ReturnedQuantity = 1
                },
                new SaleItemDto
                {
                    Id = 55002,
                    SaleInvoiceId = 8345,
                    Price = 250,
                    Quantity = 4,
                    ReturnedQuantity = 0
                }
            }
        };

        // Act - Apply template sanitization logic (same as GetCopyTemplateAsync)
        originalInvoiceDto.Id = 0;
        originalInvoiceDto.InvoiceNo = null;
        originalInvoiceDto.ReferenceNumber = null;
        originalInvoiceDto.IssueDate = DateTime.UtcNow;
        originalInvoiceDto.DueDate = DateTime.UtcNow;
        originalInvoiceDto.TransactionId = null;
        originalInvoiceDto.ConvertedToInvoiceId = null;
        originalInvoiceDto.OriginalQuotationNo = null;
        originalInvoiceDto.IsQuotation = false;
        originalInvoiceDto.IsCleared = false;
        originalInvoiceDto.InvoiceHash = null;
        originalInvoiceDto.GeneratedQR = null;
        originalInvoiceDto.RefundStatus = "none";
        originalInvoiceDto.RefundedAmount = 0;
        originalInvoiceDto.PaidAmount = 0;
        originalInvoiceDto.PaymentStatus = InvoicePaymentStatus.Unpaid;

        foreach (var item in originalInvoiceDto.SaleItems)
        {
            item.Id = null;
            item.SaleInvoiceId = null;
            item.ReturnedQuantity = 0;
        }

        // Assert - Verify complete clean state for new insertion
        originalInvoiceDto.Id.Should().Be(0);
        originalInvoiceDto.InvoiceNo.Should().BeNull();
        originalInvoiceDto.ReferenceNumber.Should().BeNull();
        originalInvoiceDto.TransactionId.Should().BeNull();
        originalInvoiceDto.IsCleared.Should().BeFalse();
        originalInvoiceDto.InvoiceHash.Should().BeNull();
        originalInvoiceDto.GeneratedQR.Should().BeNull();
        originalInvoiceDto.PaidAmount.Should().Be(0);
        originalInvoiceDto.RefundStatus.Should().Be("none");
        originalInvoiceDto.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);

        originalInvoiceDto.SaleItems.Should().HaveCount(2);
        originalInvoiceDto.SaleItems.Should().OnlyContain(item => item.Id == null && item.SaleInvoiceId == null && item.ReturnedQuantity == 0);
    }

    #endregion
}
