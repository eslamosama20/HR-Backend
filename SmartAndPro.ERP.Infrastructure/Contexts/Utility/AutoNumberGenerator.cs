using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Infrastructure.Contexts.Utility;

public class AutoNumberGenerator : IAutoNumberGenerator
{
    public async Task<string> GenerateAsync<TEntity>(DbContext context, string propertyName, string prefix, int length, string specialChar = "") where TEntity : class
    {
        if (!string.IsNullOrEmpty(specialChar) && !string.IsNullOrEmpty(prefix))
        {
            prefix += specialChar;
        }

        string generatedNumber = null;
        bool isUnique = false;
        int maxRetryAttempts = 30;  // Maximum retries for uniqueness
        int retryCount = 0;

        while (!isUnique && retryCount < maxRetryAttempts)
        {
            // Generate the number
            generatedNumber = await GenerateNumberAsync<TEntity>(context, propertyName, prefix, length, retryCount);

            // Check if the generated number exists in the database
            var existingCount = await context.Set<TEntity>()
                .IgnoreQueryFilters()  // Ignore global filters if necessary
                .CountAsync(e => EF.Property<string>(e, propertyName) == generatedNumber);

            // Also check if any other tracked entity in local ChangeTracker has this generated number
            var localTrackedCount = context.ChangeTracker.Entries<TEntity>()
                .Count(e => GetPropertyValue(e, propertyName) == generatedNumber);

            if (existingCount == 0 && localTrackedCount <= 1)
            {
                // The generated number is unique, return it
                isUnique = true;
                return generatedNumber;
            }

            // Increment retry count if the number already exists
            retryCount++;
        }

        if (!isUnique)
        {
            throw new Exception("Failed to generate a unique reference number after multiple attempts.");
        }

        return generatedNumber;
    }

    private static string? GetPropertyValue<TEntity>(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> entry, string propertyName) where TEntity : class
    {
        if (entry?.Entity == null) return null;
        try
        {
            var prop = entry.Property(propertyName);
            return prop?.CurrentValue?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> GenerateNumberAsync<TEntity>(DbContext context, string propertyName, string prefix, int length, int retryCount = 0) where TEntity : class
    {
        // Create a parameter expression for LINQ queries
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        // Create a member expression for the property
        var property = Expression.Property(parameter, propertyName);

        // Create a lambda expression
        var lambda = Expression.Lambda<Func<TEntity, string>>(property, parameter);

        if (string.IsNullOrEmpty(prefix))
        {
            var dbValues = await context.Set<TEntity>()
                .IgnoreQueryFilters()  // Ignore global filters if necessary
                .Select(lambda)
                .Where(value => !string.IsNullOrEmpty(value))
                .ToListAsync();

            var localValues = context.ChangeTracker.Entries<TEntity>()
                .Select(e => GetPropertyValue(e, propertyName))
                .Where(value => !string.IsNullOrEmpty(value))
                .ToList();

            var values = dbValues.Concat(localValues!).Distinct().ToList();

            if (!values.Any())
            {
                return (1 + retryCount).ToString($"D{length}");
            }

            var maxNumericPart = values
                .Select(value => int.TryParse(value, out var number) ? number : 0)
                .Max();

            return (maxNumericPart + 1 + retryCount).ToString($"D{length}");
        }
        else
        {
            var dbValues = await context.Set<TEntity>()
                .IgnoreQueryFilters()  // Ignore global filters if necessary
                .Select(lambda)
                .Where(value => value != null && value.StartsWith(prefix))
                .ToListAsync();

            var localValues = context.ChangeTracker.Entries<TEntity>()
                .Select(e => GetPropertyValue(e, propertyName))
                .Where(value => value != null && value.StartsWith(prefix))
                .ToList();

            var values = dbValues.Concat(localValues!).Distinct().ToList();

            if (!values.Any())
            {
                return $"{prefix}{(1 + retryCount).ToString($"D{length}")}";
            }

            var pattern = $@"^{Regex.Escape(prefix)}(\d+)$";

            var maxNumericPart = 0;
            foreach (var value in values)
            {
                var match = Regex.Match(value, pattern);
                if (match.Success)
                {
                    var numericPart = match.Groups[1].Value;
                    if (int.TryParse(numericPart, out var number))
                    {
                        if (number > maxNumericPart)
                        {
                            maxNumericPart = number;
                        }
                    }
                }
            }

            var newNumericPart = (maxNumericPart + 1 + retryCount).ToString($"D{length}");
            return $"{prefix}{newNumericPart}";
        }
    }
}
