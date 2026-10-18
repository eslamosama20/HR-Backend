using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Extensions;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Diagnostics;
using System.Reflection;

namespace SmartAndPro.ERP.Application.Services.@base;

public abstract class ServiceBase<TEntity, TPostDto, TPutDto, TResponseDto , TFilters> : IServiceBase<TEntity, TPostDto, TPutDto, TResponseDto, TFilters>
    where TEntity : Entity
    where TPostDto : class
    where TPutDto : class
    where TResponseDto : class
    where TFilters : class
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly IRepository<TEntity> _repository;

    protected ServiceBase(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;

        _repository = repositoriesFactory.GetRepository<TEntity>();
    }

    public virtual async Task<OperationResponse<List<TResponseDto>>> GetAllAsync(TFilters filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<TResponseDto>>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var operation = await _repository.GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<TResponseDto>>(operation.ErrorCode, null);

            // 1. Apply filters first
            var query = filters.AllPropertiesAreNull()
                ? operation.Data
                : operation.Data.FilterQueryableData(filters);

            // 1.5. Apply SearchKeyword if present on the filter DTO
            var searchKeywordProperty = typeof(TFilters).GetProperty("SearchKeyword");
            if (searchKeywordProperty != null)
            {
                var keywordValue = searchKeywordProperty.GetValue(filters) as string;
                if (!string.IsNullOrWhiteSpace(keywordValue))
                {
                    query = query.ApplySearchKeyword(keywordValue);
                }
            }

            // Structured logging for search calls
            var searchKeywordValue = searchKeywordProperty?.GetValue(filters) as string;
            var isSearchCall = !string.IsNullOrWhiteSpace(searchKeywordValue);
            var stopwatch = isSearchCall ? Stopwatch.StartNew() : null;

            var totalRecords = await query.CountAsync();

            // 2. Apply pagination directly in the query and protect memory
            if (pageNumber.HasValue || pageSize.HasValue)
            {
                pageNumber ??= 1;
                pageSize ??= 50;
                if (pageSize > 1000) pageSize = 1000;
                if (pageSize < 1) pageSize = 1;

                var skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
            }
            else
            {
                query = query.ApplySort(sortField, sortOrder);
            }
            
            var operationData = await query.ToListAsync();

            // Log search metrics
            if (isSearchCall && stopwatch != null)
            {
                stopwatch.Stop();
                LogService.LogInformation($"[Search] entity={typeof(TEntity).Name}, query=\"{searchKeywordValue}\", totalCount={totalRecords}, duration={stopwatch.ElapsedMilliseconds}ms");
            }
            
            if (operationData.Count == 0)
                return new OperationResponse<List<TResponseDto>>(ErrorCodes.Success, new List<TResponseDto>(), totalCount: 0, isPaginated: true);

            var responseData = _mapper.Map<List<TResponseDto>>(operationData);
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                return new OperationResponse<List<TResponseDto>>(
                    ErrorCodes.Success,
                    responseData,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<TResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch (ArgumentException ex)
        {
            LogService.LogInformation($"[Sort] Invalid sort field: {ex.Message}");
            return new OperationResponse<List<TResponseDto>>(ErrorCodes.BadRequest, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<TResponseDto>>(ErrorCodes.GeneralFail, null, message: ex.ToString());
        }
        return result;
    }

    
    public virtual async Task<OperationResponse<List<TResponseDto>>> SearchAsync(
        string? keyword,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        // Create a new filter instance and set the SearchKeyword via reflection
        var filters = Activator.CreateInstance<TFilters>();
        var searchKeywordProperty = typeof(TFilters).GetProperty("SearchKeyword");
        searchKeywordProperty?.SetValue(filters, keyword);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await GetAllAsync(filters, pageNumber, pageSize, sortField, sortOrder);
        stopwatch.Stop();

        // Set Query and Took on the response
        result.Query = keyword;
        result.Took = (int)stopwatch.ElapsedMilliseconds;

        return result;
    }

    
    public virtual async Task<OperationResponse<PaginatedResult<List<TResponseDto>>>> GetAllPaginatedAsync(
    TFilters filters,
    int? pageNumber = null,
    int? pageSize = null,
    string? sortField = null,
    int? sortOrder = null)
    {
        try
        {
            // 1️⃣ جلب كل البيانات من الـ repository
            var operation = await _repository.GetAll();

            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<PaginatedResult<List<TResponseDto>>>(operation.ErrorCode, null);

            // 2️⃣ تطبيق الـ filters
            var query = filters.AllPropertiesAreNull()
                ? operation.Data
                : operation.Data.FilterQueryableData(filters);

            // 3️⃣ عدّ عدد السجلات
            var totalRecords = await query.CountAsync();

            // 4️⃣ لو مفيش بيانات، نرجع نتيجة فاضية مباشرة
            if (totalRecords == 0)
                return new OperationResponse<PaginatedResult<List<TResponseDto>>>(
                    ErrorCodes.Success,
                    new PaginatedResult<List<TResponseDto>>(0, new List<TResponseDto>())
                );

            // 5️⃣ تعيين pagination افتراضي وحماية الذاكرة
            pageNumber ??= 1;
            pageSize ??= 50;
            if (pageSize > 100) pageSize = 100;

            var skip = (pageNumber.Value - 1) * pageSize.Value;

            // 6️⃣ جلب الصفحة المطلوبة مباشرة
            var paginatedData = await query
                .ApplySort(sortField, sortOrder)
                .Skip(skip)
                .Take(pageSize.Value)
                .AsNoTracking()
                .ToListAsync();

            // 7️⃣ تحويل النتائج إلى DTO باستخدام AutoMapper
            var mappedData = _mapper.Map<List<TResponseDto>>(paginatedData);

            // 8️⃣ إنشاء PaginatedResult
            var result = new PaginatedResult<List<TResponseDto>>(totalRecords, mappedData);

            // 9️⃣ إرجاع النتيجة
            return new OperationResponse<PaginatedResult<List<TResponseDto>>>(ErrorCodes.Success, result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<PaginatedResult<List<TResponseDto>>>(
                ErrorCodes.GeneralFail,
                null,
                message: ex.Message
            );
        }
    }

   
    public virtual async Task<OperationResponse<TResponseDto>> AddAsync(TPostDto request)
    {
        var result = new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var entity = _mapper.Map<TEntity>(request);
            var operation = await _repository.Add(entity);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<TResponseDto>(operation.ErrorCode, null, message: operation.Message);
            
            var responseData = _mapper.Map<TResponseDto>(operation.Data);
            return new OperationResponse<TResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            result.Message = ex.InnerException != null ? $"{ex.Message} --> {ex.InnerException.Message}" : ex.Message;
        }
        return result;
    }
    
    public virtual async Task<OperationResponse<List<TResponseDto>>> AddRangeAsync(List<TPostDto> request)
    {
        var result = new OperationResponse<List<TResponseDto>>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var entities = _mapper.Map<List<TEntity>>(request);
            var operation = await _repository.AddRange(entities);
            
            if (operation.ErrorCode != ErrorCodes.Success)
            {
                result = new OperationResponse<List<TResponseDto>>(operation.ErrorCode, null);
                return result;
            }
            
            var responseData = _mapper.Map<List<TResponseDto>>(operation.Data);
            return new OperationResponse<List<TResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    public virtual async Task<OperationResponse<TResponseDto>> UpdateAsync(TPutDto request)
    {
        var result = new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var entity = _mapper.Map<TEntity>(request);
            var operation = await _repository.Update(entity);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<TResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<TResponseDto>(operation.Data);
            return new OperationResponse<TResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    public virtual async Task<OperationResponse<TResponseDto>> GetAsync(Guid id)
    {
        var result = new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var operation = await _repository.Filter(x => x.Id == id);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = operation.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<TResponseDto>(ErrorCodes.NotFound, null);
            
            var responseData = _mapper.Map<TResponseDto>(entity);
            return new OperationResponse<TResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public virtual async Task<OperationResponse<TResponseDto>> DeleteAsync(Guid id)
    {
        var result = new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var entityOperation = await _repository.Filter(x => x.Id == id);
            
            if (entityOperation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail, null);
            
            var entity = entityOperation.Data!.FirstOrDefault();
            if (entity is null)
                return new OperationResponse<TResponseDto>(ErrorCodes.NotFound, null);
            
            entity.SoftDelete();
            var operation = await _repository.Update(entity);
            return operation.ErrorCode != ErrorCodes.Success 
                ? new OperationResponse<TResponseDto>(operation.ErrorCode, null) 
                : new OperationResponse<TResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public virtual async Task<OperationResponse<TResponseDto>> DeleteRangeAsync(List<Guid> ids)
    {
        var result = new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            var entitiesOperation = await _repository.Filter(x => ids.Contains(x.Id));
            if (entitiesOperation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<TResponseDto>(ErrorCodes.GeneralFail, null);

            var entities = await entitiesOperation.Data.ToListAsync();
            if (entities == null || !entities.Any())
                return new OperationResponse<TResponseDto>(ErrorCodes.NotFound, null);

            foreach (var entity in entities)
            {
                entity.SoftDelete();
            }

            var operation = await _repository.UpdateRange(entities);
            return operation.ErrorCode != ErrorCodes.Success
                ? new OperationResponse<TResponseDto>(operation.ErrorCode, null)
                : new OperationResponse<TResponseDto>(ErrorCodes.NoContent, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
}

    