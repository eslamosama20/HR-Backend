using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using QuestPDF.Fluent; // GeneratePdf extension
using SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Invoice.Sale;

/// <summary>
/// Parity tests for the server-side Print-1 replica math. Expected values are computed
/// with the FRONTEND formulas (sales-invoice-list.component.ts getItemNetPrice /
/// getItemTaxAmount / getItemTotalPrice / getNetPrice / getTaxAmount / getTotalPrice),
/// so any drift between the browser Print-1 and the server Print-1 is caught here.
/// </summary>
public class SalesInvoicePrint1ServerMathTests
{
    private static MainSalesClass SalesClass(bool isExempt = false) =>
        new()
        {
            ArabicName = "صنف",
            EnglishName = "Item",
            ReferenceCode = "REF-1",
            IsTaxExempted = isExempt
        };

    private static SaleItem Item(decimal price, decimal qty, int nights = 1, bool isExempt = false) =>
        new()
        {
            ItemNo = Guid.NewGuid().ToString("N"),
            Price = price,
            Quantity = qty,
            NoOfNights = nights,
            MainSalesClass = SalesClass(isExempt),
            MainSalesClassId = Guid.NewGuid()
        };

    private static SaleInvoice Invoice(decimal taxAmount, decimal discountAmount, params SaleItem[] items) =>
        new()
        {
            Id = 1,
            InvoiceNo = "INV-1",
            Currency = "SAR",
            IssueDate = new DateTime(2026, 8, 1),
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            SaleItems = items.ToList(),
            CustomerProfile = new CustomerProfile { ArabicName = "عميل", EnglishName = "Customer" }
        };

    [Fact]
    public void NormalInvoice_NoDiscount_MatchesFrontendFormulas()
    {
        // Frontend: item1 = 500 × 2 × 1 = 1000 → tax 150 → total 1150
        //           item2 = 50  × 2 × 1 = 100  → tax 15  → total 115
        //           net 1100, tax 165, total 1265
        var invoice = Invoice(
            taxAmount: 165m,
            discountAmount: 0m,
            Item(500m, 2m),
            Item(50m, 2m));

        Print1ServerMath.ItemSubtotal(invoice.SaleItems.ElementAt(0)).Should().Be(1000m);
        Print1ServerMath.ItemTax(invoice.SaleItems.ElementAt(0)).Should().Be(150m);
        Print1ServerMath.ItemTotal(invoice.SaleItems.ElementAt(0)).Should().Be(1150m);

        Print1ServerMath.ItemSubtotal(invoice.SaleItems.ElementAt(1)).Should().Be(100m);
        Print1ServerMath.ItemTax(invoice.SaleItems.ElementAt(1)).Should().Be(15m);
        Print1ServerMath.ItemTotal(invoice.SaleItems.ElementAt(1)).Should().Be(115m);

        Print1ServerMath.NetTotal(invoice).Should().Be(1100m);
        Print1ServerMath.TaxTotal(invoice).Should().Be(165m);
        Print1ServerMath.TotalPrice(invoice).Should().Be(1265m);
    }

    [Fact]
    public void Discount_IsAllocatedProportionally_LikeFrontend()
    {
        // Frontend: gross 200, discount 40 → per-item discount 20 → taxable base 80 each → tax 12 each
        //           net 160, tax 24, total 184
        var invoice = Invoice(
            taxAmount: 24m,
            discountAmount: 40m,
            Item(100m, 1m),
            Item(100m, 1m));

        Print1ServerMath.NetTotal(invoice).Should().Be(160m);
        Print1ServerMath.TaxTotal(invoice).Should().Be(24m);
        Print1ServerMath.TotalPrice(invoice).Should().Be(184m);
    }

    [Fact]
    public void TaxAmountZero_OnInvoice_ReturnsZeroTax_Early()
    {
        // Frontend getTaxAmount: if (invoice.taxAmount === 0) return 0 — even though items have tax.
        var invoice = Invoice(
            taxAmount: 0m,
            discountAmount: 0m,
            Item(100m, 1m),
            Item(100m, 1m));

        Print1ServerMath.TaxTotal(invoice).Should().Be(0m);
        Print1ServerMath.TotalPrice(invoice).Should().Be(200m);
    }

