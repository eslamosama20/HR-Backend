using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Return.Purchase;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.Application.Common.Constants;
using SmartAndPro.ERP.Application.Services.Accounting;
using SmartAndPro.ERP.Domains.ValueObjects;

namespace SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement;

public class SupplierService
    : ServiceBase
    <
        SupplierDetails,
        SupplierDetailsPostDto,
        SupplierDetailsPutDto,
        SupplierDetailsResponseDto,
        SupplierDetailsFiltersDto
    >, ISupplierService

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public SupplierService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory factory)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = factory;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────
    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // ─────────────────────────────────────────────────────────────────────────
    // AddAsync — fixed MainAccount uniqueness handling
    // ─────────────────────────────────────────────────────────────────────────
    public override async Task<OperationResponse<SupplierDetailsResponseDto>> AddAsync(SupplierDetailsPostDto supplierDetailsDto)
    {
        var supplierRepo = _repositoriesFactory.GetRepository<SupplierDetails>(true);
        var dbContext    = supplierRepo.GetDbContext();

        // Fallback: If English Name is missing, use Arabic Name
        if (string.IsNullOrWhiteSpace(supplierDetailsDto.EnglishName))
            supplierDetailsDto.EnglishName = supplierDetailsDto.ArabicName;

        // Guard: check for existing supplier by ArabicName (IgnoreQueryFilters catches soft-deleted)
        var normalizedArabic = supplierDetailsDto.ArabicName?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(normalizedArabic))
        {
            var existsByArabic = await dbContext.Set<SupplierDetails>()
                .IgnoreQueryFilters()
                .AnyAsync(s => s.ArabicName == normalizedArabic);
            if (existsByArabic)
            {
                // Idempotent: return the existing supplier instead of failing
                var existing = await dbContext.Set<SupplierDetails>()
                    .IgnoreQueryFilters()
                    .Include(s => s.MainAccount)
                    .Include(s => s.SupplierStatus)
                    .FirstAsync(s => s.ArabicName == normalizedArabic);
                var existingMapped = _mapper.Map<SupplierDetailsResponseDto>(existing);
                return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.Success, existingMapped);
            }
        }

        // Guard: check for existing supplier by TaxNumber (IgnoreQueryFilters)
        var normalizedTax = supplierDetailsDto.TaxNumber?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedTax))
        {
            var existsByTax = await dbContext.Set<SupplierDetails>()
                .IgnoreQueryFilters()
                .AnyAsync(s => s.TaxNumber == normalizedTax);
            if (existsByTax)
            {
                var existing = await dbContext.Set<SupplierDetails>()
                    .IgnoreQueryFilters()
                    .Include(s => s.MainAccount)
                    .Include(s => s.SupplierStatus)
                    .FirstAsync(s => s.TaxNumber == normalizedTax);
                var existingMapped = _mapper.Map<SupplierDetailsResponseDto>(existing);
                return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.Success, existingMapped);
            }
        }

        if (supplierDetailsDto.MainAccountId == null)
        {
            var supplierDetailAccountId = await dbContext.Set<DetailAccount>()
                .IgnoreQueryFilters()
                .Where(da => da.ArabicName == "دائنون")
                .Select(da => da.Id)
                .FirstOrDefaultAsync();

            if (supplierDetailAccountId == Guid.Empty)
                throw new Exception("Supplier Detail Account 'دائنون' not found");

            // Reuse existing MainAccount by ArabicName (IgnoreQueryFilters catches soft-deleted)
            var existingAccount = await dbContext.Set<MainAccount>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ma => ma.ArabicName == normalizedArabic);

            if (existingAccount != null)
            {
                supplierDetailsDto.MainAccountId = existingAccount.Id;
            }
            else
            {
                // Ensure EnglishName is unique on MainAccount
                var englishName = supplierDetailsDto.EnglishName?.Trim() ?? normalizedArabic;
                var englishTaken = await dbContext.Set<MainAccount>()
                    .IgnoreQueryFilters()
                    .AnyAsync(ma => ma.EnglishName == englishName);
                if (englishTaken)
                    englishName = englishName + "_" + Guid.NewGuid().ToString("N")[..6];

                var supplierAccount = new MainAccount
                {
                    ArabicName      = normalizedArabic,
                    EnglishName     = englishName,
                    DetailAccountId = supplierDetailAccountId,
                    CreatedAt       = DateTime.UtcNow
                };
                var mainAccountRepo = _repositoriesFactory.GetRepository<MainAccount>(true);
                var addResult = await mainAccountRepo.Add(supplierAccount);
                if (addResult.Data == null)
                    throw new Exception("Supplier MainAccount not added");
                supplierDetailsDto.MainAccountId = addResult.Data.Id;
            }
        }

        try
        {
            var result = await base.AddAsync(supplierDetailsDto);
            return result;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            var inner = ex.InnerException;
            while (inner != null) { msg += " | INNER: " + inner.Message; inner = inner.InnerException; }
            LogService.LogError(ex, msg);
            return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.GeneralFail, null, message: msg);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Excel bulk import — POST /api/inventory/supplierDetails/import
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<SupplierDetailsImportResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var result = new SupplierDetailsImportResultDto();

        var headerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["arabicname"]              = "arabicName",
            ["arabic name"]             = "arabicName",
            ["الاسم العربي"]            = "arabicName",
            ["اسم عربي"]               = "arabicName",
            ["englishname"]             = "englishName",
            ["english name"]            = "englishName",
            ["الاسم الانجليزي"]         = "englishName",
            ["الاسم الإنجليزي"]         = "englishName",
            ["taxnumber"]               = "taxNumber",
            ["tax number"]              = "taxNumber",
            ["الرقم الضريبي"]           = "taxNumber",
            ["الرقم الضريبى"]           = "taxNumber",
            ["contactnumber"]           = "contactNumber",
            ["contact number"]          = "contactNumber",
            ["رقم الجوال"]              = "contactNumber",
            ["رقم الهاتف"]              = "contactNumber",
            ["رقم التواصل"]             = "contactNumber",
            ["email"]                   = "email",
            ["البريد الالكتروني"]       = "email",
            ["البريد الإلكتروني"]       = "email",
            ["bankname"]                = "bankName",
            ["bank name"]               = "bankName",
            ["اسم البنك"]               = "bankName",
            ["bankaccountnumber"]        = "bankAccountNumber",
            ["bank account number"]      = "bankAccountNumber",
            ["رقم الحساب البنكي"]        = "bankAccountNumber",
            ["رقم الحساب"]              = "bankAccountNumber",
            ["iban"]                    = "iban",
            ["ايبان"]                   = "iban",
            ["street"]                  = "street",
            ["الشارع"]                  = "street",
            ["city"]                    = "city",
            ["المدينة"]                 = "city",
            ["country"]                 = "country",
            ["الدولة"]                  = "country",
            ["zipcode"]                 = "zipCode",
            ["zip code"]                = "zipCode",
            ["الرمز البريدي"]           = "zipCode",
            ["address"]                 = "address",
            ["العنوان"]                 = "address",
            ["mainaccountid"]           = "mainAccountId",
            ["main account id"]         = "mainAccountId",
            ["الحساب الرئيسي"]          = "mainAccountId",
        };

        using var package = new ExcelPackage(fileStream);
        var ws = package.Workbook.Worksheets[0];
        if (ws == null || ws.Dimension == null) return result;

        int totalRows    = ws.Dimension.End.Row;
        int totalColumns = ws.Dimension.End.Column;

        var colMap = new Dictionary<int, string>();
        for (int col = 1; col <= totalColumns; col++)
        {
            var raw = ws.Cells[1, col].Text?.Trim() ?? string.Empty;
            if (headerAliases.TryGetValue(raw, out var c1)) colMap[col] = c1;
            else if (headerAliases.TryGetValue(raw.ToLowerInvariant(), out var c2)) colMap[col] = c2;
        }

        if (!colMap.ContainsValue("arabicName"))
        {
            result.Failed.Add(new SupplierDetailsImportFailureDto
            {
                Row = 1, Name = string.Empty,
                Reason = "Required column 'arabicName / الاسم العربي' not found in the file header.",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
            });
            return result;
        }

        var rows = new List<(int RowNum, Dictionary<string, string> Data)>();
        for (int row = 2; row <= totalRows; row++)
        {
            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (col, field) in colMap)
                rowData[field] = ws.Cells[row, col].Text?.Trim() ?? string.Empty;
            if (rowData.Values.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add((row, rowData));
        }

        if (rows.Count == 0) return result;

        var dbContext = _repositoriesFactory.GetRepository<SupplierDetails>().GetDbContext();

        // Fetch all existing suppliers in one round-trip
        var allExisting = await dbContext.Set<SupplierDetails>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        var byTax = allExisting
            .Where(s => !string.IsNullOrWhiteSpace(s.TaxNumber))
            .GroupBy(s => s.TaxNumber!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var byArabic = allExisting
            .Where(s => !string.IsNullOrWhiteSpace(s.ArabicName))
            .GroupBy(s => s.ArabicName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Fetch DetailAccount for "دائنون" once
        var supplierDetailAccountId = await dbContext.Set<DetailAccount>()
            .IgnoreQueryFilters()
            .Where(da => da.ArabicName == "دائنون")
            .Select(da => da.Id)
            .FirstOrDefaultAsync();

        if (supplierDetailAccountId == Guid.Empty)
        {
            result.Failed.Add(new SupplierDetailsImportFailureDto
            {
                Row = 0, Name = string.Empty,
                Reason = "Supplier detail account 'دائنون' not found in the system. Cannot auto-create MainAccounts.",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
            });
            return result;
        }

        // Fetch all existing MainAccounts for lookup
        var allMainAccounts = await dbContext.Set<MainAccount>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        var mainByArabic = allMainAccounts
            .Where(m => !string.IsNullOrWhiteSpace(m.ArabicName))
            .GroupBy(m => m.ArabicName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var mainByEnglish = allMainAccounts
            .Where(m => !string.IsNullOrWhiteSpace(m.EnglishName))
            .GroupBy(m => m.EnglishName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Track newly created MainAccounts in this batch
        var newMainAccountsByArabic = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        // In-batch dedup tracking
        var processedArabicNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var processedTaxNumbers  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var toCreate = new List<SupplierDetails>();
        var toUpdate = new List<(int RowNum, string ArabicName, SupplierDetails Entity, Dictionary<string, string> Data)>();

        foreach (var (rowNum, rowData) in rows)
        {
            var arabicName = NullIfEmpty(rowData.GetValueOrDefault("arabicName"));
            if (arabicName == null)
            {
                result.Failed.Add(new SupplierDetailsImportFailureDto
                {
                    Row = rowNum,
                    Name = string.Empty,
                    Reason = "arabicName is required.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                });
                continue;
            }

            var taxNumber  = NullIfEmpty(rowData.GetValueOrDefault("taxNumber"));
            var mainAccIdRaw = NullIfEmpty(rowData.GetValueOrDefault("mainAccountId"));

            if (mainAccIdRaw != null && !Guid.TryParse(mainAccIdRaw, out _))
            {
                result.Failed.Add(new SupplierDetailsImportFailureDto
                {
                    Row = rowNum,
                    Name = arabicName,
                    Reason = "Main Account Id has an invalid format.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                });
                continue;
            }

            // In-batch dedup
            if (processedArabicNames.Contains(arabicName) || (taxNumber != null && processedTaxNumbers.Contains(taxNumber)))
            {
                var duplicateErrorCode = processedArabicNames.Contains(arabicName)
                    ? ErrorMessageConstants.ImportErrorCodes.DuplicateName
                    : ErrorMessageConstants.ImportErrorCodes.DatabaseConstraint;

                result.Failed.Add(new SupplierDetailsImportFailureDto
                {
                    Row = rowNum,
                    Name = arabicName,
                    Reason = "Supplier name or tax number is duplicated within the same import file.",
                    ErrorCode = duplicateErrorCode
                });
                continue;
            }
            processedArabicNames.Add(arabicName);
            if (taxNumber != null) processedTaxNumbers.Add(taxNumber);

            // Match existing
            if (byArabic.ContainsKey(arabicName))
            {
                result.Failed.Add(new SupplierDetailsImportFailureDto
                {
                    Row = rowNum,
                    Name = arabicName,
                    Reason = "Supplier name already exists.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                });
                continue;
            }

            SupplierDetails? existing = null;
            if (taxNumber != null && byTax.TryGetValue(taxNumber, out var byTaxMatch)) existing = byTaxMatch;

            var englishName = NullIfEmpty(rowData.GetValueOrDefault("englishName")) ?? arabicName;

            if (existing != null)
            {
                // Update
                existing.ArabicName       = arabicName;
                existing.EnglishName      = englishName;
                existing.TaxNumber        = taxNumber ?? existing.TaxNumber;
                existing.ContactNumber    = NullIfEmpty(rowData.GetValueOrDefault("contactNumber")) ?? existing.ContactNumber;
                existing.Email            = NullIfEmpty(rowData.GetValueOrDefault("email"))         ?? existing.Email;
                existing.BankName         = NullIfEmpty(rowData.GetValueOrDefault("bankName"))      ?? existing.BankName;
                existing.BankAccountNumber = NullIfEmpty(rowData.GetValueOrDefault("bankAccountNumber")) ?? existing.BankAccountNumber;
                existing.IBAN             = NullIfEmpty(rowData.GetValueOrDefault("iban"))          ?? existing.IBAN;

                // Address
                var street  = NullIfEmpty(rowData.GetValueOrDefault("street"));
                var city    = NullIfEmpty(rowData.GetValueOrDefault("city"));
                var country = NullIfEmpty(rowData.GetValueOrDefault("country"));
                var zipCode = NullIfEmpty(rowData.GetValueOrDefault("zipCode"));
                if (street != null || city != null || country != null || zipCode != null)
                {
                    var parts = new[] { street, city, country, zipCode }.Where(p => p != null);
                    existing.Address = string.Join(", ", parts);
                }

                toUpdate.Add((rowNum, arabicName, existing, rowData));
                result.Updated++;
            }
            else
            {
                // Resolve MainAccountId
                Guid? mainAccountId = null;
                if (mainByArabic.TryGetValue(arabicName, out var existingAccount))
                {
                    mainAccountId = existingAccount.Id;
                }
                else if (newMainAccountsByArabic.TryGetValue(arabicName, out var newId))
                {
                    mainAccountId = newId;
                }
                else
                {
                    // Create a new MainAccount
                    var engName = englishName;
                    if (mainByEnglish.ContainsKey(engName))
                        engName = engName + "_" + Guid.NewGuid().ToString("N")[..6];

                    var newAccount = new MainAccount
                    {
                        ArabicName      = arabicName,
                        EnglishName     = engName,
                        DetailAccountId = supplierDetailAccountId,
                        CreatedAt       = DateTime.UtcNow
                    };
                    dbContext.Set<MainAccount>().Add(newAccount);
                    await dbContext.SaveChangesAsync();
                    mainAccountId = newAccount.Id;
                    mainByArabic[arabicName]  = newAccount;
                    mainByEnglish[engName]    = newAccount;
                    newMainAccountsByArabic[arabicName] = newAccount.Id;
                }

                // Build address string
                var street  = NullIfEmpty(rowData.GetValueOrDefault("street"));
                var city    = NullIfEmpty(rowData.GetValueOrDefault("city"));
                var country = NullIfEmpty(rowData.GetValueOrDefault("country"));
                var zipCode = NullIfEmpty(rowData.GetValueOrDefault("zipCode"));
                var addressParts = new[] { street, city, country, zipCode }.Where(p => p != null);
                var addressStr = addressParts.Any() ? string.Join(", ", addressParts) : null;

                var newSupplier = new SupplierDetails
                {
                    Id                = Guid.NewGuid(),
                    ArabicName        = arabicName,
                    EnglishName       = englishName,
                    TaxNumber         = taxNumber,
                    ContactNumber     = NullIfEmpty(rowData.GetValueOrDefault("contactNumber")),
                    Email             = NullIfEmpty(rowData.GetValueOrDefault("email")),
                    BankName          = NullIfEmpty(rowData.GetValueOrDefault("bankName")),
                    BankAccountNumber = NullIfEmpty(rowData.GetValueOrDefault("bankAccountNumber")),
                    IBAN              = NullIfEmpty(rowData.GetValueOrDefault("iban")),
                    Address           = addressStr,
                    MainAccountId     = mainAccountId,
                    IsActive          = true,
                    IsFrontal         = false,
                    CreatedAt         = DateTime.UtcNow,
                    CreatedBy         = Guid.Empty
                };

                toCreate.Add(newSupplier);
            }
        }

        // Bulk save creates
        if (toCreate.Any())
        {
            try
            {
                dbContext.Set<SupplierDetails>().AddRange(toCreate);
                await dbContext.SaveChangesAsync();
                result.Imported += toCreate.Count;
            }
            catch (Exception ex)
            {
                var classifiedError = ClassifyImportException(ex);
                foreach (var s in toCreate)
                {
                    result.Failed.Add(new SupplierDetailsImportFailureDto
                    {
                        Row = 0,
                        Name = s.ArabicName,
                        Reason = classifiedError.Reason,
                        ErrorCode = classifiedError.ErrorCode
                    });
                }
            }
        }

        // Save updates individually to isolate failures
        foreach (var (rowNum, arabicName, entity, _) in toUpdate)
        {
            try
            {
                dbContext.Set<SupplierDetails>().Update(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Updated--;
                var classifiedError = ClassifyImportException(ex);
                result.Failed.Add(new SupplierDetailsImportFailureDto
                {
                    Row = rowNum,
                    Name = arabicName,
                    Reason = classifiedError.Reason,
                    ErrorCode = classifiedError.ErrorCode
                });
            }
        }

        return result;
    }

    private static (string ErrorCode, string Reason) ClassifyImportException(Exception ex)
    {
        var message = FlattenExceptionMessages(ex);

        if (message.Contains("cannot be tracked", StringComparison.OrdinalIgnoreCase))
        {
            return (ErrorMessageConstants.ImportErrorCodes.TransactionFailed, "Import operation failed due to a temporary conflict. Please try again.");
        }

        if (ex is FormatException
            || message.Contains("format", StringComparison.OrdinalIgnoreCase))
        {
            return (ErrorMessageConstants.ImportErrorCodes.InvalidFormat, "One or more values have an invalid format.");
        }

        if (ex is ArgumentException
            || message.Contains("required", StringComparison.OrdinalIgnoreCase)
            || message.Contains("cannot be null", StringComparison.OrdinalIgnoreCase)
            || message.Contains("empty", StringComparison.OrdinalIgnoreCase))
        {
            return (ErrorMessageConstants.ImportErrorCodes.MissingRequiredField, "A required field is missing.");
        }

        if (message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase))
        {
            return (ErrorMessageConstants.ImportErrorCodes.DuplicateName, "Supplier name already exists.");
        }

        if (ex is DbUpdateException
            || message.Contains("constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("conflict", StringComparison.OrdinalIgnoreCase))
        {
            return (ErrorMessageConstants.ImportErrorCodes.DatabaseConstraint, "The row contains invalid or conflicting data.");
        }

        return (ErrorMessageConstants.ImportErrorCodes.TransactionFailed, "Import operation failed. Please try again or contact support.");
    }

    private static string FlattenExceptionMessages(Exception ex)
    {
        var messages = ex.Message;
        var inner = ex.InnerException;
        while (inner != null)
        {
            messages += " | " + inner.Message;
            inner = inner.InnerException;
        }

        return messages;
    }

    public override async Task<OperationResponse<SupplierDetailsResponseDto>> UpdateAsync(SupplierDetailsPutDto request)
    {
        GuidHelper.NormalizeGuids(request);

        // Fallback for English Name if empty
        if (string.IsNullOrWhiteSpace(request.EnglishName))
            request.EnglishName = request.ArabicName;

        // Load tracked entity directly from DbContext (NOT via Filter which uses AsNoTracking)
        // so EF Core can detect changes to the Address value-converted property
        var supplierRepo = _repositoriesFactory.GetRepository<SupplierDetails>(true);
        var dbContext    = supplierRepo.GetDbContext();

        var existingSupplier = await dbContext.Set<SupplierDetails>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.Id);

        if (existingSupplier != null)
        {
            // Capture the supplier's PREVIOUS names BEFORE mapping so we can tell whether
            // the linked MainAccount is the auto-created one (named after the supplier)
            // or a manually-linked/shared account that must never be renamed.
            var previousArabicName  = existingSupplier.ArabicName;
            var previousEnglishName = existingSupplier.EnglishName;

            // Map all scalar fields from the DTO onto the tracked entity
            _mapper.Map(request, existingSupplier);

            // Always explicitly set Address and mark as modified so EF Core persists the column
            existingSupplier.Address = (Address)(request.Address ?? string.Empty);
            dbContext.Entry(existingSupplier).Property(x => x.Address).IsModified = true;

            if (existingSupplier.MainAccountId.HasValue)
            {
                // Update linked MainAccount names
                var mainAccountRepo = _repositoriesFactory.GetRepository<MainAccount>(true);
                var mainAccountDbContext = mainAccountRepo.GetDbContext();
                var mainAccount = await mainAccountDbContext.Set<MainAccount>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == existingSupplier.MainAccountId.Value);

                if (mainAccount != null)
                {
                    // Only rename when the linked account IS the auto-created one
                    // (its names match the supplier's previous names). Manually-linked
                    // or shared accounts (e.g. «موردين عام») must never be renamed.
                    var isAutoCreatedAccount =
                        string.Equals(mainAccount.ArabicName,  previousArabicName,  StringComparison.Ordinal) &&
                        string.Equals(mainAccount.EnglishName, previousEnglishName, StringComparison.Ordinal);

                    if (isAutoCreatedAccount)
                    {
                        mainAccount.EnglishName = request.EnglishName;
                        mainAccount.ArabicName  = request.ArabicName;
                    }
                }
            }

            await dbContext.SaveChangesAsync();

            var responseData = _mapper.Map<SupplierDetailsResponseDto>(existingSupplier);
            return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.Success, responseData);
        }

        return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.NotFound, null);
    }

    public async Task<OperationResponse<List<SupplierDetailsResponseDto>>> GetAllFrontalAsync()
    {
        var supplierDetailsRepository = _repositoriesFactory.GetRepository<SupplierDetails>();
        var frontalSuppliers = await supplierDetailsRepository.Filter(s => s.IsFrontal);

        if (frontalSuppliers.Data == null || !frontalSuppliers.Data.Any())
        {
            return new OperationResponse<List<SupplierDetailsResponseDto>>(ErrorCodes.NoContent, null);
        }
        var frontalSuppliersMapped = _mapper.Map<List<SupplierDetailsResponseDto>>(frontalSuppliers.Data);

        return new OperationResponse<List<SupplierDetailsResponseDto>>(ErrorCodes.Success, frontalSuppliersMapped);
    }

    public async Task<OperationResponse<SupplierDetailsResponseDto>> MakeFrontalAsync(Guid supplierId)
    {
        var supplierDetailsRepository = _repositoriesFactory.GetRepository<SupplierDetails>();
        var supplierDetailsQuery = await supplierDetailsRepository.Filter(x => x.Id == supplierId);
        var supplierDetails = supplierDetailsQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (supplierDetails == null)
        {
            return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.NotFound, null);
        }

        supplierDetails.IsFrontal = true;
        var updateResult = await supplierDetailsRepository.Update(supplierDetails);

        if (updateResult.Data == null)
        {
            return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.GeneralFail, null);
        }

        var mappedSupplier = _mapper.Map<SupplierDetailsResponseDto>(updateResult.Data);
        return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.Success, mappedSupplier);
    }
    

    public override async Task<OperationResponse<SupplierDetailsResponseDto>> DeleteAsync(Guid id)
    {
        // Reuse range logic for consistent safety checks
        var response = await DeleteRangeAsync(new List<Guid> { id });
        return response;
    }

    public override async Task<OperationResponse<SupplierDetailsResponseDto>> DeleteRangeAsync(List<Guid> ids)
    {
        var supplierRepo = _repositoriesFactory.GetRepository<SupplierDetails>(true);

        try
        {
            await supplierRepo.BeginTransactionAsync();
            var dbContext = supplierRepo.GetDbContext();

            // ── Step 1: Load suppliers (tracked — needed for SaveChanges to detect FK change) ─
            var suppliers = await dbContext.Set<SupplierDetails>()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (suppliers == null || !suppliers.Any())
                return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.NotFound, null);

            var accountsToDelete = suppliers
                .Where(s => s.MainAccountId.HasValue)
                .Select(s => s.MainAccountId!.Value)
                .Distinct()
                .ToList();

            // ── Step 2: Pre-flight constraint check (before any modification) ─────
            // If any linked account has journal entries, fail fast before touching anything.
            if (accountsToDelete.Any())
            {
                var hasLinkedEntries = await dbContext.Set<TransactionEntry>()
                    .AnyAsync(te => accountsToDelete.Contains(te.MainAccountId));

                if (hasLinkedEntries)
                    return new OperationResponse<SupplierDetailsResponseDto>(
                        ErrorCodes.Conflict, null,
                        message: "لا يمكن حذف المورد لأن الحساب المرتبط به يحتوي على قيود محاسبية.");
            }

            // ── Step 3: Break the FK (same transaction) then hard-delete accounts ─
            // We null out MainAccountId first so the DB FK constraint does not block
            // the account deletion. All of this runs inside the same DB transaction,
            // so if anything fails everything rolls back atomically.
            if (accountsToDelete.Any())
            {
                foreach (var supplier in suppliers.Where(s => s.MainAccountId.HasValue))
                {
                    supplier.MainAccountId = null;
                    supplier.MainAccount   = null;
                }
                await dbContext.SaveChangesAsync();

                // Hard-delete the accounts directly via the shared DbContext (same transaction).
                var accountsEntities = await dbContext.Set<MainAccount>()
                    .Where(ma => accountsToDelete.Contains(ma.Id))
                    .ToListAsync();

                dbContext.Set<MainAccount>().RemoveRange(accountsEntities);
                await dbContext.SaveChangesAsync();
            }

            // ── Step 4: Soft-delete the suppliers ────────────────────────────────
            foreach (var supplier in suppliers)
                supplier.SoftDelete();

            await supplierRepo.UpdateRange(suppliers);

            await supplierRepo.CommitTransactionAsync();
            return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            await supplierRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            var fullMessage = ex.Message;
            var inner = ex.InnerException;
            while (inner != null) { fullMessage += " | " + inner.Message; inner = inner.InnerException; }
            return new OperationResponse<SupplierDetailsResponseDto>(ErrorCodes.GeneralFail, null, message: fullMessage);
        }
    }
}
