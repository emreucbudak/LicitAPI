namespace Licit.AuthService.Application.DTOs;

public sealed class PendingRegistrationVerification
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ChallengeId { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int RemainingAttempts { get; set; }
}
