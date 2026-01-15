namespace Dynamics365.BusinessCentral.OData;

/// <summary>
/// Represents optional OData query modifiers such as $top, $skip and $orderby.
/// </summary>
public sealed class QueryOptions
{
    /// <summary>Limits the number of returned entities.</summary>
    public int? Top { get; internal set; }

    /// <summary>Skips the specified number of entities.</summary>
    public int? Skip { get; internal set; }

    /// <summary>Defines the ordering of the result set.</summary>
    public string? OrderBy { get; internal set; }

    /// <summary>Sets the $top query option.</summary>
    public QueryOptions WithTop(int value)
    {
        Top = value;
        return this;
    }

    /// <summary>Sets the $skip query option.</summary>
    public QueryOptions WithSkip(int value)
    {
        Skip = value;
        return this;
    }

    /// <summary>Orders the result set ascending by the given field.</summary>
    public QueryOptions OrderByAsc(string field)
    {
        OrderBy = $"{field} asc";
        return this;
    }

    /// <summary>Orders the result set descending by the given field.</summary>
    public QueryOptions OrderByDesc(string field)
    {
        OrderBy = $"{field} desc";
        return this;
    }
}
