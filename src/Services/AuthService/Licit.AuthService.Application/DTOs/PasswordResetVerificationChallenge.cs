namespace Licit.AuthService.Application.DTOs;

public sealed class PasswordResetVerificationChallenge
{
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ChallengeId { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int RemainingAttempts { get; set; }
    public bool IsCodeVerified { get; set; }
}
