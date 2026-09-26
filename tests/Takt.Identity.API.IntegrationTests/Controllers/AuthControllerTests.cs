using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.IntegrationTests.Controllers;

public class AuthControllerTests : IntegrationTestBase
{
	private const string Username1 = "authuser1";
	private const string Username2 = "authuser2";
	private const string Password1 = "!123123TestTest!";
	private const string Password2 = "!222222OOOooo";
	
	[OneTimeSetUp]
	public async Task SetUp()
	{
		using var scope = Fixture.Factory.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var users = new List<string>()
		{
			Username1,
			Username2
		};

		foreach (var u in users)
		{
			var account = await userManager.FindByNameAsync(u);
			if (account == null) continue;
			
			var result = await userManager.DeleteAsync(account);
			if (!result.Succeeded) throw new InvalidOperationException($"Unable to remove pre-existing users:{u}");
		}
	}
	
	[Test]
	public async Task PasswordLogin_ValidatesPasswordCorrectly()
	{
		await CreateUser(Username1, Password1);
		
		var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName =Username1,
			Password = Password2
		});

		Assert.That(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden, Is.True);
	}

	private async Task CreateUser(string username, string password)
	{
		using var scope = Fixture.Factory.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var result = await userManager.CreateAsync(new ApplicationUser()
		{
			UserName = username
		}, password);

		if (!result.Succeeded) throw new InvalidOperationException($"Cannot create user {username}");
	}
}