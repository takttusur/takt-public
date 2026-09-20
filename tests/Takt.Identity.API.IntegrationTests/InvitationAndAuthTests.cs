using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Takt.Identity.API.IntegrationTests.Infrastructure;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.IntegrationTests;

public sealed class InvitationAndAuthTests : IntegrationTestBase
{
    [Test]
    public async Task CreateInvitation_RequiresAdminRole()
    {
        var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/invitations", new
        {
            userName = "user1",
            role = "User"
        });

        Assert.That(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden, Is.True);
    }

    [Test]
    public async Task Invitation_CanBeConsumedOnlyOnce()
    {
        var token = "invite-token-123";

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
            db.Invitations.Add(new Invitation
            {
                UserName = "invited-user",
                NormalizedUserName = "INVITED-USER",
                TokenHash = HashToken(token),
                Role = "User",
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            });
            await db.SaveChangesAsync();
        }

        var first = await Fixture.Client.PostAsJsonAsync($"/api/v1.0/invitations/{token}/accept", new
        {
            password = "StrongPassword!1234"
        });
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var second = await Fixture.Client.PostAsJsonAsync($"/api/v1.0/invitations/{token}/accept", new
        {
            password = "StrongPassword!1234"
        });

        Assert.That(second.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict, Is.True);
    }

    [Test]
    public async Task PasswordLogin_RequiresTwoFactorSetup_WhenNotEnabled()
    {
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = "twofactor-user",
                LockoutEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var result = await userManager.CreateAsync(user, "StrongPassword!1234");
            Assert.That(result.Succeeded, Is.True);
        }

        var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password/login", new
        {
            userName = "twofactor-user",
            password = "StrongPassword!1234"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.That(payload["status"]?.GetValue<string>(), Is.EqualTo("TwoFactorSetupRequired"));
        Assert.That(payload.ContainsKey("accessToken"), Is.False);
        Assert.That(string.IsNullOrWhiteSpace(payload["setupToken"]?.GetValue<string>()), Is.False);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
