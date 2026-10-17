using System.Collections.Generic;
using FluentAssertions;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Invoice.Sale;

/// <summary>
/// Unit tests for the sales invoice frontal transition guard and sequential FINV- numbering logic.
/// Ensures consistent behavior across null/false IsFrontal states, non-standard prefixes, and dirty data.
/// </summary>
public class SalesInvoiceFrontalLogicTests
{
    [Theory]
    [InlineData(null, "INV-001")]
    [InlineData(false, "INV-001")]
    [InlineData(true, "INV-001")]         // Inconsistent: marked true but no FINV- prefix
    [InlineData(null, "FINV-001")]        // Inconsistent: has FINV- prefix but flag is null
    [InlineData(false, "FINV-001")]       // Inconsistent: has FINV- prefix but flag is false
    [InlineData(true, null)]              // Inconsistent: missing invoice number
    [InlineData(true, "")]                // Inconsistent: empty invoice number
    [InlineData(null, null)]              // Initial draft state
    public void NeedsFrontalConversion_WhenNotFullyFrontal_ReturnsTrue(bool? isFrontal, string? invoiceNo)
    {
        // Act
        var result = SaleInvoiceService.NeedsFrontalConversion(isFrontal, invoiceNo);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("FINV-1")]
    [InlineData("FINV-100")]
    [InlineData("FINV-999999")]
    public void NeedsFrontalConversion_WhenAlreadyFrontalWithValidPrefix_ReturnsFalse(string invoiceNo)
    {
        // Act
        var result = SaleInvoiceService.NeedsFrontalConversion(true, invoiceNo);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateNextFrontalNumber_WhenListIsEmptyOrNull_ReturnsOne()
    {
        // Act & Assert
        SaleInvoiceService.CalculateNextFrontalNumber(null).Should().Be(1);
        SaleInvoiceService.CalculateNextFrontalNumber(new List<string>()).Should().Be(1);
        SaleInvoiceService.CalculateNextFrontalNumber(new List<string> { "INV-1", "INV-2", "DRAFT-10" }).Should().Be(1);
    }

    [Fact]
    public void CalculateNextFrontalNumber_WhenSequentialFINVExist_ReturnsMaxPlusOne()
    {
        // Arrange
        var existingNos = new List<string> { "FINV-1", "FINV-2", "FINV-3" };

        // Act
        var nextNo = SaleInvoiceService.CalculateNextFrontalNumber(existingNos);

        // Assert
        nextNo.Should().Be(4);
    }

    [Fact]
    public void CalculateNextFrontalNumber_WhenGapsExist_ReturnsMaxPlusOne()
    {
        // Arrange
        var existingNos = new List<string> { "FINV-5", "FINV-100", "FINV-20" };

        // Act
        var nextNo = SaleInvoiceService.CalculateNextFrontalNumber(existingNos);

        // Assert
        nextNo.Should().Be(101);
    }

    [Fact]
    public void CalculateNextFrontalNumber_WhenDirtyOrMalformedDataPresent_IgnoresInvalidAndReturnsMaxPlusOne()
    {
        // Arrange
        var existingNos = new List<string?>
        {
            "FINV-1",
            null,
            "",
            "   ",
            "FINV-abc",     // Non-numeric suffix
            "FINV-",        // Empty suffix
            "INV-9999",     // Non-FINV prefix
            "FINV-50",
            "FINV-0025"
        };

        // Act
        var nextNo = SaleInvoiceService.CalculateNextFrontalNumber(existingNos);

        // Assert
        nextNo.Should().Be(51);
    }

    [Theory]
    [InlineData(0, 11)] // max = 10, next = 11
    [InlineData(1, 12)] // max = 10, retry = 1 -> 12
    [InlineData(2, 13)] // max = 10, retry = 2 -> 13
    public void CalculateNextFrontalNumber_WhenRetrying_AppliesRetryOffset(int retryCount, int expectedNumber)
    {
        // Arrange
        var existingNos = new List<string> { "FINV-10", "FINV-5" };

        // Act
        var nextNo = SaleInvoiceService.CalculateNextFrontalNumber(existingNos, retryCount);

        // Assert
        nextNo.Should().Be(expectedNumber);
    }

    [Theory]
    [InlineData(null, 0, 1)]
    [InlineData("", 0, 1)]
    [InlineData("   ", 0, 1)]
    [InlineData("INV-99", 0, 1)]
    [InlineData("FINV-abc", 0, 1)]
    [InlineData("FINV-", 0, 1)]
    [InlineData("FINV-1", 0, 2)]
    [InlineData("FINV-50", 0, 51)]
    [InlineData("FINV-100", 0, 101)]
    [InlineData("FINV-100", 1, 102)] // retry = 1
    [InlineData("FINV-100", 2, 103)] // retry = 2
    public void CalculateNextFrontalNumberFromLast_HandlesAllCasesCorrectly(string? lastNo, int retryCount, int expectedNumber)
    {
        // Act
        var nextNo = SaleInvoiceService.CalculateNextFrontalNumberFromLast(lastNo, retryCount);

        // Assert
        nextNo.Should().Be(expectedNumber);
    }
}
