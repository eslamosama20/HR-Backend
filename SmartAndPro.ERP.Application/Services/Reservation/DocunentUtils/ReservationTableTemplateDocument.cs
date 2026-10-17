using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Globalization;
using System.Linq;
using ReservationPrintTableTemplateEntity = SmartAndPro.ERP.Domains.Entities.Reservation.ReservationPrintTableTemplate;

namespace SmartAndPro.ERP.Application.Services.Reservation.DocunentUtils;

public class ReservationTableTemplateDocument : IDocument
{
    private readonly List<Domains.Entities.Reservation.Reservation> _reservations;
    private readonly ReservationPrintTableTemplateEntity _template;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration? _pdfTemplateConfig;

    public ReservationTableTemplateDocument(
        List<Domains.Entities.Reservation.Reservation> reservations,
        ReservationPrintTableTemplateEntity template,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null)
    {
        _reservations = reservations;
        _template = template;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        try
        {
            container.Page(page =>
            {
                page.PageColor(Colors.White);
                page.Margin(20);
                page.Size(
                    _template.PageOrientation==PageOrientation.Landscape
                    ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Footer().Element(ComposeFooter);

                page.Content().Column(col =>
                {
                    // Reservations Table
                    col.Item().PaddingVertical(10).Table(table =>
                    {
                        var visibleColumns = _template.GetVisibleColumns();

                        // Define table columns based on template with proper width handling
                        table.ColumnsDefinition(columns =>
                        {
                            // Serial Number Column (Left side)
                            columns.ConstantColumn(30);

                            foreach (var column in visibleColumns)
                            {
                                if (column.Width.HasValue && column.Width.Value > 0)
                                {
                                    columns.ConstantColumn(column.Width.Value);
                                }
                                else
                                {
                                    // Use different relative sizes based on data type and column name
                                    var relativeSize = GetColumnRelativeSize(column);
                                    columns.RelativeColumn(relativeSize);
                                }
                            }
                        });

                        // Header Row with improved styling
                        table.Header(header =>
                        {
                            // Serial Header
                            header.Cell()
                                .Element(HeaderCellStyle)
                                .AlignMiddle()
                                .Text("م")
                                .Bold()
                                .FontSize(11)
                                .FontColor(Colors.White)
                                .AlignCenter();

                            foreach (var column in visibleColumns)
                            {
                                var alignment = GetAlignment(column.Alignment);

                                header.Cell()
                                    .Element(HeaderCellStyle)
                                    .AlignMiddle()
                                    .Text(column.DisplayName)
                                    .Bold()
                                    .FontSize(11)
                                    .FontColor(Colors.White)
                                    .AlignCenter();
                            }
                        });

                        // Data Rows with improved styling and proper alignment
                        var isAlternateRow = false;
                        foreach (var reservation in _reservations)
                        {
                            // Serial Data
                            table.Cell()
                                .Element(container => DataCellStyle(container, isAlternateRow))
                                .AlignMiddle()
                                .AlignCenter()
                                .Text((_reservations.IndexOf(reservation) + 1).ToString())
                                .FontSize(10)
                                .FontColor(Colors.Black);

                            foreach (var column in visibleColumns)
                            {
                                var cellValue = GetCellValue(reservation, column.ColumnName);
                                var alignment = GetAlignment(column.Alignment);
                                var formattedValue = FormatCellValue(cellValue, column.DataType);

                                var cellContainer = table.Cell()
                                    .Element(container => DataCellStyle(container, isAlternateRow))
                                    .AlignMiddle();

                                // Apply text alignment based on the column alignment
                                switch (alignment)
                                {
                                    case HorizontalAlignment.Center:
                                        cellContainer.AlignCenter();
                                        break;
                                    case HorizontalAlignment.Right:
                                        cellContainer.AlignRight();
                                        break;
                                    default:
                                        cellContainer.AlignLeft();
                                        break;
                                }

                                cellContainer.Text(formattedValue)
                                    .FontSize(10)
                                    .FontColor(Colors.Black);
                            }

                            isAlternateRow = !isAlternateRow;
                        }
                    });
                });
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            LogService.LogError(e);
            throw;
        }
    }

    private string GetCellValue(Domains.Entities.Reservation.Reservation reservation, string columnName)
    {
        return columnName.ToLower() switch
        {
            "referencenumber" => reservation.ReferenceNumber ?? string.Empty,
            "guestname" => reservation.GuestName ?? string.Empty,
            "guestphone" => reservation.GuestPhone ?? string.Empty,
            "nationality" => reservation.Nationality ?? string.Empty,
            "hotelname" => reservation.Hotel?.EnglishName ?? reservation.Hotel?.ArabicName ?? string.Empty,
            "clientcompanyname" => reservation.ReservationClientCompany?.EnglishName ?? reservation.ReservationClientCompany?.ArabicName ?? string.Empty,
            "roomtype" => GetRoomTypes(reservation),
            "arrivaldate" => reservation.ArrivalDate.ToString("dd/MM/yyyy"),
            "departuredate" => reservation.DepartureDate.ToString("dd/MM/yyyy"),
            "numberofnights" => reservation.NumberOfNights.ToString(),
            "reservationtype" => reservation.ReservationType.ToString(),
            "createdat" => reservation.CreatedAt.ToString("dd/MM/yyyy"),
            "totalcost" => CalculateTotalCost(reservation).ToString("F2"),
            "totalamount" => CalculateTotalAmount(reservation).ToString("F2"),
            "roomcost" => CalculateRoomCost(reservation).ToString("F2"),
            "roomamount" => CalculateRoomAmount(reservation).ToString("F2"),
            "servicecost" => CalculateServiceCost(reservation).ToString("F2"),
            "serviceamount" => CalculateServiceAmount(reservation).ToString("F2"),
            "totalrooms" => CalculateTotalRooms(reservation).ToString(),
            "totalservices" => CalculateTotalServices(reservation).ToString(),
            "averageroomrate" => CalculateAverageRoomRate(reservation).ToString("F2"),
            "totaltax" => CalculateTotalTax(reservation).ToString("F2"),
            "totaldiscount" => CalculateTotalDiscount(reservation).ToString("F2"),
            "netamount" => CalculateNetAmount(reservation).ToString("F2"),
            "roomrates" => GetRoomRates(reservation),
            _ => string.Empty
        };
    }

    private HorizontalAlignment GetAlignment(string? alignment)
    {
        return alignment?.ToLower() switch
        {
            "center" => HorizontalAlignment.Center,
            "right" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left
        };
    }

    #region Calculation Methods

    private decimal CalculateTotalCost(Domains.Entities.Reservation.Reservation reservation)
    {
        var roomCost = reservation.ReservationRoomEntries?
            .Sum(re => (re.NumberOfRooms * reservation.NumberOfNights * (re.RoomCost ?? 0m))
                      + (((re.RoomType?.PersonNo ?? 1) * re.NumberOfRooms) * reservation.NumberOfNights * (re.MealCost ?? 0m))
                      + (re.NumberOfRooms * reservation.NumberOfNights * (re.ViewCost ?? 0m))) ?? 0m;

        var serviceCost = reservation.ReservationServiceEntries?
            .Sum(se => se.Quantity * (se.CostPrice ?? 0m)) ?? 0m;

        return roomCost + serviceCost;
    }

    private decimal CalculateTotalAmount(Domains.Entities.Reservation.Reservation reservation)
    {
        var roomAmount = reservation.ReservationRoomEntries?
            .Sum(re => (re.NumberOfRooms * reservation.NumberOfNights * re.RoomRate)
                      + (((re.RoomType?.PersonNo ?? 1) * re.NumberOfRooms) * reservation.NumberOfNights * (re.MealRate ?? 0m))
                      + (re.NumberOfRooms * reservation.NumberOfNights * (re.ViewRate ?? 0m))) ?? 0m;

        var serviceAmount = reservation.ReservationServiceEntries?
            .Sum(se => se.Quantity * se.SalesPrice) ?? 0m;

        return roomAmount + serviceAmount;
    }

    private decimal CalculateRoomCost(Domains.Entities.Reservation.Reservation reservation)
    {
        return reservation.ReservationRoomEntries?
            .Sum(re => (re.NumberOfRooms * reservation.NumberOfNights * (re.RoomCost ?? 0m))
                      + (((re.RoomType?.PersonNo ?? 1) * re.NumberOfRooms) * reservation.NumberOfNights * (re.MealCost ?? 0m))
                      + (re.NumberOfRooms * reservation.NumberOfNights * (re.ViewCost ?? 0m))) ?? 0m;
    }

    private decimal CalculateRoomAmount(Domains.Entities.Reservation.Reservation reservation)
    {
        return reservation.ReservationRoomEntries?
            .Sum(re => (re.NumberOfRooms * reservation.NumberOfNights * re.RoomRate)
                      + (((re.RoomType?.PersonNo ?? 1) * re.NumberOfRooms) * reservation.NumberOfNights * (re.MealRate ?? 0m))
                      + (re.NumberOfRooms * reservation.NumberOfNights * (re.ViewRate ?? 0m))) ?? 0m;
    }

    private decimal CalculateServiceCost(Domains.Entities.Reservation.Reservation reservation)
    {
        return reservation.ReservationServiceEntries?
            .Sum(se => se.Quantity * (se.CostPrice ?? 0m)) ?? 0m;
    }

    private decimal CalculateServiceAmount(Domains.Entities.Reservation.Reservation reservation)
    {
        return reservation.ReservationServiceEntries?
            .Sum(se => se.Quantity * se.SalesPrice) ?? 0m;
    }

    private int CalculateTotalRooms(Domains.Entities.Reservation.Reservation reservation)
    {
        return reservation.ReservationRoomEntries?.Sum(re => re.NumberOfRooms) ?? 0;
    }

    private int CalculateTotalServices(Domains.Entities.Reservation.Reservation reservation)
    {
        return (int)(reservation.ReservationServiceEntries?.Sum(se => se.Quantity) ?? 0);
    }

    private decimal CalculateAverageRoomRate(Domains.Entities.Reservation.Reservation reservation)
    {
        var totalRooms = CalculateTotalRooms(reservation);
        if (totalRooms == 0) return 0;

        var totalRoomRate = reservation.ReservationRoomEntries?
            .Sum(re => re.NumberOfRooms * re.RoomRate) ?? 0m;

        return totalRoomRate / totalRooms;
    }

    private decimal CalculateTotalTax(Domains.Entities.Reservation.Reservation reservation)
    {
        // Calculate 14% tax on total amount
        var totalAmount = CalculateTotalAmount(reservation);
        return totalAmount * 0.15m;
    }

    private decimal CalculateTotalDiscount(Domains.Entities.Reservation.Reservation reservation)
    {
        // This would need to be calculated based on your discount logic
        // For now, returning 0 as placeholder
        return 0;
    }

    private decimal CalculateNetAmount(Domains.Entities.Reservation.Reservation reservation)
    {
        return CalculateTotalAmount(reservation) + CalculateTotalTax(reservation) - CalculateTotalDiscount(reservation);
    }

    #endregion

    private string GetRoomTypes(Domains.Entities.Reservation.Reservation reservation)
    {
        if (reservation.ReservationRoomEntries == null || !reservation.ReservationRoomEntries.Any())
            return string.Empty;

        var roomTypes = reservation.ReservationRoomEntries
            .Where(re => re.RoomType != null)
            .Select(re => re.RoomType?.EnglishName ?? re.RoomType?.ArabicName ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        return string.Join(", ", roomTypes);
    }

    private string GetRoomRates(Domains.Entities.Reservation.Reservation reservation)
    {
        if (reservation.ReservationRoomEntries == null || !reservation.ReservationRoomEntries.Any())
            return string.Empty;

        // Step 1: Get the exact same distinct room types as GetRoomTypes
        var distinctRoomTypes = reservation.ReservationRoomEntries
            .Where(re => re.RoomType != null)
            .Select(re => re.RoomType?.EnglishName ?? re.RoomType?.ArabicName ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        // Step 2: For each distinct type, find the corresponding rate (RoomRate property)
        var rates = new List<string>();
        foreach (var typeName in distinctRoomTypes)
        {
            var entry = reservation.ReservationRoomEntries.FirstOrDefault(re => 
                (re.RoomType?.EnglishName == typeName || re.RoomType?.ArabicName == typeName));
            
            if (entry != null)
            {
                rates.Add(entry.RoomRate.ToString("F2"));
            }
        }

        return string.Join(", ", rates);
    }

    private int GetColumnRelativeSize(PrintTableColumn column)
    {
        // Assign relative sizes based on column name and data type
        return column.ColumnName.ToLower() switch
        {
            "referencenumber" => 2,
            "guestname" => 3,
            "guestphone" => 2,
            "nationality" => 2,
            "hotelname" => 3,
            "clientcompanyname" => 3,
            "roomtype" => 3,
            "arrivaldate" or "departuredate" or "createdat" => 2,
            "numberofnights" or "totalrooms" or "totalservices" => 1,
            "reservationtype" => 2,
            "totalcost" or "totalamount" or "roomcost" or "roomamount" or
            "servicecost" or "serviceamount" or "averageroomrate" or
            "totaltax" or "totaldiscount" or "netamount" => 2,
            _ => 1
        };
    }

    private string FormatCellValue(string value, string dataType)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        return dataType.ToLower() switch
        {
            "currency" or "decimal" or "money" => decimal.TryParse(value, out var decimalValue)
                ? $"{decimalValue:N2} ر.س" : value,
            "integer" or "int" => int.TryParse(value, out var intValue)
                ? intValue.ToString("N0") : value,
            "date" => DateTime.TryParse(value, out var dateValue)
                ? dateValue.ToString("dd/MM/yyyy") : value,
            "datetime" => DateTime.TryParse(value, out var dateTimeValue)
                ? dateTimeValue.ToString("dd/MM/yyyy HH:mm") : value,
            "percentage" => decimal.TryParse(value, out var percentValue)
                ? percentValue.ToString("P2") : value,
            _ => value
        };
    }

    private IContainer CellStyle(IContainer container)
    {
        return container.Padding(2);
    }

    private IContainer BorderedCellStyle(IContainer container)
    {
        return container.Border(1).Padding(2);
    }

    private IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Background(Colors.Blue.Darken2)
            .Padding(8);
    }

    private void ComposeHeader(IContainer container)
    {
        var baseTextStyle = TextStyle.Default.FontSize(10);

        container.Row(row =>
        {
            // Left Column: English Details
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(_companyDetails.EnglishName).Style(baseTextStyle).Bold().AlignLeft();
                
                var primaryPhone = _companyDetails.Phones.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber ?? "";
                if (!string.IsNullOrEmpty(primaryPhone))
                {
                    col.Item().Text($"Phone: {primaryPhone}").Style(baseTextStyle).AlignLeft();
                }

                if (_companyDetails.Address != null)
                {
                    col.Item().Text($"City: {_companyDetails.Address.City}").Style(baseTextStyle).AlignLeft();
                    col.Item().Text($"Address: {_companyDetails.Address.Street}").Style(baseTextStyle).AlignLeft();
                }
            });

            // Center Column: Logo and Template Name
            row.RelativeItem().Column(col =>
            {
                if (_companyDetails.Logo != null)
                {
                    col.Item().AlignCenter().MaxHeight(60).Image(_companyDetails.Logo);
                }
                else
                {
                    // Fallback if no logo
                     col.Item().Padding(10);
                }
                
                col.Item().AlignCenter().Text(_template.Name).FontSize(14).Bold();
                if (!string.IsNullOrEmpty(_template.Description))
                {
                     col.Item().AlignCenter().Text(_template.Description).FontSize(10);
                }
            });

            // Right Column: Arabic Details
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(_companyDetails.ArabicName).Style(baseTextStyle).Bold().AlignRight();

                var primaryPhone = _companyDetails.Phones.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber ?? "";
                if (!string.IsNullOrEmpty(primaryPhone))
                {
                    col.Item().Text($"رقم الهاتف: {primaryPhone}").Style(baseTextStyle).AlignRight();
                }

                if (_companyDetails.ArabicAddress != null)
                {
                    col.Item().Text($"المدينة: {_companyDetails.ArabicAddress.City}").Style(baseTextStyle).AlignRight();
                    col.Item().Text($"العنوان : {_companyDetails.ArabicAddress.Street}").Style(baseTextStyle).AlignRight();
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        var parts = new List<string>();

        if (_companyDetails.ArabicAddress != null)
        {
            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Country)) parts.Add(_companyDetails.ArabicAddress.Country);
            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City)) parts.Add(_companyDetails.ArabicAddress.City);
            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street)) parts.Add(_companyDetails.ArabicAddress.Street);
            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.ZipCode)) parts.Add($"الرقم البريدي {_companyDetails.ArabicAddress.ZipCode}");
        }

        var primaryPhone = _companyDetails.Phones.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber;
        if (!string.IsNullOrEmpty(primaryPhone))
        {
            parts.Add($"جوال {primaryPhone}");
        }

        // Example hardcoded part from request "فرع شارع المطار 8843" if dynamic data doesn't cover it, 
        // but for now relying on existing dynamic data as per plan. Use BranchDetails if available?
        // _companyDetails.BranchDetails might have it, but for simplicity of request, we use main address.

        var footerText = string.Join(" - ", parts);

        container.Column(col =>
        {
            // Separator Line
            col.Item().PaddingTop(5).Height(1).Background(Colors.Grey.Lighten2);
            
            // Footer Text
            col.Item().PaddingTop(5).AlignCenter().Text(footerText).FontSize(10);
            
            // Page Number
            col.Item().PaddingTop(3).AlignCenter().Text(x =>
            {
                x.DefaultTextStyle(ts => ts.FontSize(8));
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }

    private IContainer DataCellStyle(IContainer container, bool isAlternateRow)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(isAlternateRow ? Colors.Grey.Lighten4 : Colors.White)
            .Padding(8);
    }
}
