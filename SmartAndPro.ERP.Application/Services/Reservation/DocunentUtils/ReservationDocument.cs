using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.SharedKernel.Utilities;
using ZXing.QrCode.Internal;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace SmartAndPro.ERP.Application.Services.Reservation.DocunentUtils;

public class ReservationDocument : IDocument
{
    private readonly Domains.Entities.Reservation.Reservation _reservation;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly bool _showStamp;
    private readonly bool _showCompanySignature;
    private readonly bool _showCustomerSignature;
    public ReservationDocument(Domains.Entities.Reservation.Reservation reservation, CompanyDetails companyDetails, Domains.Entities.PdfTemplateConfiguration pdfTemplateConfig = null, bool showStamp = true, bool showCompanySignature = true, bool showCustomerSignature = true)
    {
        _reservation = reservation;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig;
        _showStamp = showStamp;
        _showCompanySignature = showCompanySignature;
        _showCustomerSignature = showCustomerSignature;
    }
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        try
        {
            var accommodationCharge =
                _reservation.ReservationRoomEntries?
                    .Sum(re =>
                        (re.NumberOfRooms * _reservation.NumberOfNights * re.RoomRate)
                        + (((re.RoomType?.PersonNo ?? 1) * re.NumberOfRooms) * _reservation.NumberOfNights * (re.MealRate ?? 0))
                        + (re.NumberOfRooms * _reservation.NumberOfNights * re.ViewRate ?? 0)
                    ) ?? 0m;
            var serviceCharge = _reservation.ReservationServiceEntries?
                .Sum(re =>
                    (re.Quantity * re.SalesPrice)
                ) ?? 0m;
            var netCharge = accommodationCharge + serviceCharge;
            container.Page(page =>
            {
                page.PageColor(Colors.White);
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                // Hardcoded Header - Arabic details on right, Logo in center, English details on left (mirrors SalesInvoiceDocument.cs:55-142)
                page.Header().Element(headerContainer =>
                {
                    headerContainer.PaddingBottom(8).Row(row =>
                    {
                        // Column 1: English Company Details (Left side)
                        row.RelativeItem(1.5f).AlignLeft().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.EnglishName))
                            {
                                col.Item().AlignLeft().Text(_companyDetails.EnglishName).SemiBold().FontSize(12);
                            }
                            if (_companyDetails?.Address != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.Address.Street))
                                {
                                    col.Item().PaddingTop(2).AlignLeft().Text(_companyDetails.Address.Street).FontSize(10);
                                }
                                if (!string.IsNullOrWhiteSpace(_companyDetails.Address.City))
                                {
                                    col.Item().PaddingTop(2).AlignLeft().Text(_companyDetails.Address.City).FontSize(10);
                                }
                            }
                            var primaryPhoneEng = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhoneEng != null && !string.IsNullOrWhiteSpace(primaryPhoneEng.PhoneNumber))
                            {
                                col.Item().PaddingTop(2).AlignLeft().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("Phone Number: ");
                                    t.Span(primaryPhoneEng.PhoneNumber);
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                            {
                                col.Item().PaddingTop(2).AlignLeft().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("Tax ID: ");
                                    t.Span(_companyDetails.TaxNumber);
                                });
                            }
                        });

                        // Column 2: Logo (Center)
                        row.RelativeItem(1).AlignCenter().AlignMiddle().Column(col =>
                        {
                            if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                            {
                                col.Item().AlignCenter().MaxHeight(80).MaxWidth(120).Image(_companyDetails.Logo).FitArea();
                            }
                        });

                        // Column 3: Arabic Company Details (Right side)
                        row.RelativeItem(1.5f).AlignRight().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.ArabicName))
                            {
                                col.Item().AlignRight().Text(_companyDetails.ArabicName).SemiBold().FontSize(12);
                            }
                            if (_companyDetails?.ArabicAddress != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                                {
                                    col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.Street).FontSize(10);
                                }
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                                {
                                    col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.City).FontSize(10);
                                }
                            }
                            var primaryPhone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                            {
                                col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("رقم الهاتف: ");
                                    t.Span(primaryPhone.PhoneNumber);
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                            {
                                col.Item().PaddingTop(2).AlignRight().DefaultTextStyle(x => x.FontSize(10)).Text(t =>
                                {
                                    t.Span("الرقم الضريبي: ");
                                    t.Span(_companyDetails.TaxNumber);
                                });
                            }
                        });
                    });
                });

                // Hardcoded Footer - Arabic address bar (mirrors SalesInvoiceDocument.cs:145-190)
                page.Footer().Element(footerContainer =>
                {
                    footerContainer.BorderTop(1).BorderColor("#E2E8F0").PaddingTop(5).AlignCenter().DefaultTextStyle(x => x.FontSize(9)).Text(t =>
                    {
                        var parts = new List<string>();

                        if (_companyDetails?.ArabicAddress != null)
                        {
                            var addressParts = new List<string>();

                            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Country))
                            {
                                addressParts.Add(_companyDetails.ArabicAddress.Country);
                            }
                            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                            {
                                addressParts.Add(_companyDetails.ArabicAddress.City);
                            }
                            if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                            {
                                addressParts.Add(_companyDetails.ArabicAddress.Street);
                            }

                            if (addressParts.Count > 0)
                            {
                                parts.Add(string.Join(" - ", addressParts));
                            }
                        }

                        if (_companyDetails?.ArabicAddress != null && !string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.ZipCode))
                        {
                            parts.Add($"الرقم البريدي: {_companyDetails.ArabicAddress.ZipCode}");
                        }

                        var primaryPhone = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                        if (primaryPhone != null && !string.IsNullOrWhiteSpace(primaryPhone.PhoneNumber))
                        {
                            parts.Add($"رقم الهاتف: {primaryPhone.PhoneNumber}");
                        }

                        t.Span(string.Join(" | ", parts));
                    });
                });

                page.Content().Column(col =>
                {
                    if (_reservation.IsCancelled) { col.Item().Element(RenderCanceledBanner); }

                    col.Item().BorderBottom(1).Padding(10).Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Column(c =>
                        {
                            c.Item().Text($"Date: {_reservation.CreatedAt:dd/MM/yyyy}").FontSize(10);
                            c.Item().Text($"To: {_reservation?.ReservationClientCompany?.EnglishName}");
                            c.Item().Text($"From: {_reservation?.ReservationCompany?.EnglishName}");
                        });

                        row.RelativeItem().AlignCenter().Text(_reservation.TransactionId == null
                                ? "Tentative Confirmation"
                                : "Definite Confirmation"
                            ).SemiBold().FontSize(14);

                        row.RelativeItem();
                    });

                    col.Item().PaddingBottom(10).Text("Thank you for your interest.");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Element(CellStyle).Text("Res. NO :");
                        table.Cell().Element(CellStyle).Text(_reservation.ReferenceNumber).AlignStart().Bold();

                        table.Cell().Element(CellStyle).Text("Hotel Name :");
                        table.Cell().Element(CellStyle).Text(_reservation?.Hotel?.EnglishName ?? _reservation?.Hotel?.ArabicName ?? "-");

                        table.Cell().Element(CellStyle).Text("Arrival date :");
                        table.Cell().Element(CellStyle).Text(_reservation.ArrivalDate.ToString("dd/MM/yyyy"));

                        table.Cell().Element(CellStyle).Text("Depart. date :");
                        table.Cell().Element(CellStyle).Text(_reservation.DepartureDate.ToString("dd/MM/yyyy"));

                        table.Cell().Element(CellStyle).Text("Guest Name :");
                        table.Cell().ColumnSpan(3).Element(CellStyle).Text(_reservation.GuestName);


                        table.Cell().Element(CellStyle).Text("Number of Nights :");
                        table.Cell().Element(CellStyle).Text(_reservation.NumberOfNights.ToString());
                    });

                    // Room Table
                    col.Item().PaddingTop(10).Element(x => x.Border(1).Padding(5)).Column(groupCol =>
                    {
                        groupCol.Item().Text("Rooms");
                        groupCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(50);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("QTY").Bold();
                                header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("Room Type").Bold();
                                header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("Room Rate").Bold();
                                header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("Meal").Bold();
                            });
                            _reservation.ReservationRoomEntries?.ForEach(re =>
                            {
                                table.Cell().Element(BorderedCellStyle).Text(re.NumberOfRooms.ToString());
                                table.Cell().Element(BorderedCellStyle).Text(re?.RoomType?.EnglishName);
                                table.Cell().Element(BorderedCellStyle).Text(re.RoomRate.ToString("F2"));
                                table.Cell().Element(BorderedCellStyle).Text(re.MealRate?.ToString("F2"));
                            });
                        });



                        groupCol.Item().Text($"Net Room Accommodation Charge : SAR {accommodationCharge:F2}");

                        // line
                        groupCol.Item()
                            .PaddingVertical(2)
                            .LineHorizontal(2)
                            .LineColor(Colors.Blue.Darken2);

                        if (_reservation.ReservationServiceEntries != null && _reservation.ReservationServiceEntries.Any())
                        {
                            groupCol.Item().Text("Services");
                            groupCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("QTY").Bold();
                                    header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("Service Type").Bold();
                                    header.Cell().Element(BorderedCellStyle).Element(HeaderStyle).Text("Service Rate").Bold();
                                });

                                _reservation.ReservationServiceEntries?.ForEach(re =>
                                {
                                    table.Cell().Element(BorderedCellStyle).Text(((int)re.Quantity).ToString());
                                    table.Cell().Element(BorderedCellStyle).Text(re?.ReservationService?.EnglishName);
                                    table.Cell().Element(BorderedCellStyle).Text((re.CostPrice ?? 0m).ToString("F2"));
                                });
                            });
                            groupCol.Item().PaddingTop(5).Text($"Net Service Charge : SAR {serviceCharge:F2}");
                        }
                    });
                    col.Item().Element(x => x.Border(1).Padding(5)).Column(groupCol =>
                    {
                        groupCol.Item().Border(1).Padding(5).AlignCenter().Column(rightCol =>
                        {
                            rightCol.Item().Text($"Total : SAR {netCharge:F2}");
                            rightCol.Item().Text($"VAT : SAR {(netCharge * (decimal)0.15):F2}");
                            rightCol.Item().Text($"Total Net Value : SAR {(netCharge + (netCharge * (decimal)0.15)):F2} ({NumberToWordsConverter.ToEnglishWords((netCharge + (netCharge * (decimal)0.15)))} SAR)").Bold();
                        });
                    });


                    col.Item().PaddingTop(10).Border(1).Background(Colors.Grey.Lighten4).Padding(5).Text("Remarks :");

                    col.Item()
                        .Border(1)
                        .Padding(5)
                        .Column(group =>
                        {
                            // Use the primary policy from the new structure
                            var selectedPolicy = _companyDetails.Policies?.FirstOrDefault(p => p.Id == _reservation.CompanyPolicyId);
                            if(selectedPolicy == null)
                            {
                              selectedPolicy = _companyDetails.Policies?.FirstOrDefault(p => p.IsPrimary);

                            }

                            if (selectedPolicy != null)
                            {
                                group.Item().Text(selectedPolicy.PolicyParagraph);
                            }
                            else
                            {
                                // Fallback to first policy if no primary is set
                                var firstPolicy = _companyDetails.Policies?.FirstOrDefault();
                                if (firstPolicy != null)
                                {
                                    group.Item().Text(firstPolicy.PolicyParagraph);
                                }
                            }
                        });

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem(2).Column(bank =>
                        {
                            bank.Item().Text("* Our Bank Acc :")
                            .Underline()
                            .Bold();
                            
                            // Use the primary bank account from the new structure
                            var selectedBankAccount = _companyDetails.BankAccounts?.FirstOrDefault(b=>b.Id==_reservation.BankAccountId);

                            if (selectedBankAccount == null)
                            {
                                selectedBankAccount = _companyDetails.BankAccounts?.FirstOrDefault(b => b.IsPrimary);
                            }
                            if (selectedBankAccount != null)
                            {
                                bank.Item().Text($"Account Name: {selectedBankAccount.AccountName}");
                                bank.Item().Text($"Bank Name: {selectedBankAccount.BankName}");
                                bank.Item().Text($"Branch: {selectedBankAccount.BankBranchName}");
                                bank.Item().Text($"Account #: {selectedBankAccount.BankAccountNumber}");
                                bank.Item().Text($"IBAN: {selectedBankAccount.IBAN}");
                                bank.Item().Text($"Swift Code: {selectedBankAccount.SwiftCode}");
                            }
                        });

                        row.RelativeItem(1).Column(info =>
                        {
                            info.Item().Text("Hotel Rsrv #: ");
                        });
                    });

                   
                    
                    col.Item().AlignCenter().Text("Thanks and Best Regards").Bold();
                    
                    if (_reservation.GeneratedQR != null)
                    {
                        col.Item().AlignCenter().Height(120).Width(120).Image(PdfTemplateUtils.GenerateQrCodeImage(_reservation.GeneratedQR));
                    }

                    // Signature section (mirrors SalesInvoiceDocument.cs:388-456)
                    if (_showCompanySignature || _showCustomerSignature)
                    {
                    col.Item().PaddingTop(30).Row(r =>
                    {
                        // Customer signature on the far left
                        if (_showCustomerSignature)
                        {
                        r.RelativeItem(3).PaddingLeft(0).PaddingRight(80).Column(c =>
                        {
                            var customerName = _reservation.ReservationClientCompany?.ArabicName ?? _reservation.ReservationClientCompany?.EnglishName ?? "";
                            if (!string.IsNullOrWhiteSpace(customerName))
                            {
                                var isCustomerArabic = !string.IsNullOrWhiteSpace(_reservation.ReservationClientCompany?.ArabicName);

                                if (isCustomerArabic)
                                {
                                    c.Item().AlignRight().Text(t =>
                                    {
                                        t.Span(customerName).FontSize(10);
                                    });

                                    c.Item().PaddingTop(5).AlignRight().Text(t =>
                                    {
                                        t.Span("................................................................................ :التوقيع").FontSize(8);
                                    });
                                }
                                else
                                {
                                    c.Item().AlignLeft().Text(t =>
                                    {
                                        t.Span(customerName).FontSize(10);
                                    });

                                    c.Item().PaddingTop(5).AlignLeft().Text(t =>
                                    {
                                        t.Span("................................................................................ :التوقيع").FontSize(8);
                                    });
                                }
                            }
                        });
                        }

                        // Company signature on the far right
                        if (_showCompanySignature)
                        {
                        r.RelativeItem(3).PaddingRight(0).PaddingLeft(80).Column(c =>
                        {
                            var companyName = _companyDetails.ArabicName ?? _companyDetails.EnglishName ?? "";
                            c.Item().AlignRight().Text(t =>
                            {
                                t.Span(companyName).FontSize(10);
                            });

                            c.Item().PaddingTop(5).AlignRight().Text(t =>
                            {
                                t.Span("................................................................................ :التوقيع").FontSize(8);
                            });

                            if (_showStamp && _companyDetails.Stamp != null && _companyDetails.Stamp.Length > 0)
                            {
                                c.Item().PaddingTop(10).AlignRight().Height(100).Width(100).Image(_companyDetails.Stamp).FitArea();
                            }
                        });
                        }

                    });
                    }

                });
            });

            // show in companion app
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            LogService.LogError(e);
            throw;
        }
    }

    IContainer CellStyle(IContainer container)
    {
        return container.Padding(2);
    }

    IContainer BorderedCellStyle(IContainer container)
    {
        return container.Border(1).Padding(2);
    }

    TextStyle ArabicStyle => TextStyle.Default
        .FontFamily("Almarai");

    private static IContainer HeaderStyle(IContainer c) => c.Background("#42acff").Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#ffffff"));

    private static void RenderCanceledBanner(IContainer container)
    {
        container.Background(Colors.Red.Medium).Padding(8).AlignCenter().Text(text =>
        {
            text.Span("CANCELED / ملغي").Bold().FontSize(14).FontColor("#ffffff");
        });
    }

}