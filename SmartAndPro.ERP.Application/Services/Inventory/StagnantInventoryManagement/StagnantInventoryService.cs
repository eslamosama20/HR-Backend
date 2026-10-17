using AutoMapper;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StagnantInventory;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.StagnantInventoryManagement.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.StagnantInventoryManagement;

public class StagnantInventoryService : ServiceBase<StagnantInventory, StagnantInventoryPostDto, StagnantInventoryPutDto, StagnantInventoryResponseDto, StagnantInventoryFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _localMapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public StagnantInventoryService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, ServiceBaseFactory serviceBaseFactory) 
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _localMapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
    }

    /// <summary>
    /// إضافة منتج للرواكد يدوياً
    /// </summary>
    public async Task<OperationResponse<StagnantInventoryResponseDto>> AddManualAsync(StagnantInventoryPostDto postDto, string addedBy)
    {
        postDto.Reason = StagnantReason.Manual;
        postDto.IsAutoAdded = false;
        postDto.AddedBy = addedBy;
        
        return await base.AddAsync(postDto);
    }

    /// <summary>
    /// إضافة منتج للرواكد من إذن الدخول مع تحديد مدة الركود
    /// </summary>
    public async Task<OperationResponse<StagnantInventoryResponseDto>> AddFromEntryPermitAsync(
        Guid? mainPurchaseClassId,
        Guid? mainSalesClassId,
        decimal quantity,
        Guid storeDetailsId,
        int stagnantPeriodMonths,
        string addedBy)
    {
        var postDto = new StagnantInventoryPostDto
        {
            MainPurchaseClassId = mainPurchaseClassId,
            MainSalesClassId = mainSalesClassId,
            Quantity = quantity,
            StoreDetailsId = storeDetailsId,
            StagnantPeriodMonths = stagnantPeriodMonths,
            Reason = StagnantReason.OnEntryPermit,
            IsAutoAdded = false,
            AddedBy = addedBy,
            LastMovementDate = DateTime.UtcNow,
            Notes = $"سيتم التحويل للرواكد تلقائياً بعد {stagnantPeriodMonths} شهر في حالة عدم وجود حركات"
        };
        
        return await base.AddAsync(postDto);
    }

    /// <summary>
    /// الحصول على الرواكد مع الفلاتر
    /// </summary>
    public async Task<List<StagnantInventoryResponseDto>> GetStagnantInventoryWithFiltersAsync(StagnantInventoryFiltersDto filters)
    {
        var repository = _repositoriesFactory.GetRepository<StagnantInventory>();
        var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
        var purchaseClassRepository = _repositoriesFactory.GetRepository<MainPurchaseClass>();
        var salesClassRepository = _repositoriesFactory.GetRepository<MainSalesClass>();

        // Load all stagnant items
        var stagnantResult = await repository.Filter(x => x.IsActive == true);
        var items = stagnantResult.Data.ToList();

        // Apply filters
        if (filters.StoreDetailsId.HasValue)
        {
            items = items.Where(x => x.StoreDetailsId == filters.StoreDetailsId.Value).ToList();
        }

        if (filters.Reason.HasValue)
        {
            items = items.Where(x => x.Reason == filters.Reason.Value).ToList();
        }

        if (filters.IsDisposed.HasValue)
        {
            items = items.Where(x => x.IsDisposed == filters.IsDisposed.Value).ToList();
        }

        if (filters.IsAutoAdded.HasValue)
        {
            items = items.Where(x => x.IsAutoAdded == filters.IsAutoAdded.Value).ToList();
        }

        if (filters.FromDate.HasValue)
        {
            var fromDate = filters.FromDate.Value.Date;
            items = items.Where(x => x.AddedToStagnantDate >= fromDate).ToList();
        }

        if (filters.ToDate.HasValue)
        {
            var toDate = filters.ToDate.Value.Date.AddDays(1);
            items = items.Where(x => x.AddedToStagnantDate < toDate).ToList();
        }

        // Get unique IDs for related data
        var storeIds = items.Where(x => x.StoreDetailsId.HasValue).Select(x => x.StoreDetailsId!.Value).Distinct().ToList();
        var purchaseClassIds = items.Where(x => x.MainPurchaseClassId.HasValue).Select(x => x.MainPurchaseClassId!.Value).Distinct().ToList();
        var salesClassIds = items.Where(x => x.MainSalesClassId.HasValue).Select(x => x.MainSalesClassId!.Value).Distinct().ToList();

        // Load related data
        var storesResult = storeIds.Any() ? await storeRepository.Filter(x => storeIds.Contains(x.Id)) : null;
        var stores = storesResult?.Data.ToDictionary(x => x.Id, x => x) ?? new Dictionary<Guid, StoreDetails>();

        var purchaseClassesResult = purchaseClassIds.Any() ? await purchaseClassRepository.Filter(x => purchaseClassIds.Contains(x.Id)) : null;
        var purchaseClasses = purchaseClassesResult?.Data.ToDictionary(x => x.Id, x => x) ?? new Dictionary<Guid, MainPurchaseClass>();

        var salesClassesResult = salesClassIds.Any() ? await salesClassRepository.Filter(x => salesClassIds.Contains(x.Id)) : null;
        var salesClasses = salesClassesResult?.Data.ToDictionary(x => x.Id, x => x) ?? new Dictionary<Guid, MainSalesClass>();

        // Apply search filter (now we have the related data)
        if (!string.IsNullOrEmpty(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            items = items.Where(x =>
            {
                var purchaseClassName = x.MainPurchaseClassId.HasValue && purchaseClasses.ContainsKey(x.MainPurchaseClassId.Value)
                    ? purchaseClasses[x.MainPurchaseClassId.Value].ArabicName : null;
                var salesClassName = x.MainSalesClassId.HasValue && salesClasses.ContainsKey(x.MainSalesClassId.Value)
                    ? salesClasses[x.MainSalesClassId.Value].ArabicName : null;

                return (purchaseClassName != null && purchaseClassName.ToLower().Contains(searchTerm)) ||
                       (salesClassName != null && salesClassName.ToLower().Contains(searchTerm)) ||
                       (x.Notes != null && x.Notes.ToLower().Contains(searchTerm));
            }).ToList();
        }

        // Order and map results
        var orderedItems = items.OrderByDescending(x => x.AddedToStagnantDate).ToList();

        return orderedItems.Select(x =>
        {
            var store = x.StoreDetailsId.HasValue && stores.ContainsKey(x.StoreDetailsId.Value) ? stores[x.StoreDetailsId.Value] : null;
            var purchaseClass = x.MainPurchaseClassId.HasValue && purchaseClasses.ContainsKey(x.MainPurchaseClassId.Value) ? purchaseClasses[x.MainPurchaseClassId.Value] : null;
            var salesClass = x.MainSalesClassId.HasValue && salesClasses.ContainsKey(x.MainSalesClassId.Value) ? salesClasses[x.MainSalesClassId.Value] : null;

            return new StagnantInventoryResponseDto
            {
                Id = x.Id,
                MainPurchaseClassId = x.MainPurchaseClassId,
                MainPurchaseClassName = purchaseClass?.ArabicName,
                ProductCode = purchaseClass?.ReferenceCode ?? salesClass?.ReferenceCode,
                MainSalesClassId = x.MainSalesClassId,
                MainSalesClassName = salesClass?.ArabicName,
                Quantity = x.Quantity,
                LastMovementDate = x.LastMovementDate,
                AddedToStagnantDate = x.AddedToStagnantDate,
                Reason = x.Reason,
                ReasonDisplay = GetReasonDisplay(x.Reason),
                StagnantPeriodMonths = x.StagnantPeriodMonths,
                IsAutoAdded = x.IsAutoAdded,
                Notes = x.Notes,
                AddedBy = x.AddedBy,
                StoreDetailsId = x.StoreDetailsId,
                StoreName = store?.ArabicName,
                IsDisposed = x.IsDisposed,
                DisposedDate = x.DisposedDate,
                DisposalMethod = x.DisposalMethod,
                CreatedAt = x.CreatedAt,
                IsActive = x.IsActive
            };
        }).ToList();
    }

    /// <summary>
    /// التخلص من منتج راكد
    /// </summary>
    public async Task<OperationResponse<StagnantInventoryResponseDto>> DisposeAsync(Guid id, string disposalMethod, string disposedBy)
    {
        var repository = _repositoriesFactory.GetRepository<StagnantInventory>();
        var entityResult = await repository.Filter(x => x.Id == id);
        var entity = entityResult.Data.FirstOrDefault();
        
        if (entity == null)
        {
            return new OperationResponse<StagnantInventoryResponseDto>
            {
                ErrorCode = ErrorCodes.NotFound,
                Message = "المنتج غير موجود"
            };
        }

        entity.IsDisposed = true;
        entity.DisposedDate = DateTime.UtcNow;
        entity.DisposalMethod = disposalMethod;
        entity.Notes = $"{entity.Notes}\n\n--- تم التخلص بواسطة: {disposedBy} ---";

        var updateResult = await repository.Update(entity);
        
        return new OperationResponse<StagnantInventoryResponseDto>
        {
            ErrorCode = updateResult.ErrorCode,
            Data = MapToResponseDto(entity)
        };
    }

    /// <summary>
    /// إنشاء تقرير الرواكد PDF
    /// </summary>
    public async Task<byte[]> GetStagnantInventoryReportDocument(Guid? storeId, DateTime? startDate, DateTime? endDate, string? printedBy = null)
    {
        var filters = new StagnantInventoryFiltersDto
        {
            StoreDetailsId = storeId,
            FromDate = startDate,
            ToDate = endDate
        };

        var items = await GetStagnantInventoryWithFiltersAsync(filters);
        
        // Get company info
        var companyRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyResult = await companyRepository.Filter(c => c.IsActive == true);
        var company = companyResult.Data.FirstOrDefault();

        // Get store info
        string storeName = "جميع المخازن";
        if (storeId.HasValue)
        {
            var storeRepository = _repositoriesFactory.GetRepository<StoreDetails>();
            var storeResult = await storeRepository.Filter(x => x.Id == storeId.Value);
            var store = storeResult.Data.FirstOrDefault();
            storeName = store?.ArabicName ?? "غير محدد";
        }

        var document = new StagnantInventoryReportDocument(
            items,
            company,
            storeName,
            startDate,
            endDate,
            "ar",
            printedBy
        );

        return document.GeneratePdf();
    }

    private StagnantInventoryResponseDto MapToResponseDto(StagnantInventory x)
    {
        return new StagnantInventoryResponseDto
        {
            Id = x.Id,
            MainPurchaseClassId = x.MainPurchaseClassId,
            MainPurchaseClassName = x.MainPurchaseClass?.ArabicName,
            ProductCode = x.MainPurchaseClass?.ReferenceCode ?? x.MainSalesClass?.ReferenceCode,
            MainSalesClassId = x.MainSalesClassId,
            MainSalesClassName = x.MainSalesClass?.ArabicName,
            Quantity = x.Quantity,
            LastMovementDate = x.LastMovementDate,
            AddedToStagnantDate = x.AddedToStagnantDate,
            Reason = x.Reason,
            ReasonDisplay = GetReasonDisplay(x.Reason),
            StagnantPeriodMonths = x.StagnantPeriodMonths,
            IsAutoAdded = x.IsAutoAdded,
            Notes = x.Notes,
            AddedBy = x.AddedBy,
            StoreDetailsId = x.StoreDetailsId,
            StoreName = x.StoreDetails?.ArabicName,
            IsDisposed = x.IsDisposed,
            DisposedDate = x.DisposedDate,
            DisposalMethod = x.DisposalMethod,
            CreatedAt = x.CreatedAt,
            IsActive = x.IsActive
        };
    }

    private string GetReasonDisplay(StagnantReason reason)
    {
        return reason switch
        {
            StagnantReason.Manual => "تحويل يدوي",
            StagnantReason.AutoByPeriod => "تلقائي - انتهاء المدة",
            StagnantReason.OnEntryPermit => "من إذن الدخول",
            _ => "غير محدد"
        };
    }
}
