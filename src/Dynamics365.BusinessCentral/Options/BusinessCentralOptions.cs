namespace Dynamics365.BusinessCentral.Options;

public sealed class BusinessCentralOptions
{
    public string TenantId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string Company { get; set; } = default!;
    public string Scope { get; set; } = "https://api.businesscentral.dynamics.com/.default";
    public string TokenEndpoint { get; set; } =
        "https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";
}
