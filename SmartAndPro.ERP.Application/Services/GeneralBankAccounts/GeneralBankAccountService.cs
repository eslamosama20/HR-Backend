using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Constants;
using SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.GeneralBankAccounts
{
    public class GeneralBankAccountService : ServiceBase
        <
        GeneralBankAccount,
        GeneralBankAccountPostDto,
        GeneralBankAccountPutDto,
        GeneralBankAccountResponseDto,
        GeneralBankAccountFiltersDto
        >,
        IGeneralBankAccountService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public GeneralBankAccountService(RepositoriesFactory repositoriesFactory,AppSettings appSettings,
            IMapper mapper) : base(repositoriesFactory,appSettings,mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        public async Task<OperationResponse<List<GeneralBankAccountResponseDto>>> GetAllFrontalAsync()
        {
            var generalBankAccountRepository = _repositoriesFactory.GetRepository<GeneralBankAccount>();

            try
            {
                var frontalGeneralBankAccounts = await generalBankAccountRepository.Filter(b => b.IsFrontal);

                if (frontalGeneralBankAccounts.Data == null || !frontalGeneralBankAccounts.Data.Any())
                {
                    return new OperationResponse<List<GeneralBankAccountResponseDto>>(ErrorCodes.NoContent, null);
                }
                var frontalGeneralBankAccountsMapped = _mapper.Map<List<GeneralBankAccountResponseDto>>(frontalGeneralBankAccounts.Data);

                return new OperationResponse<List<GeneralBankAccountResponseDto>>(ErrorCodes.Success, frontalGeneralBankAccountsMapped);

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                throw;
            }

        }

        public async Task<OperationResponse<GeneralBankAccountResponseDto>> MakeFrontalAsync(Guid generalBankAccountId)
        {
            var generalBankAccountRepository = _repositoriesFactory.GetRepository<GeneralBankAccount>();

            try
            {
                var generalBankAccountQuery = await generalBankAccountRepository.Filter(e => e.Id == generalBankAccountId);
                var generalBankAccount = generalBankAccountQuery.Data?.FirstOrDefault();

                if (generalBankAccount == null)
                    return new OperationResponse<GeneralBankAccountResponseDto>(ErrorCodes.NotFound, null);

                generalBankAccount.IsFrontal = true;

                var updateResult = await generalBankAccountRepository.Update(generalBankAccount);

                if (updateResult.Data == null)
                {
                    return new OperationResponse<GeneralBankAccountResponseDto>(ErrorCodes.GeneralFail, null);
                }
                var generalBankAccountMapped = _mapper.Map<GeneralBankAccountResponseDto>(updateResult.Data);
                return new OperationResponse<GeneralBankAccountResponseDto>(ErrorCodes.Success, generalBankAccountMapped);

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                throw;
            }
        }

        public async Task<GeneralBankAccountImportResultDto> ImportFromExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var result = new GeneralBankAccountImportResultDto();

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets[0];
            if (ws == null || ws.Dimension == null)
                return result;

            int totalRows = ws.Dimension.End.Row;
            int totalColumns = ws.Dimension.End.Column;

            var colMap = new Dictionary<int, string>();
            for (int col = 1; col <= totalColumns; col++)
            {
                var raw = ws.Cells[1, col].Text?.Trim() ?? string.Empty;
                if (ImportFieldMappings.GeneralBankAccount.HeaderAliases.TryGetValue(raw, out var canonical))
                    colMap[col] = canonical;
                else if (ImportFieldMappings.GeneralBankAccount.HeaderAliases.TryGetValue(raw.ToLowerInvariant(), out var canonical2))
                    colMap[col] = canonical2;
            }

            if (!colMap.ContainsValue("accountName"))
            {
                result.Failed.Add(new GeneralBankAccountImportFailureDto
                {
                    Row = 1,
                    Name = string.Empty,
                    Reason = "Required column 'accountName / اسم الحساب' not found in the file header.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                });
                return result;
            }

            var parsedRows = new List<(int RowNum, Dictionary<string, string> Data)>();
            for (int row = 2; row <= totalRows; row++)
            {
                var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (col, field) in colMap)
                    rowData[field] = ws.Cells[row, col].Text?.Trim() ?? string.Empty;

                if (rowData.Values.All(string.IsNullOrWhiteSpace))
                    continue;

                parsedRows.Add((row, rowData));
            }

            if (parsedRows.Count == 0)
                return result;

            var dbContext = _repositoriesFactory.GetRepository<GeneralBankAccount>().GetDbContext();

            var existingAccounts = await dbContext.Set<GeneralBankAccount>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(x => new { x.AccountName, x.BankAccountNumber })
                .ToListAsync();

            var existingByAccountName = new HashSet<string>(
                existingAccounts
                    .Where(x => !string.IsNullOrWhiteSpace(x.AccountName))
                    .Select(x => NormalizeLookupKey(x.AccountName)),
                StringComparer.OrdinalIgnoreCase);

            var existingByBankAccountNumber = new HashSet<string>(
                existingAccounts
                    .Where(x => !string.IsNullOrWhiteSpace(x.BankAccountNumber))
                    .Select(x => NormalizeLookupKey(x.BankAccountNumber)),
                StringComparer.OrdinalIgnoreCase);

            var mainAccounts = await dbContext.Set<MainAccount>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(m => new { m.Id, m.ArabicName, m.EnglishName, m.ReferenceCode })
                .ToListAsync();

            var mainAccountIdSet = new HashSet<Guid>(mainAccounts.Select(m => m.Id));
            var mainAccountByArabicName = mainAccounts
                .Where(m => !string.IsNullOrWhiteSpace(m.ArabicName))
                .GroupBy(m => NormalizeLookupKey(m.ArabicName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
            var mainAccountByEnglishName = mainAccounts
                .Where(m => !string.IsNullOrWhiteSpace(m.EnglishName))
                .GroupBy(m => NormalizeLookupKey(m.EnglishName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
            var mainAccountByReferenceCode = mainAccounts
                .Where(m => !string.IsNullOrWhiteSpace(m.ReferenceCode))
                .GroupBy(m => NormalizeLookupKey(m.ReferenceCode), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

            var seenAccountNameInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenBankAccountNumberInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var toCreate = new List<(int RowNum, string AccountName, GeneralBankAccount Entity)>();

            foreach (var (rowNum, rowData) in parsedRows)
            {
                var accountNameRaw = rowData.GetValueOrDefault("accountName", string.Empty);
                var accountName = accountNameRaw.Trim();
                if (string.IsNullOrWhiteSpace(accountName))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = string.Empty,
                        Reason = "Account Name is required.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    continue;
                }

                var bankName = rowData.GetValueOrDefault("bankName", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(bankName))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = "Bank Name is required.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    continue;
                }

                var bankAccountNumberRaw = rowData.GetValueOrDefault("bankAccountNumber", string.Empty);
                var bankAccountNumber = bankAccountNumberRaw.Trim();
                if (string.IsNullOrWhiteSpace(bankAccountNumber))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = "Bank Account Number is required.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    continue;
                }

                var bankBranchName = rowData.GetValueOrDefault("bankBranchName", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(bankBranchName))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = "Branch Name is required.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    continue;
                }

                var normalizedAccountName = NormalizeLookupKey(accountName);
                var normalizedBankAccountNumber = NormalizeLookupKey(bankAccountNumber);

                if (!seenAccountNameInBatch.Add(normalizedAccountName) ||
                    !seenBankAccountNumberInBatch.Add(normalizedBankAccountNumber))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = $"Bank account '{accountName}' is duplicated in this file.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                    });
                    continue;
                }

                if (existingByAccountName.Contains(normalizedAccountName) ||
                    existingByBankAccountNumber.Contains(normalizedBankAccountNumber))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = $"Bank account '{accountName}' already exists.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                    });
                    continue;
                }

                var mainAccountIdRaw = rowData.GetValueOrDefault("mainAccountId", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(mainAccountIdRaw))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = "mainAccountId is required for bank account creation.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    continue;
                }

                Guid mainAccountId;
                var normalizedMainAccountLookup = NormalizeLookupKey(mainAccountIdRaw);
                if (Guid.TryParse(mainAccountIdRaw, out var parsedGuid))
                {
                    mainAccountId = parsedGuid;
                }
                else if (mainAccountByArabicName.TryGetValue(normalizedMainAccountLookup, out var resolvedByArabic))
                {
                    mainAccountId = resolvedByArabic;
                }
                else if (mainAccountByEnglishName.TryGetValue(normalizedMainAccountLookup, out var resolvedByEnglish))
                {
                    mainAccountId = resolvedByEnglish;
                }
                else if (mainAccountByReferenceCode.TryGetValue(normalizedMainAccountLookup, out var resolvedByReferenceCode))
                {
                    mainAccountId = resolvedByReferenceCode;
                }
                else
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = $"mainAccountId '{mainAccountIdRaw}' was not found (GUID or account name).",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                if (!mainAccountIdSet.Contains(mainAccountId))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = $"mainAccountId '{mainAccountIdRaw}' was not found (GUID or account name).",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                var swiftCode = NullIfEmpty(rowData.GetValueOrDefault("swiftCode", string.Empty));
                var iban = NullIfEmpty(rowData.GetValueOrDefault("iban", string.Empty));

                bool isActive = true;
                var isActiveRaw = rowData.GetValueOrDefault("isActive", string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(isActiveRaw) && !TryParseBoolean(isActiveRaw, out isActive))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = $"isActive '{isActiveRaw}' is not a valid boolean value.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                bool isFrontal = false;
                var isFrontalRaw = rowData.GetValueOrDefault("isFrontal", string.Empty).Trim();
                if (!TryParseBoolean(isFrontalRaw, out isFrontal))
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = $"isFrontal '{isFrontalRaw}' is not a valid boolean value.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                toCreate.Add((rowNum, accountName, new GeneralBankAccount
                {
                    AccountName = accountName,
                    BankName = bankName,
                    BankAccountNumber = bankAccountNumber,
                    BankBranchName = bankBranchName,
                    SwiftCode = swiftCode,
                    IBAN = iban,
                    MainAccountId = mainAccountId,
                    IsActive = isActive,
                    IsFrontal = isFrontal
                }));
            }

            if (toCreate.Count == 0)
                return result;

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                await dbContext.Set<GeneralBankAccount>().AddRangeAsync(toCreate.Select(x => x.Entity));
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Imported += toCreate.Count;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                foreach (var (rowNum, accountName, _) in toCreate)
                {
                    result.Failed.Add(new GeneralBankAccountImportFailureDto
                    {
                        Row = rowNum,
                        Name = accountName,
                        Reason = "Import transaction failed and was rolled back.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
                    });
                }

                LogService.LogError(ex);
            }

            return result;
        }

        private static bool TryParseBoolean(string value, out bool parsedValue)
        {
            parsedValue = false;
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "true":
                case "1":
                case "yes":
                case "y":
                case "نعم":
                    parsedValue = true;
                    return true;
                case "false":
                case "0":
                case "no":
                case "n":
                case "لا":
                    parsedValue = false;
                    return true;
                default:
                    return false;
            }
        }

        private static string? NullIfEmpty(string value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static string NormalizeLookupKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var trimmed = value.Trim();
            var withoutInvisible = trimmed
                .Replace("\u200B", string.Empty)
                .Replace("\u200C", string.Empty)
                .Replace("\u200D", string.Empty)
                .Replace("\uFEFF", string.Empty);

            var normalized = Regex.Replace(withoutInvisible, "\\s+", " ").Trim();
            if (normalized.StartsWith("="))
            {
                normalized = normalized[1..].Trim();
            }

            normalized = normalized.Trim('\'', '"', '`', '“', '”', '‘', '’');
            return normalized.Trim();
        }
    }
}
