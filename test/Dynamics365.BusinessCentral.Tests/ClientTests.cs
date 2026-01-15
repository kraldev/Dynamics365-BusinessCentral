using System.Net;

namespace Dynamics365.BusinessCentral.Tests;

public class ClientTests
{
    [Fact]
    public async Task Token_Is_Cached()
    {
        // Arrange
        var tokenCalls = 0;

        var client = TestBase.CreateClient(req =>
        {
            if (req.RequestUri!.AbsoluteUri.Contains("auth"))
            {
                tokenCalls++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":3600}")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"value\":[]}")
            };
        });

        // Act
        await client.QueryAsync<object>("orders", "true");
        await client.QueryAsync<object>("orders", "true");

        // Assert
        Assert.Equal(1, tokenCalls);
    }

    [Fact]
    public async Task Query_Retries_On_401()
    {
        // Arrange
        var calls = 0;

        var client = TestBase.CreateClient(req =>
        {
            if (req.RequestUri!.AbsoluteUri.Contains("auth"))
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":3600}")
                };

            calls++;
            return calls == 1
                ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"value\":[]}")
                };
        });

        // Act
        var result = await client.QueryAsync<object>("orders", "true");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task QueryAll_Pages_Correctly()
    {
        // Arrange
        var skip = 0;

        var client = TestBase.CreateClient(req =>
        {
            if (req.RequestUri!.AbsoluteUri.Contains("auth"))
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":3600}")
                };

            var page = skip switch
            {
                0 => "[{\"id\":1},{\"id\":2}]",
                2 => "[{\"id\":3}]",
                _ => "[]"
            };

            skip += 2;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"value\":{page}}}")
            };
        });

        // Act
        var all = await client.QueryAllAsync<TestEntity>("orders");

        // Assert
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task Filter_Is_Escaped()
    {
        // Arrange
        string? url = null;

        var client = TestBase.CreateClient(req =>
        {
            url = req.RequestUri!.AbsoluteUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"value\":[]}")
            };
        });

        // Act
        await client.QueryAsync<object>("orders", "Name eq 'A & B'");

        // Assert
        Assert.NotNull(url);
        Assert.Contains("%26", url);
    }

    [Fact]
    public async Task Cancellation_Propagates()
    {
        // Arrange
        var client = TestBase.CreateClient(_ => throw new TaskCanceledException());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.QueryAsync<object>("orders", "true", ct: cts.Token));
    }

    private sealed class TestEntity
    {
        public int Id { get; set; }
    }
}
