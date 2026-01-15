using Dynamics365.BusinessCentral.Errors;
using Dynamics365.BusinessCentral.OData;
using Dynamics365.BusinessCentral.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dynamics365.BusinessCentral.Client;

public sealed class BusinessCentralClient : IBusinessCentralClient
{
    private readonly HttpClient _http;
    private readonly BusinessCentralOptions _options;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private CachedAccessToken? _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BusinessCentralClient(HttpClient http, BusinessCentralOptions options)
    {
        _http = http;
        _options = options;
    }

    public async Task<List<TEntity>> QueryAsync<TEntity>(
        string path,
        string filter,
        Action<QueryOptions>? options = null,
        IEnumerable<string>? select = null,
        CancellationToken ct = default)
    {
        var queryOptions = new QueryOptions();
        options?.Invoke(queryOptions);

        var response = await SendAsync(path, filter, queryOptions, select, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        try
        {
            var wrapper = JsonSerializer.Deserialize<ODataWrapper<TEntity>>(json, JsonOptions);
            return wrapper?.Value ?? [];
        }
        catch (JsonException ex)
        {
            throw new BusinessCentralException("Deserialization", ex.Message, response.StatusCode);
        }
    }

    public async Task<TResponse> QueryRawAsync<TResponse>(
        string path,
        CancellationToken ct = default)
        where TResponse : class
    {
        var token = await GetTokenAsync(ct);

        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.BaseUrl}/Company('{_options.Company}')/{path}");

        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            throw await CreateExceptionAsync(res, ct);

        var json = await res.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<TResponse>(json, JsonOptions)
               ?? throw new BusinessCentralException("Deserialization", "Null response", res.StatusCode);
    }

    public async Task<List<TEntity>> QueryAllAsync<TEntity>(
        string path,
        ODataFilter? filter = null,
        Action<QueryOptions>? options = null,
        IEnumerable<string>? select = null,
        CancellationToken ct = default)
    {
        var all = new List<TEntity>();
        var skip = 0;

        var baseOptions = new QueryOptions();
        options?.Invoke(baseOptions);

        var pageSize = baseOptions.Top ?? 1000;

        while (true)
        {
            var page = await QueryAsync<TEntity>(
                path,
                filter,
                o =>
                {
                    o.WithTop(pageSize)
                     .WithSkip(skip);

                    // copy ordering if present
                    if (baseOptions.OrderBy != null)
                        o.OrderBy = baseOptions.OrderBy;
                },
                select,
                ct);

            if (page.Count == 0)
                break;

            all.AddRange(page);
            skip += page.Count;
        }

        return all;
    }

    public async Task PatchAsync<TPayload>(
        string path,
        string keyFilter,
        TPayload payload,
        string ifMatch = "*",
        CancellationToken ct = default)
    {
        var token = await GetTokenAsync(ct);
        var url = $"{_options.BaseUrl}/Company('{_options.Company}')/{path}({keyFilter})";

        using var req = new HttpRequestMessage(HttpMethod.Patch, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            throw await CreateExceptionAsync(res, ct);
    }

    private async Task<HttpResponseMessage> SendAsync(
        string path,
        string filter,
        QueryOptions options,
        IEnumerable<string>? select,
        CancellationToken ct)
    {
        async Task<HttpRequestMessage> CreateRequestAsync()
        {
            var token = await GetTokenAsync(ct);

            var url = $"{_options.BaseUrl}/Company('{_options.Company}')/{path}?$filter={Uri.EscapeDataString(filter)}";

            if (select != null)
                url += "&$select=" + string.Join(",", select.Select(Uri.EscapeDataString));

            if (options.Top != null)
                url += "&$top=" + options.Top;

            if (options.Skip != null)
                url += "&$skip=" + options.Skip;

            if (options.OrderBy != null)
                url += "&$orderby=" + Uri.EscapeDataString(options.OrderBy);

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return req;
        }

        using var req = await CreateRequestAsync();
        var res = await _http.SendAsync(req, ct);

        if (res.StatusCode == HttpStatusCode.Unauthorized)
        {
            _token = null;

            using var retryReq = await CreateRequestAsync();
            res = await _http.SendAsync(retryReq, ct);
        }

        if (!res.IsSuccessStatusCode)
            throw await CreateExceptionAsync(res, ct);

        return res;
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_token != null && !_token.IsExpired)
            return _token.Token;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_token != null && !_token.IsExpired)
                return _token.Token;

            var endpoint = _options.TokenEndpoint.Replace("{TenantId}", _options.TenantId);

            var body = new StringContent(
                $"client_id={_options.ClientId}&client_secret={_options.ClientSecret}&scope={_options.Scope}&grant_type=client_credentials",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var res = await _http.PostAsync(endpoint, body, ct);

            if (!res.IsSuccessStatusCode)
                throw await CreateExceptionAsync(res, ct);

            var json = await res.Content.ReadAsStringAsync(ct);
            var token = JsonSerializer.Deserialize<TokenResponse>(json, JsonOptions)
                        ?? throw new BusinessCentralException("Auth", "Invalid token response", res.StatusCode);

            _token = new CachedAccessToken
            {
                Token = token.AccessToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
            };

            return _token.Token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public Task<List<TEntity>> QueryAsync<TEntity>(
        string path,
        ODataFilter? filter = null,
        Action<QueryOptions>? options = null,
        IEnumerable<string>? select = null,
        CancellationToken ct = default) => QueryAsync<TEntity>(path, filter?.Value ?? "true", options, select, ct);


    private static async Task<BusinessCentralException> CreateExceptionAsync(
        HttpResponseMessage res,
        CancellationToken ct)
    {
        var content = await res.Content.ReadAsStringAsync(ct);
        return new BusinessCentralException(res.StatusCode.ToString(), content, res.StatusCode);
    }

    private sealed class ODataWrapper<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new();
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class CachedAccessToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
