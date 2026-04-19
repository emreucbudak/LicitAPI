namespace Licit.AuthService.Application.Constants;

public static class AuthTokenTypes
{
    public const string Access = "access";
    public const string Refresh = "refresh";
    public const string PendingTwoFactor = "pending-2fa";
    public const string PendingRegister = "pending-register";
    public const string PendingPasswordReset = "pending-password-reset";
}
