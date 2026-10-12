using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OtpNet;
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
	private const string Username7LockOutAfterPasswordLogin = "authuser7";
	private const string Username8ValidTwoFactor = "authuser8";
	private const string Username9LockOutAfterInvalidTwoFactor = "authuser9";
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
			Username3LockOut,
			Username4HasTwoFactor,
			Username5Disabled,
			Username6LoggedInWaitingForSecondFactor,
			Username7LockOutAfterPasswordLogin,
			Username8ValidTwoFactor,
			Username9LockOutAfterInvalidTwoFactor
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
		await CreateUser(Username7LockOutAfterPasswordLogin, Password1);
		await CreateUser(Username8ValidTwoFactor, Password1);
		await CreateUser(Username9LockOutAfterInvalidTwoFactor, Password1);

		var twoFaUser = await userManager.FindByNameAsync(Username4HasTwoFactor);
		await userManager.SetTwoFactorEnabledAsync(twoFaUser!, true);
		await userManager.ResetAuthenticatorKeyAsync(twoFaUser!);
		
		var disabledUser =  await userManager.FindByNameAsync(Username5Disabled);
		disabledUser!.IsDisabled = true;
		await userManager.UpdateAsync(disabledUser);
		
		var halfLoggedInUser = await userManager.FindByNameAsync(Username6LoggedInWaitingForSecondFactor);
		await userManager.SetTwoFactorEnabledAsync(halfLoggedInUser!, true);
		await userManager.ResetAuthenticatorKeyAsync(halfLoggedInUser!);

		var lockOutAfterChallengeUser = await userManager.FindByNameAsync(Username7LockOutAfterPasswordLogin);
		await userManager.SetTwoFactorEnabledAsync(lockOutAfterChallengeUser!, true);
		await userManager.ResetAuthenticatorKeyAsync(lockOutAfterChallengeUser!);

		var validTwoFactorUser = await userManager.FindByNameAsync(Username8ValidTwoFactor);
		await userManager.SetTwoFactorEnabledAsync(validTwoFactorUser!, true);
		await userManager.ResetAuthenticatorKeyAsync(validTwoFactorUser!);

		var lockOutByWrongTwoFactorUser = await userManager.FindByNameAsync(Username9LockOutAfterInvalidTwoFactor);
		await userManager.SetTwoFactorEnabledAsync(lockOutByWrongTwoFactorUser!, true);
		await userManager.ResetAuthenticatorKeyAsync(lockOutByWrongTwoFactorUser!);
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

	[Test]
	public async Task VerifyTwoFactor_HandlesDisabledUser()
	{
		var challengeToken = await RequestSecondFactorChallengeToken(Username6LoggedInWaitingForSecondFactor, Password1);

		using (var scope = Fixture.Factory.Services.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var user = await userManager.FindByNameAsync(Username6LoggedInWaitingForSecondFactor);
			user!.IsDisabled = true;
			await userManager.UpdateAsync(user);
		}

		var result = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/2fa/verify", new
		{
			ChallengeToken = challengeToken,
			Code = "123456"
		});

		Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
			"Should deny second-factor verification for user disabled after password login");
	}

	[Test]
	public async Task VerifyTwoFactor_LocksOutUser()
	{
		var challengeToken = await RequestSecondFactorChallengeToken(Username7LockOutAfterPasswordLogin, Password1);
		string validCode;

		using (var scope = Fixture.Factory.Services.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var user = await userManager.FindByNameAsync(Username7LockOutAfterPasswordLogin);
			await userManager.SetLockoutEndDateAsync(user!, DateTimeOffset.UtcNow.AddMinutes(5));
			var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user!);
			Assert.That(authenticatorKey, Is.Not.Empty, "Authenticator key should exist for lockout scenario user");
			validCode = GenerateAuthenticatorCode(authenticatorKey!);
		}

		var result = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/2fa/verify", new
		{
			ChallengeToken = challengeToken,
			Code = validCode
		});

		Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
			"Should deny second-factor verification for user locked out after password login");
	}

	[Test]
	public async Task VerifyTwoFactor_AcceptsValidToken()
	{
		var challengeToken = await RequestSecondFactorChallengeToken(Username8ValidTwoFactor, Password1);
		string validCode;

		using (var scope = Fixture.Factory.Services.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var user = await userManager.FindByNameAsync(Username8ValidTwoFactor);
			var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user!);
			Assert.That(authenticatorKey, Is.Not.Empty, "Authenticator key should exist for valid 2FA scenario user");
			validCode = GenerateAuthenticatorCode(authenticatorKey!);
		}

		var result = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/2fa/verify", new
		{
			ChallengeToken = challengeToken,
			Code = validCode
		});
		var json = JObject.Parse(await result.Content.ReadAsStringAsync());
		Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK),
			"Should accept second-factor verification with real challenge token and real code");
		Assert.That(json.Value<string>("accessToken"), Is.Not.Empty, "Access token should be issued");
		Assert.That(json.Value<string>("refreshToken"), Is.Not.Empty, "Refresh token should be issued");
	}

	[Test]
	public async Task VerifyTwoFactor_UserLockedOutAfterSeveralAttempts()
	{
		const int maxFailedAttempts = 5;
		var challengeToken = await RequestSecondFactorChallengeToken(Username9LockOutAfterInvalidTwoFactor, Password1);

		for (var i = 0; i < maxFailedAttempts; i++)
		{
			var verifyResponse = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/2fa/verify", new
			{
				ChallengeToken = challengeToken,
				Code = "not-a-valid-authenticator-code"
			});

			Assert.That(verifyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
				"Invalid second-factor code should be rejected");
		}

		using (var scope = Fixture.Factory.Services.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var user = await userManager.FindByNameAsync(Username9LockOutAfterInvalidTwoFactor);
			var isLockedOut = await userManager.IsLockedOutAsync(user!);
			Assert.That(isLockedOut, Is.True, "User should be locked out after too many invalid second-factor attempts");
		}

		var passwordLoginResponse = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName = Username9LockOutAfterInvalidTwoFactor,
			Password = Password1
		});

		Assert.That(passwordLoginResponse.StatusCode, Is.EqualTo(HttpStatusCode.Locked),
			"Password login should return locked after lockout caused by invalid second-factor attempts");
	}

	private async Task<string> RequestSecondFactorChallengeToken(string username, string password)
	{
		var response = await Fixture.Client.PostAsJsonAsync("/api/v1.0/auth/password-login", new
		{
			UserName = username,
			Password = password
		});

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
			$"Password login should succeed for user '{username}'");

		var json = JObject.Parse(await response.Content.ReadAsStringAsync());
		Assert.That(json.Value<string>("status"), Is.EqualTo("TwoFactorVerificationRequired"),
			"Password login should require second-factor verification");
		Assert.That(json.Value<string>("challengeToken"), Is.Not.Empty,
			"Challenge token should be issued");

		return json.Value<string>("challengeToken")!;
	}

	private static string GenerateAuthenticatorCode(string base32Secret)
	{
		var keyBytes = Base32Encoding.ToBytes(base32Secret.Replace(" ", string.Empty, StringComparison.Ordinal));
		var totp = new Totp(keyBytes);
		return totp.ComputeTotp(DateTime.UtcNow);
	}
}