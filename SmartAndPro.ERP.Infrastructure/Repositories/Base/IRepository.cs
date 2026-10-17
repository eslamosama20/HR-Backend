using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;

namespace SmartAndPro.ERP.Infrastructure.Repositories.Base;

public interface IRepository<T> where T : class
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<OperationResponse<IQueryable<T>>> GetAll();
    Task<OperationResponse<IQueryable<T>>> Filter(Expression<Func<T, bool>> predicate);
    Task<OperationResponse<T>> Add(T entity);
    Task<OperationResponse<T>> Update(T entity);
    Task<OperationResponse<List<T>>> AddRange(List<T> entities);
    Task<OperationResponse<List<T>>> UpdateRange(List<T> entities);

    Task<OperationResponse<T>> Delete(T entity);
    
    Task<List<T>> ExecuteStored<T>(string stored, Dictionary<string, dynamic> parameters = null);
    
    DbContext GetDbContext();
}