using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using Takt.Identity.API.Tests.Infrastructure;

namespace Takt.Identity.API.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class ProxyHeadersTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task HealthEndpoint_DoesNotRedirectToHttps_ByDefault()
    {
        var response = await fixture.Client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HttpsRedirection_UsesForwardedHostAndPrefix_WhenEnabled()
    {
        await using var factory = new IdentityApiFactory(
            fixture.ConnectionString,
            new Dictionary<string, string?>
            {
                ["Network:HttpsRedirectionEnabled"] = "true",
                ["https_port"] = "443"
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "http");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", "gateway.local");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/identity");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://gateway.local/identity/health", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task OpenApiAndScalar_UseForwardedPrefix()
    {
        using var openApiRequest = new HttpRequestMessage(HttpMethod.Get, "/openapi/v1.json");
        openApiRequest.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "http");
        openApiRequest.Headers.TryAddWithoutValidation("X-Forwarded-Host", "gateway.local");
        openApiRequest.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/identity");

        var openApiResponse = await fixture.Client.SendAsync(openApiRequest);
        Assert.Equal(HttpStatusCode.OK, openApiResponse.StatusCode);

        var openApiContent = await openApiResponse.Content.ReadAsStringAsync();
        var openApiNode = JsonNode.Parse(openApiContent)!.AsObject();
        var serverUrl = openApiNode["servers"]?[0]?["url"]?.GetValue<string>();
        var paths = openApiNode["paths"]?.AsObject();
        Assert.Equal("http://gateway.local/identity", serverUrl);
        Assert.NotNull(paths);
        Assert.NotEmpty(paths!);

        var accountMePath = paths!.FirstOrDefault(kv => kv.Key.EndsWith("/account/me", StringComparison.Ordinal)).Key;
        var passwordLoginPath = paths.FirstOrDefault(kv => kv.Key.EndsWith("/auth/password/login", StringComparison.Ordinal)).Key;
        Assert.False(string.IsNullOrWhiteSpace(accountMePath));
        Assert.False(string.IsNullOrWhiteSpace(passwordLoginPath));

        var accountMeSecurity = paths[accountMePath]?["get"]?["security"]?.AsArray();
        Assert.NotNull(accountMeSecurity);
        Assert.NotEmpty(accountMeSecurity!);

        var loginSecurity = paths[passwordLoginPath]?["post"]?["security"];
        Assert.True(loginSecurity is null || loginSecurity.AsArray().Count == 0);

        using var scalarRequest = new HttpRequestMessage(HttpMethod.Get, "/scalar/v1");
        scalarRequest.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "http");
        scalarRequest.Headers.TryAddWithoutValidation("X-Forwarded-Host", "gateway.local");
        scalarRequest.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/identity");

        var scalarResponse = await fixture.Client.SendAsync(scalarRequest);
        Assert.Equal(HttpStatusCode.OK, scalarResponse.StatusCode);
    }

    [Fact]
    public async Task OpenApi_UsesConfiguredExternalBaseUrl_WhenProvided()
    {
        await using var factory = new IdentityApiFactory(
            fixture.ConnectionString,
            new Dictionary<string, string?>
            {
                ["Network:ExternalBaseUrl"] = "http://localhost:3333/identity"
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var openApiNode = JsonNode.Parse(content)!.AsObject();
        var serverUrl = openApiNode["servers"]?[0]?["url"]?.GetValue<string>();
        Assert.Equal("http://localhost:3333/identity", serverUrl);
    }
}
