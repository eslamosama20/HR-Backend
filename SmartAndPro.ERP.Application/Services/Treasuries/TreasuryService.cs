using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Constants;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Text.RegularExpressions;

namespace SmartAndPro.ERP.Application.Services.Treasuries
{
    public class TreasuryService : ServiceBase
        <
        Treasury,
        TreasuryPostDto,
        TreasuryPutDto,
        TreasuryResponseDto,
        TreasuryFiltersDto
        >,
        ITreasuryService
    {
        #region Failds
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public TreasuryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) : base(repositoriesFactory, appSettings, mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        public async Task<OperationResponse<List<TreasuryResponseDto>>> GetAllFrontalAsync()
        {
            var treasuryRepository = _repositoriesFactory.GetRepository<Treasury>();
            var frontalTreasuries = await treasuryRepository.Filter(s => s.IsFrontal);

            if (frontalTreasuries.Data == null || !frontalTreasuries.Data.Any())
            {
                return new OperationResponse<List<TreasuryResponseDto>>(ErrorCodes.NoContent, null);
            }
            var frontalTreasuriesMapped = _mapper.Map<List<TreasuryResponseDto>>(frontalTreasuries.Data);

            return new OperationResponse<List<TreasuryResponseDto>>(ErrorCodes.Success, frontalTreasuriesMapped);
        }

        public async Task<OperationResponse<TreasuryResponseDto>> MakeFrontalAsync(Guid treasuryId)
        {
            var treasuryRepository = _repositoriesFactory.GetRepository<Treasury>();
            var treasuryQuery = await treasuryRepository.Filter(x => x.Id == treasuryId);
            var treasury = treasuryQuery.Data?.FirstOrDefault();
            if (treasury == null)
            {
                return new OperationResponse<TreasuryResponseDto>(ErrorCodes.NotFound, null);
            }

            treasury.IsFrontal = true;
            var updateResult = await treasuryRepository.Update(treasury);

            if (updateResult.Data == null)
            {
                return new OperationResponse<TreasuryResponseDto>(ErrorCodes.GeneralFail, null);
            }

            var treasuryMapped = _mapper.Map<TreasuryResponseDto>(updateResult.Data);
            return new OperationResponse<TreasuryResponseDto>(ErrorCodes.Success, treasuryMapped);
        }

        public async Task<TreasuryImportResultDto> ImportFromExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var result = new TreasuryImportResultDto();

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
                if (ImportFieldMappings.Treasury.HeaderAliases.TryGetValue(raw, out var canonical))
                    colMap[col] = canonical;
                else if (ImportFieldMappings.Treasury.HeaderAliases.TryGetValue(raw.ToLowerInvariant(), out var canonical2))
                    colMap[col] = canonical2;
            }

            if (!colMap.ContainsValue("name"))
            {
                result.Failed.Add(new TreasuryImportFailureDto
                {
                    Row = 1,
                    Name = string.Empty,
                    Reason = "Required column 'name / اسم الخزينة' not found in the file header.",
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

            var dbContext = _repositoriesFactory.GetRepository<Treasury>().GetDbContext();

            var existingTreasuries = await dbContext.Set<Treasury>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                .Select(t => t.Name.Trim())
                .ToListAsync();

            var existingNames = new HashSet<string>(existingTreasuries, StringComparer.OrdinalIgnoreCase);
            var seenInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

            var stagedRows = new List<(int RowNum, string Name, Treasury Entity)>();

            foreach (var (rowNum, rowData) in parsedRows)
            {
                var name = rowData.GetValueOrDefault("name", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = string.Empty,
                        Reason = "Name is required.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                    });
                    continue;
                }

                if (!seenInBatch.Add(name))
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
                        Reason = $"Treasury '{name}' is duplicated in this file.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                    });
                    continue;
                }

                if (existingNames.Contains(name))
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
                        Reason = $"Treasury '{name}' already exists.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                    });
                    continue;
                }

                var mainAccountIdRaw = rowData.GetValueOrDefault("mainAccountId", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(mainAccountIdRaw))
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
                        Reason = "mainAccountId is required for treasury creation.",
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
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
                        Reason = $"mainAccountId '{mainAccountIdRaw}' was not found (GUID or account name).",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                if (!mainAccountIdSet.Contains(mainAccountId))
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
                        Reason = $"mainAccountId '{mainAccountIdRaw}' was not found (GUID or account name).",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                bool isFrontal = false;
                var isFrontalRaw = rowData.GetValueOrDefault("isFrontal", string.Empty).Trim();
                if (!TryParseBoolean(isFrontalRaw, out isFrontal))
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
                        Reason = $"isFrontal '{isFrontalRaw}' is not a valid boolean value.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }

                stagedRows.Add((rowNum, name, new Treasury
                {
                    Name = name,
                    MainAccountId = mainAccountId,
                    IsFrontal = isFrontal,
                    IsActive = true
                }));
            }

            if (stagedRows.Count == 0)
                return result;

            var toCreate = stagedRows;

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                await dbContext.Set<Treasury>().AddRangeAsync(toCreate.Select(x => x.Entity));
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Imported += toCreate.Count;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                foreach (var (rowNum, name, _) in toCreate)
                {
                    result.Failed.Add(new TreasuryImportFailureDto
                    {
                        Row = rowNum,
                        Name = name,
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

            // Excel/user-entered values sometimes include wrapping quotes or formula prefix
            // like "mohamed", 'mohamed, or ="mohamed".
            if (normalized.StartsWith("="))
            {
                normalized = normalized[1..].Trim();
            }

            normalized = normalized.Trim('\'', '"', '`', '“', '”', '‘', '’');

            return normalized.Trim();
        }
        #endregion
    }
}
