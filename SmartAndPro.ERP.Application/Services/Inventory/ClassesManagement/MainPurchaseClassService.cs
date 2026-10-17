using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryManagement.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.InventoryOperation;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;

public class MainPurchaseClassService
    : ServiceBase
    <
        MainPurchaseClass,
        MainPurchaseClassPostDto,
        MainPurchaseClassPutDto,
        MainPurchaseClassResponseDto,
        MainPurchaseClassFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public MainPurchaseClassService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<List<MainPurchaseClassResponseDto>>> GetAllAsync(
        MainPurchaseClassFiltersDto filter, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<MainPurchaseClassResponseDto>>(ErrorCodes.GeneralFail,null);
        try
        {
            var mainPurchaseClassesResponse = await base.GetAllAsync(filter);

            if(mainPurchaseClassesResponse.Data==null || !mainPurchaseClassesResponse.Data.Any())
                return mainPurchaseClassesResponse;

            var mainPurchaseClassIds = mainPurchaseClassesResponse.Data.Select(x => x.Id).ToList();
            
            // Query all operation entries in a single query
            var operationEntryRepository = _repositoriesFactory.GetRepository<OperationEntry>();
            var operationEntries = await operationEntryRepository.GetAll();

            // Group operation entries by MainPurchaseClassId for easy access
            var groupedOperationEntries = operationEntries.Data
                .GroupBy(x => x.MainPurchaseClassId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            
            foreach (var mainPurchaseClass in mainPurchaseClassesResponse.Data)
            {
                if (!groupedOperationEntries.TryGetValue(mainPurchaseClass.Id, out var entries)) continue;
                    mainPurchaseClass.ProductUnitsCount = entries.Sum(pu => pu.Quantity);

                    var lastItemPrice = entries
                        .OrderBy(oe => oe.InventoryOperation.OperationDate)
                        .LastOrDefault()?.UnitPrice ?? 0;
                    mainPurchaseClass.LastPrice = lastItemPrice;

                    var recentEntries = entries.Where(pi => pi.CreatedAt >= DateTime.UtcNow.AddMonths(-1)).ToList();
                    var totalAmount = recentEntries.Sum(pi => pi.UnitPrice * pi.Quantity);
                    var totalQuantity = recentEntries.Sum(pi => pi.Quantity);
                
                    mainPurchaseClass.LastMonthAveragePrice = totalQuantity != 0
                        ? Math.Round(totalAmount / totalQuantity, 2)
                        : 0;
                
            }

            return mainPurchaseClassesResponse;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    public override async Task<OperationResponse<MainPurchaseClassResponseDto>> AddAsync(MainPurchaseClassPostDto mainPurchaseClassPost)
    {
        try
        {
            var mainPurchaseClass = _mapper.Map<MainPurchaseClass>(mainPurchaseClassPost);
            var referenceCode = mainPurchaseClass.ReferenceCode;
            var arabicName = mainPurchaseClass.ArabicName;
            var englishName = mainPurchaseClass.EnglishName;
            
            // check if root account or a headerPurchaseClass or a detailPurchaseClass or a MainPurchaseClass has that referenceCode
            var rootPurchaseClassRepository = _repositoriesFactory.GetRepository<RootPurchaseClass>();
            var rootPurchaseClassOperation = await rootPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (rootPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var headerPurchaseClassRepository = _repositoriesFactory.GetRepository<HeaderPurchaseClass>();
            var headerPurchaseClassOperation = await headerPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (headerPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var detailPurchaseClassRepository = _repositoriesFactory.GetRepository<DetailPurchaseClass>();
            var detailPurchaseClassOperation = await detailPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (detailPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var mainPurchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var mainPurchaseClassOperation = await mainPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (mainPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            // Check for duplicate Arabic name
            var duplicateArabicNameOperation = await mainPurchaseClassRepository.Filter(x => x.ArabicName == arabicName);
            if (duplicateArabicNameOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
            
            // Check for duplicate English name
            var duplicateEnglishNameOperation = await mainPurchaseClassRepository.Filter(x => x.EnglishName == englishName);
            if (duplicateEnglishNameOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
            
            return await base.AddAsync(mainPurchaseClassPost);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            // Check if it's a database constraint violation
            if (ex.InnerException?.Message?.Contains("UNIQUE constraint") == true || 
                ex.InnerException?.Message?.Contains("duplicate key") == true ||
                ex.Message?.Contains("UNIQUE constraint") == true ||
                ex.Message?.Contains("duplicate key") == true)
            {
                if (ex.Message?.Contains("ArabicName") == true || ex.InnerException?.Message?.Contains("ArabicName") == true)
                    return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while saving the record");
        }
    }
    
    public override async Task<OperationResponse<MainPurchaseClassResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            
            var mainPurchaseClass =await _repositoriesFactory.GetRepository<MainPurchaseClass>().Filter(x => x.Id == id);
            if (mainPurchaseClass.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = mainPurchaseClass.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.NotFound, null);
            
            var operation = await _repositoriesFactory.GetRepository<MainPurchaseClass>().Delete(entity);
            return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);
    }
    
    public override async Task<OperationResponse<MainPurchaseClassResponseDto>> UpdateAsync(MainPurchaseClassPutDto mainPurchaseClassPut)
    {
        try
        {
            var mainPurchaseClass = _mapper.Map<MainPurchaseClass>(mainPurchaseClassPut);
            var referenceCode = mainPurchaseClass.ReferenceCode;
            var arabicName = mainPurchaseClass.ArabicName;
            var englishName = mainPurchaseClass.EnglishName;
            var id = mainPurchaseClass.Id;
            
            // check if root account or a headerPurchaseClass or a detailPurchaseClass or a MainPurchaseClass has that referenceCode (excluding current record)
            var rootPurchaseClassRepository = _repositoriesFactory.GetRepository<RootPurchaseClass>();
            var rootPurchaseClassOperation = await rootPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (rootPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var headerPurchaseClassRepository = _repositoriesFactory.GetRepository<HeaderPurchaseClass>();
            var headerPurchaseClassOperation = await headerPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (headerPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var detailPurchaseClassRepository = _repositoriesFactory.GetRepository<DetailPurchaseClass>();
            var detailPurchaseClassOperation = await detailPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (detailPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var mainPurchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
            var mainPurchaseClassOperation = await mainPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode && x.Id != id);
            if (mainPurchaseClassOperation.Data.Any())
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var existingRecordQuery = await mainPurchaseClassRepository.Filter(x => x.Id == id);
            var existingRecord = await existingRecordQuery.Data.IgnoreAutoIncludes().FirstOrDefaultAsync();
            if (existingRecord != null)
            {
                if (existingRecord.ArabicName != arabicName)
                {
                    var duplicateArabicNameOperation = await mainPurchaseClassRepository.Filter(x => x.ArabicName == arabicName && x.Id != id);
                    if (duplicateArabicNameOperation.Data.Any())
                        return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                }

                if (existingRecord.EnglishName != englishName)
                {
                    var duplicateEnglishNameOperation = await mainPurchaseClassRepository.Filter(x => x.EnglishName == englishName && x.Id != id);
                    if (duplicateEnglishNameOperation.Data.Any())
                        return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                }
            }
            
            return await base.UpdateAsync(mainPurchaseClassPut);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            // Check if it's a database constraint violation
            if (ex.InnerException?.Message?.Contains("UNIQUE constraint") == true || 
                ex.InnerException?.Message?.Contains("duplicate key") == true ||
                ex.Message?.Contains("UNIQUE constraint") == true ||
                ex.Message?.Contains("duplicate key") == true)
            {
                if (ex.Message?.Contains("ArabicName") == true || ex.InnerException?.Message?.Contains("ArabicName") == true)
                    return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<MainPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while updating the record");
        }
    }

    /// <summary>
    /// Generate PDF report document for purchase classes
    /// </summary>
    public async Task<byte[]> GetPurchaseClassesReportDocument(string? printedBy = null)
    {
        var filter = new MainPurchaseClassFiltersDto();
        var itemsResponse = await base.GetAllAsync(filter);
        var items = itemsResponse.Data ?? new List<MainPurchaseClassResponseDto>();

        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        // Map to report items with measurement unit info
        var reportItems = items.Select(item => new InventoryClassReportItem
        {
            Id = item.Id,
            ArabicName = item.ArabicName,
            EnglishName = item.EnglishName,
            ReferenceCode = item.ReferenceCode,
            MeasurementUnitArabicName = item.MeasurementUnit?.ArabicName,
            MeasurementUnitEnglishName = item.MeasurementUnit?.EnglishName,
            MeasurementUnitArabicSymbol = item.MeasurementUnit?.ArabicSymbol,
            MeasurementUnitEnglishSymbol = item.MeasurementUnit?.EnglishSymbol,
            ProductUnitsCount = item.ProductUnitsCount,
            CreatedAt = item.CreatedAt
        }).ToList();

        var document = new InventoryClassesReportDocument(
            reportItems,
            company,
            "purchase",
            "ar",
            printedBy
        );

        return document.GeneratePDF();
    }
    
}