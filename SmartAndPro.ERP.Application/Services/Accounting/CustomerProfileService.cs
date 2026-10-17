using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Notes;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.HotelAllotmentInvoices;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.Application.Common.Constants;
using SmartAndPro.ERP.Application.Common.Extensions;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Domains.Entities.Accounting;

namespace SmartAndPro.ERP.Application.Services.Accounting;



public class CustomerProfileService
    : ServiceBase
    <
        CustomerProfile,
        CustomerProfilePostDto,
        CustomerProfilePutDto,
        CustomerProfileResponseDto,
        CustomerProfileFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private static long _fallbackTaxSeed = DateTime.UtcNow.Ticks;
    
    public CustomerProfileService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper, ServiceBaseFactory factory) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = factory;
    }
    
    // ─────────────────────────────────────────────────────────────────────────
    // Excel bulk import — POST /api/accounting/customerProfile/import
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses the uploaded .xlsx file server-side (EPPlus), performs a bulk upsert
    /// inside a single transaction, and returns a structured result with counts and
    /// per-row failure details.  The frontend no longer needs to parse Excel or loop
    /// over individual POST requests.
    /// </summary>
    public async Task<CustomerProfileImportResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var result = new CustomerProfileImportResultDto();

        // ── Column-header aliases (same set as the frontend, AR + EN) ────────
        var headerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["arabicname"]            = "arabicName",
            ["arabic name"]           = "arabicName",
            ["arabic_name"]           = "arabicName",
            ["الاسم العربي"]          = "arabicName",
            ["اسم عربي"]              = "arabicName",
            ["englishname"]           = "englishName",
            ["english name"]          = "englishName",
            ["english_name"]          = "englishName",
            ["الاسم الانجليزي"]       = "englishName",
            ["الاسم الإنجليزي"]       = "englishName",
            ["اسم انجليزي"]           = "englishName",
            ["taxnumber"]             = "taxNumber",
            ["tax number"]            = "taxNumber",
            ["tax_number"]            = "taxNumber",
            ["الرقم الضريبي"]         = "taxNumber",
            ["الرقم الضريبى"]         = "taxNumber",
            ["contactnumber"]         = "contactNumber",
            ["contact number"]        = "contactNumber",
            ["contact_number"]        = "contactNumber",
            ["رقم التواصل"]           = "contactNumber",
            ["رقم الهاتف"]            = "contactNumber",
            ["رقم الجوال"]            = "contactNumber",
            ["email"]                 = "email",
            ["البريد الالكتروني"]     = "email",
            ["البريد الإلكتروني"]     = "email",
            ["bankname"]              = "bankName",
            ["bank name"]             = "bankName",
            ["bank_name"]             = "bankName",
            ["اسم البنك"]             = "bankName",
            ["bankaccountnumber"]     = "bankAccountNumber",
            ["bank account number"]   = "bankAccountNumber",
            ["bank_account_number"]   = "bankAccountNumber",
            ["رقم الحساب البنكي"]     = "bankAccountNumber",
            ["رقم الحساب"]            = "bankAccountNumber",
            ["iban"]                  = "iban",
            ["ايبان"]                 = "iban",
            ["street"]                = "street",
            ["الشارع"]                = "street",
            ["district"]              = "district",
            ["الحي"]                  = "district",
            ["shortaddress"]          = "shortAddress",
            ["short address"]         = "shortAddress",
            ["short_address"]         = "shortAddress",
            ["العنوان المختصر"]       = "shortAddress",
            ["buildingnumber"]        = "buildingNumber",
            ["building number"]       = "buildingNumber",
            ["building_number"]       = "buildingNumber",
            ["رقم المبنى"]            = "buildingNumber",
            ["رقم المبني"]            = "buildingNumber",
            ["postalcode"]            = "postalCode",
            ["postal code"]           = "postalCode",
            ["postal_code"]           = "postalCode",
            ["الرمز البريدي"]         = "postalCode",
            ["city"]                  = "city",
            ["المدينة"]               = "city",
            ["country"]               = "country",
            ["الدولة"]                = "country",
            ["additionalcontactnumber"] = "additionalContactNumber",
            ["additional contact number"] = "additionalContactNumber",
            ["additional_contact_number"] = "additionalContactNumber",
            ["رقم اتصال اضافي"]         = "additionalContactNumber",
            ["رقم اتصال إضافي"]         = "additionalContactNumber",
            ["fax"]                   = "fax",
            ["فاكس"]                  = "fax",
            ["website"]               = "website",
            ["الموقع الالكتروني"]     = "website",
            ["الموقع الإلكتروني"]     = "website",
            ["instructions"]          = "instructions",
            ["تعليمات"]               = "instructions",
            ["gaztnumber"]            = "gaztNumber",
            ["gazt number"]           = "gaztNumber",
            ["gazt_number"]           = "gaztNumber",
            ["gazt no."]              = "gaztNumber",
            ["gazt no"]               = "gaztNumber",
            ["mainaccountid"]         = "mainAccountId",
            ["main account id"]       = "mainAccountId",
            ["main_account_id"]       = "mainAccountId",
            ["معرف الحساب الرئيسي"]   = "mainAccountId",
            ["الحساب الرئيسي"]        = "mainAccountId",
        };

        // ── Parse Excel ───────────────────────────────────────────────────────
        using var package = new ExcelPackage(fileStream);
        var ws = package.Workbook.Worksheets[0];
        if (ws == null || ws.Dimension == null)
            return result; // empty file

        int totalRows    = ws.Dimension.End.Row;
        int totalColumns = ws.Dimension.End.Column;

        // Map column index → canonical field name
        var colMap = new Dictionary<int, string>(); // colIndex → "arabicName" etc.
        int headerRow = 1;
        for (int col = 1; col <= totalColumns; col++)
        {
            var raw = ws.Cells[headerRow, col].Text?.Trim() ?? string.Empty;
            if (headerAliases.TryGetValue(raw.ToLowerInvariant(), out var canonical))
                colMap[col] = canonical;
            else if (headerAliases.TryGetValue(raw, out var canonical2))
                colMap[col] = canonical2;
        }

        if (!colMap.ContainsValue("arabicName"))
        {
            // Cannot identify the required arabicName column — fail the whole import
            result.Failed.Add(new CustomerProfileImportFailureDto
            {
                Row    = 1,
                Name   = string.Empty,
                Reason = "Required column 'arabicName / الاسم العربي' not found in the file header.",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
            });
            return result;
        }

        // ── Parse all data rows into raw dictionaries ─────────────────────────
        var rows = new List<(int RowNum, Dictionary<string, string> Data)>();
        for (int row = headerRow + 1; row <= totalRows; row++)
        {
            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (col, field) in colMap)
                rowData[field] = ws.Cells[row, col].Text?.Trim() ?? string.Empty;

            // Skip completely empty rows
            if (rowData.Values.All(string.IsNullOrWhiteSpace))
                continue;

            rows.Add((row, rowData));
        }

        if (rows.Count == 0)
            return result;

        // ── Single DB round-trip: fetch all existing customers ────────────────
        var dbContext = _repositoriesFactory.GetRepository<CustomerProfile>().GetDbContext();

        var allExisting = await dbContext.Set<CustomerProfile>()
            .IgnoreQueryFilters()
            .Include(c => c.MainAccount)
            .AsNoTracking()
            .ToListAsync();

        var byTax = allExisting
            .Where(c => !string.IsNullOrWhiteSpace(c.TaxNumber))
            .GroupBy(c => c.TaxNumber!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var byArabic = allExisting
            .Where(c => !string.IsNullOrWhiteSpace(c.ArabicName))
            .GroupBy(c => c.ArabicName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Fetch the "العملاء" DetailAccount Id once
        var customerDetailAccountId = await dbContext.Set<DetailAccount>()
            .IgnoreQueryFilters()
            .Where(da => da.ArabicName == "العملاء")
            .Select(da => da.Id)
            .FirstOrDefaultAsync();

        if (customerDetailAccountId == Guid.Empty)
        {
            result.Failed.Add(new CustomerProfileImportFailureDto
            {
                Row    = 0,
                Name   = string.Empty,
                Reason = "System configuration error: 'العملاء' DetailAccount not found.",
                ErrorCode = ErrorMessageConstants.ImportErrorCodes.TransactionFailed
            });
            return result;
        }

        // Fetch all existing MainAccounts for duplicate-name checking (in-memory)
        var allMainAccounts = await dbContext.Set<MainAccount>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(ma => new { ma.Id, ma.ArabicName, ma.EnglishName })
            .ToListAsync();

        var mainByArabic  = allMainAccounts
            .Where(m => !string.IsNullOrWhiteSpace(m.ArabicName))
            .GroupBy(m => m.ArabicName!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var mainEnglishNames = new HashSet<string>(
            allMainAccounts
                .Where(m => !string.IsNullOrWhiteSpace(m.EnglishName))
                .Select(m => m.EnglishName!.Trim()),
            StringComparer.OrdinalIgnoreCase);

        // ── Classify each row ─────────────────────────────────────────────────
        var toCreate = new List<(int RowNum, string ArabicName, CustomerProfile Profile, MainAccount? NewMainAccount)>();
        var toUpdate = new List<(int RowNum, string ArabicName, CustomerProfile Existing, Dictionary<string, string> Data)>();

        // Track Arabic names seen in THIS batch to avoid in-batch duplicates
        var seenArabicInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenTaxInBatch    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (rowNum, data) in rows)
        {
            var arabicName  = data.GetValueOrDefault("arabicName", string.Empty).Trim();
            var taxNumber   = data.GetValueOrDefault("taxNumber",  string.Empty).Trim();
            var englishName = data.GetValueOrDefault("englishName", string.Empty).Trim();
            var mainAccIdRaw = data.GetValueOrDefault("mainAccountId", string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(arabicName))
            {
                result.Failed.Add(new CustomerProfileImportFailureDto
                {
                    Row    = rowNum,
                    Name   = arabicName,
                    Reason = "Arabic Name is required and was empty.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.MissingRequiredField
                });
                continue;
            }

            // ── Resolve mainAccountId (if supplied as a GUID string) ──────────
            Guid? suppliedMainAccountId = null;
            if (!string.IsNullOrWhiteSpace(mainAccIdRaw))
            {
                if (Guid.TryParse(mainAccIdRaw, out var parsedGuid))
                {
                    suppliedMainAccountId = parsedGuid;
                }
                else
                {
                    result.Failed.Add(new CustomerProfileImportFailureDto
                    {
                        Row = rowNum,
                        Name = arabicName,
                        Reason = "Main Account Id has an invalid format.",
                        ErrorCode = ErrorMessageConstants.ImportErrorCodes.InvalidFormat
                    });
                    continue;
                }
            }

            // ── Determine: update or create? ──────────────────────────────────
            // Business rule: if Arabic customer name already exists, do not update.
            // Report duplicate to the user instead.
            if (byArabic.ContainsKey(arabicName))
            {
                result.Failed.Add(new CustomerProfileImportFailureDto
                {
                    Row = rowNum,
                    Name = arabicName,
                    Reason = "Customer name already exists.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                });
                continue;
            }

            CustomerProfile? existing = null;
            if (!string.IsNullOrWhiteSpace(taxNumber) && byTax.TryGetValue(taxNumber, out var byTaxMatch))
                existing = byTaxMatch;

            if (existing != null)
            {
                toUpdate.Add((rowNum, arabicName, existing, data));
                continue;
            }

            // ── It's a create — check for in-batch duplicates ─────────────────
            if (seenArabicInBatch.Contains(arabicName))
            {
                result.Failed.Add(new CustomerProfileImportFailureDto
                {
                    Row    = rowNum,
                    Name   = arabicName,
                    Reason = "Customer name already exists within the same import file.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.DuplicateName
                });
                continue;
            }
            if (!string.IsNullOrWhiteSpace(taxNumber) && seenTaxInBatch.Contains(taxNumber))
            {
                result.Failed.Add(new CustomerProfileImportFailureDto
                {
                    Row    = rowNum,
                    Name   = arabicName,
                    Reason = "Tax number already exists within the same import file.",
                    ErrorCode = ErrorMessageConstants.ImportErrorCodes.DatabaseConstraint
                });
                continue;
            }

            seenArabicInBatch.Add(arabicName);
            if (!string.IsNullOrWhiteSpace(taxNumber)) seenTaxInBatch.Add(taxNumber);

            // ── Resolve or build a MainAccount for this new customer ──────────
            Guid? resolvedMainAccountId = suppliedMainAccountId;
            MainAccount? newMainAccount = null;

            if (resolvedMainAccountId == null)
            {
                if (mainByArabic.TryGetValue(arabicName, out var existingMainId))
                {
                    // Reuse existing MainAccount
                    resolvedMainAccountId = existingMainId;
                }
                else
                {
                    // Build a new MainAccount entity (saved in the same transaction below)
                    var enName = !string.IsNullOrWhiteSpace(englishName) ? englishName : arabicName;

                    // Make EnglishName unique within this batch and against existing
                    var candidate = enName;
                    while (mainEnglishNames.Contains(candidate))
                        candidate = $"{enName} ({Guid.NewGuid().ToString("N")[..6]})";

                    mainEnglishNames.Add(candidate); // reserve it for subsequent rows
                    mainByArabic[arabicName] = Guid.Empty; // placeholder — real Id set after save

                    newMainAccount = new MainAccount
                    {
                        ArabicName     = arabicName,
                        EnglishName    = candidate,
                        DetailAccountId = customerDetailAccountId,
                        CreatedAt      = DateTime.UtcNow,
                    };
                }
            }

            var profile = new CustomerProfile
            {
                ArabicName      = arabicName,
                EnglishName     = !string.IsNullOrWhiteSpace(englishName) ? englishName : arabicName,
                TaxNumber       = !string.IsNullOrWhiteSpace(taxNumber) ? taxNumber : null,
                ContactNumber   = NullIfEmpty(data.GetValueOrDefault("contactNumber", string.Empty)),
                Email           = NullIfEmpty(data.GetValueOrDefault("email",         string.Empty)),
                BankName        = NullIfEmpty(data.GetValueOrDefault("bankName",      string.Empty)),
                BankAccountNumber = NullIfEmpty(data.GetValueOrDefault("bankAccountNumber", string.Empty)),
                IBAN            = NullIfEmpty(data.GetValueOrDefault("iban",          string.Empty)),
                Street          = NullIfEmpty(data.GetValueOrDefault("street",        string.Empty)),
                District        = NullIfEmpty(data.GetValueOrDefault("district",      string.Empty)),
                ShortAddress    = NullIfEmpty(data.GetValueOrDefault("shortAddress",  string.Empty)),
                BuildingNumber  = NullIfEmpty(data.GetValueOrDefault("buildingNumber",string.Empty)),
                PostalCode      = NullIfEmpty(data.GetValueOrDefault("postalCode",    string.Empty)),
                City            = NullIfEmpty(data.GetValueOrDefault("city",          string.Empty)),
                Country         = NullIfEmpty(data.GetValueOrDefault("country",       string.Empty)),
                AdditionalContactNumber = NullIfEmpty(data.GetValueOrDefault("additionalContactNumber", string.Empty)),
                Fax             = NullIfEmpty(data.GetValueOrDefault("fax",           string.Empty)),
                Website         = NullIfEmpty(data.GetValueOrDefault("website",       string.Empty)),
                Instructions    = NullIfEmpty(data.GetValueOrDefault("instructions",  string.Empty)),
                GaztNumber      = NullIfEmpty(data.GetValueOrDefault("gaztNumber",    string.Empty)),
                IsActive        = true,
                IsFrontal       = false,
                MainAccountId   = resolvedMainAccountId,
                CreatedAt       = DateTime.UtcNow,
            };

            toCreate.Add((rowNum, arabicName, profile, newMainAccount));
        }

        // ── Execute everything in a single transaction ─────────────────────────
        var repo = _repositoriesFactory.GetRepository<CustomerProfile>(true);
        var mainRepo = _repositoriesFactory.GetRepository<MainAccount>(true);
        var txDbContext = repo.GetDbContext();

        await repo.BeginTransactionAsync();
        try
        {
            // 1. Insert new MainAccounts and wire their Ids back to profiles
            foreach (var (rowNum, arabicName, profile, newMainAccount) in toCreate)
            {
                if (newMainAccount != null && profile.MainAccountId == null)
                {
                    txDbContext.Set<MainAccount>().Add(newMainAccount);
                    await txDbContext.SaveChangesAsync();   // get the generated Id
                    profile.MainAccountId = newMainAccount.Id;

                    // Update in-memory lookup so subsequent rows can reuse it
                    if (!mainByArabic.ContainsKey(arabicName))
                        mainByArabic[arabicName] = newMainAccount.Id;
                }
            }

            // 2. Bulk-insert new CustomerProfiles
            if (toCreate.Count > 0)
            {
                var newProfiles = toCreate.Select(t => t.Profile).ToList();
                await txDbContext.Set<CustomerProfile>().AddRangeAsync(newProfiles);
                await txDbContext.SaveChangesAsync();
                result.Imported = newProfiles.Count;
            }

            // 3. Update existing CustomerProfiles
            foreach (var (rowNum, arabicName, existing, data) in toUpdate)
            {
                try
                {
                    var englishName   = data.GetValueOrDefault("englishName", string.Empty).Trim();
                    var taxNumber     = data.GetValueOrDefault("taxNumber",   string.Empty).Trim();

                    existing.ArabicName       = arabicName;
                    existing.EnglishName      = !string.IsNullOrWhiteSpace(englishName) ? englishName : arabicName;
                    existing.TaxNumber        = !string.IsNullOrWhiteSpace(taxNumber) ? taxNumber : existing.TaxNumber;
                    existing.ContactNumber    = NullIfEmpty(data.GetValueOrDefault("contactNumber",    string.Empty)) ?? existing.ContactNumber;
                    existing.Email            = NullIfEmpty(data.GetValueOrDefault("email",            string.Empty)) ?? existing.Email;
                    existing.BankName         = NullIfEmpty(data.GetValueOrDefault("bankName",         string.Empty)) ?? existing.BankName;
                    existing.BankAccountNumber= NullIfEmpty(data.GetValueOrDefault("bankAccountNumber",string.Empty)) ?? existing.BankAccountNumber;
                    existing.IBAN             = NullIfEmpty(data.GetValueOrDefault("iban",             string.Empty)) ?? existing.IBAN;
                    existing.Street           = NullIfEmpty(data.GetValueOrDefault("street",           string.Empty)) ?? existing.Street;
                    existing.District         = NullIfEmpty(data.GetValueOrDefault("district",         string.Empty)) ?? existing.District;
                    existing.ShortAddress     = NullIfEmpty(data.GetValueOrDefault("shortAddress",     string.Empty)) ?? existing.ShortAddress;
                    existing.BuildingNumber   = NullIfEmpty(data.GetValueOrDefault("buildingNumber",   string.Empty)) ?? existing.BuildingNumber;
                    existing.PostalCode       = NullIfEmpty(data.GetValueOrDefault("postalCode",       string.Empty)) ?? existing.PostalCode;
                    existing.City             = NullIfEmpty(data.GetValueOrDefault("city",             string.Empty)) ?? existing.City;
                    existing.Country          = NullIfEmpty(data.GetValueOrDefault("country",          string.Empty)) ?? existing.Country;
                    existing.AdditionalContactNumber = NullIfEmpty(data.GetValueOrDefault("additionalContactNumber", string.Empty)) ?? existing.AdditionalContactNumber;
                    existing.Fax              = NullIfEmpty(data.GetValueOrDefault("fax",              string.Empty)) ?? existing.Fax;
                    existing.Website          = NullIfEmpty(data.GetValueOrDefault("website",          string.Empty)) ?? existing.Website;
                    existing.Instructions     = NullIfEmpty(data.GetValueOrDefault("instructions",     string.Empty)) ?? existing.Instructions;
                    existing.GaztNumber       = NullIfEmpty(data.GetValueOrDefault("gaztNumber",       string.Empty)) ?? existing.GaztNumber;

                    // Also sync the linked MainAccount name
                    if (existing.MainAccount != null)
                    {
                        // Use the entity already present on `existing` to avoid tracking conflicts.
                        existing.MainAccount.ArabicName = arabicName;
                        existing.MainAccount.EnglishName = existing.EnglishName;
                    }

                    txDbContext.Set<CustomerProfile>().Update(existing);
                    result.Updated++;
                }
                catch (Exception ex)
                {
                    var classifiedError = ClassifyImportException(ex);
                    result.Failed.Add(new CustomerProfileImportFailureDto
                    {
                        Row    = rowNum,
                        Name   = arabicName,
                        Reason = classifiedError.Reason,
                        ErrorCode = classifiedError.ErrorCode
                    });
                }
            }

            await txDbContext.SaveChangesAsync();
            await repo.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            try { await repo.RollbackTransactionAsync(); } catch { /* ignore */ }
            LogService.LogError(ex);
            var classifiedError = ClassifyImportException(ex);

            result.Failed.Add(new CustomerProfileImportFailureDto
            {
                Row    = 0,
                Name   = string.Empty,
                Reason = classifiedError.Reason,
                ErrorCode = classifiedError.ErrorCode
            });
            result.Imported = 0;
            result.Updated  = 0;
        }

        return result;
    }

    // ─── tiny helper ─────────────────────────────────────────────────────────
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
            return (ErrorMessageConstants.ImportErrorCodes.DuplicateName, "Customer name already exists.");
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

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;

    public override async Task<OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>> GetAllPaginatedAsync(
        CustomerProfileFiltersDto filters,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        try
        {
            var repository = _repositoriesFactory.GetRepository<CustomerProfile>();
            var operation = await repository.GetAll();

            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(operation.ErrorCode, null);

            filters ??= new CustomerProfileFiltersDto();

            var query = operation.Data.Include(c => c.Departments).AsQueryable();

            bool isCompanyFilter = filters.IsReservationCompany ?? false;
            if (isCompanyFilter)
            {
                query = query.Where(c => c.IsReservationCompany == true);
            }
            else
            {
                // EF Core translates "!= true" as "= 0" in SQL which excludes NULLs.
                // Explicit OR NULL check ensures old records (NULL) are included.
                query = query.Where(c => c.IsReservationCompany == false || c.IsReservationCompany == null);
            }

            filters.IsReservationCompany = null;

            if (!filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }

            var totalRecords = await query.CountAsync();

            if (totalRecords == 0)
                return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(
                    ErrorCodes.Success,
                    new PaginatedResult<List<CustomerProfileResponseDto>>(0, new List<CustomerProfileResponseDto>())
                );

            pageNumber ??= 1;
            pageSize ??= 50;
            // Removed the 100 limit here to allow dropdowns to fetch all customers:
            // if (pageSize > 100) pageSize = 100;

            var skip = (pageNumber.Value - 1) * pageSize.Value;

            var paginatedData = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(pageSize.Value)
                .AsNoTracking()
                .ToListAsync();

            var mappedData = _mapper.Map<List<CustomerProfileResponseDto>>(paginatedData);

            var result = new PaginatedResult<List<CustomerProfileResponseDto>>(totalRecords, mappedData);

            return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(ErrorCodes.Success, result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(
                ErrorCodes.GeneralFail,
                null,
                message: ex.Message
            );
        }
    }

    public async Task<OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>> SearchAsync(
        string? q,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null,
        bool? isReservationCompany = null)
    {
        try
        {
            var repository = _repositoriesFactory.GetRepository<CustomerProfile>();
            var operation = await repository.GetAll();

            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(operation.ErrorCode, null);

            var query = operation.Data.Include(c => c.Departments).AsQueryable();

            // Filter by IsReservationCompany
            bool showCompanies = isReservationCompany ?? false;
            if (showCompanies)
            {
                query = query.Where(c => c.IsReservationCompany == true);
            }
            else
            {
                // Include records where IsReservationCompany is false OR null (old records)
                query = query.Where(c => c.IsReservationCompany == false || c.IsReservationCompany == null);
            }

            // Apply text search across multiple fields
            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim().ToLower();
                query = query.Where(c =>
                    (c.ArabicName != null && c.ArabicName.ToLower().Contains(search)) ||
                    (c.EnglishName != null && c.EnglishName.ToLower().Contains(search)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(search)) ||
                    (c.ContactNumber != null && c.ContactNumber.Contains(search))
                );
            }

            var totalRecords = await query.CountAsync();

            if (totalRecords == 0)
                return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(
                    ErrorCodes.Success,
                    new PaginatedResult<List<CustomerProfileResponseDto>>(0, new List<CustomerProfileResponseDto>())
                );

            pageNumber ??= 1;
            pageSize ??= 50;
            var skip = (pageNumber.Value - 1) * pageSize.Value;

            var paginatedData = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(pageSize.Value)
                .AsNoTracking()
                .ToListAsync();

            var mappedData = _mapper.Map<List<CustomerProfileResponseDto>>(paginatedData);
            var result = new PaginatedResult<List<CustomerProfileResponseDto>>(totalRecords, mappedData);

            return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(ErrorCodes.Success, result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaginatedResult<List<CustomerProfileResponseDto>>>(
                ErrorCodes.GeneralFail,
                null,
                message: ex.Message
            );
        }
    }


    public override async Task<OperationResponse<CustomerProfileResponseDto>> AddAsync(
        CustomerProfilePostDto customerProfileDto)
    {
        var customerProfileRepo = _repositoriesFactory.GetRepository<CustomerProfile>(true);
        var detailAccountRepo = _repositoriesFactory.GetRepository<DetailAccount>(true);
        var mainAccountRepo = _repositoriesFactory.GetRepository<MainAccount>(true);

        try
        {
            await customerProfileRepo.BeginTransactionAsync();

            // Fallback: If English Name is missing (e.g., from Excel import), use Arabic Name
            if (string.IsNullOrWhiteSpace(customerProfileDto.EnglishName))
            {
                customerProfileDto.EnglishName = customerProfileDto.ArabicName;
            }


            // Idempotent upsert guard: if a CustomerProfile with the same ArabicName or TaxNumber
            // already exists (including soft-deleted — IgnoreQueryFilters bypasses IsActive filter),
            // return the existing record immediately instead of failing with a 500.
            // This handles the case where the same row is sent twice (e.g. during a batch import
            // where the frontend list hasn't refreshed yet after earlier rows were created).
            var normalizedArabic = customerProfileDto.ArabicName?.Trim();
            var normalizedTax = customerProfileDto.TaxNumber?.Trim();
            var cpDbContext = customerProfileRepo.GetDbContext();

            CustomerProfile? existingCustomer = null;

            if (!string.IsNullOrWhiteSpace(normalizedTax))
            {
                existingCustomer = await cpDbContext.Set<CustomerProfile>()
                    .IgnoreQueryFilters()
                    .Include(c => c.MainAccount)
                    .FirstOrDefaultAsync(c => c.TaxNumber == normalizedTax);
            }

            if (existingCustomer == null && !string.IsNullOrWhiteSpace(normalizedArabic))
            {
                existingCustomer = await cpDbContext.Set<CustomerProfile>()
                    .IgnoreQueryFilters()
                    .Include(c => c.MainAccount)
                    .FirstOrDefaultAsync(c => c.ArabicName == normalizedArabic);
            }

            if (existingCustomer != null)
            {
                // Return the existing record — treat as success (idempotent)
                await customerProfileRepo.RollbackTransactionAsync();
                var existingDto = _mapper.Map<CustomerProfileResponseDto>(existingCustomer);
                return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.Success, existingDto);
            }

            if (customerProfileDto.MainAccountId == null)
            {
                // First, check if a MainAccount with the same ArabicName already exists.
                // MainAccount has a unique DB index on ArabicName — blindly inserting a duplicate
                // causes a SqlException (IX_MainAccount_ArabicName) that surfaces as a 500.
                // We use IgnoreQueryFilters() to also find soft-deleted accounts (IsActive=false),
                // since the unique index applies regardless of the IsActive flag.
                var normalizedArabicForAccount = customerProfileDto.ArabicName?.Trim();
                if (!string.IsNullOrWhiteSpace(normalizedArabicForAccount))
                {
                    var dbContext = mainAccountRepo.GetDbContext();
                    var existingMainAccount = await dbContext.Set<MainAccount>()
                        .IgnoreQueryFilters()
                        .Where(ma => ma.ArabicName == normalizedArabicForAccount)
                        .FirstOrDefaultAsync();

                    if (existingMainAccount != null)
                    {
                        // Reuse the existing MainAccount (active or soft-deleted) — no insert needed
                        customerProfileDto.MainAccountId = existingMainAccount.Id;
                    }
                }

                // Only create a new MainAccount if none was found above
                if (customerProfileDto.MainAccountId == null)
                {
                    var customerDetailAccountQuery = await detailAccountRepo.Filter(da => da.ArabicName == "العملاء");
                    var customerDetailAccountId = customerDetailAccountQuery.Data?.Select(da => da.Id).FirstOrDefault();
                    if (customerDetailAccountId == null || customerDetailAccountId == Guid.Empty)
                        throw new Exception("Client Detail Account not found");

                    // MainAccount.EnglishName has a unique DB index and is required.
                    // When no English name is provided, we use the Arabic name as fallback.
                    // To avoid IX_MainAccount_EnglishName collisions when multiple customers
                    // share the same Arabic name, append a short unique suffix if needed.
                    var dbContextForEn = mainAccountRepo.GetDbContext();
                    var desiredEnglishName = customerProfileDto.EnglishName?.Trim();
                    if (string.IsNullOrWhiteSpace(desiredEnglishName))
                        desiredEnglishName = customerProfileDto.ArabicName?.Trim();

                    var enNameExists = await dbContextForEn.Set<MainAccount>()
                        .IgnoreQueryFilters()
                        .AnyAsync(ma => ma.EnglishName == desiredEnglishName);
                    if (enNameExists)
                    {
                        // Append a short unique suffix to avoid the unique index violation
                        desiredEnglishName = $"{desiredEnglishName} ({Guid.NewGuid().ToString("N")[..6]})";
                    }

                    // Create the customer account
                    var customerAccount = new MainAccount
                    {
                        EnglishName = desiredEnglishName,
                        ArabicName = customerProfileDto.ArabicName,
                        DetailAccountId = (Guid)customerDetailAccountId,
                        CreatedAt = DateTime.UtcNow
                    };
                    var customerAccountAddResult = await mainAccountRepo.Add(customerAccount);
                    if (customerAccountAddResult.Data == null)
                        throw new Exception("Customer Account not added");

                    customerProfileDto.MainAccountId = customerAccountAddResult.Data.Id;
                }
            }
            var result = await base.AddAsync(customerProfileDto);
            if (result.ErrorCode != ErrorCodes.Success || result.Data == null)
                throw new Exception("Customer Profile not added");

            if (customerProfileDto.Departments != null && customerProfileDto.Departments.Any())
            {
                var dbContext = customerProfileRepo.GetDbContext();
                await SyncCustomerDepartmentsAsync(result.Data.Id, customerProfileDto.Departments, dbContext);
                await dbContext.SaveChangesAsync();
            }

            await customerProfileRepo.CommitTransactionAsync();

            return result;

        }
        catch (Exception ex)
        {
            try { await customerProfileRepo.RollbackTransactionAsync(); } catch { /* ignore rollback errors */ }

            // Build full exception chain for diagnostic logging
            var fullMessage = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                fullMessage += " | INNER: " + inner.Message;
                inner = inner.InnerException;
            }
            LogService.LogError(ex, fullMessage);
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.GeneralFail, null, message: fullMessage);
        }

    }

    public override async Task<OperationResponse<CustomerProfileResponseDto>> GetAsync(Guid id)
    {
        try
        {
            var dbContext = _repositoriesFactory.GetRepository<CustomerProfile>().GetDbContext();
            var customer = await dbContext.Set<CustomerProfile>()
                .Include(c => c.MainAccount)
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.NotFound, null);

            var responseDto = _mapper.Map<CustomerProfileResponseDto>(customer);
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.Success, responseDto);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }
    
    public async Task<OperationResponse<List<CustomerProfileResponseDto>>> GetAllFrontalAsync()
    {
        var customerProfileRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
        var frontalCustomers = await customerProfileRepository.Filter(s => s.IsFrontal);
        
        if (frontalCustomers.Data == null || !frontalCustomers.Data.Any())
        {
            return new OperationResponse<List<CustomerProfileResponseDto>>(ErrorCodes.NoContent, null);
        }
        var frontalCustomersMapped = _mapper.Map<List<CustomerProfileResponseDto>>(frontalCustomers.Data);

        return new OperationResponse<List<CustomerProfileResponseDto>>(ErrorCodes.Success, frontalCustomersMapped);
    }

    public async Task<OperationResponse<CustomerProfileResponseDto>> MakeFrontalAsync(Guid customerId)
    {
        var customerProfileRepository = _repositoriesFactory.GetRepository<CustomerProfile>();
        var customerProfileQuery = await customerProfileRepository.Filter(x => x.Id == customerId);
        var customerProfile = customerProfileQuery.Data?
            .IgnoreAutoIncludes()
            .FirstOrDefault();
        if (customerProfile == null)
        {
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.NotFound, null);
        }

        customerProfile.IsFrontal = true;
        var updateResult = await customerProfileRepository.Update(customerProfile);

        if (updateResult.Data == null)
        {
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.GeneralFail, null);
        }

        var mappedCustomer = _mapper.Map<CustomerProfileResponseDto>(updateResult.Data);
        return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.Success, mappedCustomer);
    }

    public override async Task<OperationResponse<CustomerProfileResponseDto>> UpdateAsync(CustomerProfilePutDto request)
    {
        GuidHelper.NormalizeGuids(request);
        
        var customerProfileRepo = _repositoriesFactory.GetRepository<CustomerProfile>(true);
        var dbContext = customerProfileRepo.GetDbContext();
        
        await customerProfileRepo.BeginTransactionAsync();
        try
        {
            var existingProfile = await dbContext.Set<CustomerProfile>()
                .Include(c => c.MainAccount)
                .Include(c => c.Departments)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (existingProfile == null)
            {
                await customerProfileRepo.RollbackTransactionAsync();
                return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.NotFound, null);
            }

            if (existingProfile.IsReservationCompany)
            {
                await customerProfileRepo.RollbackTransactionAsync();
                return new OperationResponse<CustomerProfileResponseDto>
                {
                    ErrorCode = ErrorCodes.GeneralFail,
                    Message = "لا يمكن تعديل بيانات شركة حجز من شاشة العملاء. يرجى التعديل من شاشة شركات الحجز."
                };
            }

            // Fallback: If English Name is missing, use Arabic Name
            if (string.IsNullOrWhiteSpace(request.EnglishName))
            {
                request.EnglishName = request.ArabicName;
            }

            if (existingProfile.MainAccountId.HasValue)
            {
                var mainAccount = await dbContext.Set<MainAccount>()
                    .FirstOrDefaultAsync(x => x.Id == existingProfile.MainAccountId.Value);

                if (mainAccount != null)
                {
                    mainAccount.EnglishName = request.EnglishName;
                    mainAccount.ArabicName = request.ArabicName;
                    dbContext.Set<MainAccount>().Update(mainAccount);
                }
            }

            // Map request properties to existingProfile
            _mapper.Map(request, existingProfile);

            // Sync departments
            if (request.Departments != null)
            {
                await SyncCustomerDepartmentsAsync(existingProfile.Id, request.Departments, dbContext);
            }

            dbContext.Set<CustomerProfile>().Update(existingProfile);
            await dbContext.SaveChangesAsync();
            await customerProfileRepo.CommitTransactionAsync();

            var mappedCustomer = _mapper.Map<CustomerProfileResponseDto>(existingProfile);
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.Success, mappedCustomer);
        }
        catch (Exception ex)
        {
            await customerProfileRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public override async Task<OperationResponse<List<CustomerProfileResponseDto>>> GetAllAsync(CustomerProfileFiltersDto filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        try
        {
            var dbContext = _repositoriesFactory.GetRepository<CustomerProfile>().GetDbContext();
            var query = dbContext.Set<CustomerProfile>()
                .Include(c => c.MainAccount)
                .Include(c => c.Departments)
                .Where(c => c.IsActive == true);

            if (filters != null && !filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }

            // Apply SearchKeyword: searches across all [Searchable] entity properties
            if (!string.IsNullOrWhiteSpace(filters?.SearchKeyword))
            {
                query = query.ApplySearchKeyword(filters.SearchKeyword);
            }

            var totalRecords = await query.CountAsync();

            pageNumber ??= 1;
            pageSize ??= 1000000; // Load all for select lists if page parameters are not specified

            var skip = (pageNumber.Value - 1) * pageSize.Value;
            query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);

            var operationData = await query.ToListAsync();
            var responseData = _mapper.Map<List<CustomerProfileResponseDto>>(operationData);

            return new OperationResponse<List<CustomerProfileResponseDto>>(
                ErrorCodes.Success,
                responseData,
                totalCount: totalRecords,
                isPaginated: true
            );
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<CustomerProfileResponseDto>>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }



    private async Task SyncCustomerDepartmentsAsync(Guid customerId, List<CustomerDepartmentDto> departmentDtos, DbContext dbContext)
    {
        var existingDeps = await dbContext.Set<CustomerDepartment>()
            .Where(x => x.CustomerProfileId == customerId && x.IsActive == true)
            .ToListAsync();

        // 1. Deletions
        var incomingIds = departmentDtos.Where(d => d.Id.HasValue).Select(d => d.Id!.Value).ToList();
        var deletedDeps = existingDeps.Where(d => !incomingIds.Contains(d.Id)).ToList();
        foreach (var dep in deletedDeps)
        {
            dep.IsActive = false;
            dbContext.Set<CustomerDepartment>().Update(dep);
        }

        // 2. Add or Update
        foreach (var dto in departmentDtos)
        {
            if (dto.Id.HasValue)
            {
                var dep = existingDeps.FirstOrDefault(d => d.Id == dto.Id.Value);
                if (dep != null)
                {
                    dep.NameAr = dto.NameAr;
                    dep.NameEn = dto.NameEn;
                    dbContext.Set<CustomerDepartment>().Update(dep);
                }
            }
            else
            {
                var newDep = new CustomerDepartment
                {
                    Id = Guid.NewGuid(),
                    CustomerProfileId = customerId,
                    NameAr = dto.NameAr,
                    NameEn = dto.NameEn,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await dbContext.Set<CustomerDepartment>().AddAsync(newDep);
            }
        }
    }

    public override async Task<OperationResponse<CustomerProfileResponseDto>> DeleteAsync(Guid id)
    {
        // Reuse range logic for consistent safety checks
        var response = await DeleteRangeAsync(new List<Guid> { id });
        
        // Map response type if needed (DeleteRangeAsync returns CustomerProfileResponseDto as well)
        return response;
    }

    public override async Task<OperationResponse<CustomerProfileResponseDto>> DeleteRangeAsync(List<Guid> ids)
    {
        var customerProfileRepo = _repositoriesFactory.GetRepository<CustomerProfile>(true);

        try
        {
            await customerProfileRepo.BeginTransactionAsync();
            var dbContext = customerProfileRepo.GetDbContext();

            // ── Step 1: Load customer profiles (tracked — needed for SaveChanges to detect FK change) ─
            var customerProfiles = await dbContext.Set<CustomerProfile>()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (customerProfiles == null || !customerProfiles.Any())
                return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.NotFound, null);

            if (customerProfiles.Any(x => x.IsReservationCompany))
            {
                return new OperationResponse<CustomerProfileResponseDto>
                {
                    ErrorCode = ErrorCodes.GeneralFail,
                    Message = "لا يمكن حذف شركة حجز من شاشة العملاء. يرجى الحذف من شاشة شركات الحجز."
                };
            }
            var accountsToDelete = customerProfiles
                .Where(p => p.MainAccountId.HasValue)
                .Select(p => p.MainAccountId!.Value)
                .Distinct()
                .ToList();

            // ── Step 2: Pre-flight constraint check (before any modification) ─────
            // If any linked account has journal entries, fail fast before touching anything.
            if (accountsToDelete.Any())
            {
                var hasLinkedEntries = await dbContext.Set<TransactionEntry>()
                    .AnyAsync(te => accountsToDelete.Contains(te.MainAccountId));

                if (hasLinkedEntries)
                    return new OperationResponse<CustomerProfileResponseDto>(
                        ErrorCodes.Conflict, null,
                        message: "لا يمكن حذف العميل لأن الحساب المرتبط به يحتوي على قيود محاسبية.");
            }

            // ── Step 3: Break the FK (same transaction) then hard-delete accounts ─
            // We null out MainAccountId first so the DB FK constraint does not block
            // the account deletion. All of this runs inside the same DB transaction,
            // so if anything fails everything rolls back atomically.
            if (accountsToDelete.Any())
            {
                foreach (var profile in customerProfiles.Where(p => p.MainAccountId.HasValue))
                {
                    profile.MainAccountId = null;
                    profile.MainAccount   = null;
                }
                await dbContext.SaveChangesAsync();

                // Hard-delete the accounts directly via the shared DbContext (same transaction).
                var accountsEntities = await dbContext.Set<MainAccount>()
                    .Where(ma => accountsToDelete.Contains(ma.Id))
                    .ToListAsync();

                dbContext.Set<MainAccount>().RemoveRange(accountsEntities);
                await dbContext.SaveChangesAsync();
            }

            // ── Step 4: Soft-delete the customer profiles ─────────────────────────
            foreach (var profile in customerProfiles)
                profile.SoftDelete();

            await customerProfileRepo.UpdateRange(customerProfiles);

            await customerProfileRepo.CommitTransactionAsync();
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            await customerProfileRepo.RollbackTransactionAsync();
            LogService.LogError(ex);
            var fullMessage = ex.Message;
            var inner = ex.InnerException;
            while (inner != null) { fullMessage += " | " + inner.Message; inner = inner.InnerException; }
            return new OperationResponse<CustomerProfileResponseDto>(ErrorCodes.GeneralFail, null, message: fullMessage);
        }
    }
}
