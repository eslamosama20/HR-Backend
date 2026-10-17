using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Application.Utility;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices.DocumentUtils;

public class HotelAllotmentInvoiceDocument : IDocument
{
    private readonly HotelAllotmentInvoice _invoice;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly bool _isLandscape;
    private readonly string? _sellerName;
    private readonly string? _timeZone;

    public HotelAllotmentInvoiceDocument(
        HotelAllotmentInvoice invoice,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        bool isLandscape = false,
        string? sellerName = null,
        string? timeZone = null)
    {
        _invoice = invoice;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
        _isLandscape = isLandscape;
        _sellerName = sellerName;
        _timeZone = timeZone;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(_isLandscape ? PageSizes.A4.Landscape() : PageSizes.A4);
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

            page.Header().Element(c => ComposeHeader(c, _companyDetails));
            page.Footer().Element(c => ComposeFooter(c, _companyDetails));

            page.Content().Column(col =>
            {
                col.Item().Element(c => ComposeInvoiceInfo(c, _invoice, _sellerName, _timeZone));
                col.Item().PaddingTop(4).Element(c => ComposeRoomCostTable(c, _invoice));
                col.Item().PaddingTop(4).Element(c => ComposeTotals(c, _invoice));
                col.Item().PaddingTop(8).Element(c => ComposeSignatures(c, _invoice, _companyDetails));
            });
        });
    }

    internal static void ComposeHeader(IContainer container, CompanyDetails companyDetails)
    {
        container.PaddingBottom(8).Row(row =>
        {
            row.RelativeItem(1.5f).AlignLeft().Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(companyDetails?.EnglishName))
                {
                    col.Item().AlignLeft().Text(companyDetails.EnglishName).SemiBold().FontSize(12);
                }
                if (companyDetails?.Address != null)
                {
                    if (!string.IsNullOrWhiteSpace(companyDetails.Address.Street))
                    {
                        col.Item().PaddingTop(2).AlignLeft().Text(companyDetails.Address.Street).FontSize(10);
                    }
                    if (!string.IsNullOrWhiteSpace(companyDetails.Address.City))
                    {
                        col.Item().PaddingTop(2).AlignLeft().Text(companyDetails.Address.City).FontSize(10);
                    }
                }
                var primaryPhoneEng = companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                if (primaryPhoneEng != null && !string.IsNullOrWhiteSpace(primaryPhoneEng.PhoneNumber))
                {
                    col.Item().PaddingTop(2).AlignLeft().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                    {
                        t.Span("Phone Number: ");
                        t.Span(primaryPhoneEng.PhoneNumber);
                    });
                }
                if (!string.IsNullOrWhiteSpace(companyDetails?.TaxNumber))
                {
                    col.Item().PaddingTop(2).AlignLeft().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                    {
                        t.Span("Tax ID: ");
                        t.Span(companyDetails.TaxNumber);
                    });
                }
            });

            row.RelativeItem(1).AlignCenter().AlignMiddle().Column(col =>
            {
                if (companyDetails?.Logo != null && companyDetails.Logo.Length > 0)
                {
                    col.Item().AlignCenter().MaxHeight(80).MaxWidth(120).Image(companyDetails.Logo).FitArea();
                }
            });

            row.RelativeItem(1.5f).AlignRight().Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(companyDetails?.ArabicName))
                {
                    col.Item().AlignRight().Text(companyDetails.ArabicName).SemiBold().FontSize(12);
                }
                if (companyDetails?.ArabicAddress != null)
                {
                    if (!string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.Street))
                    {
                        col.Item().PaddingTop(2).AlignRight().Text(companyDetails.ArabicAddress.Street).FontSize(10);
                    }
                    if (!string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.City))
                    {
                        col.Item().PaddingTop(2).AlignRight().Text(companyDetails.ArabicAddress.City).FontSize(10);
                    }
                }
                var primaryPhone = companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                {
                    col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                    {
                        t.Span("رقم الهاتف: ");
                        t.Span(primaryPhone.PhoneNumber);
                    });
                }
                if (!string.IsNullOrWhiteSpace(companyDetails?.TaxNumber))
                {
                    col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                    {
                        t.Span("الرقم الضريبي: ");
                        t.Span(companyDetails.TaxNumber);
                    });
                }
            });
        });
    }

    internal static void ComposeFooter(IContainer container, CompanyDetails companyDetails)
    {
        container.BorderTop(1).BorderColor("#E2E8F0").PaddingTop(5).AlignCenter().DefaultTextStyle(x => x.FontSize(9)).Text(t =>
        {
            var parts = new List<string>();

            if (companyDetails?.ArabicAddress != null)
            {
                var addressParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.Country))
                    addressParts.Add(companyDetails.ArabicAddress.Country);
                if (!string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.City))
                    addressParts.Add(companyDetails.ArabicAddress.City);
                if (!string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.Street))
                    addressParts.Add(companyDetails.ArabicAddress.Street);
                if (addressParts.Count > 0)
                    parts.Add(string.Join(" - ", addressParts));
            }

            if (companyDetails?.ArabicAddress != null && !string.IsNullOrWhiteSpace(companyDetails.ArabicAddress.ZipCode))
                parts.Add($"الرقم البريدي: {companyDetails.ArabicAddress.ZipCode}");

            var primaryPhone = companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
            if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                parts.Add($"رقم الهاتف: {primaryPhone.PhoneNumber}");

            t.Span(string.Join(" | ", parts));
        });
    }

    internal static void ComposeInvoiceInfo(IContainer container, HotelAllotmentInvoice invoice, string? sellerName, string? timeZone)
    {
        container.Column(col =>
        {
            col.Item().Border(1).BorderColor("#E2E8F0").Padding(8).Row(r =>
            {
                r.RelativeItem().AlignLeft().Column(c =>
                {
                    c.Item().Text(t =>
                    {
                        t.Span("رقم الفاتورة ").SemiBold();
                        t.Span(invoice.InvoiceNo);
                    });
                    if (!string.IsNullOrWhiteSpace(sellerName))
                    {
                        c.Item().PaddingTop(4).Text(t =>
                        {
                            t.Span("البائع: ").SemiBold();
                            t.Span(sellerName);
                        });
                    }
                });

                r.RelativeItem().AlignCenter().Text("فاتورة حجوزات فنادق").SemiBold().FontSize(14);

                r.RelativeItem().AlignRight().Text(t =>
                {
                    t.Span("التاريخ: ").SemiBold();
                    var issueDate = timeZone != null
                        ? TimeZoneUtils.ApplyTimeZone(timeZone, invoice.IssueDate)
                        : invoice.IssueDate;
                    t.Span(issueDate.ToString("dd/MM/yyyy"));
                });
            });

            col.Item().PaddingTop(2).PaddingBottom(8).Row(r =>
            {
                if (invoice.MainAccount != null)
                {
                    r.ConstantItem(200).AlignLeft().Text(t =>
                    {
                        t.Span("الحساب: ").SemiBold();
                        var accountName = invoice.MainAccount.ArabicName ?? invoice.MainAccount.EnglishName ?? "";
                        t.Span(accountName);
                    });
                }

                r.RelativeItem();

                if (invoice.Hotel != null)
                {
                    r.ConstantItem(200).AlignRight().Text(t =>
                    {
                        t.Span("الفندق: ").SemiBold();
                        var hotelName = invoice.Hotel.ArabicName ?? invoice.Hotel.EnglishName ?? "";
                        t.Span(hotelName);
                    });
                }
            });
        });
    }

    internal static void ComposeRoomCostTable(IContainer container, HotelAllotmentInvoice invoice)
    {
        var dayRows = BuildDayRows(invoice);

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);
                columns.ConstantColumn(50);
                columns.ConstantColumn(65);
                columns.ConstantColumn(50);
                columns.ConstantColumn(50);
                columns.ConstantColumn(65);
                columns.ConstantColumn(55);
                columns.ConstantColumn(50);
                columns.RelativeColumn(2f);
                columns.ConstantColumn(25);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).AlignCenter().Text("الإجمالي");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("الضريبة");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("قبل الضريبة");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("الغرف");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("اليوم");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("التاريخ");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("الإطلالة");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("الوجبة");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("نوع الغرفة");
                header.Cell().Element(HeaderStyle).AlignCenter().Text("م");
            });

            foreach (var row in dayRows)
            {
                var dayName = ArabicDayNames.TryGetValue(row.DayOfWeek, out var dn) ? dn : row.DayOfWeek.ToString();

                table.Cell().Element(DataStyle).AlignCenter().Text(row.Total.ToString("0.##"));
                table.Cell().Element(DataStyle).AlignCenter().Text(row.Tax.ToString("0.##"));
                table.Cell().Element(DataStyle).AlignCenter().Text(row.CostBeforeTax.ToString("0.##"));
                table.Cell().Element(DataStyle).AlignCenter().Text(row.Rooms.ToString());
                table.Cell().Element(DataStyle).AlignCenter().Text(dayName);
                table.Cell().Element(DataStyle).AlignCenter().Text(row.Date.ToString("dd/MM/yyyy"));
                table.Cell().Element(c => AlignByDirection(DataStylePadded(c), row.ViewName)).Text(row.ViewName ?? "-");
                table.Cell().Element(c => AlignByDirection(DataStyle(c), row.MealPlanName)).Text(row.MealPlanName ?? "-");
                table.Cell().Element(c => AlignByDirection(DataStylePadded(c), row.RoomTypeName)).Text(row.RoomTypeName);
                table.Cell().Element(DataStyle).AlignCenter().Text(row.Index.ToString());
            }
        });
    }

    internal static void ComposeTotals(IContainer container, HotelAllotmentInvoice invoice)
    {
        container.AlignLeft().Width(160).PaddingTop(6).Table(t =>
        {
            t.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.RelativeColumn(1);
            });

            TotRow(t, "قبل الضريبة", invoice.TotalCostBeforeVAT, false);

            TotRow(t, "ضريبة القيمة المضافة", invoice.TaxAmount, false);

            if (invoice.DiscountAmount > 0)
            {
                TotRow(t, "الخصم", invoice.DiscountAmount, false);
            }

            TotRow(t, "الإجمالي العام", invoice.NetPrice, true);
        });
    }

    internal static void ComposeSignatures(IContainer container, HotelAllotmentInvoice invoice, CompanyDetails companyDetails)
    {
        container.PaddingTop(30).Row(r =>
        {
            r.RelativeItem(3).PaddingLeft(0).PaddingRight(80).Column(c =>
            {
                var hotelName = invoice.Hotel?.ArabicName ?? invoice.Hotel?.EnglishName ?? "";
                if (!string.IsNullOrWhiteSpace(hotelName))
                {
                    var isArabic = HasArabic(hotelName);
                    if (isArabic)
                    {
                        c.Item().AlignRight().Text(t => t.Span(hotelName).FontSize(10));
                        c.Item().PaddingTop(5).AlignRight().Text(t => t.Span("................................................................................ :التوقيع").FontSize(8));
                    }
                    else
                    {
                        c.Item().AlignLeft().Text(t => t.Span(hotelName).FontSize(10));
                        c.Item().PaddingTop(5).AlignLeft().Text(t => t.Span("................................................................................ :التوقيع").FontSize(8));
                    }
                }
            });

            r.RelativeItem(3).PaddingRight(0).PaddingLeft(80).Column(c =>
            {
                var companyName = companyDetails.ArabicName ?? companyDetails.EnglishName ?? "";
                c.Item().AlignRight().Text(t => t.Span(companyName).FontSize(10));
                c.Item().PaddingTop(5).AlignRight().Text(t => t.Span("................................................................................ :التوقيع").FontSize(8));

                if (companyDetails.Stamp != null && companyDetails.Stamp.Length > 0)
                {
                    c.Item().PaddingTop(10).AlignRight().Height(100).Width(100).Image(companyDetails.Stamp).FitArea();
                }
            });
        });
    }

    internal static void TotRow(TableDescriptor t, string label, decimal value, bool bold)
    {
        var valueCell = t.Cell().Element(TotalValueStyle).AlignCenter().Text(value.ToString("0.##"));
        if (bold) valueCell.SemiBold();
        t.Cell().Element(TotalLabelStyle).AlignRight().Text(label);
    }

    internal static List<RoomDayRow> BuildDayRows(HotelAllotmentInvoice invoice)
    {
        var rows = new List<RoomDayRow>();
        var index = 1;

        foreach (var room in invoice.HotelAllotmentRooms)
        {
            for (var date = room.FromDate; date <= room.ToDate; date = date.AddDays(1))
            {
                var dayOfWeek = date.DayOfWeek;
                var price = room.PricePerDay.TryGetValue(dayOfWeek, out var p) ? p : 0;
                var roomsCount = room.RoomsPerDay.TryGetValue(dayOfWeek, out var r) ? r : 0;
                var costBeforeTax = (price + room.MealPlanCost + room.ViewCost) * roomsCount;
                if (room.Discount > 0)
                    costBeforeTax -= costBeforeTax * (room.Discount / 100);
                var tax = Math.Round(costBeforeTax * room.Tax / 100, 2);
                var total = costBeforeTax + tax;

                rows.Add(new RoomDayRow
                {
                    Index = index++,
                    RoomTypeName = room.RoomType?.ArabicName ?? room.RoomType?.EnglishName ?? "",
                    MealPlanName = room.MealPlan?.ArabicName ?? room.MealPlan?.EnglishName,
                    ViewName = room.View?.ArabicName ?? room.View?.EnglishName,
                    Date = date,
                    DayOfWeek = dayOfWeek,
                    Rooms = roomsCount,
                    CostBeforeTax = costBeforeTax,
                    Tax = tax,
                    Total = total
                });
            }
        }

        return rows;
    }

    internal static readonly Dictionary<DayOfWeek, string> ArabicDayNames = new()
    {
        [DayOfWeek.Sunday] = "الأحد",
        [DayOfWeek.Monday] = "الإثنين",
        [DayOfWeek.Tuesday] = "الثلاثاء",
        [DayOfWeek.Wednesday] = "الأربعاء",
        [DayOfWeek.Thursday] = "الخميس",
        [DayOfWeek.Friday] = "الجمعة",
        [DayOfWeek.Saturday] = "السبت"
    };

    internal class RoomDayRow
    {
        public int Index { get; set; }
        public string RoomTypeName { get; set; } = "";
        public string? MealPlanName { get; set; }
        public string? ViewName { get; set; }
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int Rooms { get; set; }
        public decimal CostBeforeTax { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }

    internal static IContainer HeaderStyle(IContainer c) => c.Background("#fff5cc").Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#000000"));
    internal static IContainer DataStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(2);
    internal static IContainer DataStylePadded(IContainer c) => c.Border(1).BorderColor("#E2E8F0").PaddingLeft(5).PaddingRight(5).PaddingVertical(2);
    internal static IContainer TotalLabelStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.FontSize(9));
    internal static IContainer TotalValueStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.FontSize(9));

    internal static bool HasArabic(string? text) =>
        !string.IsNullOrWhiteSpace(text) && text.Any(ch => ch >= 0x0600 && ch <= 0x06FF);

    internal static IContainer AlignByDirection(IContainer container, string? text) =>
        HasArabic(text) ? container.AlignRight() : container.AlignLeft();
}
