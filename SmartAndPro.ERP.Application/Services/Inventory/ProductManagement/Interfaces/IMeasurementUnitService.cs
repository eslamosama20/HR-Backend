using SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;

namespace SmartAndPro.ERP.Application.Services.Inventory.ProductManagement.Interfaces;

public interface IMeasurementUnitService
    : IServiceBase
    <
        MeasurementUnit,
        MeasurementUnitPostDto,
        MeasurementUnitPutDto,
        MeasurementUnitResponseDto,
        MeasurementUnitFiltersDto
    >
{
}