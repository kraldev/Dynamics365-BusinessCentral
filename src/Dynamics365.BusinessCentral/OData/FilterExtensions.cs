namespace Dynamics365.BusinessCentral.OData;

/// <summary>
/// Extension methods for composing OData filters using logical operators.
/// </summary>
public static class FilterExtensions
{
    /// <summary>Combines two filters using logical AND.</summary>
    public static ODataFilter And(this ODataFilter left, ODataFilter right) =>
        new($"({left}) and ({right})");

    /// <summary>Combines two filters using logical OR.</summary>
    public static ODataFilter Or(this ODataFilter left, ODataFilter right) =>
        new($"({left}) or ({right})");

    /// <summary>Negates a filter using logical NOT.</summary>
    public static ODataFilter Not(this ODataFilter filter) =>
        new($"not ({filter})");
}
