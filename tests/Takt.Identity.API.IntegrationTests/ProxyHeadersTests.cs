using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using Takt.Identity.API.IntegrationTests.Infrastructure;

namespace Takt.Identity.API.IntegrationTests;

public sealed class ProxyHeadersTests : IntegrationTestBase
{
    [Test]
    public async Task HealthEndpoint_DoesNotRedirectToHttps_ByDefault()
    {
        var response = await Fixture.Client.GetAsync("/health");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task HttpsRedirection_UsesForwardedHostAndPrefix_WhenEnabled()
    {
        await using var factory = new IdentityApiFactory(
            Fixture.ConnectionString,
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

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.TemporaryRedirect));
        Assert.That(response.Headers.Location?.ToString(), Is.EqualTo("https://gateway.local/identity/health"));
    }

    [Test]
    public async Task OpenApiAndScalar_UseForwardedPrefix()
    {
        using var openApiRequest = new HttpRequestMessage(HttpMethod.Get, "/openapi/v1.json");
        openApiRequest.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "http");
        openApiRequest.Headers.TryAddWithoutValidation("X-Forwarded-Host", "gateway.local");
        openApiRequest.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/identity");

        var openApiResponse = await Fixture.Client.SendAsync(openApiRequest);
        Assert.That(openApiResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var openApiContent = await openApiResponse.Content.ReadAsStringAsync();
        var openApiNode = JsonNode.Parse(openApiContent)!.AsObject();
        var serverUrl = openApiNode["servers"]?[0]?["url"]?.GetValue<string>();
        var paths = openApiNode["paths"]?.AsObject();
        Assert.That(serverUrl, Is.EqualTo("http://gateway.local/identity"));
        Assert.That(paths, Is.Not.Null);
        var nonNullPaths = paths!;
        Assert.That(nonNullPaths, Is.Not.Empty);

        var accountMePath = nonNullPaths.FirstOrDefault(kv => kv.Key.EndsWith("/account/me", StringComparison.Ordinal)).Key;
        var passwordLoginPath = nonNullPaths.FirstOrDefault(kv => kv.Key.EndsWith("/auth/password/login", StringComparison.Ordinal)).Key;
        Assert.That(string.IsNullOrWhiteSpace(accountMePath), Is.False);
        Assert.That(string.IsNullOrWhiteSpace(passwordLoginPath), Is.False);

        var accountMeSecurity = nonNullPaths[accountMePath]?["get"]?["security"]?.AsArray();
        Assert.That(accountMeSecurity, Is.Not.Null);
        Assert.That(accountMeSecurity!, Is.Not.Empty);

        var loginSecurity = nonNullPaths[passwordLoginPath]?["post"]?["security"];
        Assert.That(loginSecurity is null || loginSecurity.AsArray().Count == 0, Is.True);

        using var scalarRequest = new HttpRequestMessage(HttpMethod.Get, "/scalar/v1");
        scalarRequest.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "http");
        scalarRequest.Headers.TryAddWithoutValidation("X-Forwarded-Host", "gateway.local");
        scalarRequest.Headers.TryAddWithoutValidation("X-Forwarded-Prefix", "/identity");

        var scalarResponse = await Fixture.Client.SendAsync(scalarRequest);
        Assert.That(scalarResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task OpenApi_UsesConfiguredExternalBaseUrl_WhenProvided()
    {
        await using var factory = new IdentityApiFactory(
            Fixture.ConnectionString,
            new Dictionary<string, string?>
            {
                ["Network:BaseUrl"] = "http://localhost:3333/identity"
            });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/openapi/v1.json");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var openApiNode = JsonNode.Parse(content)!.AsObject();
        var serverUrl = openApiNode["servers"]?[0]?["url"]?.GetValue<string>();
        Assert.That(serverUrl, Is.EqualTo("http://localhost:3333/identity"));
    }
}
