using System.Linq.Expressions;

namespace SmartAndPro.ERP.SharedKernel.Utilities;

public static class FilterHandler
{
    public static bool AllPropertiesAreNull(this object obj)
    {
        // Get all properties of the object
        var properties = obj.GetType().GetProperties();
        // Loop through each property
        return properties.All(property => property.GetValue(obj) == null);
    }
    
    public static IQueryable<T> FilterQueryableData<T>(this IQueryable<T> query, object obj)
    {
        // Get all properties of the object
        var properties = obj.GetType().GetProperties();
        // Loop through each property and for each property that is not null, add a filter to the query
        foreach (var property in properties)
        {
            // Get the value of the property
            var value = property.GetValue(obj);
        
            // Skip null values
            if (value == null)
                continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            
            // Try to get the property from the entity
            var entityProperty = typeof(T).GetProperty(property.Name);
            if (entityProperty == null)
                continue; // Skip if property doesn't exist on entity
                
            var propertyAccess = Expression.Property(parameter, property.Name);
            
            // Handle nullable type conversion - get the underlying value if nullable
            var actualValue = value;
            var entityPropertyType = entityProperty.PropertyType;
            var filterPropertyType = property.PropertyType;
            
            // If filter property is nullable but entity property is not, extract the value
            var underlyingType = Nullable.GetUnderlyingType(filterPropertyType);
            if (underlyingType != null && entityPropertyType == underlyingType)
            {
                // The filter is Guid? but entity is Guid, use the actual value with correct type
                actualValue = Convert.ChangeType(value, entityPropertyType);
            }
            
            var constant = Expression.Constant(actualValue, entityPropertyType);

            Expression? methodExp;
            // Check if property type is string, then use Contains instead of Equal
            if (entityPropertyType == typeof(string))
            {
                var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                methodExp = Expression.Call(propertyAccess, containsMethod!, constant);
            }
            else // For non-string properties, use Equality
            {
                methodExp = Expression.Equal(propertyAccess, constant);
            }

            var propertyFilter = Expression.Lambda<Func<T, bool>>(methodExp, parameter);
        
            query = query.Where(propertyFilter);
        }
        return query;
    }


}