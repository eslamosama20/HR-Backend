using System;
using System.Collections.Generic;
using Xunit;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers.Invoice;

public class SalesInvoicePrint1ServerDocumentRenderTests
{
    private static SaleInvoice BuildInvoice(int itemCount = 1)
    {
        var inv = new SaleInvoice { Id = 4, InvoiceNo = "Inv-4", IssueDate = new DateTime(2026, 7, 9), DueDate = new DateTime(2026, 7, 9), DiscountAmount = 0, TaxAmount = 72.6m };
        inv.SaleItems = new List<SaleItem>();
        for (var i = 0; i < itemCount; i++)
        {
            inv.SaleItems.Add(new SaleItem { Price = 22m, Quantity = 22m, NoOfNights = 1, Name = "test", MainSalesClass = new MainSalesClass { ArabicName = "اختبار", EnglishName = "test", IsTaxExempted = false } });
        }
        inv.CustomerProfile = new CustomerProfile { ArabicName = "عميل", EnglishName = "test", TaxNumber = "310123456700003" };
        return inv;
    }

    private static CompanyDetails BuildCompany() => new()
    {
        ArabicName = "شركة كيان", EnglishName = "test-company", TaxNumber = "310123456700003",
        BuildingNumber = "1", SubNumber = "2", District = "الرياض",
        Address = new Address("st", "Riyadh", "SA", "12211"),
        ArabicAddress = new Address("شارع", "الرياض", "السعودية", "12211"),
        UnifiedNumber = "3",
        Policies = new List<CompanyPolicy> { new CompanyPolicy { PolicyParagraph = "لا ترد المبالغ." } },
        BankAccounts = new List<BankAccount> { new BankAccount { AccountName = "asdasdasd", BankName = "sadasdadasd", BankAccountNumber = "21313412313", IBAN = "213123141231", SwiftCode = "213124123" } }
    };

    [Fact]
    public void GeneratePdf_ArabicAndEnglish_Smoke()
    {
        var inv = BuildInvoice();
        var company = BuildCompany();

        QuestPDF.Settings.License = LicenseType.Community;
        var doc = new SalesInvoicePrint1ServerDocument(inv, company, language: "ar", sellerName: "admin admin", timeZone: "Asia/Riyadh");
        var docEn = new SalesInvoicePrint1ServerDocument(inv, company, language: "en", sellerName: "admin admin", timeZone: "Asia/Riyadh");
        try
        {
            var bytes = doc.GeneratePdf();
            Assert.True(bytes.Length > 1000);
            var bytesEn = docEn.GeneratePdf();
            Assert.True(bytesEn.Length > 1000);
        }
        catch (Exception ex)
        {
            throw new Exception("REPRO_EXCEPTION: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    /// <summary>
    /// orientation=portrait|landscape (user-forced) wins over auto-detect; omitted
    /// orientation keeps the frontend-mirroring auto rule (items &gt; 8 → landscape).
    /// QuestPDF writes /MediaBox [0 0 W H] in points (rounded integers) — assert on bytes.
    /// </summary>
    [Theory]
    [InlineData("portrait", 1, 595, 842)]
    [InlineData("landscape", 1, 842, 595)]
    [InlineData("PORTRAIT", 1, 595, 842)] // case-insensitive
    [InlineData("landscape", 12, 842, 595)] // forced landscape beats auto-detect
    [InlineData("portrait", 12, 595, 842)] // forced portrait beats auto-detect
    [InlineData(null, 12, 842, 595)] // auto: items > 8 → landscape
    [InlineData(null, 1, 595, 842)] // auto: 1 item → portrait
    [InlineData("sideways", 1, 595, 842)] // invalid → auto (portrait)
    public void GeneratePdf_Orientation_SetsPageSize(string? orientation, int itemCount, int width, int height)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var doc = new SalesInvoicePrint1ServerDocument(
            BuildInvoice(itemCount),
            BuildCompany(),
            language: "ar",
            sellerName: "admin admin",
            timeZone: "Asia/Riyadh",
            orientation: orientation);

        var bytes = doc.GeneratePdf();
        Assert.True(bytes.Length > 1000);
        var ascii = System.Text.Encoding.ASCII.GetString(bytes);
        Assert.Contains($"/MediaBox [0 0 {width} {height}]", ascii);
    }

    /// <summary>
    /// NATURAL FLOW pagination: the items table splits across pages automatically — no fixed
    /// per-page limits. The synthetic single-line "test" rows flow densely: 50 items → 3
    /// landscape pages (page 1 ≈9 + continuation ≈20-21 each; the real Arabic seed data with
    /// longer names gives 4), 17 items → 2, small invoices stay on one page.
    /// Page count = /MediaBox occurrences (one per page).
    /// </summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(8, 1)]
    [InlineData(9, 2)]
    [InlineData(17, 2)]
    [InlineData(50, 3)]
    public void GeneratePdf_NaturalFlow_PageCount(int itemCount, int expectedPages)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var doc = new SalesInvoicePrint1ServerDocument(
            BuildInvoice(itemCount),
            BuildCompany(),
            language: "ar",
            sellerName: "admin admin",
            timeZone: "Asia/Riyadh");

        var bytes = doc.GeneratePdf();
        var ascii = System.Text.Encoding.ASCII.GetString(bytes);
        var pageCount = ascii.Split("/MediaBox", StringSplitOptions.None).Length - 1;

        Assert.Equal(expectedPages, pageCount);
    }

    /// <summary>
    /// Page numbering must render on EVERY page EVEN when the company has no address — the
    /// footer no longer early-returns on an empty address, so the page-number line
    /// (CurrentPageNumber/TotalPages) always executes. QuestPDF encodes text via subset-font
    /// glyph codes (not readable ASCII), so the assertion is structural: a company without any
    /// address must render successfully with the expected page count (any exception in the
    /// page-number path would fail here). Presence of the rendered text is verified at the
    /// API level via pdftotext (see /home/mohamed/e2e-out/sv-pageno-*.pdf).
    /// </summary>
    [Fact]
    public void GeneratePdf_CompanyWithoutAddress_Renders()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var company = BuildCompany();
        company.Address = null; // no English address
        company.ArabicAddress = null; // no Arabic address
        var doc = new SalesInvoicePrint1ServerDocument(
            BuildInvoice(1),
            company,
            language: "en",
            sellerName: "admin admin",
            timeZone: "Asia/Riyadh");

        var bytes = doc.GeneratePdf();
        Assert.True(bytes.Length > 1000);
        var ascii = System.Text.Encoding.ASCII.GetString(bytes);
        var pageCount = ascii.Split("/MediaBox", StringSplitOptions.None).Length - 1;
        Assert.Equal(1, pageCount);
    }
}
