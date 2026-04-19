namespace Licit.AuthService.Application.DTOs;

public record PendingVerificationResponse(
    string TemporaryToken,
    DateTime ExpiresAt,
    string Email
);