    [Fact]
    public void TaxExemptItems_HaveZeroItemAndInvoiceTax()
    {
        var invoice = Invoice(
            taxAmount: 150m, // persisted on the invoice for a non-exempt item
            discountAmount: 0m,
            Item(1000m, 1m, isExempt: true),
            Item(1000m, 1m, isExempt: false));

        Print1ServerMath.ItemTax(invoice.SaleItems.ElementAt(0)).Should().Be(0m);
        Print1ServerMath.ItemTax(invoice.SaleItems.ElementAt(1)).Should().Be(150m);
        // net 2000, tax 150
        Print1ServerMath.NetTotal(invoice).Should().Be(2000m);
        Print1ServerMath.TaxTotal(invoice).Should().Be(150m);
        Print1ServerMath.TotalPrice(invoice).Should().Be(2150m);
    }

    [Fact]
    public void NoOfNights_MultipliesTheSubtotal()
    {
        // Frontend subtotal = price × qty × noOfNights → 100 × 2 × 3 = 600
        var invoice = Invoice(
            taxAmount: 90m,
            discountAmount: 0m,
            Item(100m, 2m, nights: 3));

        Print1ServerMath.ItemSubtotal(invoice.SaleItems.Single()).Should().Be(600m);
        Print1ServerMath.ItemTax(invoice.SaleItems.Single()).Should().Be(90m);
        Print1ServerMath.NetTotal(invoice).Should().Be(600m);
        Print1ServerMath.TaxTotal(invoice).Should().Be(90m);
        Print1ServerMath.TotalPrice(invoice).Should().Be(690m);
    }

    [Fact]
    public void Rounding_IsTwoDecimals_AwayFromZero()
    {
        // 33.33 × 3 = 99.99 (no rounding drift); tax 99.99 × 0.15 = 14.9985 → 15.00
        var invoice = Invoice(
            taxAmount: 15m,
            discountAmount: 0m,
            Item(33.33m, 3m));

        Print1ServerMath.ItemSubtotal(invoice.SaleItems.Single()).Should().Be(99.99m);
        Print1ServerMath.ItemTax(invoice.SaleItems.Single()).Should().Be(15m);
        Print1ServerMath.TaxTotal(invoice).Should().Be(15m);
        Print1ServerMath.TotalPrice(invoice).Should().Be(114.99m);
    }

    #region Document smoke tests (QuestPDF generation)

    [Fact]
    public void SingleDocument_WithNineItems_GeneratesNonEmptyPdf_WithoutThrowing()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var invoice = Invoice(
            taxAmount: 135m,
            discountAmount: 0m,
            Enumerable.Range(0, 9).Select(i => Item(100m, 1m)).ToArray());
        var company = new CompanyDetails { ArabicName = "شركة", EnglishName = "Company", TaxNumber = "123456" };

        var bytes = new SalesInvoicePrint1ServerDocument(invoice, company).GeneratePdf();

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void MergedDocument_TwoInvoices_GeneratesNonEmptyPdf_WithoutThrowing()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var inv1 = Invoice(15m, 0m, Item(100m, 1m));
        var inv2 = Invoice(30m, 0m, Item(200m, 1m));
        var company = new CompanyDetails { ArabicName = "شركة", EnglishName = "Company", TaxNumber = "123456" };

        var bytes = new CombinedSalesInvoicesPrint1ServerDocument(
            new[] { inv1, inv2 },
            company,
            language: "ar").GeneratePdf();

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(1000);
    }

    /// <summary>
    /// Pagination is natural flow (no chunking): the items table splits across pages
    /// automatically, so this test only guards the merged-document smoke path and the
    /// totals/formula parity below.
    /// </summary>
    [Fact]
    public void NaturalFlow_ManyItems_SingleTable_GeneratesPdf_WithoutThrowing()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var items = Enumerable.Range(1, 50)
            .Select(i => Item(10m, 1m))
            .ToList();
        var inv = Invoice(500m, 75m, items.ToArray());
        var company = new CompanyDetails { ArabicName = "شركة", EnglishName = "Company", TaxNumber = "123456" };

        var bytes = new SalesInvoicePrint1ServerDocument(inv, company, language: "ar").GeneratePdf();

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(1000);
    }

    #endregion
}
