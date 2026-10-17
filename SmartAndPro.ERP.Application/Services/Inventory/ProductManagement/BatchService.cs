using AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement;

public class BatchService
    : ServiceBase
    <
        ProductBatchDetails,
        BatchDetailsPostDto,
        BatchDetailsPutDto,
        BatchDetailsResponseDto,
        BatchDetailsFiltersDto
    >, IBatchService
{
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;

    public BatchService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper) : base(
        repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }



    public async Task<OperationResponse<BatchDetailsResponseDto>> AssignProductToBatch(
        ProductBatchToUnitAssignmentsDto productBatchToUnitAssignmentsDto)
    {
        var result = new OperationResponse<BatchDetailsResponseDto>(ErrorCodes.GeneralFail,
            null);

        try
        {
            var productBatchToUnitAssignments =
                _mapper.Map<ProductBatchToUnitAssignments>(productBatchToUnitAssignmentsDto);
            if (productBatchToUnitAssignments is not null)
            {
                var queryableData = await _repositoriesFactory.GetRepository<ProductBatchToUnitAssignments>()
                    .Add(productBatchToUnitAssignments);

                result.Data = _mapper.Map<BatchDetailsResponseDto>(queryableData.Data.ProductBatchDetails);
                result.ErrorCode = ErrorCodes.Success;
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
    
}