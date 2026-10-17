using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Application.Common.Extensions;

public static class SortByExtensions
{
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _propertyCache = new();
    private static readonly HashSet<Type> _sortableTypes = new()
    {
        typeof(string), typeof(int), typeof(int?), typeof(long), typeof(long?),
        typeof(decimal), typeof(decimal?), typeof(double), typeof(double?),
        typeof(float), typeof(float?), typeof(bool), typeof(bool?),
        typeof(DateTime), typeof(DateTime?), typeof(DateOnly), typeof(DateOnly?), typeof(Guid), typeof(Guid?)
    };

    public static IQueryable<TEntity> ApplySort<TEntity>(this IQueryable<TEntity> query, string? sortField, int? sortOrder)
        where TEntity : Entity
    {
        if (string.IsNullOrWhiteSpace(sortField))
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }

        var property = _propertyCache.GetOrAdd((typeof(TEntity), sortField), key =>
        {
            // Try direct (case-sensitive) lookup first — fast path
            var prop = typeof(TEntity).GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
            // Fall back to case-insensitive lookup — the frontend sends camelCase (arabicName)
            // but C# entity properties are PascalCase (ArabicName)
            if (prop == null)
            {
                prop = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => string.Equals(p.Name, key.Item2, StringComparison.OrdinalIgnoreCase));
            }
            // Only allow sorting on primitive/string types, reject navigation properties
            if (prop != null && !_sortableTypes.Contains(prop.PropertyType))
                return null;
            return prop;
        });

        if (property == null)
        {
            throw new ArgumentException($"Sort field '{sortField}' does not exist on entity '{typeof(TEntity).Name}'. Valid fields: {string.Join(", ", typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => _sortableTypes.Contains(p.PropertyType)).Select(p => p.Name))}");
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);

        bool ascending = sortOrder == null || sortOrder == 1;

        string methodName = ascending ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), property.PropertyType);

        return (IQueryable<TEntity>)method.Invoke(null, new object[] { query, lambda })!;
    }
}
