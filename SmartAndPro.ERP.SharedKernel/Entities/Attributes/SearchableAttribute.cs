namespace SmartAndPro.ERP.SharedKernel.Entities.Attributes;

/// <summary>
/// Marks a property as searchable by the server-side search feature.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class SearchableAttribute : Attribute
{
    /// <summary>
    /// Weight for future weighted ranking. Higher values rank higher.
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Order for future ordering of search results.
    /// </summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// Explicitly excludes a property from server-side search.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NotSearchableAttribute : Attribute
{
}
