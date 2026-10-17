using System.Reflection;

namespace SmartAndPro.ERP.Application.Common.Utility;

public static class GuidHelper
{
    /// <summary>
    /// Recursively sets all properties of type Guid or Guid? with a value of Guid.Empty to null.
    /// </summary>
    /// <param name="entity">The object to process.</param>
    public static void NormalizeGuids<T>(T entity) where T : class
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(Guid?) && p.CanRead && p.CanWrite);

        foreach (var property in properties)
        {
            var value = (Guid?)property.GetValue(entity);
            if (value == Guid.Empty)
            {
                property.SetValue(entity, null);
            }
        }
    }
}