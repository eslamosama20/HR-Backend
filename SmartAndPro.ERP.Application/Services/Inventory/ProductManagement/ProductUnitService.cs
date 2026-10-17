using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement;

public class ProductUnitService : ServiceBase
<
    ProductUnit,
    ProductUnitPostDto,
    ProductUnitPutDto,
    ProductUnitResponseDto,
    ProductUnitFiltersDto
>, IProductUnitService
{
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;

    public ProductUnitService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<ProductUnitResponseDto>>> AddProductUnitsToInventoryAsync(
        List<ProductUnitPostDto> units)
    {
        var result = new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.GeneralFail, null);
        try
        {
            foreach (var entity in units)
            {
                if (entity.Quantity is null)
                    return new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.GeneralFail, null);

                await AddAsync(entity);
            }
            return new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    public async Task<OperationResponse<List<ProductUnitResponseDto>>> RemoveProductUnitsFromInventoryAsync(
        List<ProductUnitPostDto> units)
    {
        var result = new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.GeneralFail, null);
        try
        {
            foreach (var entity in units)
            {
                if (entity.Quantity is null)
                    return new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.GeneralFail, null);

                // check if the product unit exists in the inventory
                var productUnitRepository = _repositoriesFactory.GetRepository<ProductUnit>();
                var productUnitQuery = await productUnitRepository.Filter(x => x.StoreDetailsId == entity.StoreDetailsId &&
                                                                       x.MainPurchaseClassId ==
                                                                       entity.MainPurchaseClassId );
                var productUnit = productUnitQuery.Data.FirstOrDefault();
                if (productUnit is null)
                    return new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.NotFound, null);
                // delete the product units
                if (entity.Quantity > productUnit.Quantity)
                {
                    return new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.NotFound, null);
                }

                productUnit.Quantity -= entity.Quantity;
                productUnit.MainPurchaseClass = null;
                productUnit.MainSalesClass = null;
                productUnit.StoreDetails = null;
                await productUnitRepository.Update(productUnit);
            }
            return new OperationResponse<List<ProductUnitResponseDto>>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<ProductUnitResponseDto>> AddAsync(ProductUnitPostDto request)
    {
        if (request.Quantity is null)
            return new OperationResponse<ProductUnitResponseDto>(ErrorCodes.GeneralFail, null);
        var result = new OperationResponse<ProductUnitResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            var productUnitRequest = _mapper.Map<ProductUnit>(request);
            var productUnitRepository = _repositoriesFactory.GetRepository<ProductUnit>();
            var productUnitQuery = await productUnitRepository.Filter(x => x.StoreDetailsId == productUnitRequest.StoreDetailsId &&
                                                                           x.MainPurchaseClassId ==
                                                                           productUnitRequest.MainPurchaseClassId );
            var productUnit = productUnitQuery.Data.FirstOrDefault();
            if (productUnit is null)
            {
                var operation = await _repositoriesFactory.GetRepository<ProductUnit>().Add(productUnitRequest);
                if (operation.ErrorCode != ErrorCodes.Success)
                    return new OperationResponse<ProductUnitResponseDto>(operation.ErrorCode, null);
            
                var response = _mapper.Map<ProductUnitResponseDto>(operation.Data);
                return new OperationResponse<ProductUnitResponseDto>(ErrorCodes.Success, response);
            }

            productUnit.Quantity += productUnitRequest.Quantity ?? 0;
            productUnit.MainPurchaseClass = null;
            productUnit.MainSalesClass = null;
            productUnit.StoreDetails = null;
            var updateResult = await productUnitRepository.Update(productUnit);
            result.Data = _mapper.Map<ProductUnitResponseDto>(updateResult.Data);
            result.ErrorCode = updateResult.ErrorCode;
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
}