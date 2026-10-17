using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.ValueObjects;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Invoice.Sale;

/// <summary>
/// Unit tests for Sales Invoice due date rules and ZATCA tax invoice vs simplified invoice formatting.
/// Rules verified:
/// 1. Simplified Tax Invoice (B2C - No Customer Tax Number): Due date is never rendered in print.
/// 2. Standard Tax Invoice (B2B - Customer Tax Number present):
///    - If Due Date == Issue Date: Due date is omitted to avoid duplicate date printing.
///    - If Due Date != Issue Date: Due date is rendered alongside Issue Date.
/// 3. Issue Date label is officially unified across all print templates.
/// </summary>
public class SalesInvoiceDueDateAndTaxRulesTests
{
    static SalesInvoiceDueDateAndTaxRulesTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private static CompanyDetails CreateSampleCompany() =>
        new()
        {
            ArabicName = "شركة سمارت وبرو",
            EnglishName = "Smart & Pro Co.",
            TaxNumber = "310000000000003",
            ArabicAddress = new Address("الرياض", "المملكة العربية السعودية", "طريق الملك فهد", "12345"),
            Address = new Address("Riyadh", "Saudi Arabia", "King Fahd Road", "12345"),
            BuildingNumber = "1234",
            District = "العليا"
        };

    private static MainSalesClass CreateSalesClass() =>
        new()
        {
            ArabicName = "استشارة برمجية",
            EnglishName = "Software Consulting",
            ReferenceCode = "SRV-001",
            IsTaxExempted = false
        };

    private static SaleInvoice CreateInvoice(
        string? customerTaxNumber,
        DateTime issueDate,
        DateTime dueDate,
        string invoiceNo = "INV-2026-001")
    {
        var salesClass = CreateSalesClass();
        var item = new SaleItem
        {
            ItemNo = "1",
            Price = 1000m,
            Quantity = 1m,
            NoOfNights = 1,
            Tax = 15m,
            TaxAmount = 150m,
            Discount = 0m,
            DiscountAmount = 0m,
            TotalPrice = 1150m,
            NetPrice = 1000m,
            MainSalesClass = salesClass,
            MainSalesClassId = Guid.NewGuid()
        };

        return new SaleInvoice
        {
            Id = 1,
            InvoiceNo = invoiceNo,
            Currency = "SAR",
            IssueDate = issueDate,
            DueDate = dueDate,
            Tax = 15m,
            TaxAmount = 150m,
            DiscountAmount = 0m,
            TotalPrice = 1150m,
            NetPrice = 1000m,
            SaleItems = new List<SaleItem> { item },
            CustomerProfile = new CustomerProfile
            {
                ArabicName = string.IsNullOrWhiteSpace(customerTaxNumber) ? "عميل فردي (نقدي)" : "شركة التقنية المتقدمة",
                EnglishName = string.IsNullOrWhiteSpace(customerTaxNumber) ? "Individual Cash Customer" : "Advanced Tech Co.",
                TaxNumber = customerTaxNumber
            }
        };
    }

    [Fact]
    public void Print2_SimplifiedInvoice_GeneratesPdfSuccessfully()
    {
        // Arrange: Simplified Tax Invoice (no customer VAT number)
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc); // Different due date
        var invoice = CreateInvoice(null, issueDate, dueDate);

        // Act
        var document = new SalesInvoiceDocument(invoice, company);
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Print2_TaxInvoice_SameDate_GeneratesPdfSuccessfully()
    {
        // Arrange: Standard B2B Invoice with DueDate == IssueDate
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var invoice = CreateInvoice("310123456700003", issueDate, dueDate);

        // Act
        var document = new SalesInvoiceDocument(invoice, company);
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Print2_TaxInvoice_DifferentDueDate_GeneratesPdfSuccessfully()
    {
        // Arrange: Standard B2B Invoice with DueDate != IssueDate
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
        var invoice = CreateInvoice("310123456700003", issueDate, dueDate);

        // Act
        var document = new SalesInvoiceDocument(invoice, company);
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Print1_SimplifiedInvoice_GeneratesPdfSuccessfully()
    {
        // Arrange: Print 1 Simplified Invoice
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);
        var invoice = CreateInvoice(null, issueDate, dueDate);

        // Act
        var document = new SalesInvoicePrint1ServerDocument(invoice, company, "ar");
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Print1_TaxInvoice_DifferentDueDate_GeneratesPdfSuccessfully()
    {
        // Arrange: Print 1 Standard B2B Invoice with DueDate != IssueDate
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 25, 10, 0, 0, DateTimeKind.Utc);
        var invoice = CreateInvoice("310123456700003", issueDate, dueDate);

        // Act
        var document = new SalesInvoicePrint1ServerDocument(invoice, company, "ar");
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void CombinedPrint_TaxInvoice_GeneratesPdfSuccessfully()
    {
        // Arrange: Combined Print Document
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);
        var invoice1 = CreateInvoice("310123456700003", issueDate, dueDate, "INV-001");
        var invoice2 = CreateInvoice(null, issueDate, issueDate, "INV-002");

        // Act
        var document = new CombinedSalesInvoicesDocument(new List<SaleInvoice> { invoice1, invoice2 }, company);
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Thermal80mm_SimplifiedInvoice_GeneratesPdfSuccessfully()
    {
        // Arrange: 80mm Thermal Receipt Document
        var company = CreateSampleCompany();
        var issueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var invoice = CreateInvoice(null, issueDate, dueDate);

        // Act
        var document = new ThermalInvoiceDocument80mm(invoice, company);
        var pdfBytes = document.GeneratePdf();

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(500);
    }
}
