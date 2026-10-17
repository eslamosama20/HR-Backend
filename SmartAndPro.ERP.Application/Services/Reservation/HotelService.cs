using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Hotel;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Reservation;

public class HotelService
    : ServiceBase<Hotel, HotelPostDto, HotelPutDto, HotelResponseDto, HotelFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public HotelService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AddAsync — fixed MainAccount uniqueness handling (mirrors CustomerProfileService)
    // ─────────────────────────────────────────────────────────────────────────
    public override async Task<OperationResponse<HotelResponseDto>> AddAsync(HotelPostDto hotelDto)
    {
        var hotelRepo = _repositoriesFactory.GetRepository<Hotel>(true);
        var dbContext = hotelRepo.GetDbContext();

        var normalizedArabic = hotelDto.ArabicName?.Trim() ?? string.Empty;
        var normalizedTax    = hotelDto.TaxNumber?.Trim();

        // Idempotent guard: return existing if ArabicName already exists
        if (!string.IsNullOrWhiteSpace(normalizedArabic))
        {
            var existsByArabic = await dbContext.Set<Hotel>()
                .IgnoreQueryFilters()
                .AnyAsync(h => h.ArabicName == normalizedArabic);
            if (existsByArabic)
            {
                var existing = await dbContext.Set<Hotel>()
                    .IgnoreQueryFilters()
                    .Include(h => h.MainAccount)
                    .FirstAsync(h => h.ArabicName == normalizedArabic);
                return new OperationResponse<HotelResponseDto>(ErrorCodes.Success, _mapper.Map<HotelResponseDto>(existing));
            }
        }

        // Idempotent guard: return existing if TaxNumber already exists
        if (!string.IsNullOrWhiteSpace(normalizedTax))
        {
            var existsByTax = await dbContext.Set<Hotel>()
                .IgnoreQueryFilters()
                .AnyAsync(h => h.TaxNumber == normalizedTax);
            if (existsByTax)
            {
                var existing = await dbContext.Set<Hotel>()
                    .IgnoreQueryFilters()
                    .Include(h => h.MainAccount)
                    .FirstAsync(h => h.TaxNumber == normalizedTax);
                return new OperationResponse<HotelResponseDto>(ErrorCodes.Success, _mapper.Map<HotelResponseDto>(existing));
            }
        }

        try
        {
            await hotelRepo.BeginTransactionAsync();

            if (hotelDto.MainAccountId == null)
            {
                var hotelDetailAccountId = await dbContext.Set<DetailAccount>()
                    .IgnoreQueryFilters()
                    .Where(da => da.ArabicName == "الفنادق")
                    .Select(da => (Guid?)da.Id)
                    .FirstOrDefaultAsync()
                    ?? await dbContext.Set<DetailAccount>()
                        .IgnoreQueryFilters()
                        .Select(da => (Guid?)da.Id)
                        .FirstOrDefaultAsync();

                // Reuse existing MainAccount by ArabicName (IgnoreQueryFilters catches soft-deleted)
                var existingAccount = await dbContext.Set<MainAccount>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(ma => ma.ArabicName == normalizedArabic);

                if (existingAccount != null)
                {
                    hotelDto.MainAccountId = existingAccount.Id;
                }
                else
                {
                    var englishName = string.IsNullOrWhiteSpace(hotelDto.EnglishName)
                        ? normalizedArabic
                        : hotelDto.EnglishName.Trim();

                    // Ensure EnglishName is unique on MainAccount
                    var englishTaken = await dbContext.Set<MainAccount>()
                        .IgnoreQueryFilters()
                        .AnyAsync(ma => ma.EnglishName == englishName);
                    if (englishTaken)
                        englishName = englishName + "_" + Guid.NewGuid().ToString("N")[..6];

                    var customerAccount = new MainAccount
                    {
                        ArabicName      = normalizedArabic,
                        EnglishName     = englishName,
                        DetailAccountId = hotelDetailAccountId,
                        CreatedAt       = DateTime.UtcNow
                    };

                    dbContext.Set<MainAccount>().Add(customerAccount);
                    await dbContext.SaveChangesAsync();
                    hotelDto.MainAccountId = customerAccount.Id;
                }
            }

            var result = await base.AddAsync(hotelDto);
            if (result.ErrorCode != ErrorCodes.Success)
                throw new Exception("Hotel not added");

            await hotelRepo.CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            try { await hotelRepo.RollbackTransactionAsync(); } catch { /* ignore rollback errors */ }
            LogService.LogError(ex);
            return new OperationResponse<HotelResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ImportFromExcelAsync — exactly mirrors CustomerProfileService pattern
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<HotelImportResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var result = new HotelImportResultDto();

        var headerAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["arabicname"]               = "arabicName",
            ["arabic name"]              = "arabicName",
            ["الاسم العربي"]             = "arabicName",
            ["englishname"]              = "englishName",
            ["english name"]             = "englishName",
            ["الاسم الانجليزي"]          = "englishName",
            ["الاسم الإنجليزي"]          = "englishName",
            ["taxnumber"]                = "taxNumber",
            ["tax number"]               = "taxNumber",
            ["الرقم الضريبي"]            = "taxNumber",
            ["contactnumber"]            = "contactNumber",
            ["contact number"]           = "contactNumber",
            ["رقم الجوال"]               = "contactNumber",
            ["email"]                    = "email",
            ["البريد الإلكتروني"]        = "email",
            ["البريد الالكتروني"]        = "email",
            ["bankname"]                 = "bankName",
            ["bank name"]                = "bankName",
            ["اسم البنك"]                = "bankName",
            ["bankaccountnumber"]         = "bankAccountNumber",
            ["bank account number"]       = "bankAccountNumber",
            ["رقم الحساب البنكي"]         = "bankAccountNumber",
            ["iban"]                     = "iban",
            ["الآيبان"]                  = "iban",
            ["ايبان"]                    = "iban",
            ["mainaccountid"]            = "mainAccountId",
            ["main account id"]          = "mainAccountId",
            ["معرف الحساب الرئيسي"]      = "mainAccountId",
            ["street"]                   = "street",
            ["street name"]              = "street",
            ["الشارع"]                   = "street",
            ["district"]                 = "district",
            ["الحي"]                     = "district",
            ["city"]                     = "city",
            ["المدينة"]                  = "city",
            ["country"]                  = "country",
            ["البلد"]                    = "country",
            ["shortaddress"]             = "shortAddress",
            ["short address"]            = "shortAddress",
            ["العنوان المختصر"]           = "shortAddress",
            ["buildingnumber"]           = "buildingNumber",
            ["building number"]          = "buildingNumber",
            ["رقم المبنى"]               = "buildingNumber",
            ["postalcode"]               = "postalCode",
            ["postal code"]              = "postalCode",
            ["الرمز البريدي"]             = "postalCode",
            ["additionalcontactnumber"]  = "additionalContactNumber",
            ["additional contact number"]= "additionalContactNumber",
            ["رقم اتصال اضافي"]          = "additionalContactNumber",
            ["fax"]                      = "fax",
            ["فاكس"]                    = "fax",
            ["website"]                  = "website",
            ["الموقع الإلكتروني"]         = "website",
            ["الموقع"]                   = "website",
            ["gaztnumber"]               = "gaztNumber",
            ["gazt number"]              = "gaztNumber",
            ["رقم GAZT"]                 = "gaztNumber",
        };

        using var package = new ExcelPackage(fileStream);
        var ws = package.Workbook.Worksheets[0];
        if (ws == null || ws.Dimension == null) return result;

        int totalRows    = ws.Dimension.End.Row;
        int totalColumns = ws.Dimension.End.Column;

        // Build column-index → field-name map from header row
        var colMap = new Dictionary<int, string>();
        for (int col = 1; col <= totalColumns; col++)
        {
            var raw = ws.Cells[1, col].Text?.Trim() ?? string.Empty;
            if (headerAliases.TryGetValue(raw, out var mapped))
                colMap[col] = mapped;
        }

        if (!colMap.ContainsValue("arabicName"))
        {
            result.Failed.Add(new HotelImportFailureDto
            {
                Row    = 1,
                Name   = string.Empty,
                Reason = "IMPORT_MISSING_COLUMN"
            });
            return result;
        }

        // Parse all non-empty data rows
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

        // Fetch ALL existing hotels once for in-memory lookup
        var cpDbContext = _repositoriesFactory.GetRepository<Hotel>().GetDbContext();
        var allExisting = await cpDbContext.Set<Hotel>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();

        var existingByTax = allExisting
            .Where(h => !string.IsNullOrWhiteSpace(h.TaxNumber))
            .GroupBy(h => h.TaxNumber!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var existingByArabic = allExisting
            .Where(h => !string.IsNullOrWhiteSpace(h.ArabicName))
            .GroupBy(h => h.ArabicName.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        // Process each row sequentially — mirrors CustomerProfileService exactly
        foreach (var (rowNum, data) in rows)
        {
            var g = data;
            var arabicName        = g.GetValueOrDefault("arabicName",        string.Empty).Trim();
            var englishName       = g.GetValueOrDefault("englishName",       string.Empty).Trim();
            var taxNumber         = g.GetValueOrDefault("taxNumber",         string.Empty).Trim();
            var contactNumber     = g.GetValueOrDefault("contactNumber",     string.Empty).Trim();
            var email             = g.GetValueOrDefault("email",             string.Empty).Trim();
            var bankName          = g.GetValueOrDefault("bankName",          string.Empty).Trim();
            var bankAccountNumber = g.GetValueOrDefault("bankAccountNumber", string.Empty).Trim();
            var iban              = g.GetValueOrDefault("iban",              string.Empty).Trim();
            var mainAccountIdStr  = g.GetValueOrDefault("mainAccountId",     string.Empty).Trim();
            var street                = g.GetValueOrDefault("street",                string.Empty).Trim();
            var district              = g.GetValueOrDefault("district",              string.Empty).Trim();
            var city                  = g.GetValueOrDefault("city",                  string.Empty).Trim();
            var country               = g.GetValueOrDefault("country",               string.Empty).Trim();
            var shortAddress          = g.GetValueOrDefault("shortAddress",          string.Empty).Trim();
            var buildingNumber        = g.GetValueOrDefault("buildingNumber",        string.Empty).Trim();
            var postalCode            = g.GetValueOrDefault("postalCode",            string.Empty).Trim();
            var website               = g.GetValueOrDefault("website",               string.Empty).Trim();
            var additionalContactNumber = g.GetValueOrDefault("additionalContactNumber", string.Empty).Trim();
            var fax                   = g.GetValueOrDefault("fax",                   string.Empty).Trim();
            var gaztNumber            = g.GetValueOrDefault("gaztNumber",            string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(arabicName))
            {
                result.Failed.Add(new HotelImportFailureDto
                {
                    Row    = rowNum,
                    Name   = string.Empty,
                    Reason = "IMPORT_ARABIC_NAME_REQUIRED"
                });
                continue;
            }

            try
            {
                // Match existing hotel: taxNumber first, then arabicName
                Hotel? matchedHotel = null;
                if (!string.IsNullOrWhiteSpace(taxNumber) && existingByTax.TryGetValue(taxNumber, out var byTax))
                    matchedHotel = byTax;
                else if (existingByArabic.TryGetValue(arabicName.ToLowerInvariant(), out var byArabic))
                    matchedHotel = byArabic;

                if (matchedHotel != null)
                {
                    // UPDATE — mirrors CustomerProfileService.UpdateAsync call
                    var putDto = new HotelPutDto
                    {
                        Id                = matchedHotel.Id,
                        ArabicName        = arabicName,
                        EnglishName       = string.IsNullOrWhiteSpace(englishName)       ? matchedHotel.EnglishName       : englishName,
                        TaxNumber         = string.IsNullOrWhiteSpace(taxNumber)         ? matchedHotel.TaxNumber         : taxNumber,
                        ContactNumber     = string.IsNullOrWhiteSpace(contactNumber)     ? matchedHotel.ContactNumber     : contactNumber,
                        Email             = string.IsNullOrWhiteSpace(email)             ? matchedHotel.Email             : email,
                        BankName          = string.IsNullOrWhiteSpace(bankName)          ? matchedHotel.BankName          : bankName,
                        BankAccountNumber = string.IsNullOrWhiteSpace(bankAccountNumber) ? matchedHotel.BankAccountNumber : bankAccountNumber,
                        IBAN              = string.IsNullOrWhiteSpace(iban)              ? matchedHotel.IBAN              : iban,
                        MainAccountId     = Guid.TryParse(mainAccountIdStr, out var upId) ? upId : matchedHotel.MainAccountId,
                        IsActive          = matchedHotel.IsActive,
                        IsFrontal         = matchedHotel.IsFrontal,
                        Street                = string.IsNullOrWhiteSpace(street)                ? matchedHotel.Street                : street,
                        District              = string.IsNullOrWhiteSpace(district)              ? matchedHotel.District              : district,
                        City                  = string.IsNullOrWhiteSpace(city)                  ? matchedHotel.City                  : city,
                        Country               = string.IsNullOrWhiteSpace(country)               ? matchedHotel.Country               : country,
                        ShortAddress          = string.IsNullOrWhiteSpace(shortAddress)          ? matchedHotel.ShortAddress          : shortAddress,
                        BuildingNumber        = string.IsNullOrWhiteSpace(buildingNumber)        ? matchedHotel.BuildingNumber        : buildingNumber,
                        PostalCode            = string.IsNullOrWhiteSpace(postalCode)            ? matchedHotel.PostalCode            : postalCode,
                        Website               = string.IsNullOrWhiteSpace(website)               ? matchedHotel.Website               : website,
                        AdditionalContactNumber = string.IsNullOrWhiteSpace(additionalContactNumber) ? matchedHotel.AdditionalContactNumber : additionalContactNumber,
                        Fax                   = string.IsNullOrWhiteSpace(fax)                   ? matchedHotel.Fax                   : fax,
                        GaztNumber            = string.IsNullOrWhiteSpace(gaztNumber)            ? matchedHotel.GaztNumber            : gaztNumber,
                    };

                    var updateRes = await base.UpdateAsync(putDto);
                    if (updateRes.ErrorCode == ErrorCodes.Success)
                    {
                        result.Updated++;
                        existingByArabic[arabicName.ToLowerInvariant()] = matchedHotel;
                        if (!string.IsNullOrWhiteSpace(taxNumber)) existingByTax[taxNumber] = matchedHotel;
                    }
                    else
                    {
                        result.Failed.Add(new HotelImportFailureDto { Row = rowNum, Name = arabicName, Reason = "IMPORT_UPDATE_FAILED" });
                    }
                }
                else
                {
                    // CREATE — resolve MainAccountId first, then call base.AddAsync directly
                    // (avoids the override which loads navigation properties causing serialization issues)
                    Guid? resolvedMainAccountId = Guid.TryParse(mainAccountIdStr, out var crId) ? crId : null;

                    if (resolvedMainAccountId == null)
                    {
                        // Reuse existing MainAccount or create a new one
                        var existingAccount = await cpDbContext.Set<MainAccount>()
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(ma => ma.ArabicName == arabicName);

                        if (existingAccount != null)
                        {
                            resolvedMainAccountId = existingAccount.Id;
                        }
                        else
                        {
                            // Try 'الفنادق' first, fallback to any available DetailAccount
                            var hotelDetailAccountId = await cpDbContext.Set<DetailAccount>()
                                .IgnoreQueryFilters()
                                .Where(da => da.ArabicName == "الفنادق")
                                .Select(da => (Guid?)da.Id)
                                .FirstOrDefaultAsync()
                                ?? await cpDbContext.Set<DetailAccount>()
                                    .IgnoreQueryFilters()
                                    .Select(da => (Guid?)da.Id)
                                    .FirstOrDefaultAsync();

                            if (hotelDetailAccountId == null)
                            {
                                result.Failed.Add(new HotelImportFailureDto { Row = rowNum, Name = arabicName, Reason = "IMPORT_NO_DETAIL_ACCOUNT" });
                                continue;
                            }

                            var engName = string.IsNullOrWhiteSpace(englishName) ? arabicName : englishName.Trim();
                            var engTaken = await cpDbContext.Set<MainAccount>()
                                .IgnoreQueryFilters()
                                .AnyAsync(ma => ma.EnglishName == engName);
                            if (engTaken) engName = engName + "_" + Guid.NewGuid().ToString("N")[..6];

                            var newAccount = new MainAccount
                            {
                                ArabicName      = arabicName,
                                EnglishName     = engName,
                                DetailAccountId = hotelDetailAccountId, // nullable — null if 'الفنادق' not found
                                CreatedAt       = DateTime.UtcNow
                            };
                            cpDbContext.Set<MainAccount>().Add(newAccount);
                            await cpDbContext.SaveChangesAsync();
                            resolvedMainAccountId = newAccount.Id;
                        }
                    }

                    var postDto = new HotelPostDto
                    {
                        ArabicName        = arabicName,
                        EnglishName       = string.IsNullOrWhiteSpace(englishName) ? arabicName : englishName.Trim(),
                        TaxNumber         = string.IsNullOrWhiteSpace(taxNumber)         ? null : taxNumber,
                        ContactNumber     = string.IsNullOrWhiteSpace(contactNumber)     ? null : contactNumber,
                        Email             = string.IsNullOrWhiteSpace(email)             ? null : email,
                        BankName          = string.IsNullOrWhiteSpace(bankName)          ? null : bankName,
                        BankAccountNumber = string.IsNullOrWhiteSpace(bankAccountNumber) ? null : bankAccountNumber,
                        IBAN              = string.IsNullOrWhiteSpace(iban)              ? null : iban,
                        MainAccountId     = resolvedMainAccountId,
                        IsActive          = true,
                        IsFrontal         = false,
                        Street                = string.IsNullOrWhiteSpace(street)                ? null : street,
                        District              = string.IsNullOrWhiteSpace(district)              ? null : district,
                        City                  = string.IsNullOrWhiteSpace(city)                  ? null : city,
                        Country               = string.IsNullOrWhiteSpace(country)               ? null : country,
                        ShortAddress          = string.IsNullOrWhiteSpace(shortAddress)          ? null : shortAddress,
                        BuildingNumber        = string.IsNullOrWhiteSpace(buildingNumber)        ? null : buildingNumber,
                        PostalCode            = string.IsNullOrWhiteSpace(postalCode)            ? null : postalCode,
                        Website               = string.IsNullOrWhiteSpace(website)               ? null : website,
                        AdditionalContactNumber = string.IsNullOrWhiteSpace(additionalContactNumber) ? null : additionalContactNumber,
                        Fax                   = string.IsNullOrWhiteSpace(fax)                   ? null : fax,
                        GaztNumber            = string.IsNullOrWhiteSpace(gaztNumber)            ? null : gaztNumber,
                    };

                    // Call base.AddAsync (ServiceBase) directly — skips our override which loads
                    // navigation properties (AutoInclude on MainAccount) that cause serialization issues
                    var addRes = await base.AddAsync(postDto);
                    if (addRes.ErrorCode == ErrorCodes.Success)
                    {
                        result.Imported++;
                        var created = addRes.Data;
                        if (created != null)
                        {
                            var stub = new Hotel { Id = created.Id, ArabicName = arabicName, TaxNumber = string.IsNullOrWhiteSpace(taxNumber) ? null : taxNumber };
                            existingByArabic[arabicName.ToLowerInvariant()] = stub;
                            if (!string.IsNullOrWhiteSpace(taxNumber)) existingByTax[taxNumber] = stub;
                        }
                    }
                    else
                    {
                        result.Failed.Add(new HotelImportFailureDto { Row = rowNum, Name = arabicName, Reason = "IMPORT_CREATE_FAILED" });
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                result.Failed.Add(new HotelImportFailureDto { Row = rowNum, Name = arabicName, Reason = "IMPORT_CREATE_FAILED" });
            }
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAllFrontalAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<OperationResponse<List<HotelResponseDto>>> GetAllFrontalAsync()
    {
        var frontalHotels = await _repositoriesFactory.GetRepository<Hotel>().Filter(h => h.IsFrontal);

        if (frontalHotels.Data == null || !frontalHotels.Data.Any())
        {
            return new OperationResponse<List<HotelResponseDto>>(
                ErrorCodes.Success,
                new List<HotelResponseDto>()
            );
        }

        var hotelDtos = _mapper.Map<List<HotelResponseDto>>(frontalHotels.Data);
        return new OperationResponse<List<HotelResponseDto>>(ErrorCodes.Success, hotelDtos);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MakeFrontalAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<OperationResponse<HotelResponseDto>> MakeFrontalAsync(Guid hotelId)
    {
        var hotelQuery = await _repositoriesFactory.GetRepository<Hotel>().Filter(h => h.Id == hotelId);
        var hotel = hotelQuery.Data?.IgnoreAutoIncludes().FirstOrDefault();
        if (hotel == null)
            return new OperationResponse<HotelResponseDto>(ErrorCodes.NotFound, null);

        hotel.IsFrontal = true;
        var updateResult = await _repositoriesFactory.GetRepository<Hotel>().Update(hotel);

        if (updateResult.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<HotelResponseDto>(ErrorCodes.GeneralFail, null);

        return new OperationResponse<HotelResponseDto>(ErrorCodes.Success, _mapper.Map<HotelResponseDto>(updateResult.Data));
    }

}
