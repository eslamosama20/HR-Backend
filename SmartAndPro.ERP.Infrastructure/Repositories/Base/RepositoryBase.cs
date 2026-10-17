using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Linq.Expressions;

namespace SmartAndPro.ERP.Infrastructure.Repositories.Base;

public class RepositoryBase<T, TContext> : IRepository<T> where T : class where TContext : DbContext
{
    private readonly DbContext _context;
    private readonly bool _useTransaction;
    private readonly ILogger<T> _logger;
    private IDbContextTransaction _currentTransaction;

    public RepositoryBase(DbContext context,bool useTransaction = false)
    {
        _logger = new LoggerFactory().CreateLogger<T>();
        _context = context;
        _useTransaction = useTransaction;
    }

    public async Task BeginTransactionAsync()
    {
        if(_useTransaction && _currentTransaction == null)
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        if(_useTransaction && _currentTransaction != null)
        {
            await _context.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if(_useTransaction && _currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    public async Task<OperationResponse<T>> Add(T entity)
    {
        var response = new OperationResponse<T>(ErrorCodes.GeneralFail, entity);
        try
        {
            var addAction = _context.Set<T>().Add(entity);
            if (!_useTransaction)
            {
                var numberOfRecords = _context.SaveChanges();

                if (numberOfRecords > 0)
                {
                    response.ErrorCode = ErrorCodes.Success;
                    try
                    {
                        await _context.Entry(entity).ReloadAsync();
                    }
                    catch
                    {
                        // Ignored: reload might fail for entities with key precision differences (e.g. DateTime PK)
                    }
                    response.Data = entity;
                }
            }
            else
            {
                response.ErrorCode = ErrorCodes.Success;
                response.Data = entity;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, entity);
            LogService.LogError(ex);
            // Detach the entity (and related tracked entries) so lingering Added-state entities
            // don't contaminate subsequent SaveChanges calls on the shared DbContext.
            try
            {
                foreach (var entry in _context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added)
                    .ToList())
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch { /* ignore cleanup errors */ }
        }

        return response;
    }

    public async Task<OperationResponse<List<T>>> AddRange(List<T> entities)
    {
        var response = new OperationResponse<List<T>>(ErrorCodes.GeneralFail, entities);
        try
        {
            _context.Set<T>().AddRange(entities);
            if (!_useTransaction)
            {

                response.Data = entities;
                var numberOfRecords = await _context.SaveChangesAsync();
                if (numberOfRecords > 0)
                    response.ErrorCode = ErrorCodes.Success;
            }
            else
            {
                response.ErrorCode = ErrorCodes.Success;
                response.Data = entities;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, entities);
            LogService.LogError(ex);
        }

        return response;
    }

    public async Task<OperationResponse<IQueryable<T>>> Filter(Expression<Func<T, bool>> predicate)
    {
        var response = new OperationResponse<IQueryable<T>>(ErrorCodes.GeneralFail, null);
        ;
        try
        {
            var query = _context.Set<T>().Where(predicate).Where(predicate).AsNoTracking();

            response.Data = query;
            response.ErrorCode = ErrorCodes.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, predicate);
            LogService.LogError(ex);
        }

        return response;
    }
    // QueryInclude method to include related entities in query


    public async Task<OperationResponse<IQueryable<T>>> GetAll()
    {
        var response = new OperationResponse<IQueryable<T>>(ErrorCodes.GeneralFail, null);
        try
        {
            var query = _context.Set<T>().AsNoTracking();

            response.Data = query;
            response.ErrorCode = ErrorCodes.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            LogService.LogError(ex);
        }

        return response;
    }

    public async Task<OperationResponse<T>> Update(T entity)
    {
        var response = new OperationResponse<T>(ErrorCodes.GeneralFail, entity);
        try
        {
            // Check if the entity is already being tracked
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(p => entity.GetType().GetProperty(p.Name).GetValue(entity)).ToArray();
            var trackedEntity = _context.ChangeTracker.Entries<T>()
                .FirstOrDefault(e => e.Properties.Any(p => key.Contains(p.CurrentValue)));
            if (trackedEntity != null && !ReferenceEquals(trackedEntity.Entity, entity))
            {
                // Detach the existing tracked entity if it's a different instance
                _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
            }
            
            var navigationProperties = _context.Entry(entity).Navigations;
            foreach (var navigation in navigationProperties)
            {
                if (navigation.Metadata.IsCollection)
                {
                    var collection = (IEnumerable<object>)navigation.CurrentValue;
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            try
                            {
                                var itemEntry = _context.Entry(item);
                                if (itemEntry.State == EntityState.Detached)
                                    itemEntry.State = EntityState.Unchanged;
                            }
                            catch (InvalidOperationException) { /* already tracked with different instance - skip */ }
                        }
                    }
                }
                else
                {
                    if (navigation.CurrentValue != null)
                    {
                        try
                        {
                            var navEntry = _context.Entry(navigation.CurrentValue);
                            if (navEntry.State == EntityState.Detached)
                                navEntry.State = EntityState.Unchanged;
                        }
                        catch (InvalidOperationException) { /* already tracked with different instance - skip */ }
                    }
                }
            }
            _context.Update(entity);

            if (entity.GetType().GetProperty("CreatedBy") is not null)
                _context.Entry(entity).Property("CreatedBy").IsModified = false;
            if (entity.GetType().GetProperty("CreatedAt") is not null)
                _context.Entry(entity).Property("CreatedAt").IsModified = false;
            if (entity.GetType().GetProperty("Address") is not null)
                _context.Entry(entity).Property("Address").IsModified = true;

            response.Data = entity;

            if (!_useTransaction)
            {
                var numberOfRecords = await _context.SaveChangesAsync();
                response.ErrorCode = numberOfRecords > 0 ? ErrorCodes.Success : ErrorCodes.NoRecordsUpdated;
            }
            else
            {
                response.ErrorCode = ErrorCodes.Success; 

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, entity);
            LogService.LogError(ex);
        }

        return response;
    }

