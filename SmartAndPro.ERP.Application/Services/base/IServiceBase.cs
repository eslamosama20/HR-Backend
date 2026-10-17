using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Application.Services.@base;

public interface IServiceBase<TEntity, TPostDto, TPutDto, TResponseDto, TFilters>
    where TEntity : Entity
    where TPostDto : class
    where TPutDto : class
    where TResponseDto : class
    where TFilters : class
    
{
    Task<OperationResponse<List<TResponseDto>>> GetAllAsync(TFilters filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null);
    Task<OperationResponse<TResponseDto>> AddAsync(TPostDto request);
    Task<OperationResponse<TResponseDto>> UpdateAsync(TPutDto request);
    Task<OperationResponse<TResponseDto>> GetAsync(Guid id);
    Task<OperationResponse<TResponseDto>> DeleteAsync(Guid id);
    Task<OperationResponse<TResponseDto>> DeleteRangeAsync(List<Guid> ids);
}