using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Application.Common.Constants;

namespace SmartAndPro.ERP.Application.Services.Invoice.Sale;

public class SalesInvoiceImportService : ISalesInvoiceImportService
{
    private const decimal VatRate = 0.15m;
    private static readonly DateTime MinAllowedInvoiceDate = new(2020, 1, 1);
    private readonly RepositoriesFactory _repositoriesFactory;

    public SalesInvoiceImportService(RepositoriesFactory repositoriesFactory)
    {
        _repositoriesFactory = repositoriesFactory;
    }

    public Task<SaleInvoiceImportResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        return ImportInternalAsync(fileStream);
    }

    private async Task<SaleInvoiceImportResultDto> ImportInternalAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var result = new SaleInvoiceImportResultDto
        {
            Imported = 0,
            Updated = 0,
            Failed = new List<SaleInvoiceImportFailureDto>()
        };

        using var package = new ExcelPackage(fileStream);
        try
        {
            // Evaluate formula cells before parsing values.
            package.Workbook.Calculate();
        }
        catch
        {
            // Continue with raw/cached cell values if calculation fails.
        }

        var ws = package.Workbook.Worksheets.FirstOrDefault();
        if (ws?.Dimension == null)
        {
            result.Failed.Add(new SaleInvoiceImportFailureDto
            {
                Row = 0,
                Reason = "No data found in file.",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
            });
            return result;
        }

        var totalRows = ws.Dimension.End.Row;
        var totalColumns = ws.Dimension.End.Column;
        var headerRow = 1;

        var headerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Ref No.
            ["refno"] = "refNo",
            ["ref no"] = "refNo",
            ["reference"] = "refNo",
            ["reference no"] = "refNo",
            ["رقم المرجع"] = "refNo",
            ["رقم مرجع"] = "refNo",

            // Customer Name
            ["customername"] = "customerName",
            ["customer name"] = "customerName",
            ["customer"] = "customerName",
            ["اسم العميل"] = "customerName",
            ["اسم الزبون"] = "customerName",

            // Invoice Date
            ["invoicedate"] = "invoiceDate",
            ["invoice date"] = "invoiceDate",
            ["date"] = "invoiceDate",
            ["تاريخ الفاتورة"] = "invoiceDate",
            ["التاريخ"] = "invoiceDate",

            // Item Name
            ["itemname"] = "itemName",
            ["item name"] = "itemName",
            ["item"] = "itemName",
            ["product"] = "itemName",
            ["اسم الصنف"] = "itemName",
            ["اسم المنتج"] = "itemName",

            // Quantity
            ["quantity"] = "quantity",
            ["qty"] = "quantity",
            ["الكمية"] = "quantity",

            // Price
            ["price"] = "price",
            ["unitprice"] = "price",
            ["unit price"] = "price",
            ["السعر"] = "price",
            ["سعر الوحدة"] = "price"
        };

        var colMap = new Dictionary<int, string>();
        for (var col = 1; col <= totalColumns; col++)
        {
            var raw = ws.Cells[headerRow, col].Text?.Trim() ?? string.Empty;
            var key = NormalizeHeader(raw);
            if (headerAliases.TryGetValue(key, out var mapped))
                colMap[col] = mapped;
        }

        var requiredFields = new[] { "customerName", "invoiceDate", "itemName", "quantity", "price" };
        foreach (var field in requiredFields)
        {
            if (!colMap.ContainsValue(field))
            {
                result.Failed.Add(new SaleInvoiceImportFailureDto
                {
                    Row = 1,
                    Reason = $"Required column '{field}' was not found in file header.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                });
                return result;
            }
        }

        var parsedRows = new List<ImportRow>();
        for (var row = headerRow + 1; row <= totalRows; row++)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (col, field) in colMap)
                data[field] = ws.Cells[row, col].Text?.Trim() ?? string.Empty;

            if (data.Values.All(string.IsNullOrWhiteSpace))
                continue;

            var rowRefNo = data.GetValueOrDefault("refNo", string.Empty).Trim();
            var customerName = data.GetValueOrDefault("customerName", string.Empty).Trim();
            var itemName = data.GetValueOrDefault("itemName", string.Empty).Trim();
            var invoiceNoForError = string.IsNullOrWhiteSpace(rowRefNo) ? string.Empty : rowRefNo;

            if (string.IsNullOrWhiteSpace(customerName))
            {
                result.Failed.Add(new SaleInvoiceImportFailureDto
                {
                    Row = row,
                    InvoiceNo = invoiceNoForError,
                    CustomerName = customerName,
                    Reason = "Customer name is required.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                });
                continue;
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                result.Failed.Add(new SaleInvoiceImportFailureDto
                {
                    Row = row,
                    InvoiceNo = invoiceNoForError,
                    CustomerName = customerName,
                    Reason = "Item name is required.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                });
                continue;
            }

            if (!TryParseDecimal(ws, row, colMap, "quantity", out var quantity) || quantity <= 0)
            {
                result.Failed.Add(new SaleInvoiceImportFailureDto
                {
                    Row = row,
                    InvoiceNo = invoiceNoForError,
                    CustomerName = customerName,
                    Reason = "Quantity must be a valid number greater than zero.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                });
                continue;
            }

            if (!TryParseDecimal(ws, row, colMap, "price", out var price) || price < 0)
            {
                result.Failed.Add(new SaleInvoiceImportFailureDto
                {
                    Row = row,
                    InvoiceNo = invoiceNoForError,
                    CustomerName = customerName,
                    Reason = "Price must be a valid number greater than or equal to zero.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                });
                continue;
            }

            if (!TryParseDate(ws, row, colMap, "invoiceDate", out var invoiceDate))
            {
                result.Failed.Add(new SaleInvoiceImportFailureDto
                {
                    Row = row,
                    InvoiceNo = invoiceNoForError,
                    CustomerName = customerName,
                    Reason = "Invalid date format. Supported: yyyy-MM-dd, dd/MM/yyyy, Excel serial.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                });
                continue;
            }

            parsedRows.Add(new ImportRow
            {
                RowNum = row,
                RefNo = rowRefNo,
                CustomerName = customerName,
                ItemName = itemName,
                InvoiceDate = invoiceDate.Date,
                Quantity = quantity,
                Price = price
            });
        }

        if (parsedRows.Count == 0)
            return result;

        var dbContext = _repositoriesFactory.GetRepository<SaleInvoice>(true).GetDbContext();

        var allCustomers = await dbContext.Set<CustomerProfile>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(c => new { c.Id, c.ArabicName, c.EnglishName })
            .ToListAsync();

        var allItems = await dbContext.Set<MainSalesClass>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(i => new { i.Id, i.ArabicName, i.EnglishName, i.IsTaxExempted })
            .ToListAsync();

        var customerLookup = allCustomers
            .SelectMany(c => new[]
            {
                new { Key = NormalizeLookup(c.ArabicName), Value = c.Id },
                new { Key = NormalizeLookup(c.EnglishName), Value = c.Id }
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

        var itemLookup = allItems
            .SelectMany(i => new[]
            {
                new { Key = NormalizeLookup(i.ArabicName), Value = i.Id },
                new { Key = NormalizeLookup(i.EnglishName), Value = i.Id }
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

        var itemTaxExemption = allItems.ToDictionary(x => x.Id, x => x.IsTaxExempted);

        var groupedRows = parsedRows
            .GroupBy(r => string.IsNullOrWhiteSpace(r.RefNo)
                ? $"ROW-{r.RowNum}"
                : $"REF-{NormalizeLookup(r.RefNo)}")
            .ToList();

        var validInvoices = new List<ParsedInvoice>();
        foreach (var group in groupedRows)
        {
            var first = group.First();
            var invoiceNoFromSheet = string.IsNullOrWhiteSpace(first.RefNo) ? string.Empty : first.RefNo;
            var customerKey = NormalizeLookup(first.CustomerName);
            if (!customerLookup.TryGetValue(customerKey, out var customerId))
            {
                var createdCustomerId = await ResolveOrCreateCustomerAsync(
                    dbContext,
                    customerLookup,
                    first.CustomerName);

                if (createdCustomerId == null)
                {
                    foreach (var row in group)
                    {
                        result.Failed.Add(new SaleInvoiceImportFailureDto
                        {
                            Row = row.RowNum,
                            InvoiceNo = invoiceNoFromSheet,
                            CustomerName = row.CustomerName,
                            Reason = "Customer not found and could not be created automatically.",
                            ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
                        });
                    }
                    continue;
                }

                customerId = createdCustomerId.Value;
            }

            var hasRowError = false;
            var parsedItems = new List<ParsedItem>();
            foreach (var row in group)
            {
                var itemKey = NormalizeLookup(row.ItemName);
                if (!itemLookup.TryGetValue(itemKey, out var itemId))
                {
                    result.Failed.Add(new SaleInvoiceImportFailureDto
                    {
                        Row = row.RowNum,
                        InvoiceNo = invoiceNoFromSheet,
                        CustomerName = row.CustomerName,
                        Reason = "Item not found.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    hasRowError = true;
                    continue;
                }

                var isTaxExempted = itemTaxExemption.GetValueOrDefault(itemId);
                var lineNet = RoundMoney(row.Price * row.Quantity);
                var lineTaxAmount = isTaxExempted ? 0m : RoundMoney(lineNet * VatRate);
                var lineTotal = RoundMoney(lineNet + lineTaxAmount);

                parsedItems.Add(new ParsedItem
                {
                    SourceRow = row.RowNum,
                    MainSalesClassId = itemId,
                    Quantity = row.Quantity,
                    Price = row.Price,
                    Tax = isTaxExempted ? 0m : 15m,
                    TaxAmount = lineTaxAmount,
                    TotalPrice = lineTotal,
                    NetPrice = lineNet,
                    NoOfNights = 1
                });
            }

            if (hasRowError || parsedItems.Count == 0)
                continue;

            var issueDate = first.InvoiceDate.Date;
            if (issueDate < MinAllowedInvoiceDate || issueDate > DateTime.UtcNow.Date)
            {
                foreach (var row in group)
                {
                    result.Failed.Add(new SaleInvoiceImportFailureDto
                    {
                        Row = row.RowNum,
                        InvoiceNo = invoiceNoFromSheet,
                        CustomerName = row.CustomerName,
                        Reason = $"Invoice date must be between {MinAllowedInvoiceDate:yyyy-MM-dd} and {DateTime.UtcNow:yyyy-MM-dd}.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                }
                continue;
            }

            validInvoices.Add(new ParsedInvoice
            {
                RefNo = invoiceNoFromSheet,
                CustomerName = first.CustomerName,
                CustomerProfileId = customerId,
                IssueDate = issueDate,
                DueDate = issueDate,
                Items = parsedItems
            });
        }

        if (validInvoices.Count == 0)
            return result;

        var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>(true);
        await saleInvoiceRepo.BeginTransactionAsync();

        try
        {
            var nextInvoiceNo = await GetNextInvoiceNoAsync(dbContext);

            foreach (var parsed in validInvoices)
            {
                var invoiceItems = parsed.Items.Select(x => new SaleItem
                {
                    ItemNo = BuildItemNo(parsed.RefNo, x.SourceRow),
                    MainSalesClassId = x.MainSalesClassId,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Tax = x.Tax,
                    TaxAmount = x.TaxAmount,
                    Discount = 0m,
                    DiscountAmount = 0m,
                    TotalPrice = x.TotalPrice,
                    NetPrice = x.NetPrice,
                    NoOfNights = x.NoOfNights
                }).ToList();

                var net = RoundMoney(invoiceItems.Sum(i => i.NetPrice));
                var taxAmount = RoundMoney(invoiceItems.Sum(i => i.TaxAmount));
                var total = RoundMoney(net + taxAmount);

                var newInvoice = new SaleInvoice
                {
                    InvoiceNo = nextInvoiceNo(),
                    Currency = "SAR",
                    CustomerProfileId = parsed.CustomerProfileId,
                    Description = BuildDescription(parsed.RefNo, null),
                    IssueDate = parsed.IssueDate,
                    DueDate = parsed.DueDate,
                    Tax = invoiceItems.Any(i => i.Tax > 0m) ? 15m : 0m,
                    TaxAmount = taxAmount,
                    Discount = 0m,
                    DiscountAmount = 0m,
                    TotalPrice = total,
                    NetPrice = net,
                    IsQuotation = false,
                    IsFrontal = false,
                    IsActive = true,
                    SaleItems = invoiceItems
                };

                dbContext.Set<SaleInvoice>().Add(newInvoice);
                result.Imported++;
            }

            await dbContext.SaveChangesAsync();
            await saleInvoiceRepo.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            try { await saleInvoiceRepo.RollbackTransactionAsync(); } catch { /* ignore */ }
            result.Imported = 0;
            result.Updated = 0;
            result.Failed.Clear();
            result.Failed.Add(new SaleInvoiceImportFailureDto
            {
                Row = 0,
                Reason = FlattenExceptionMessages(ex),
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
            });
        }

        return result;
    }

    private static bool TryParseDecimal(ExcelWorksheet worksheet, int row, Dictionary<int, string> colMap, string field, out decimal value)
    {
        value = 0m;
        var col = colMap.FirstOrDefault(x => x.Value.Equals(field, StringComparison.OrdinalIgnoreCase)).Key;
        if (col <= 0)
            return false;

        var cell = worksheet.Cells[row, col];
        var raw = cell.Value;
        if (raw == null)
            return false;

        if (raw is double d)
        {
            value = Convert.ToDecimal(d);
            return true;
        }
        if (raw is decimal dc)
        {
            value = dc;
            return true;
        }

        var text = cell.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Replace(",", ".", StringComparison.Ordinal);
        return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
               || decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }

    private static readonly string[] SupportedDateFormats = new[]
    {
        "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy",
        "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy",
        "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd",
        "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy",
        "MM-dd-yyyy", "M-d-yyyy", "M-dd-yyyy", "MM-d-yyyy",
        "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy hh:mm:ss tt",
        "dd/MM/yyyy H:mm:ss", "dd/MM/yyyy h:mm:ss tt",
        "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy hh:mm:ss tt",
        "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fffZ",
        "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt",
        "dd/MM/yy", "d/M/yy", "dd-MM-yy", "d-M-yy", "yy-MM-dd", "yy/MM/dd",
        "MM/dd/yy", "M/d/yy", "MM-dd-yy", "M-d-yy"
    };

    private static string NormalizeArabicDigits(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
            .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9')
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
            .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9');
    }

    private static bool TryParseDate(ExcelWorksheet worksheet, int row, Dictionary<int, string> colMap, string field, out DateTime value)
    {
        value = default;
        var col = colMap.FirstOrDefault(x => x.Value.Equals(field, StringComparison.OrdinalIgnoreCase)).Key;
        if (col <= 0)
            return false;

        var cell = worksheet.Cells[row, col];
        var raw = cell.Value;
        if (raw == null)
            return false;

        if (raw is DateTime dt)
        {
            value = dt.Date;
            return true;
        }
        if (raw is double serial)
        {
            try
            {
                value = DateTime.FromOADate(serial).Date;
                return true;
            }
            catch
            {
                return false;
            }
        }

        var text = cell.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (DateTime.TryParseExact(text, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            value = exact.Date;
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            value = parsed.Date;
            return true;
        }

        var normalizedDigitsText = NormalizeArabicDigits(text);
        if (!string.Equals(normalizedDigitsText, text, StringComparison.Ordinal))
        {
            if (DateTime.TryParseExact(normalizedDigitsText, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtNormExact))
            {
                value = dtNormExact.Date;
                return true;
            }

            if (DateTime.TryParse(normalizedDigitsText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtNormParsed))
            {
                value = dtNormParsed.Date;
                return true;
            }
        }

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var serialText))
        {
            try
            {
                value = DateTime.FromOADate(serialText).Date;
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static string NormalizeHeader(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return new string(input.Trim().ToLowerInvariant().Where(c => !char.IsPunctuation(c)).ToArray());
    }

    private static string NormalizeLookup(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return string.Join(" ", input.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static decimal RoundMoney(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string BuildItemNo(string refNo, int rowNum)
    {
        if (!string.IsNullOrWhiteSpace(refNo))
            return refNo.Trim();

        return $"ROW-{rowNum}";
    }

    private static string? BuildDescription(string refNo, string? currentDescription)
    {
        if (string.IsNullOrWhiteSpace(refNo))
            return currentDescription;

        return $"Excel Ref: {refNo.Trim()}";
    }

    private static string FlattenExceptionMessages(Exception ex)
    {
        var parts = new List<string>();
        var current = ex;
        while (current != null)
        {
            if (!string.IsNullOrWhiteSpace(current.Message))
                parts.Add(current.Message.Trim());
            current = current.InnerException!;
        }

        return parts.Count == 0 ? "Import failed unexpectedly." : string.Join(" | ", parts.Distinct());
    }

    private static async Task<Func<string>> GetNextInvoiceNoAsync(DbContext dbContext)
    {
        var existingNumbers = await dbContext.Database.SqlQueryRaw<string>(
            @"SELECT [InvoiceNo] AS [Value]
              FROM [SaleInvoice]
              WHERE [IsQuotation] = CAST(0 AS bit)
                AND ([InvoiceNo] LIKE N'Inv-%' OR [InvoiceNo] LIKE N'INV-%')")
            .ToListAsync();

        var max = 0;
        foreach (var invoiceNo in existingNumbers)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo))
                continue;

            var digits = new string(invoiceNo.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var parsed) && parsed > max)
                max = parsed;
        }

        var cursor = max;
        return () =>
        {
            cursor++;
            return $"Inv-{cursor}";
        };
    }

    private static async Task<Guid?> ResolveOrCreateCustomerAsync(
        DbContext dbContext,
        Dictionary<string, Guid> customerLookup,
        string customerName)
    {
        var normalizedName = (customerName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
            return null;

        var lookupKey = NormalizeLookup(normalizedName);
        if (customerLookup.TryGetValue(lookupKey, out var existingId))
            return existingId;

        var existingCustomer = await dbContext.Set<CustomerProfile>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c =>
                (c.ArabicName != null && c.ArabicName.ToLower() == normalizedName.ToLower()) ||
                (c.EnglishName != null && c.EnglishName.ToLower() == normalizedName.ToLower()));

        if (existingCustomer != null)
        {
            customerLookup[NormalizeLookup(existingCustomer.ArabicName)] = existingCustomer.Id;
            if (!string.IsNullOrWhiteSpace(existingCustomer.EnglishName))
                customerLookup[NormalizeLookup(existingCustomer.EnglishName)] = existingCustomer.Id;
            return existingCustomer.Id;
        }

        var newCustomer = new CustomerProfile
        {
            ArabicName = normalizedName,
            EnglishName = normalizedName,
            IsActive = true,
            IsFrontal = false,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            dbContext.Set<CustomerProfile>().Add(newCustomer);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            var retryExisting = await dbContext.Set<CustomerProfile>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c =>
                    (c.ArabicName != null && c.ArabicName.ToLower() == normalizedName.ToLower()) ||
                    (c.EnglishName != null && c.EnglishName.ToLower() == normalizedName.ToLower()));

            if (retryExisting == null)
                return null;

            customerLookup[NormalizeLookup(retryExisting.ArabicName)] = retryExisting.Id;
            if (!string.IsNullOrWhiteSpace(retryExisting.EnglishName))
                customerLookup[NormalizeLookup(retryExisting.EnglishName)] = retryExisting.Id;
            return retryExisting.Id;
        }

        customerLookup[NormalizeLookup(newCustomer.ArabicName)] = newCustomer.Id;
        if (!string.IsNullOrWhiteSpace(newCustomer.EnglishName))
            customerLookup[NormalizeLookup(newCustomer.EnglishName)] = newCustomer.Id;

        return newCustomer.Id;
    }

    private sealed class ImportRow
    {
        public int RowNum { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }

    private sealed class ParsedInvoice
    {
        public string RefNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public Guid CustomerProfileId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<ParsedItem> Items { get; set; } = new();
    }

    private sealed class ParsedItem
    {
        public int SourceRow { get; set; }
        public Guid MainSalesClassId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal NetPrice { get; set; }
        public int NoOfNights { get; set; }
    }
}
