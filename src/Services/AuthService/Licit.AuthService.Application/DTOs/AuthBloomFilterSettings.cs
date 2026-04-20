namespace Licit.AuthService.Application.DTOs;

public class AuthBloomFilterSettings
{
    public string RegisteredEmailsKey { get; set; } = "auth:bloom:registered-emails";
    public string PasswordsKeyPrefix { get; set; } = "auth:bloom:passwords";
    public string PasswordFingerprintsKeyPrefix { get; set; } = "auth:password-fingerprints";
    public double ErrorRate { get; set; } = 0.0001;
    public long RegisteredEmailsCapacity { get; set; } = 100000;
    public long PasswordCapacity { get; set; } = 10;
    public string PasswordFingerprintSecret { get; set; } = string.Empty;
}
