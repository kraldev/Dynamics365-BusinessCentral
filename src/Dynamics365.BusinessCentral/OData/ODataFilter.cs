namespace Dynamics365.BusinessCentral.OData;

/// <summary>
/// Represents an immutable OData $filter expression.
/// Instances are created via the <see cref="Filter"/> factory and combined via extension methods.
/// </summary>
public sealed class ODataFilter
{
    /// <summary>
    /// The raw OData filter expression string.
    /// </summary>
    public string Value { get; }

    internal ODataFilter(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns the underlying OData filter string.
    /// </summary>
    public override string ToString() => Value;
}