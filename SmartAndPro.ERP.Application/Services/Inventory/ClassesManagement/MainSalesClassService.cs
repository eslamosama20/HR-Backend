using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.InventoryManagement.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;

public class MainSalesClassService
    : ServiceBase
    <
        MainSalesClass,
        MainSalesClassPostDto,
        MainSalesClassPutDto,
        MainSalesClassResponseDto,
        MainSalesClassFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public MainSalesClassService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<List<MainSalesClassResponseDto>>> GetAllAsync(
        MainSalesClassFiltersDto filter, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<MainSalesClassResponseDto>>(ErrorCodes.GeneralFail,null);
        try
        {
            // Default condition: do not return reservation items unless explicitly requested
            if (filter.IsReservationItem == null)
            {
                filter.IsReservationItem = false;
            }

            var mainSalesClassesResponse = await base.GetAllAsync(filter, pageNumber, pageSize);
            if(mainSalesClassesResponse.Data == null || !mainSalesClassesResponse.Data.Any())
                return mainSalesClassesResponse;

            foreach (var mainSalesClass in mainSalesClassesResponse.Data)
            {
                var productUnitsQuery = await _repositoriesFactory.GetRepository<ProductUnit>()
                    .Filter(pu => pu.MainSalesClassId == mainSalesClass.Id);
                var productUnits = productUnitsQuery.Data?.ToList() ?? new List<ProductUnit>();
                if (productUnits.Count == 0)
                {
                    mainSalesClass.ProductUnitsCount = 0;
                    continue;
                }
                mainSalesClass.ProductUnitsCount = productUnits.Sum(pu => pu.Quantity ?? 0);
            }

            return mainSalesClassesResponse;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    public override async Task<OperationResponse<MainSalesClassResponseDto>> AddAsync(MainSalesClassPostDto mainSalesClassPost)
    {
        try
        {
            var mainSalesClass = _mapper.Map<MainSalesClass>(mainSalesClassPost);
            var referenceCode = mainSalesClass.ReferenceCode;
            var arabicName = mainSalesClass.ArabicName;
            var englishName = mainSalesClass.EnglishName;
            
            // check if root account or a headerSalesClass or a detailSalesClass or a MainSalesClass has that referenceCode
            var rootSalesClassRepository = _repositoriesFactory.GetRepository<RootSalesClass>();
            var rootSalesClassOperation = await rootSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (rootSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var headerSalesClassRepository = _repositoriesFactory.GetRepository<HeaderSalesClass>();
            var headerSalesClassOperation = await headerSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (headerSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var detailSalesClassRepository = _repositoriesFactory.GetRepository<DetailSalesClass>();
            var detailSalesClassOperation = await detailSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (detailSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var mainSalesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();
            var mainSalesClassOperation = await mainSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (mainSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            // Check for duplicate Arabic name
            var duplicateArabicNameOperation = await mainSalesClassRepository.Filter(x => x.ArabicName == arabicName);
            if (duplicateArabicNameOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
            
            // Check for duplicate English name
            var duplicateEnglishNameOperation = await mainSalesClassRepository.Filter(x => x.EnglishName == englishName);
            if (duplicateEnglishNameOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
            
            return await base.AddAsync(mainSalesClassPost);
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
                    return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while saving the record");
        }
    }
    
    public override async Task<OperationResponse<MainSalesClassResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            var mainSalesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();
            var mainSalesClass = await mainSalesClassRepository.Filter(x => x.Id == id);
            
            // Use IgnoreAutoIncludes to avoid loading navigation properties
            var entity = await mainSalesClass.Data
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync();
                
            if (entity is null)
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.NotFound, null);
            
            // Soft delete instead of hard delete
            entity.SoftDelete();
            var operation = await _repositoriesFactory.GetRepository<MainSalesClass>().Update(entity);
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<MainSalesClassResponseDto>(operation.ErrorCode, null);
            
            return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.GeneralFail, null);
    }
    
    public override async Task<OperationResponse<MainSalesClassResponseDto>> UpdateAsync(MainSalesClassPutDto mainSalesClassPut)
    {
        try
        {
            var mainSalesClass = _mapper.Map<MainSalesClass>(mainSalesClassPut);
            var referenceCode = mainSalesClass.ReferenceCode;
            var arabicName = mainSalesClass.ArabicName;
            var englishName = mainSalesClass.EnglishName;
            var id = mainSalesClass.Id;
            
            // check if root account or a headerSalesClass or a detailSalesClass or a MainSalesClass has that referenceCode (excluding current record)
            var rootSalesClassRepository = _repositoriesFactory.GetRepository<RootSalesClass>();
            var rootSalesClassOperation = await rootSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (rootSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var headerSalesClassRepository = _repositoriesFactory.GetRepository<HeaderSalesClass>();
            var headerSalesClassOperation = await headerSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (headerSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var detailSalesClassRepository = _repositoriesFactory.GetRepository<DetailSalesClass>();
            var detailSalesClassOperation = await detailSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
            if (detailSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var mainSalesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();
            var mainSalesClassOperation = await mainSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode && x.Id != id);
            if (mainSalesClassOperation.Data.Any())
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            
            var existingRecordQuery = await mainSalesClassRepository.Filter(x => x.Id == id);
            var existingRecord = await existingRecordQuery.Data.IgnoreAutoIncludes().FirstOrDefaultAsync();
            if (existingRecord != null)
            {
                if (existingRecord.ArabicName != arabicName)
                {
                    var duplicateArabicNameOperation = await mainSalesClassRepository.Filter(x => x.ArabicName == arabicName && x.Id != id);
                    if (duplicateArabicNameOperation.Data.Any())
                        return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                }

                if (existingRecord.EnglishName != englishName)
                {
                    var duplicateEnglishNameOperation = await mainSalesClassRepository.Filter(x => x.EnglishName == englishName && x.Id != id);
                    if (duplicateEnglishNameOperation.Data.Any())
                        return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                }
            }
            
            return await base.UpdateAsync(mainSalesClassPut);
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
                    return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<MainSalesClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while updating the record");
        }
    }


    public async Task<OperationResponse<List<MainSalesClassResponseDto>>> EnsureItemsExistAsync(string[] itemNames)
    {
        if (itemNames == null || itemNames.Length == 0)
            return new OperationResponse<List<MainSalesClassResponseDto>>(ErrorCodes.NoContent, null);

        var mainSalesClassRepo = _repositoriesFactory.GetRepository<MainSalesClass>();
        var measurementUnitRepo = _repositoriesFactory.GetRepository<MeasurementUnit>();
        var createdItems = new List<MainSalesClass>();

        var measurementUnit = (await measurementUnitRepo.Filter(x => x.EnglishName == "Nos")).Data.FirstOrDefault();
        if (measurementUnit == null)
            throw new Exception("Measurement unit 'Nos' not found in DB");

        Guid measurementUnitId = measurementUnit.Id;

        foreach (var name in itemNames)
        {
            var referenceCode = name.ToUpper();

            var exists = await mainSalesClassRepo.Filter(x => x.ReferenceCode == referenceCode);
            if (exists.Data.Any())
                continue;

            var detailContainerQuery = _repositoriesFactory.GetRepository<DetailSalesClass>().Filter(x => x.EnglishName == "Detail Container");
            var detailContainer = detailContainerQuery.Result.Data.FirstOrDefault();
            if (detailContainer == null)
                throw new Exception("Detail Container not found in DB");
            var DetailSalesClassId = detailContainer.Id;
            var newItem = new MainSalesClass
            {
                EnglishName = name,
                ArabicName = name,
                ReferenceCode = referenceCode,
                MeasurementUnitId = measurementUnitId,
                ClassType = ClassType.Direct,
                DetailSalesClassId = DetailSalesClassId
            };

            await mainSalesClassRepo.Add(newItem); 
            createdItems.Add(newItem);
        }

        var resultDto = _mapper.Map<List<MainSalesClassResponseDto>>(createdItems);
        return new OperationResponse<List<MainSalesClassResponseDto>>(ErrorCodes.NoContent, resultDto);
    }

    /// <summary>
    /// Generate PDF report document for sales classes
    /// </summary>
    public async Task<byte[]> GetSalesClassesReportDocument(string? printedBy = null)
    {
        var filter = new MainSalesClassFiltersDto();
        var itemsResponse = await base.GetAllAsync(filter);
        var items = itemsResponse.Data ?? new List<MainSalesClassResponseDto>();

        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        // Map to report items with measurement unit info and class type
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
            ClassType = item.ClassType,
            CreatedAt = item.CreatedAt
        }).ToList();

        var document = new InventoryClassesReportDocument(
            reportItems,
            company,
            "sales",
            "ar",
            printedBy
        );

        return document.GeneratePDF();
    }

}