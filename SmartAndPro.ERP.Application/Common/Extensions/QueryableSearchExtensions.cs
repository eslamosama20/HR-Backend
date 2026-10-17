using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;

namespace SmartAndPro.ERP.Application.Common.Extensions;

public static class QueryableSearchExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _searchablePropertiesCache = new();

    public static IQueryable<TEntity> ApplySearchKeyword<TEntity>(this IQueryable<TEntity> query, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return query;

        var properties = _searchablePropertiesCache.GetOrAdd(typeof(TEntity), type =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<SearchableAttribute>() != null 
                         && p.GetCustomAttribute<NotSearchableAttribute>() == null
                         && p.PropertyType == typeof(string))
                .ToArray());

        if (properties.Length == 0)
            return Enumerable.Empty<TEntity>().AsQueryable(); // Return empty when no searchable properties (footgun protection)

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression? combinedExpression = null;

        foreach (var property in properties)
        {
            var propertyAccess = Expression.Property(parameter, property.Name);
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            
            var propertyToLower = Expression.Call(propertyAccess, toLowerMethod);
            var keywordConstant = Expression.Constant(keyword.ToLower(CultureInfo.InvariantCulture));
            var containsExpression = Expression.Call(propertyToLower, containsMethod, keywordConstant);
            
            combinedExpression = combinedExpression == null
                ? containsExpression
                : Expression.OrElse(combinedExpression, containsExpression);
        }

        if (combinedExpression == null)
            return Enumerable.Empty<TEntity>().AsQueryable();

        var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
        return query.Where(lambda);
    }
}