    public async Task<OperationResponse<List<T>>> UpdateRange(List<T> entities)
    {
        var response = new OperationResponse<List<T>>(ErrorCodes.GeneralFail, entities);
        try
        {
            foreach (var entity in entities) _context.Entry(entity).State = EntityState.Modified;
            if (!_useTransaction)
            {

                var numberOfRecords = await _context.SaveChangesAsync();

                if (numberOfRecords > 0)
                    response.ErrorCode = ErrorCodes.Success;
                else
                    response.ErrorCode = ErrorCodes.NoRecordsUpdated;
            }
            else { response.ErrorCode = ErrorCodes.Success;}

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, entities);
            LogService.LogError(ex);
        }

        return response;
    }

    public async Task<OperationResponse<T>> Delete(T entity)
    {
        var response = new OperationResponse<T>(ErrorCodes.GeneralFail, entity);
        try
        {
            // Check if the entity is already being tracked
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(p => entity.GetType().GetProperty(p.Name).GetValue(entity)).ToArray();
            var trackedEntity = _context.ChangeTracker.Entries<T>()
                .FirstOrDefault(e => e.Properties.Any(p => key.Contains(p.CurrentValue)));
            if (trackedEntity != null)
            {
                // Detach the existing tracked entity
                _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
            }
            _context.Set<T>().Remove(entity);
            response.Data = entity;
            if (!_useTransaction)
            {

                var numberOfRecords = await _context.SaveChangesAsync();
                response.ErrorCode = numberOfRecords > 0 ? ErrorCodes.Success : ErrorCodes.NoRecordsUpdated;
            }
            else
            {
                response.ErrorCode = ErrorCodes.Success;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, entity);
            LogService.LogError(ex);
        }

        return response;
    }

    public async Task<List<T>> ExecuteStored<T>(string stored, Dictionary<string, dynamic> parameters = null)
    {
        try
        {
            // Build the SQL query string for the stored procedure
            string? sql = BuildSqlQuery(stored, parameters);

            // Create SQL parameters from dictionary
            SqlParameter[]? sqlParams = parameters?
                .Select(p => new SqlParameter($"@{p.Key}", p.Value ?? DBNull.Value))
                .ToArray();

            // Execute the stored procedure and get the result
            var result = await _context.Database.SqlQueryRaw<T>(sql, sqlParams).ToListAsync();
            //var result = _context.Database.SqlQuery<T>($"DECLARE @DateTo DATETIME2(7) = '2024-09-18T23:59:59'; \r\n            DECLARE @StoreId UNIQUEIDENTIFIER = '7248FFD7-65F3-4986-B279-08DCCBA61159';\r\n\r\n\r\n            EXEC[dbo].[GetInventoryReport]\r\n            @DateFrom = @DateFrom, \r\n            @DateTo = @DateTo, \r\n            @StoreId = @StoreId;").ToList();


            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, stored);
            LogService.LogError(ex, stored);
            return default;

        }
    }

    private string BuildSqlQuery(string stored, Dictionary<string, dynamic> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return $"EXEC dbo.{stored}";

        var paramNames = string.Join(", ", parameters.Keys.Select(k => $"@{k}"));
        return $"EXEC dbo.{stored} {paramNames}";
    }

    public DbContext GetDbContext()
    {
        return _context;
    }
}