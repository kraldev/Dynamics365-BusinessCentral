using Dynamics365.BusinessCentral.Client;
using Dynamics365.BusinessCentral.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamics365.BusinessCentral.Tests;

public class ServiceCollectionExtensionsTests
{
    private class TestObserver : IBusinessCentralObserver
    {
        public bool WasCalled { get; private set; }

        public void OnRequestStarting(BusinessCentralRequestInfo info) => WasCalled = true;
        public void OnRequestSucceeded(BusinessCentralRequestInfo info) { }
        public void OnRequestFailed(BusinessCentralErrorInfo info) { }
        public void OnTokenRequested() { }
        public void OnTokenRefreshed(BusinessCentralTokenInfo info) { }
        public void OnDeserializationFailed(BusinessCentralErrorInfo info) { }
    }

    [Fact]
    public void AddBusinessCentral_Registers_Client()
    {
        var services = new ServiceCollection();

        services.AddBusinessCentral(o =>
        {
            o.BaseUrl = "https://test";
            o.Company = "Test";
            o.TenantId = "t";
            o.ClientId = "c";
            o.ClientSecret = "s";
            o.Scope = "scope";
            o.TokenEndpoint = "https://auth/{TenantId}";
        });

        var provider = services.BuildServiceProvider();

        var client = provider.GetService<IBusinessCentralClient>();

        Assert.NotNull(client);
    }

    [Fact]
    public void AddObserver_Registers_Custom_Observer()
    {
        var services = new ServiceCollection();

        services.AddBusinessCentral(o =>
        {
            o.BaseUrl = "https://test";
            o.Company = "Test";
            o.TenantId = "t";
            o.ClientId = "c";
            o.ClientSecret = "s";
            o.Scope = "scope";
            o.TokenEndpoint = "https://auth/{TenantId}";
        })
        .AddObserver<TestObserver>();

        var provider = services.BuildServiceProvider();

        var observer = provider.GetService<IBusinessCentralObserver>();

        Assert.NotNull(observer);
        Assert.IsType<TestObserver>(observer);
    }

    [Fact]
    public void Client_Uses_NullObserver_When_None_Registered()
    {
        var services = new ServiceCollection();

        services.AddBusinessCentral(o =>
        {
            o.BaseUrl = "https://test";
            o.Company = "Test";
            o.TenantId = "t";
            o.ClientId = "c";
            o.ClientSecret = "s";
            o.Scope = "scope";
            o.TokenEndpoint = "https://auth/{TenantId}";
        });

        var provider = services.BuildServiceProvider();

        var observer = provider.GetService<IBusinessCentralObserver>();

        // No observer registered -> should be null in DI
        Assert.Null(observer);
    }

    [Fact]
    public void Client_Can_Be_Constructed_With_Custom_Observer()
    {
        var services = new ServiceCollection();

        services.AddBusinessCentral(o =>
        {
            o.BaseUrl = "https://test";
            o.Company = "Test";
            o.TenantId = "t";
            o.ClientId = "c";
            o.ClientSecret = "s";
            o.Scope = "scope";
            o.TokenEndpoint = "https://auth/{TenantId}";
        })
        .AddObserver<TestObserver>();

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IBusinessCentralClient>();

        Assert.NotNull(client);
    }
}