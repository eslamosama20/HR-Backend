using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailPurchaseClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;

public class DetailPurchaseClassService
    : ServiceBase
    <
        DetailPurchaseClass,
        DetailPurchaseClassPostDto,
        DetailPurchaseClassPutDto,
        DetailPurchaseClassResponseDto,
        DetailPurchaseClassFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public DetailPurchaseClassService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<DetailPurchaseClassResponseDto>> AddAsync(DetailPurchaseClassPostDto detailPurchaseClassPost)
    {
        try
        {
            var detailPurchaseClass = _mapper.Map<DetailPurchaseClass>(detailPurchaseClassPost);
            var referenceCode = detailPurchaseClass.ReferenceCode;
            var arabicName = detailPurchaseClass.ArabicName;
            var englishName = detailPurchaseClass.EnglishName;

            if (!string.IsNullOrWhiteSpace(referenceCode))
            {
                var rootPurchaseClassRepository = _repositoriesFactory.GetRepository<RootPurchaseClass>();
                var rootPurchaseClassOperation = await rootPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (rootPurchaseClassOperation.Data.Any())
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var headerPurchaseClassRepository = _repositoriesFactory.GetRepository<HeaderPurchaseClass>();
                var headerPurchaseClassOperation = await headerPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (headerPurchaseClassOperation.Data.Any())
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var detailPurchaseClassRepository = _repositoriesFactory.GetRepository<DetailPurchaseClass>();
                var detailPurchaseClassOperation = await detailPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (detailPurchaseClassOperation.Data.Any())
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var mainPurchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var mainPurchaseClassOperation = await mainPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (mainPurchaseClassOperation.Data.Any())
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            }

            var repo = _repositoriesFactory.GetRepository<DetailPurchaseClass>();
            if (!string.IsNullOrWhiteSpace(arabicName))
            {
                var duplicateArabic = await repo.Filter(x => x.ArabicName == arabicName);
                if (duplicateArabic.Data.Any())
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
            }
            if (!string.IsNullOrWhiteSpace(englishName))
            {
                var duplicateEnglish = await repo.Filter(x => x.EnglishName == englishName);
                if (duplicateEnglish.Data.Any())
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
            }
            
            return await base.AddAsync(detailPurchaseClassPost);
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
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while saving the record");
        }
    }
    
    public override async Task<OperationResponse<DetailPurchaseClassResponseDto>> DeleteAsync(Guid id)
    {
        try{
            var detailPurchaseClassRepository = _repositoriesFactory.GetRepository<DetailPurchaseClass>();
            var detailPurchaseClass = await detailPurchaseClassRepository.Filter(x => x.Id == id);
            if (detailPurchaseClass.Data.Any(x => x.MainPurchaseClasses.Any()))
                return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);

            // check if detail account has any MainPurchaseClasses
            if (detailPurchaseClass.Data.Any(x => x.MainPurchaseClasses.Any()))
                return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Conflict, null,
                    message:"DetailPurchaseClass has MainPurchaseClasses");
            
            if (detailPurchaseClass.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = detailPurchaseClass.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.NotFound, null);
            
            var operation = await _repositoriesFactory.GetRepository<DetailPurchaseClass>().Delete(entity);
            return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.Success, null);
        }catch(Exception ex){
            LogService.LogError(ex);
            return new OperationResponse<DetailPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }
    
}