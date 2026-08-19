namespace QabilHire.Api.RateLimiting;

public static class RateLimitPolicyNames
{
    public const string Login = "auth-login";
    public const string Registration = "auth-registration";
    public const string Session = "auth-session";
    public const string PasswordRecovery = "auth-password-recovery";
}
