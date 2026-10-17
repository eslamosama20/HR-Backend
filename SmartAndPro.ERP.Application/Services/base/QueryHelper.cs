using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using SmartAndPro.ERP.Application.Common.Dtos;

namespace SmartAndPro.ERP.Application.Services.@base;

public static class QueryHelper
{
    public static IQueryable<T> ApplyGlobalSearch<T>(this IQueryable<T> source, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        // Limit recursion to 2 levels
        Expression searchExpression = BuildSearchExpression(typeof(T), parameter, searchTerm, 2);

        if (searchExpression == null)
            return source;

        var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
        return source.Where(lambda);
    }

    private static Expression BuildSearchExpression(Type type, Expression parameter, string searchTerm, int depth)
    {
        if (depth <= 0) return null;

        Expression searchExpression = null;

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.PropertyType == typeof(string))
            {
                // Build expression: x.PropertyName
                var propertyAccess = Expression.Property(parameter, property);

                // Add null-check to avoid null reference exceptions in SQL
                var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));

                // Build Contains expression: x.PropertyName.Contains(searchTerm)
                var searchTermExpression = Expression.Constant(searchTerm);
                var containsExpression = Expression.Call(
                    propertyAccess,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    searchTermExpression
                );

                // Combine null-check and Contains: x.PropertyName != null && x.PropertyName.Contains(searchTerm)
                var propertySearchExpression = Expression.AndAlso(nullCheck, containsExpression);

                // Combine with the existing search expression: OR condition
                searchExpression = searchExpression == null
                    ? propertySearchExpression
                    : Expression.OrElse(searchExpression, propertySearchExpression);
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                // For navigation properties, build a recursive search expression
                var navigationParameter = Expression.Property(parameter, property);
                var nestedSearchExpression = BuildSearchExpression(property.PropertyType, navigationParameter, searchTerm, depth - 1);

                if (nestedSearchExpression != null)
                {
                    searchExpression = searchExpression == null
                        ? nestedSearchExpression
                        : Expression.OrElse(searchExpression, nestedSearchExpression);
                }
            }
        }

        return searchExpression;
    }


    public static IQueryable<T> WhereDynamic<T>(this IQueryable<T> source, string propertyName, List<QueryFilter> conditions)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var capitalizedPropertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);

        // Check if the property exists on the entity
        var propertyChecker = typeof(T).GetProperty(capitalizedPropertyName);
        if (propertyChecker == null)
        {
            // Skip if the property doesn't exist
            return source;
        }
        var property = Expression.PropertyOrField(parameter, capitalizedPropertyName);
        Expression? combinedExpression = null;

        foreach (var condition in conditions)
        {
            var matchMode = condition.MatchMode.ToLower();
            var value = condition.Value;
            // Skip condition if value is null
            if (value == null)
            {
                continue; // Skip this iteration and move to the next condition
            }
            Expression? currentExpression = null;

            if (property.Type.IsEnum)
            {
                if (!Enum.TryParse(property.Type, value, true, out var enumValue))
                {
                    // Ignore invalid enum values
                    return source;
                }
            
                var constantEnum = Expression.Constant(enumValue);
                switch (matchMode.ToLower())
                {
                    case "equals":
                    case "equal":
                        currentExpression = Expression.Equal(property, constantEnum);
                        break;
                    case "notequals":
                    case "notequal":
                        currentExpression = Expression.NotEqual(property, constantEnum);
                        break;
                    case "contains":
                        currentExpression = Expression.Equal(property, constantEnum);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported match mode: {matchMode}");
                }

            }
            else if (property.Type == typeof(bool) || property.Type == typeof(bool?))
            {
                // Boolean handling
                if (!bool.TryParse(value, out var boolValue))
                {
                    throw new ArgumentException($"Value '{value}' is not a valid boolean for property '{propertyName}'.");
                }

                var constantBool = Expression.Constant(boolValue, typeof(bool));
                currentExpression = matchMode switch
                {
                    "equals" or "equal" => Expression.Equal(property, constantBool),
                    "notequals" or "notequal" => Expression.NotEqual(property, constantBool),
                    _ => throw new ArgumentException($"Unsupported match mode for boolean: {matchMode}")
                };
            }
            else
            {
                // Handle strings, numbers, and dates
                switch (matchMode)
                {
                    case "contains":
                        currentExpression = Expression.Call(property, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, Expression.Constant(value));
                        break;
                    case "startswith":
                        currentExpression = Expression.Call(property, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, Expression.Constant(value));
                        break;
                    case "endswith":
                        currentExpression = Expression.Call(property, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, Expression.Constant(value));
                        break;
                    case "equals":
                    case "equal":
                        currentExpression = Expression.Equal(property, Expression.Constant(value, property.Type));
                        break;
                    case "notequals":
                    case "notequal":
                        currentExpression = Expression.NotEqual(property, Expression.Constant(value, property.Type));
                        break;
                    case "lt":
                    case "lte":
                    case "gt":
                    case "gte":
                    case "dateis":
                    case "dateisnot":
                    case "datebefore":
                    case "dateafter":
                        if (DateTime.TryParse(value, out var dateValue))
                        {
                            Expression constantExpression;

                            if (property.Type == typeof(DateOnly))
                            {
                                // Convert DateTime to DateOnly
                                var constantDateOnly = DateOnly.FromDateTime(dateValue);
                                constantExpression = Expression.Constant(constantDateOnly, typeof(DateOnly));
                            }
                            else if (property.Type == typeof(DateTime))
                            {
                                // Use DateTime directly
                                constantExpression = Expression.Constant(dateValue, typeof(DateTime));
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unsupported property type: {property.Type}");
                            }
                            
                            currentExpression = matchMode switch
                            {
                                "lt" => Expression.LessThan(property, constantExpression),
                                "lte" => Expression.LessThanOrEqual(property, constantExpression),
                                "gt" => Expression.GreaterThan(property, constantExpression),
                                "gte" => Expression.GreaterThanOrEqual(property, constantExpression),
                                "dateis" => Expression.Equal(property, constantExpression),
                                "dateisnot" => Expression.NotEqual(property, constantExpression),
                                "datebefore" => Expression.LessThan(property, constantExpression),
                                "dateafter" => Expression.GreaterThan(property, constantExpression),
                                _ => throw new ArgumentException($"Unsupported match mode: {matchMode}")
                            };
                        }
                        else if (decimal.TryParse(value, out var numericValue))
                        {
                            var constantNumeric = Expression.Constant(numericValue, typeof(decimal));
                            currentExpression = matchMode switch
                            {
                                "lt" => Expression.LessThan(property, constantNumeric),
                                "lte" => Expression.LessThanOrEqual(property, constantNumeric),
                                "gt" => Expression.GreaterThan(property, constantNumeric),
                                "gte" => Expression.GreaterThanOrEqual(property, constantNumeric),
                                _ => throw new ArgumentException($"Unsupported match mode: {matchMode}")
                            };
                        }
                        else
                        {
                            throw new ArgumentException($"Value '{value}' is not a valid number or date.");
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unsupported match mode: {matchMode}");
                }
            }

            if (currentExpression == null)
            {
                throw new ArgumentException($"Failed to build expression for property '{propertyName}' and match mode '{matchMode}'.");
            }

            // Combine expressions with OR
            combinedExpression = combinedExpression == null
                ? currentExpression
                : Expression.AndAlso(combinedExpression, currentExpression);
        }

        if (combinedExpression == null)
        {
            return source;
        }

        var predicate = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return source.Where(predicate);
    }

    
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool ascending)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = ascending ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { source, lambda })!;
    }

}
