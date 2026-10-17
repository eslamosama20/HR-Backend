using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.RootPurchaseClass;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ClassesManagement;

public class RootPurchaseClassService
    : ServiceBase
    <
        RootPurchaseClass,
        RootPurchaseClassPostDto,
        RootPurchaseClassPutDto,
        RootPurchaseClassResponseDto,
        RootPurchaseClassFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public RootPurchaseClassService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<RootPurchaseClassResponseDto>> AddAsync(RootPurchaseClassPostDto rootPurchaseClassPost)
    {
        try
        {
            var rootPurchaseClass = _mapper.Map<RootPurchaseClass>(rootPurchaseClassPost);
            var referenceCode = rootPurchaseClass.ReferenceCode;
            var arabicName = rootPurchaseClass.ArabicName;
            var englishName = rootPurchaseClass.EnglishName;

            if (!string.IsNullOrWhiteSpace(referenceCode))
            {
                var rootPurchaseClassRepository = _repositoriesFactory.GetRepository<RootPurchaseClass>();
                var rootPurchaseClassOperation = await rootPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (rootPurchaseClassOperation.Data.Any())
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var headerPurchaseClassRepository = _repositoriesFactory.GetRepository<HeaderPurchaseClass>();
                var headerPurchaseClassOperation = await headerPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (headerPurchaseClassOperation.Data.Any())
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var detailPurchaseClassRepository = _repositoriesFactory.GetRepository<DetailPurchaseClass>();
                var detailPurchaseClassOperation = await detailPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (detailPurchaseClassOperation.Data.Any())
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                
                var mainPurchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
                var mainPurchaseClassOperation = await mainPurchaseClassRepository.Filter(x => x.ReferenceCode == referenceCode);
                if (mainPurchaseClassOperation.Data.Any())
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
            }

            var repo = _repositoriesFactory.GetRepository<RootPurchaseClass>();
            if (!string.IsNullOrWhiteSpace(arabicName))
            {
                var duplicateArabic = await repo.Filter(x => x.ArabicName == arabicName);
                if (duplicateArabic.Data.Any())
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
            }
            if (!string.IsNullOrWhiteSpace(englishName))
            {
                var duplicateEnglish = await repo.Filter(x => x.EnglishName == englishName);
                if (duplicateEnglish.Data.Any())
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
            }
            
            return await base.AddAsync(rootPurchaseClassPost);
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
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ArabicName already exists");
                if (ex.Message?.Contains("EnglishName") == true || ex.InnerException?.Message?.Contains("EnglishName") == true)
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "EnglishName already exists");
                if (ex.Message?.Contains("ReferenceCode") == true || ex.InnerException?.Message?.Contains("ReferenceCode") == true)
                    return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "ReferenceCode already exists");
                return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null, message: "A record with the same name or code already exists");
            }
            return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message ?? "An error occurred while saving the record");
        }
    }
    

    public override async Task<OperationResponse<RootPurchaseClassResponseDto>> DeleteAsync(Guid id)
    {
        try{
            // check if root account - headerPurchaseClass - detailPurchaseClass - MainPurchaseClass has any transactionEntries
            var rootPurchaseClassRepository = _repositoriesFactory.GetRepository<RootPurchaseClass>();
            var rootPurchaseClass = await rootPurchaseClassRepository.Filter(x => x.Id == id);
            if (rootPurchaseClass.Data.Any(x =>
                    x.HeaderPurchaseClasses.Any(y =>
                        y.DetailPurchaseClasses.Any(z => z.MainPurchaseClasses.Any()))))
                return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);

            // check if root account has any headerPurchaseClasses
            if (rootPurchaseClass.Data.Any(x => x.HeaderPurchaseClasses.Any()))
                return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Conflict, null,
                    message:"RootPurchaseClass has headerPurchaseClasss");
            
            
            if (rootPurchaseClass.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = rootPurchaseClass.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.NotFound, null);
            
            var operation = await _repositoriesFactory.GetRepository<RootPurchaseClass>().Delete(entity);
            return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.Success, null);
        }catch(Exception ex){
            LogService.LogError(ex);
            return new OperationResponse<RootPurchaseClassResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }
    
}