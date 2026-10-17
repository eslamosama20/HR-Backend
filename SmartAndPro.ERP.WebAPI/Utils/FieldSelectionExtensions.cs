using System.Reflection;
using System.Text.Json;

namespace SmartAndPro.ERP.WebAPI.Utils;

public static class FieldSelectionExtensions
{
    public static object ApplyFieldSelection<T>(this IEnumerable<T> items, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return items;
        }

        var requestedFields = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requestedFields.Count == 0)
        {
            return items;
        }

        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        var selectedProperties = properties
            .Where(p =>
            {
                var camelName = JsonNamingPolicy.CamelCase.ConvertName(p.Name);
                return requestedFields.Contains(p.Name) || requestedFields.Contains(camelName);
            })
            .ToList();

        if (selectedProperties.Count == 0)
        {
            return items;
        }

        var projected = items.Select(item =>
        {
            var selectedValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in selectedProperties)
            {
                var camelName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                selectedValues[camelName] = property.GetValue(item);
            }

            return selectedValues;
        }).ToList();

        return projected;
    }
}
