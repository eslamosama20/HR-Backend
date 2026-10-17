using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Infrastructure.Contexts.Utility;

public interface IAutoNumberGenerator
{ 
    Task<string> GenerateAsync<TEntity>(DbContext context, string propertyName, string prefix, int length,string specialChar = "") where TEntity : class;
}