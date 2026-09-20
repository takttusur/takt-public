namespace Takt.Identity.API.Security;

public static class AuthFlowPurposes
{
    public const string TwoFactorSetup = "TwoFactorSetup";
    public const string TwoFactorVerify = "TwoFactorVerify";
    public const string PasskeyRegistration = "PasskeyRegistration";
    public const string PasskeyAuthentication = "PasskeyAuthentication";
}
