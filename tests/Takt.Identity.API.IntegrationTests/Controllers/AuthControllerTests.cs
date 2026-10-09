using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.IntegrationTests.Controllers;

public class AuthControllerTests : IntegrationTestBase
{
	private const string Username1IncorrectPassword = "authuser1";
	private const string Username2TwoFactorRequired = "authuser2";
	private const string Username3LockOut = "authuser3";
	private const string Username4HasTwoFactor = "authuser4";
	private const string Username5Disabled = "authuser5";
	private const string Username6LoggedInWaitingForSecondFactor = "authuser6";
	private const string Password1 = "!123123TestTest!";
	private const string Password2 = "!222222OOOooo";
	
	[OneTimeSetUp]
	public async Task SetUp()
	{
		using var scope = Fixture.Factory.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var users = new List<string>()
		{
			Username1IncorrectPassword,
			Username2TwoFactorRequired,
			Username3LockOut
		};

		foreach (var u in users)
		{
			var account = await userManager.FindByNameAsync(u);
			if (account == null) continue;
			
			var result = await userManager.DeleteAsync(account);
			if (!result.Succeeded) throw new InvalidOperationException($"Unable to remove pre-existing users:{u}");
		}
		
		await CreateUser(Username1IncorrectPassword, Password1);
		await CreateUser(Username2TwoFactorRequired, Password1);
		await CreateUser(Username3LockOut, Password1);
		await CreateUser(Username4HasTwoFactor, Password1);
		await CreateUser(Username5Disabled, Password1);
		await CreateUser(Username6LoggedInWaitingForSecondFactor, Password1);

		var twoFaUser = await userManager.FindByNameAsync(Username4HasTwoFactor);
		twoFaUser.TwoFactorEnabled = true;
		await userManager.UpdateAsync(twoFaUser);
		
		var disabledUser =  await userManager.FindByNameAsync(Username5Disabled);
		disabledUser.IsDisabled = true;
		await userManager.UpdateAsync(disabledUser);
		
		var halfLoggedInUser = await userManager.FindByNameAsync(Username6LoggedInWaitingForSecondFactor);
		await userManager.SetTwoFactorEnabledAsync(halfLoggedInUser, true);
		await userManager.ResetAuthenticatorKeyAsync(halfLoggedInUser);
		var twoFaToken = await userManager.GetAuthenticatorKeyAsync(halfLoggedInUser);
		// generate twofa token

	}
	
	[TestCase(Username1IncorrectPassword, Password2, "existing user with wrong password")]
	[TestCase(Username1IncorrectPassword, "", "existing user with empty password")]
	[TestCase(Username1IncorrectPassword, Username1IncorrectPassword, "existing user with username as a password")]
	[TestCase("notExistingUser", "notExistingUser", "unknown user")]
	[Test]
	public async Task PasswordLogin_HandlesIncorrectTry(string user, string password, string description)
	{
		var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName =user,
			Password = password
		});

		Assert.That(
			response.StatusCode is HttpStatusCode.Unauthorized, 
			Is.True,
			$"Should return error when {description} tries to log in");
	}

	[Test]
	public async Task PasswordLogin_TwoFactorSetupRequired()
	{
		var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName = Username2TwoFactorRequired,
			Password = Password1
		});

		var dataString = await response.Content.ReadAsStringAsync();
		var json = JObject.Parse(dataString);
		
		Assert.That(
			response.StatusCode, 
			Is.EqualTo(HttpStatusCode.OK),
			$"Should return ok");
		Assert.That(
			json.Value<string>("status"), 
			Is.EqualTo("TwoFactorSetupRequired"),
			"Should return correct status");
		Assert.That(
			json.Value<string>("setupToken"),
			Is.Not.Empty,
			"Setup token should not be empty");
	}

	[Test]
	public async Task PasswordLogin_LocksOutUser()
	{
		const int maxFailedAttempts = 5;
		
		for (var i = 0; i < maxFailedAttempts; i++)
		{
			await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
			{
				UserName = Username3LockOut,
				Password = "wrong-password"
			});
		}
		
		// Try to log in with correct password
		var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName = Username3LockOut,
			Password = Password1
		});
		
		Assert.That(
			response.StatusCode, 
			Is.EqualTo(HttpStatusCode.Locked),
			"User should be locked out");
	}

	[Test]
	public async Task PasswordLogin_DoesntAcceptDisabledUser()
	{
		var result = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName = Username5Disabled,
			Password = Password1
		});
		
		Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
			"Should not accept disabled user, even if password is correct");
	}

	[TestCase("","", HttpStatusCode.BadRequest)]
	[TestCase("random","", HttpStatusCode.BadRequest)]
	[TestCase("","random", HttpStatusCode.BadRequest)]
	[TestCase("random", "random", HttpStatusCode.Unauthorized)]
	[Test]
	public async Task VerifyTwoFactor_HandlesInvalidData(string challengeToken, string code, HttpStatusCode expectedStatus)
	{
		var result = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/2fa/verify", new
		{
			ChallengeToken = challengeToken,
			Code = code
		});
		
		Assert.That(result.StatusCode, Is.EqualTo(expectedStatus), $"Should return handle invalid data");
	}

	public async Task VerifyTwoFactor_HandlesDisabledUser()
	{
		
	}

	public async Task VerifyTwoFactor_LocksOutUser()
	{
		
	}

	public async Task VerifyTwoFactor_AcceptsValidToken()
	{
		
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