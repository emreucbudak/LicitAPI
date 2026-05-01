namespace Licit.AuthService.Application.DTOs;

public sealed record PendingRegistrationVerification
{
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public int RemainingAttempts { get; init; }
}
