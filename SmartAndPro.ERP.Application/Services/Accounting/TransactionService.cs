using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Constants;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.Transaction;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Enforcers;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Exceptions;
using SmartAndPro.ERP.Application.Common.Extensions;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Globalization;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class TransactionService
    : ServiceBase
    <
        Transaction,
        TransactionPostDto,
        TransactionPutDto,
        TransactionResponseDto,
        TransactionFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public TransactionService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    private (bool CanDelete, string ReasonCode, string Reason, string ReasonAr) CheckDeleteEligibility(Transaction transaction)
    {
        if (transaction.SaleInvoiceId.HasValue)
            return (false, "LINKED_SALE_INVOICE", "Transaction is linked to a sale invoice", "القيد مرتبط بفاتورة بيع");
        if (transaction.PurchaseInvoiceId.HasValue)
            return (false, "LINKED_PURCHASE_INVOICE", "Transaction is linked to a purchase invoice", "القيد مرتبط بفاتورة شراء");
        if (transaction.SalesReturnId.HasValue)
            return (false, "LINKED_SALES_RETURN", "Transaction is linked to a sales return", "القيد مرتبط بمردود مبيعات");
        if (transaction.PurchaseReturnId.HasValue)
            return (false, "LINKED_PURCHASE_RETURN", "Transaction is linked to a purchase return", "القيد مرتبط بمردود مشتريات");
        if (transaction.PaymentReceiptId.HasValue)
            return (false, "LINKED_PAYMENT_RECEIPT", "Transaction is linked to a payment receipt", "القيد مرتبط بايصال دفع");
        if (transaction.PaycheckReceiptId.HasValue)
            return (false, "LINKED_PAYCHECK_RECEIPT", "Transaction is linked to a paycheck receipt", "القيد مرتبط بايصال شيك");
        if (transaction.InventoryPermitId.HasValue)
            return (false, "LINKED_INVENTORY_PERMIT", "Transaction is linked to an inventory permit", "القيد مرتبط باذن مخزني");
        if (transaction.InventoryStocktakingId.HasValue)
            return (false, "LINKED_INVENTORY_STOCKTAKING", "Transaction is linked to an inventory stocktaking", "القيد مرتبط بجرد مخزني");
        if (transaction.ReservationId.HasValue)
            return (false, "LINKED_RESERVATION", "Transaction is linked to a reservation", "القيد مرتبط بحجز");
        if (transaction.DebitNoteId.HasValue)
            return (false, "LINKED_DEBIT_NOTE", "Transaction is linked to a debit note", "القيد مرتبط بملاحظة خصم");
        if (transaction.SalesCreditNoteId.HasValue)
            return (false, "LINKED_CREDIT_NOTE", "Transaction is linked to a credit note", "القيد مرتبط بملاحظةCreditNote");

        return (true, string.Empty, string.Empty, string.Empty);
    }

    public async Task<TransactionImportResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var result = new TransactionImportResultDto();

        var headerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["serial"] = "serial",
            ["serial no"] = "serial",
            ["serial no."] = "serial",
            ["serial number"] = "serial",
            ["serialnumber"] = "serial",
            ["reference"] = "serial",
            ["reference number"] = "serial",
            ["gen. no."] = "serial",
            ["gen no."] = "serial",
            ["gen. no"] = "serial",
            ["gen no"] = "serial",
            ["sn"] = "serial",
            ["رقم القيد"] = "serial",
            ["transaction date"] = "transactionDate",
            ["transactiondate"] = "transactionDate",
            ["transaction_date"] = "transactionDate",
            ["date"] = "transactionDate",
            ["تاريخ العملية"] = "transactionDate",
            ["تاريخ الحركة"] = "transactionDate",
            ["تاريخ القيد"] = "transactionDate",
            ["التاريخ"] = "transactionDate",
            ["تاريخ"] = "transactionDate",
            ["description"] = "transactionDescription",
            ["transaction description"] = "transactionDescription",
            ["وصف الحركة"] = "transactionDescription",
            ["الوصف"] = "transactionDescription",
            ["main account"] = "mainAccount",
            ["mainaccount"] = "mainAccount",
            ["الحساب الرئيسي"] = "mainAccount",
            ["رقم الحساب"] = "mainAccount",
            ["اسم الحساب"] = "mainAccount",
            ["acc. name"] = "mainAccount",
            ["acc name"] = "mainAccount",
            ["acc. no."] = "mainAccount",
            ["acc. no"] = "mainAccount",
            ["acc no."] = "mainAccount",
            ["acc no"] = "mainAccount",
            ["account name"] = "mainAccount",
            ["account number"] = "mainAccount",
            ["account no"] = "mainAccount",
            ["debit"] = "debit",
            ["المدين"] = "debit",
            ["مدين"] = "debit",
            ["db"] = "debit",
            ["dr"] = "debit",
            ["credit"] = "credit",
            ["الدائن"] = "credit",
            ["دائن"] = "credit",
            ["cr"] = "credit",
            ["entry description"] = "entryDescription",
            ["description 1"] = "entryDescription",
            ["description_1"] = "entryDescription",
            ["بيان القيد"] = "entryDescription",
            ["document type"] = "entryDescription",
            ["نوع المستند"] = "entryDescription",
        };

        using var package = new ExcelPackage(fileStream);
        var ws = package.Workbook.Worksheets[0];
        if (ws == null || ws.Dimension == null)
            return result;

        var totalRows = ws.Dimension.End.Row;
        var totalColumns = ws.Dimension.End.Column;
        var headerRow = 1;

        // If row 1 is empty, auto-detect the first non-empty header row (up to row 10)
        for (var r = 1; r <= Math.Min(totalRows, 10); r++)
        {
            var hasKnownHeader = false;
            for (var col = 1; col <= totalColumns; col++)
            {
                var cellVal = ws.Cells[r, col].Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(cellVal) && headerAliases.ContainsKey(cellVal))
                {
                    hasKnownHeader = true;
                    break;
                }
            }
            if (hasKnownHeader)
            {
                headerRow = r;
                break;
            }
        }

        for (var col = 1; col <= totalColumns; col++)
        {
            result.Headers.Add(ws.Cells[headerRow, col].Text?.Trim() ?? string.Empty);
        }

        var colMap = new Dictionary<int, string>();
        var colByField = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var descriptionHeaderCount = 0;

        for (var col = 1; col <= totalColumns; col++)
        {
            var rawHeader = ws.Cells[headerRow, col].Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawHeader))
                continue;

            if (!headerAliases.TryGetValue(rawHeader, out var canonical)
                && !headerAliases.TryGetValue(rawHeader.ToLowerInvariant(), out canonical))
            {
                continue;
            }

            if (canonical == "transactionDescription")
            {
                // The historical file shape has two "description" columns.
                canonical = descriptionHeaderCount == 0 ? "transactionDescription" : "entryDescription";
                descriptionHeaderCount++;
            }

            colMap[col] = canonical;
            if (!colByField.ContainsKey(canonical))
                colByField[canonical] = col;
        }

        var requiredHeaders = new[] { "serial", "transactionDate", "mainAccount", "debit", "credit", "transactionDescription" };
        var missingHeaders = requiredHeaders.Where(h => !colByField.ContainsKey(h)).ToList();
        if (missingHeaders.Count > 0)
        {
            result.Failed.Add(new TransactionImportFailureDto
            {
                Row = 1,
                Name = string.Empty,
                Reason = $"الأعمدة المطلوبة غير موجودة: {string.Join(", ", missingHeaders)} / Required columns are missing: {string.Join(", ", missingHeaders)}.",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
            });
            return result;
        }

        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);
        var dbContext = transactionRepo.GetDbContext();

        var transactionTypeId = await dbContext.Set<TransactionType>()
            .IgnoreQueryFilters()
            .Where(t => t.ArabicName == "قيد يوميه" || t.EnglishName == "Transaction")
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();

        if (transactionTypeId == null)
        {
            result.Failed.Add(new TransactionImportFailureDto
            {
                Row = 0,
                Name = string.Empty,
                Reason = "نوع الحركة 'قيد يوميه / Transaction' غير موجود / Transaction type 'قيد يوميه / Transaction' was not found",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
            });
            return result;
        }

        var accounts = await dbContext.Set<MainAccount>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        var accountByArabicName = accounts
            .Where(a => !string.IsNullOrWhiteSpace(a.ArabicName))
            .GroupBy(a => NormalizeImportValue(a.ArabicName), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);

        var accountByEnglishName = accounts
            .Where(a => !string.IsNullOrWhiteSpace(a.EnglishName))
            .GroupBy(a => NormalizeImportValue(a.EnglishName), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);

        var accountByReferenceCode = accounts
            .Where(a => !string.IsNullOrWhiteSpace(a.ReferenceCode))
            .GroupBy(a => NormalizeImportValue(a.ReferenceCode), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);

        var validRows = new List<ImportRow>();

        for (var row = headerRow + 1; row <= totalRows; row++)
        {
            var rowValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (col, field) in colMap)
            {
                rowValues[field] = ws.Cells[row, col].Text?.Trim() ?? string.Empty;
            }

            if (rowValues.Values.All(string.IsNullOrWhiteSpace))
                continue;

            var serial = NullIfWhiteSpace(rowValues.GetValueOrDefault("serial"));
            if (serial == null)
            {
                result.Failed.Add(new TransactionImportFailureDto
                {
                    Row = row,
                    Name = string.Empty,
                    Reason = "رقم السجل مطلوب / Serial is required",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField,
                    Values = GetRowValues(ws, row, totalColumns)
                });
                continue;
            }

            if (!TryParseDateOnlyCell(ws.Cells[row, colByField["transactionDate"]], out var transactionDate))
            {
                result.Failed.Add(new TransactionImportFailureDto
                {
                    Row = row,
                    Name = serial,
                    Reason = "تنسيق التاريخ غير صحيح / Date format is invalid",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat,
                    Values = GetRowValues(ws, row, totalColumns)
                });
                continue;
            }

            var mainAccountRaw = rowValues.GetValueOrDefault("mainAccount");
            var mainAccountId = ResolveMainAccountId(mainAccountRaw, accountByArabicName, accountByEnglishName, accountByReferenceCode);
            if (mainAccountId == null)
            {
                result.Failed.Add(new TransactionImportFailureDto
                {
                    Row = row,
                    Name = serial,
                    Reason = $"الحساب الرئيسي '{mainAccountRaw}' غير موجود / Main account '{mainAccountRaw}' not found",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField,
                    Values = GetRowValues(ws, row, totalColumns)
                });
                continue;
            }

            if (!TryParseDecimalCell(ws.Cells[row, colByField["debit"]], out var debitAmount)
                || !TryParseDecimalCell(ws.Cells[row, colByField["credit"]], out var creditAmount))
            {
                result.Failed.Add(new TransactionImportFailureDto
                {
                    Row = row,
                    Name = serial,
                    Reason = "تنسيق المبلغ غير صحيح / Amount format is invalid",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat,
                    Values = GetRowValues(ws, row, totalColumns)
                });
                continue;
            }

            if (debitAmount == 0m && creditAmount == 0m)
            {
                result.Failed.Add(new TransactionImportFailureDto
                {
                    Row = row,
                    Name = serial,
                    Reason = "المدين والدائن لا يمكن أن يكونا صفر في نفس الوقت / Debit and credit cannot both be zero",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField,
                    Values = GetRowValues(ws, row, totalColumns)
                });
                continue;
            }

            var transactionDescription = NullIfWhiteSpace(rowValues.GetValueOrDefault("transactionDescription")) ?? string.Empty;
            var entryDescription = NullIfWhiteSpace(rowValues.GetValueOrDefault("entryDescription"));

            validRows.Add(new ImportRow
            {
                SourceRow = row,
                Serial = serial,
                TransactionDate = transactionDate,
                TransactionDescription = transactionDescription,
                EntryDescription = entryDescription,
                MainAccountId = mainAccountId.Value,
                DebitAmount = debitAmount,
                CreditAmount = creditAmount
            });
        }

        if (validRows.Count == 0)
            return result;

        var transactionEnforcer = new TransactionEnforcer();
        var toCreate = new List<Transaction>();
        var nextReferenceNumber = await GetNextTransactionReferenceNumberAsync(dbContext);

        //var groupedRows = validRows
        //    .GroupBy(
        //        r => $"{r.Serial}||{r.TransactionDate:yyyy-MM-dd}||{r.TransactionDescription}",
        //        StringComparer.OrdinalIgnoreCase);


        var groupedRows = validRows
    .GroupBy(
        r => r.Serial,
        StringComparer.OrdinalIgnoreCase);


        foreach (var group in groupedRows)
        {
            var firstRow = group.First();
            var transactionEntries = new List<TransactionEntry>();

            foreach (var row in group)
            {
                // When both values are filled on one row, split to two entries so
                // the existing business rule "at least 2 entries" stays satisfied.
                if (row.DebitAmount > 0m && row.CreditAmount > 0m)
                {
                    transactionEntries.Add(new TransactionEntry
                    {
                        MainAccountId = row.MainAccountId,
                        DebitAmount = row.DebitAmount,
                        CreditAmount = 0m,
                        Description = row.EntryDescription ?? row.TransactionDescription
                    });

                    transactionEntries.Add(new TransactionEntry
                    {
                        MainAccountId = row.MainAccountId,
                        DebitAmount = 0m,
                        CreditAmount = row.CreditAmount,
                        Description = row.EntryDescription ?? row.TransactionDescription
                    });
                }
                else
                {
                    transactionEntries.Add(new TransactionEntry
                    {
                        MainAccountId = row.MainAccountId,
                        DebitAmount = row.DebitAmount,
                        CreditAmount = row.CreditAmount,
                        Description = row.EntryDescription ?? row.TransactionDescription
                    });
                }
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionTypeId = transactionTypeId.Value,
                TransactionDate = firstRow.TransactionDate,
                Description = firstRow.TransactionDescription,
                ReferenceNumber = $"TRN-{nextReferenceNumber:D4}",
                TransactionEntries = transactionEntries,
                IsOpeningTransaction = false,
                IsFrontal = false,
                IsActive = true
            };
            nextReferenceNumber++;

            foreach (var entry in transactionEntries)
            {
                entry.Id = Guid.NewGuid();
                entry.TransactionId = transaction.Id;
                entry.Transaction = transaction;
            }

            var enforcerResult = transactionEnforcer.EnforceRules(transaction);
            if (!enforcerResult.IsSuccess)
            {
                var firstError = enforcerResult.Errors.FirstOrDefault()?.ErrorMessage
                                 ?? "Transaction validation failed.";

                foreach (var row in group)
                {
                    result.Failed.Add(new TransactionImportFailureDto
                    {
                        Row = row.SourceRow,
                        Name = row.TransactionDescription,
                        Reason = $"فشل التحقق من القيد / Transaction validation failed: {firstError}",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat,
                        Values = GetRowValues(ws, row.SourceRow, totalColumns)
                    });
                }

                continue;
            }

            toCreate.Add(transaction);
        }

        if (toCreate.Count == 0)
            return result;

        try
        {
            await transactionRepo.BeginTransactionAsync();

            await dbContext.Set<Transaction>().AddRangeAsync(toCreate);
            var savedRows = await dbContext.SaveChangesAsync();
            if (savedRows <= 0)
                throw new Exception("Import did not persist any rows.");

            await transactionRepo.CommitTransactionAsync();
            result.Imported += toCreate.Count;
        }
        catch (Exception ex)
        {
            await transactionRepo.RollbackTransactionAsync();
            LogService.LogError(ex, FlattenExceptionMessages(ex));

            result.Failed.Add(new TransactionImportFailureDto
            {
                Row = 0,
                Name = string.Empty,
                Reason = "فشل استيراد القيود وتم التراجع عن العملية / Import transaction failed and was rolled back",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
            });
        }

        return result;
    }

    private static string NormalizeImportValue(string? value)
        => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ExtractAccountCode(string value)
    {
        var accountParts = value.Trim().Split(new[] { "-", "–", "—" }, 2, StringSplitOptions.None);
        return accountParts.Length > 1 ? accountParts[0].Trim() : string.Empty;
    }

    private static Guid? ResolveMainAccountId(
        string? importedAccountValue,
        IReadOnlyDictionary<string, Guid> accountByArabicName,
        IReadOnlyDictionary<string, Guid> accountByEnglishName,
        IReadOnlyDictionary<string, Guid> accountByReferenceCode)
    {
        var accountValue = NullIfWhiteSpace(importedAccountValue);
        if (accountValue == null)
            return null;

        var normalized = NormalizeImportValue(accountValue);
        if (accountByArabicName.TryGetValue(normalized, out var arabicId))
            return arabicId;

        if (accountByEnglishName.TryGetValue(normalized, out var englishId))
            return englishId;

        if (accountByReferenceCode.TryGetValue(normalized, out var referenceId))
            return referenceId;

        var accountCode = ExtractAccountCode(accountValue);
        if (string.IsNullOrWhiteSpace(accountCode))
            return null;

        var normalizedCode = NormalizeImportValue(accountCode);
        return accountByReferenceCode.TryGetValue(normalizedCode, out var parsedReferenceId)
            ? parsedReferenceId
            : null;
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

    private static bool TryParseDateOnlyCell(ExcelRange cell, out DateOnly dateOnly)
    {
        dateOnly = default;
        if (cell?.Value == null)
            return false;

        try
        {
            switch (cell.Value)
            {
                case DateTime dt:
                    dateOnly = DateOnly.FromDateTime(dt);
                    return true;
                case double oa:
                    dateOnly = DateOnly.FromDateTime(DateTime.FromOADate(oa));
                    return true;
                case float oa:
                    dateOnly = DateOnly.FromDateTime(DateTime.FromOADate(oa));
                    return true;
                case int oa:
                    dateOnly = DateOnly.FromDateTime(DateTime.FromOADate(oa));
                    return true;
                case decimal oa:
                    dateOnly = DateOnly.FromDateTime(DateTime.FromOADate((double)oa));
                    return true;
            }
        }
        catch
        {
            return false;
        }

        var text = cell.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // 1. Try explicit formats with priority on dd/MM/yyyy and yyyy-MM-dd
        if (DateOnly.TryParseExact(text, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOnly))
            return true;

        if (DateTime.TryParseExact(text, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtExact))
        {
            dateOnly = DateOnly.FromDateTime(dtExact);
            return true;
        }

        // 2. Try standard culture parsers
        if (DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateOnly))
            return true;

        if (DateOnly.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dateOnly))
            return true;

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
        {
            dateOnly = DateOnly.FromDateTime(dateTime);
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var dtCurrent))
        {
            dateOnly = DateOnly.FromDateTime(dtCurrent);
            return true;
        }

        // 3. Handle Arabic numbers
        var normalizedDigitsText = NormalizeArabicDigits(text);
        if (normalizedDigitsText != text)
        {
            if (DateOnly.TryParseExact(normalizedDigitsText, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOnly))
                return true;

            if (DateTime.TryParseExact(normalizedDigitsText, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtNormExact))
            {
                dateOnly = DateOnly.FromDateTime(dtNormExact);
                return true;
            }

            if (DateOnly.TryParse(normalizedDigitsText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateOnly))
                return true;
        }

        // 4. Try OADate numeric string
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
        {
            try
            {
                dateOnly = DateOnly.FromDateTime(DateTime.FromOADate(oaDate));
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

private static bool TryParseDecimalCell(ExcelRange cell, out decimal value)
    {
        value = 0m;
        if (cell?.Value == null)
            return true;

        switch (cell.Value)
        {
            case decimal d:
                value = d;
                return true;
            case double d:
                value = Convert.ToDecimal(d);
                return true;
            case float d:
                value = Convert.ToDecimal(d);
                return true;
            case int d:
                value = d;
                return true;
            case long d:
                value = d;
                return true;
        }

        var text = cell.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            return true;

        var normalized = text.Replace(",", string.Empty);
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private static List<string> GetRowValues(ExcelWorksheet ws, int row, int totalColumns)
    {
        var values = new List<string>();
        for (var col = 1; col <= totalColumns; col++)
        {
            values.Add(ws.Cells[row, col].Text?.Trim() ?? string.Empty);
        }
        return values;
    }

    private static string FlattenExceptionMessages(Exception ex)
    {
        var message = ex.Message;
        var inner = ex.InnerException;
        while (inner != null)
        {
            message += $" | {inner.Message}";
            inner = inner.InnerException;
        }

        return message;
    }

    private static async Task<int> GetNextTransactionReferenceNumberAsync(DbContext dbContext)
    {
        var references = await dbContext.Set<Transaction>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.ReferenceNumber != null && t.ReferenceNumber.StartsWith("TRN-"))
            .Select(t => t.ReferenceNumber!)
            .ToListAsync();

        var maxNumber = 0;
        foreach (var reference in references)
        {
            var numericPart = reference[4..];
            if (!int.TryParse(numericPart, out var parsed))
                continue;

            if (parsed > maxNumber)
                maxNumber = parsed;
        }

        return maxNumber + 1;
    }

    private sealed class ImportRow
    {
        public int SourceRow { get; set; }
        public string Serial { get; set; } = string.Empty;
        public DateOnly TransactionDate { get; set; }
        public string TransactionDescription { get; set; } = string.Empty;
        public string? EntryDescription { get; set; }
        public Guid MainAccountId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public async Task<OperationResponse<List<TransactionResponseDto>>> GetAllFrontalAsync(TransactionFiltersDto filters = null, int? pageNumber = null, int? pageSize = null, bool includeInactive = false, string? sortField = null, int? sortOrder = null)
    {
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var queryOperation = await transactionRepository.GetAll();
        
        if (queryOperation.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<List<TransactionResponseDto>>(queryOperation.ErrorCode, null);

        var query = queryOperation.Data
            .Where(s => s.IsFrontal);
        
        if (includeInactive || (filters != null && filters.IsActive.HasValue))
        {
            query = query.IgnoreQueryFilters().Where(s => s.IsFrontal);
        }
        
        if (filters != null)
        {
            if (filters.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filters.IsActive.Value);
            }
            if (!string.IsNullOrWhiteSpace(filters.SearchKeyword))
            {
                query = query.ApplySearchKeyword(filters.SearchKeyword);
            }
            if (!filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }
            if (filters.FromTransactionDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= filters.FromTransactionDate.Value);
            }
            if (filters.ToTransactionDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= filters.ToTransactionDate.Value);
            }
            if (filters.TransactionDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate == filters.TransactionDate.Value);
            }
            if (filters.FromCreatedAt.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filters.FromCreatedAt.Value);
            }
            if (filters.ToCreatedAt.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= filters.ToCreatedAt.Value);
            }
        }

        var totalRecords = await query.CountAsync();

        pageNumber ??= 1;
        pageSize ??= 10;
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 1;

        if (pageNumber > 0 && pageSize > 0)
        {
            var skip = (pageNumber.Value - 1) * pageSize.Value;
            query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
        }
        
        var frontalCustomers = await query.ToListAsync();
        
        if (frontalCustomers == null || !frontalCustomers.Any())
        {
            return new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.Success, new List<TransactionResponseDto>(), totalCount: 0, isPaginated: true);
        }
        var frontalCustomersMapped = _mapper.Map<List<TransactionResponseDto>>(frontalCustomers);

        return new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.Success, frontalCustomersMapped, totalCount: totalRecords, isPaginated: true);
    }

    public async Task<OperationResponse<List<TransactionResponseDto>>> GetAllAsync(TransactionFiltersDto filters = null, int? pageNumber = null, int? pageSize = null, bool includeInactive = false, string? sortField = null, int? sortOrder = null)
    {
        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);
        var operation = await transactionRepo.GetAll();
        
        if (operation.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<List<TransactionResponseDto>>(operation.ErrorCode, null);

        var query = operation.Data;
        if (includeInactive || (filters != null && filters.IsActive.HasValue))
        {
            query = query.IgnoreQueryFilters();
        }

        if (filters != null)
        {
            if (filters.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filters.IsActive.Value);
            }
            if (!string.IsNullOrWhiteSpace(filters.SearchKeyword))
            {
                query = query.ApplySearchKeyword(filters.SearchKeyword);
            }
            if (!filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }
            if (filters.FromTransactionDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= filters.FromTransactionDate.Value);
            }
            if (filters.ToTransactionDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= filters.ToTransactionDate.Value);
            }
            if (filters.TransactionDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate == filters.TransactionDate.Value);
            }
            if (filters.FromCreatedAt.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filters.FromCreatedAt.Value);
            }
            if (filters.ToCreatedAt.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= filters.ToCreatedAt.Value);
            }
        }

        var totalRecords = await query.CountAsync();

        pageNumber ??= 1;
        pageSize ??= 10;
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 1;

        if (pageNumber > 0 && pageSize > 0)
        {
            var skip = (pageNumber.Value - 1) * pageSize.Value;
            query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
        }

        var result = await query.ToListAsync();
        if (result == null || !result.Any())
            return new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.Success, new List<TransactionResponseDto>(), totalCount: 0, isPaginated: true);

        var mapped = _mapper.Map<List<TransactionResponseDto>>(result);
        
        return new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.Success, mapped, totalCount: totalRecords, isPaginated: true);
    }
    
    public async Task<OperationResponse<TransactionResponseDto>> MakeFrontalAsync(Guid transactionId)
    {
        var transactionRepository = _repositoriesFactory.GetRepository<Transaction>();
        var transactionQuery = await transactionRepository.Filter(x => x.Id == transactionId);
        var transaction = transactionQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (transaction == null)
        {
            return new OperationResponse<TransactionResponseDto>(ErrorCodes.NotFound, null);
        }

        transaction.IsFrontal = true;
        
        var lastFrontalTransactionQuery = await transactionRepository
            .Filter(r => r.IsFrontal && r.ReferenceNumber.StartsWith("FTRN-"));

        var lastFrontalTransaction = lastFrontalTransactionQuery.Data?
            .OrderByDescending(r => r.ReferenceNumber)
            .FirstOrDefault();

        int nextNumber = 1;
        if (lastFrontalTransaction != null)
        {
            var numberPart = lastFrontalTransaction.ReferenceNumber.Substring("FTRN-".Length);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        transaction.ReferenceNumber = $"FTRN-{nextNumber}";



        
        var updateResult = await transactionRepository.Update(transaction);
        
        if (updateResult.Data == null)
        {
            return new OperationResponse<TransactionResponseDto>(ErrorCodes.GeneralFail, null);
        }

        var mappedCustomer = _mapper.Map<TransactionResponseDto>(updateResult.Data);
        return new OperationResponse<TransactionResponseDto>(ErrorCodes.Success, mappedCustomer);
    }
    
    
    
    
    public override async Task<OperationResponse<TransactionResponseDto>> AddAsync(TransactionPostDto transactionPost)
    {
        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);
        var transaction = _mapper.Map<Transaction>(transactionPost);
        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
        if (!enforcerResult.IsSuccess)
            throw new BusinessRuleViolationException(enforcerResult.Errors);
        try
        {
            await transactionRepo.BeginTransactionAsync();

            var isOpeningTransactionQuery =
                    await transactionRepo.Filter(x => x.IsOpeningTransaction == true);
                if (transactionPost.IsOpeningTransaction == true && isOpeningTransactionQuery.Data!.Any(x => x.CreatedAt.Year == DateTime.UtcNow.Year) && transactionPost.TransactionDate!.Value.Year == DateTime.UtcNow.Year)
                    return new OperationResponse<TransactionResponseDto>(ErrorCodes.GeneralFail, null);
                var result = await base.AddAsync(transactionPost);
                if (result.ErrorCode == ErrorCodes.Success)
                {
                    if (result.Data.IsFrontal == true)
                    {
                        var makeFrontalResult = await MakeFrontalAsync(result.Data.Id);
                        if (makeFrontalResult.ErrorCode != ErrorCodes.Success)
                        {
                            return new OperationResponse<TransactionResponseDto>(makeFrontalResult.ErrorCode, null);
                        }
                        result.Data.ReferenceNumber = makeFrontalResult.Data.ReferenceNumber;

                    }
                        await transactionRepo.CommitTransactionAsync();
                }
                return result;        
        }

        catch (Exception ex)
        {
            await transactionRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return new OperationResponse<TransactionResponseDto>(ErrorCodes.GeneralFail, null);

        }

    }
    
    public override async Task<OperationResponse<List<TransactionResponseDto>>> AddRangeAsync(List<TransactionPostDto> transactionPosts)
    {
        var result = new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.GeneralFail, null);
        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);
        try
        {
            await transactionRepo.BeginTransactionAsync();
            // Map the list of transaction DTOs to entities
            var transactions = _mapper.Map<List<Transaction>>(transactionPosts);

            var transactionEnforcer = new TransactionEnforcer();
            
            var validTransactions = transactions.Where(transaction =>
            {
                var enforcerResult = transactionEnforcer.EnforceRules(transaction);
                return enforcerResult.IsSuccess;
            }).ToList();
            
            // If no transactions are valid, return an empty result
            if (validTransactions.Count == 0)
            {
                return new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.GeneralFail, null);
            }

            var addedTransactions = new List<Transaction>();
            // Add the transactions
            /*var operation = await _repositoriesFactory.GetRepository<Transaction>().AddRange(transactions);*/
            foreach (var transaction in validTransactions)
            {
                var operation = await transactionRepo.Add(transaction);
                if (operation.ErrorCode == ErrorCodes.Success)
                {
                    addedTransactions.Add(transaction);
                }
            }
            
            // Map the added transactions back to response DTOs
            var responseData = _mapper.Map<List<TransactionResponseDto>>(addedTransactions);

            await transactionRepo.CommitTransactionAsync();

            return new OperationResponse<List<TransactionResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch (BusinessRuleViolationException ex)
        {
            LogService.LogError(ex);
            throw;
        }
        catch (Exception ex)
        {
            await transactionRepo.RollbackTransactionAsync();
            LogService.LogError(ex);

        }

        return result;
    }

    
    
    public override async Task<OperationResponse<TransactionResponseDto>> UpdateAsync(TransactionPutDto transactionPut)
    {
        var transaction = _mapper.Map<Transaction>(transactionPut);
        GuidHelper.NormalizeGuids<Transaction>(transaction);
        
        var transactionEnforcer = new TransactionEnforcer();
        var enforcerResult = transactionEnforcer.EnforceRules(transaction);
        if (!enforcerResult.IsSuccess)
            throw new BusinessRuleViolationException(enforcerResult.Errors);
        
        var result = new OperationResponse<TransactionResponseDto>(ErrorCodes.GeneralFail,
            null);
        var transactionEntryRepo = _repositoriesFactory.GetRepository<TransactionEntry>(true);
        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);
        try
        {
            await transactionEntryRepo.BeginTransactionAsync();
            var transactionEntries = transactionPut.TransactionEntries;
            var existingTransactionEntriesQuery = await transactionEntryRepo.Filter(x => x.TransactionId == transaction.Id);
            var existingTransactionEntries = existingTransactionEntriesQuery.Data!.ToList();
            foreach (var transactionEntry in transactionEntries)
            {
                var entryEntity = _mapper.Map<TransactionEntry>(transactionEntry);
                GuidHelper.NormalizeGuids<TransactionEntry>(entryEntity);
                entryEntity.MainAccount = null;
                entryEntity.Transaction = null;

                // Check if the entry already exists in existingTransactionEntries
                var existingEntry = existingTransactionEntries.FirstOrDefault(x => x.Id == transactionEntry.Id);
                
                if (existingEntry != null)
                {
                    existingEntry.MainAccount = null;
                    existingEntry.Transaction = null;
                    if(
                        existingEntry.CreditAmount == entryEntity.CreditAmount &&
                        existingEntry.DebitAmount == entryEntity.DebitAmount &&
                        existingEntry.Description == entryEntity.Description &&
                        existingEntry.MainAccountId == entryEntity.MainAccountId
                        )
                        continue;
                    // Update existing entry
                    var entryOperation = await transactionEntryRepo.Update(entryEntity);
                }
                else
                {
                    // Add new entry
                    var entryOperation = transactionEntryRepo.Add(entryEntity);
                }
            }
            // Handle deletion of entries that are in existingTransactionEntries but not in transactionEntries
            var entriesToDelete = existingTransactionEntries.Where(x => !transactionEntries.Any(te => te.Id == x.Id)).ToList();
            foreach (var entryToDelete in entriesToDelete)
            {
                entryToDelete.MainAccount = null;
                entryToDelete.Transaction = null;
                var entryOperation = await transactionEntryRepo.Delete(entryToDelete);
            }
            
            transaction.TransactionEntries = null;
            var operation = await transactionRepo.Update(transaction);
            
            
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<TransactionResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<TransactionResponseDto>(operation.Data);

            await transactionEntryRepo.CommitTransactionAsync();

            return new OperationResponse<TransactionResponseDto>(ErrorCodes.Success, responseData);

        }
        catch (Exception ex)
        {
            await transactionEntryRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<TransactionResponseDto>> DeleteAsync(Guid id)
    {
        var result = new OperationResponse<TransactionResponseDto>(ErrorCodes.GeneralFail, null);
        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);
        try
        {
            await transactionRepo.BeginTransactionAsync();

            var dbContext = transactionRepo.GetDbContext();
            var transaction = await dbContext.Set<Transaction>()
                .IgnoreQueryFilters()
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(t => t.Id == id);
        
            if (transaction == null)
                return new OperationResponse<TransactionResponseDto>(ErrorCodes.NotFound, null);
            
            if (!transaction.IsActive)
            {
                await transactionRepo.CommitTransactionAsync();
                return new OperationResponse<TransactionResponseDto>(ErrorCodes.Success, null, message: "Transaction already deleted");
            }
            
            var (canDelete, reasonCode, reason, reasonAr) = CheckDeleteEligibility(transaction);
            if (!canDelete)
                return new OperationResponse<TransactionResponseDto>(ErrorCodes.Validation, null, message: $"{reasonCode}: {reason} | {reasonAr}");
            
            transaction.SoftDelete();
            dbContext.Entry(transaction).Property(t => t.IsActive).IsModified = true;
            
            await dbContext.SaveChangesAsync();
            await transactionRepo.CommitTransactionAsync();

            var mapped = _mapper.Map<TransactionResponseDto>(transaction);
            return new OperationResponse<TransactionResponseDto>(ErrorCodes.Success, mapped);
        }
        catch (Exception ex)
        {
            await transactionRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
        }
    
        return result;
    }

    public async Task<OperationResponse<TransactionResponseDto>> RestoreAsync(Guid id)
    {
        var result = new OperationResponse<TransactionResponseDto>(ErrorCodes.GeneralFail, null);
        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);

        try
        {
            await transactionRepo.BeginTransactionAsync();

            var dbContext = transactionRepo.GetDbContext();
            var transaction = await dbContext.Set<Transaction>()
                .IgnoreQueryFilters()
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                await transactionRepo.RollbackTransactionAsync();
                return new OperationResponse<TransactionResponseDto>(ErrorCodes.NotFound, null);
            }

            if (transaction.IsActive)
            {
                await transactionRepo.CommitTransactionAsync();
                var alreadyActiveMapped = _mapper.Map<TransactionResponseDto>(transaction);
                return new OperationResponse<TransactionResponseDto>(ErrorCodes.Success, alreadyActiveMapped, message: "Transaction is already active");
            }

            transaction.IsActive = true;
            dbContext.Entry(transaction).Property(t => t.IsActive).IsModified = true;

            await dbContext.SaveChangesAsync();
            await transactionRepo.CommitTransactionAsync();

            var restoredMapped = _mapper.Map<TransactionResponseDto>(transaction);
            return new OperationResponse<TransactionResponseDto>(ErrorCodes.Success, restoredMapped);
        }
        catch (Exception ex)
        {
            await transactionRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
        }

        return result;
    }

    public async Task<OperationResponse<TransactionBulkDeleteResultDto>> BulkDeleteAsync(TransactionBulkDeleteRequestDto request)
    {
        var result = new OperationResponse<TransactionBulkDeleteResultDto>(ErrorCodes.GeneralFail, null);
        
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return new OperationResponse<TransactionBulkDeleteResultDto>(ErrorCodes.Validation, new TransactionBulkDeleteResultDto
            {
                Requested = 0,
                Deleted = 0,
                Failed = new List<TransactionBulkDeleteFailureDto>
                {
                    new TransactionBulkDeleteFailureDto
                    {
                        Id = Guid.Empty,
                        ReferenceNumber = string.Empty,
                        ReasonCode = "INVALID_REQUEST",
                        Reason = "No transaction IDs provided",
                        ReasonAr = "لم يتم توفير معرفات القيود"
                    }
                }
            });
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var resultDto = new TransactionBulkDeleteResultDto
        {
            Requested = uniqueIds.Count,
            Deleted = 0,
            Failed = new List<TransactionBulkDeleteFailureDto>()
        };

        var transactionRepo = _repositoriesFactory.GetRepository<Transaction>(true);

        try
        {
            await transactionRepo.BeginTransactionAsync();

            var dbContext = transactionRepo.GetDbContext();
            var transactions = await dbContext.Set<Transaction>()
                .IgnoreQueryFilters()
                .IgnoreAutoIncludes()
                .Where(t => uniqueIds.Contains(t.Id))
                .ToListAsync();

            var foundIds = new HashSet<Guid>(transactions.Select(t => t.Id));
            var notFoundIds = uniqueIds.Where(id => !foundIds.Contains(id)).ToList();

            foreach (var id in notFoundIds)
            {
                resultDto.Failed.Add(new TransactionBulkDeleteFailureDto
                {
                    Id = id,
                    ReferenceNumber = string.Empty,
                    ReasonCode = "NOT_FOUND",
                    Reason = "Transaction not found",
                    ReasonAr = "القيد غير موجود"
                });
            }

            foreach (var transaction in transactions)
            {
                if (!transaction.IsActive)
                {
                    resultDto.Failed.Add(new TransactionBulkDeleteFailureDto
                    {
                        Id = transaction.Id,
                        ReferenceNumber = transaction.ReferenceNumber,
                        ReasonCode = "ALREADY_DELETED",
                        Reason = "Transaction already deleted",
                        ReasonAr = "القيد محذوف بالفعل"
                    });
                    continue;
                }

                var (canDelete, reasonCode, reason, reasonAr) = CheckDeleteEligibility(transaction);
                
                if (!canDelete)
                {
                    resultDto.Failed.Add(new TransactionBulkDeleteFailureDto
                    {
                        Id = transaction.Id,
                        ReferenceNumber = transaction.ReferenceNumber,
                        ReasonCode = reasonCode,
                        Reason = reason,
                        ReasonAr = reasonAr
                    });
                    continue;
                }

                transaction.SoftDelete();
                dbContext.Entry(transaction).Property(t => t.IsActive).IsModified = true;
                resultDto.Deleted++;
            }

            await dbContext.SaveChangesAsync();
            await transactionRepo.CommitTransactionAsync();

            return new OperationResponse<TransactionBulkDeleteResultDto>(ErrorCodes.Success, resultDto);
        }
        catch (Exception ex)
        {
            await transactionRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
        }

        return result;
    }
    
}
