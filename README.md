# Dynamics365.BusinessCentral
*A lightweight, strongly-typed client for the Dynamics 365 Business Central OData API.*

# ✨ Features

- Strongly-typed OData querying with fluent filter composition
- Full CRUD support (GET, POST, PUT, PATCH, DELETE)
- Built-in OAuth2 client credentials authentication
- Automatic token caching and refresh
- Fluent paging, ordering and field selection helpers
- Optional observability hooks for logging and monitoring
- Clean dependency injection integration
- No runtime dependencies beyond HttpClient and System.Text.Json

# 📦 Installation

```bash
dotnet add package Dynamics365.BusinessCentral
```

# 🧩 Dependency Injection

```csharp
services.AddBusinessCentral(options =>
{
    options.TenantId = "your-tenant-id";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.Company = "CRONUS AG";
    options.BaseUrl = "https://api.businesscentral.dynamics.com/v2.0/{tenant}/Production/ODataV4";
    options.TokenEndpoint = "https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";
    options.Scope = "https://api.businesscentral.dynamics.com/.default";
});

public class MyService
{
    private readonly IBusinessCentralClient _client;

    public MyService(IBusinessCentralClient client)
    {
        _client = client;
    }
}
```

# 🔍 Querying

Simple Query
```csharp
var orders = await client.QueryAsync<Order>("salesOrders");
```

With Filters
```csharp
var filter =
    Filter.Equals("Status", "Open")
          .And(Filter.GreaterThan("Amount", 100));

var orders = await client.QueryAsync<Order>("salesOrders", filter);
```

Paging and Ordering
```csharp
var orders = await client.QueryAsync<Order>(
    "salesOrders",
    Filter.Equals("Status", "Open"),
    o => o.WithTop(100).OrderByAsc("No"));
```

Query All
```csharp
var allOrders = await client.QueryAllAsync<Order>("salesOrders");
```

Raw Query
```csharp
var raw = await client.QueryRawAsync<JsonElement>("salesOrders?$top=5");
```

Patch
```csharp
await client.PatchAsync(
    "salesOrders",
    "No='1000'",
    new { Status = "Released" });
```

Post
```csharp
var created = await client.PostAsync(
    "salesOrders",
    new { CustomerNo = "10000", Description = "Test Order" });
```

Put
```csharp
var updated = await client.PutAsync(
    "salesOrders",
    systemId: "a3f1c2d1-8b0a-4e3d-9f6c-123456789abc",
    payload: orderDto);
```

Delete
```csharp
await client.DeleteAsync(
    "salesOrders",
    systemId: "a3f1c2d1-8b0a-4e3d-9f6c-123456789abc");
```

# 🧪 Filters

| Method                  | Expression                |
| ----------------------- | ------------------------- |
| `Filter.Equals`         | `field eq value`          |
| `Filter.NotEquals`      | `field ne value`          |
| `Filter.GreaterThan`    | `field gt value`          |
| `Filter.GreaterOrEqual` | `field ge value`          |
| `Filter.LessThan`       | `field lt value`          |
| `Filter.LessOrEqual`    | `field le value`          |
| `Filter.Contains`       | `contains(field,value)`   |
| `Filter.StartsWith`     | `startswith(field,value)` |
| `Filter.EndsWith`       | `endswith(field,value)`   |
| `Filter.In`             | `field in (...)`          |
| `Filter.IsNull`         | `field eq null`           |
| `Filter.IsNotNull`      | `field ne null`           |

# 📡 Observability (optional)
The client supports optional diagnostics hooks so you can integrate your own logging framework without taking extra dependencies.

```csharp
services.AddBusinessCentral(options => { ... })
        .AddObserver<MyLoggingObserver>();
```
Example Observer:
```csharp
public class MyLoggingObserver : IBusinessCentralObserver
{
    public void OnRequestStarting(BusinessCentralRequestInfo info) =>
        Console.WriteLine($"Starting {info.Method} {info.Url}");

    public void OnRequestSucceeded(BusinessCentralRequestInfo info) =>
        Console.WriteLine($"Success {info.StatusCode} in {info.Duration}");

    public void OnRequestFailed(BusinessCentralErrorInfo error) =>
        Console.WriteLine($"Failed: {error.Exception?.Message}");

    public void OnTokenRequested() =>
        Console.WriteLine("Requesting new OAuth token");

    public void OnTokenRefreshed(BusinessCentralTokenInfo info) =>
        Console.WriteLine($"Token refreshed. Cached: {info.FromCache}");

    public void OnDeserializationFailed(BusinessCentralErrorInfo error) =>
        Console.WriteLine($"Deserialization error: {error.Exception?.Message}");
}
```

# Example
```csharp
public static class SmokeTestEndpoints
{
    public static void MapSmokeTests(this WebApplication app)
    {
        app.MapGet("/bc/orders", async (IBusinessCentralClient client) =>
        {
            var orders = await client.QueryAsync<dynamic>(
                "LDATProductionOrd1er",
                Filter.Equals("Status", "Released"),
                o => o.WithTop(5));

            return Results.Ok(orders);
        });

        app.MapGet("/bc/orders/all", async (IBusinessCentralClient client) =>
        {
            var orders = await client.QueryAllAsync<dynamic>("salesOrders");
            return Results.Ok(orders.Count);
        });
    }
}
```
