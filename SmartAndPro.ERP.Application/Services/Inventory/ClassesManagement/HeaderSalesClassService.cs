using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderSalesClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;

public class HeaderSalesClassService
    : ServiceBase
    <
        HeaderSalesClass,
        HeaderSalesClassPostDto,
        HeaderSalesClassPutDto,
        HeaderSalesClassResponseDto,
        HeaderSalesClassFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public HeaderSalesClassService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<HeaderSalesClassResponseDto>> AddAsync(HeaderSalesClassPostDto headerSalesClassPost)
    {
        try
        {
            var headerSalesClass = _mapper.Map<HeaderSalesClass>(headerSalesClassPost);
            var referenceCode = headerSalesClass.ReferenceCode;
            var arabicName = headerSalesClass.ArabicName;
            var englishName = headerSalesClass.EnglishName;

            if (!string.IsNullOrWhiteSpace(referenceCode))
            {
                var rootSalesClassRepository = _repositoriesFactory.GetRepository<RootSalesClass>();
                var rootSalesClassOperation = await rootSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (rootSalesClassOperation.Data.Any())
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var headerSalesClassRepository = _repositoriesFactory.GetRepository<HeaderSalesClass>();
                var headerSalesClassOperation = await headerSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (headerSalesClassOperation.Data.Any())
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var detailSalesClassRepository = _repositoriesFactory.GetRepository<DetailSalesClass>();
                var detailSalesClassOperation = await detailSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (detailSalesClassOperation.Data.Any())
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var mainSalesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();
                var mainSalesClassOperation = await mainSalesClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (mainSalesClassOperation.Data.Any())
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            }

            var repo = _repositoriesFactory.GetRepository<HeaderSalesClass>();
            if (!string.IsNullOrWhiteSpace(arabicName))
            {
                var duplicateArabic = await repo.Filter(x => x.ArabicName == arabicName);
                if (duplicateArabic.Data.Any())
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
            }
            if (!string.IsNullOrWhiteSpace(englishName))
            {
                var duplicateEnglish = await repo.Filter(x => x.EnglishName == englishName);
                if (duplicateEnglish.Data.Any())
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
            }
            
            return await base.AddAsync(headerSalesClassPost);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            if (ex.InnerException?.Message?.Contains("UNIQUE constraint") == true || 
                ex.InnerException?.Message?.Contains("duplicate key") == true ||
                ex.Message?.Contains("UNIQUE constraint") == true ||
                ex.Message?.Contains("duplicate key") == true)
            {
                if (ex.Message?.Contains("ArabicName") == true || ex.InnerException?.Message?.Contains("ArabicName") == true)
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while saving the record");
        }
    }

    public override async Task<OperationResponse<HeaderSalesClassResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            var headerSalesClassRepository = _repositoriesFactory.GetRepository<HeaderSalesClass>();
            var headerSalesClass = await headerSalesClassRepository.Filter(x => x.Id == id);
            
            // Use IgnoreAutoIncludes to avoid loading navigation properties
            var entity = await headerSalesClass.Data
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync();
                
            if (entity is null)
                return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.NotFound, null);

            // Check if has children before delete
            var childrenCheck = await _repositoriesFactory.GetRepository<DetailSalesClass>()
                .Filter(x => x.HeaderSalesClassId == id);
            var hasChildren = await childrenCheck.Data.AnyAsync();
                
            if (hasChildren)
                return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.Conflict, null,
                    message:"HeaderSalesClass has detailSalesClasses");

            // Soft delete instead of hard delete
            entity.SoftDelete();
            var operation = await _repositoriesFactory.GetRepository<HeaderSalesClass>().Update(entity);
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<HeaderSalesClassResponseDto>(operation.ErrorCode, null);
            
            return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return new OperationResponse<HeaderSalesClassResponseDto>(ErrorCodes.GeneralFail, null);
    }
    
}