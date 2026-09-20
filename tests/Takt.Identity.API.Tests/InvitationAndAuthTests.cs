using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Services;
using Takt.Identity.API.Tests.Infrastructure;

namespace Takt.Identity.API.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class InvitationAndAuthTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task CreateInvitation_RequiresAdminRole()
    {
        var response = await fixture.Client.PostAsJsonAsync("/api/v1.0/invitations", new
        {
            userName = "user1",
            role = "User"
        });

        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invitation_CanBeConsumedOnlyOnce()
    {
        var token = "invite-token-123";

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<ITokenHasher>();
            db.Invitations.Add(new Invitation
            {
                UserName = "invited-user",
                NormalizedUserName = "INVITED-USER",
                TokenHash = hasher.HashToken(token),
                Role = "User",
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            });
            await db.SaveChangesAsync();
        }

        var first = await fixture.Client.PostAsJsonAsync($"/api/v1.0/invitations/{token}/accept", new
        {
            password = "StrongPassword!1234"
        });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await fixture.Client.PostAsJsonAsync($"/api/v1.0/invitations/{token}/accept", new
        {
            password = "StrongPassword!1234"
        });

        Assert.True(second.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PasswordLogin_RequiresTwoFactorSetup_WhenNotEnabled()
    {
        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = "twofactor-user",
                LockoutEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var result = await userManager.CreateAsync(user, "StrongPassword!1234");
            Assert.True(result.Succeeded);
        }

        var response = await fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password/login", new
        {
            userName = "twofactor-user",
            password = "StrongPassword!1234"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("TwoFactorSetupRequired", payload["status"]?.GetValue<string>());
        Assert.False(payload.ContainsKey("accessToken"));
        Assert.False(string.IsNullOrWhiteSpace(payload["setupToken"]?.GetValue<string>()));
    }
}
