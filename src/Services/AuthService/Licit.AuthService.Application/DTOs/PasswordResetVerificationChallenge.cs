namespace Licit.AuthService.Application.DTOs;

public sealed record PasswordResetVerificationChallenge
{
    public Guid? UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ChallengeId { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public int RemainingAttempts { get; init; }
    public bool IsCodeVerified { get; init; }
}
