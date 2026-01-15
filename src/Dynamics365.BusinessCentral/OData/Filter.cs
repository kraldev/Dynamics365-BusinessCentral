using System.Globalization;

namespace Dynamics365.BusinessCentral.OData;

/// <summary>
/// Factory methods for creating OData filter expressions.
/// </summary>
public static class Filter
{
    /// <summary>Creates a filter of the form: field eq value</summary>
    public static ODataFilter Equals(string field, object value) =>
        new($"{field} eq {Format(value)}");

    /// <summary>Creates a filter of the form: field ne value</summary>
    public static ODataFilter NotEquals(string field, object value) =>
        new($"{field} ne {Format(value)}");

    /// <summary>Creates a filter of the form: field gt value</summary>
    public static ODataFilter GreaterThan(string field, object value) =>
        new($"{field} gt {Format(value)}");

    /// <summary>Creates a filter of the form: field ge value</summary>
    public static ODataFilter GreaterOrEqual(string field, object value) =>
        new($"{field} ge {Format(value)}");

    /// <summary>Creates a filter of the form: field lt value</summary>
    public static ODataFilter LessThan(string field, object value) =>
        new($"{field} lt {Format(value)}");

    /// <summary>Creates a filter of the form: field le value</summary>
    public static ODataFilter LessOrEqual(string field, object value) =>
        new($"{field} le {Format(value)}");

    /// <summary>Creates a filter using the contains(...) function.</summary>
    public static ODataFilter Contains(string field, string value) =>
        new($"contains({field}, {Format(value)})");

    /// <summary>Creates a filter using the startswith(...) function.</summary>
    public static ODataFilter StartsWith(string field, string value) =>
        new($"startswith({field}, {Format(value)})");

    /// <summary>Creates a filter using the endswith(...) function.</summary>
    public static ODataFilter EndsWith(string field, string value) =>
        new($"endswith({field}, {Format(value)})");

    /// <summary>Creates a filter of the form: field in (value1,value2,...)</summary>
    public static ODataFilter In(string field, params object[] values) =>
        new($"{field} in ({string.Join(",", values.Select(Format))})");

    /// <summary>Creates a filter of the form: field eq null</summary>
    public static ODataFilter IsNull(string field) =>
        new($"{field} eq null");

    /// <summary>Creates a filter of the form: field ne null</summary>
    public static ODataFilter IsNotNull(string field) =>
        new($"{field} ne null");

    private static string Format(object value) =>
        value switch
        {
            null => "null",
            string s => $"'{s.Replace("'", "''")}'",
            DateTime dt => dt.ToUniversalTime().ToString("O"),
            DateTimeOffset dto => dto.ToUniversalTime().ToString("O"),
            bool b => b.ToString().ToLowerInvariant(),
            Enum e => $"'{e}'",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
        };
}
