using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using System.Linq;
using System.Collections.Generic;
using QRCoder;

namespace SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;

public static class PdfTemplateUtils
{
    public static byte[] GenerateQrCodeImage(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);
        
        // IMPORTANT: Use BitmapByteQRCode, not QRCode class
        var qrCode = new BitmapByteQRCode(qrCodeData);
        byte[] qrBytes = qrCode.GetGraphic(30); // Get PNG byte array

        return qrBytes;
    }
    private const float ItemVerticalSpacing = 2; // 2pt spacing between vertical items

    public static void ApplyHeaderFooter(
        PageDescriptor page,
        Domains.Entities.PdfTemplateConfiguration config,
        Action<PageDescriptor, PdfHeaderFooterConfig> setHeader,
        Action<PageDescriptor, PdfHeaderFooterConfig> setFooter,
        PdfHeaderFooterConfig defaultHeader = null,
        PdfHeaderFooterConfig defaultFooter = null)
    {
        var header = config?.Header ?? defaultHeader;
        if (header != null)
            setHeader(page, header);

        var footer = config?.Footer ?? defaultFooter;
        if (footer != null)
            setFooter(page, footer);
    }

    public static void RenderHeaderFooter(IContainer container, PdfHeaderFooterConfig config, CompanyDetails company)
    {
        if (config == null) return;

        // Outer Row: Each column group is a vertical column
        container.Row(row =>
        {
            foreach (var columnGroup in config.Columns)
            {
                row.RelativeItem().Column(col =>
                {
                    // Each row group in the column
                    bool firstRow = true;
                    foreach (var rowGroup in columnGroup.Rows)
                    {
                        if (!firstRow)
                            col.Item().PaddingVertical(ItemVerticalSpacing); // add spacing between vertical items
                        firstRow = false;

                        col.Item().Row(rowItems =>
                        {
                            // Each item in the row (horizontally)
                            foreach (var item in rowGroup.Items)
                            {
                                rowItems.RelativeItem().Column(itemCol =>
                                {
                                    RenderColumnItem(itemCol, item, config, company);
                                });
                            }
                        });
                    }
                });
            }
        });
    }

    private static void RenderColumnItem(ColumnDescriptor col, PdfItem item, PdfHeaderFooterConfig config, CompanyDetails company)
    {
        float size = ParseFontSize(item.FontSize);

        if (item.Type == "custom")
        {
            var customText = config.CustomTexts?.FirstOrDefault(c => c.Id == item.Id)?.Value ?? "";
            AddAlignedText(col, customText, size, item.Align, item.Label, item);;
        }
        else if (item.Id == "logo")
        {
            if (company?.Logo != null && IsValidImageBytes(company.Logo))
            {
                AddAlignedImage(col, company.Logo, item.Align, item.FontSize);
            }
            else
            {
                // Fallback: show company name if logo is not available
                var companyName = company?.EnglishName ?? company?.ArabicName ?? "";
                AddAlignedText(col, companyName, 14, item.Align, "", item);
            }
        }
        else if (item.Id == "address" && config.AddressSubfields?.Any() == true)
        {
            col.Item().Column(addrCol =>
            {
                // Add label above subfields if exists
                if (!string.IsNullOrWhiteSpace(item.Label))
                {
                    AddAlignedText(addrCol, "", size, "left", item.Label, item);
                    addrCol.Item().PaddingBottom(ItemVerticalSpacing);
                }

                bool firstSub = true;
                foreach (var sub in config.AddressSubfields)
                {
                    if (!firstSub)
                        addrCol.Item().PaddingVertical(ItemVerticalSpacing);
                    firstSub = false;

                    string label = GetAddressSubfieldLabel(sub);
                    string value = GetAddressSubfieldValue(company, sub);
                    AddAlignedText(addrCol, value, size, "left", label, item);
                }
            });
        }
        else if (item.Id == "arabicAddress" && config.AddressSubfields?.Any() == true)
        {
            col.Item().Column(addrCol =>
            {
                // Add label above subfields if exists
                if (!string.IsNullOrWhiteSpace(item.Label))
                {
                    AddAlignedText(addrCol, "", size, "right", item.Label, item);
                    addrCol.Item().PaddingBottom(ItemVerticalSpacing);
                }

                bool firstSub = true;
                foreach (var sub in config.AddressSubfields)
                {
                    if (!firstSub)
                        addrCol.Item().PaddingVertical(ItemVerticalSpacing);
                    firstSub = false;

                    string label = GetArabicAddressSubfieldLabel(sub);
                    string value = GetArabicAddressSubfieldValue(company, sub);
                    AddAlignedText(addrCol, value, size, "right", label, item);
                }
            });
        }
        else if (item.Id == "primaryPhone")
        {
            // Get the primary phone from the new structure
            var primaryPhone = company?.Phones?.FirstOrDefault(p => p.IsPrimary);
            var phoneValue = primaryPhone?.PhoneNumber ?? "";
            AddAlignedText(col, phoneValue, size, item.Align, item.Label, item);
        }
        else if (item.Id == "phone")
        {
            // Legacy support - use primary phone if available, otherwise fallback to old property if it exists
            var primaryPhone = company?.Phones?.FirstOrDefault(p => p.IsPrimary);
            var phoneValue = primaryPhone?.PhoneNumber ?? "";
            AddAlignedText(col, phoneValue, size, item.Align, item.Label, item);
        }
        else if (item.Id == "primaryBankAccount")
        {
            // Get the primary bank account details
            var primaryBank = company?.BankAccounts?.FirstOrDefault(b => b.IsPrimary);
            var bankValue = primaryBank != null ? $"{primaryBank.BankName} - {primaryBank.BankAccountNumber}" : "";
            AddAlignedText(col, bankValue, size, item.Align, item.Label, item);
        }
        else if (item.Id == "primaryBankName")
        {
            var primaryBank = company?.BankAccounts?.FirstOrDefault(b => b.IsPrimary);
            var bankName = primaryBank?.BankName ?? "";
            AddAlignedText(col, bankName, size, item.Align, item.Label, item);
        }
        else if (item.Id == "primaryBankAccountNumber")
        {
            var primaryBank = company?.BankAccounts?.FirstOrDefault(b => b.IsPrimary);
            var accountNumber = primaryBank?.BankAccountNumber ?? "";
            AddAlignedText(col, accountNumber, size, item.Align, item.Label, item);
        }
        else if (item.Id == "primaryIban")
        {
            var primaryBank = company?.BankAccounts?.FirstOrDefault(b => b.IsPrimary);
            var iban = primaryBank?.IBAN ?? "";
            AddAlignedText(col, iban, size, item.Align, item.Label, item);
        }
        else if (item.Id == "primaryPolicy")
        {
            // Get the primary policy
            var primaryPolicy = company?.Policies?.FirstOrDefault(p => p.IsPrimary);
            var policyValue = primaryPolicy?.PolicyParagraph ?? "";
            AddAlignedText(col, policyValue, size, item.Align, item.Label, item);
        }
        else
        {
            // Normal company property
            var prop = typeof(CompanyDetails).GetProperty(ToPascalCase(item.Id));
            var value = prop?.GetValue(company)?.ToString() ?? "";
            AddAlignedText(col, value, size, item.Align, item.Label, item);;
        }
    }

    private static void AddAlignedText(ColumnDescriptor col, string value, float size, string align, string label, PdfItem? item = null)
    {
        string text = string.IsNullOrWhiteSpace(label)
            ? value
            : $"{label}{(string.IsNullOrWhiteSpace(label) ? "" : ":")} {value}";

        var textDescriptor = col.Item().Text(text).FontSize(size);

        // Apply alignment
        switch (align)
        {
            case "center":
                textDescriptor = textDescriptor.AlignCenter();
                break;
            case "right":
                textDescriptor = textDescriptor.AlignRight();
                break;
            default:
                textDescriptor = textDescriptor.AlignLeft();
                break;
        }
        
        // Apply styles if item is provided
        if (item != null)
        {
            if (item.IsBold) textDescriptor = textDescriptor.Bold();
            if (item.IsItalic) textDescriptor = textDescriptor.Italic();
            if (item.IsUnderline) textDescriptor = textDescriptor.Underline();
            if (item.IsStrikethrough) textDescriptor = textDescriptor.Strikethrough();
        }
    }

    private static void AddAlignedImage(ColumnDescriptor col, byte[] imageBytes, string align, string fontSize)
    {
        var width = ParseFontSize(fontSize);
        var img = col.Item();

        switch (align)
        {
            case "center":
                img.AlignCenter().Width(width).Image(imageBytes);
                break;
            case "right":
                img.AlignRight().Width(width).Image(imageBytes);
                break;
            default:
                img.AlignLeft().Width(width).Image(imageBytes);
                break;
        }
    }

    private static string ToPascalCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    private static float ParseFontSize(string fontSize)
    {
        if (string.IsNullOrWhiteSpace(fontSize) || fontSize == "inherit") return 10;
        if (fontSize.EndsWith("px") && float.TryParse(fontSize.Replace("px", ""), out var size))
            return size;
        if (float.TryParse(fontSize, out var num)) return num;
        return 10;
    }

    private static string GetAddressSubfieldLabel(string subfield)
        => ToPascalCase(subfield);

    private static string GetAddressSubfieldValue(CompanyDetails company, string subfield)
    {
        // Assumes company.Address is a class with PascalCase subfields
        var address = company?.Address;
        if (address == null) return "";
        var prop = address.GetType().GetProperty(ToPascalCase(subfield));
        return prop?.GetValue(address)?.ToString() ?? "";
    }

    private static string GetArabicAddressSubfieldLabel(string subfield)
    {
        switch (subfield)
        {
            case "street":
                return "الشارع";
            case "city":
                return "المدينة";
            case "country":
                return "البلد";
            case "zipCode":
                return "الرقم البريدي";
            default:
                return "";
        }
    }

    private static string GetArabicAddressSubfieldValue(CompanyDetails company, string subfield)
    {
        // Assumes company.Address is a class with PascalCase subfields
        var address = company?.ArabicAddress;
        if (address == null) return "";
        var prop = address.GetType().GetProperty(ToPascalCase(subfield));
        return prop?.GetValue(address)?.ToString() ?? "";
    }

    public static bool IsValidImageBytes(byte[]? bytes)
    {
        if (bytes == null || bytes.Length < 4) return false;
        
        // PNG
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return true;
            
        // JPEG
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return true;
            
        // GIF
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
            return true;

        // BMP
        if (bytes[0] == 0x42 && bytes[1] == 0x4D)
            return true;

        return false;
    }
}