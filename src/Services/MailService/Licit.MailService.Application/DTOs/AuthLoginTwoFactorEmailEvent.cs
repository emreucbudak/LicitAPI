namespace Licit.MailService.Application.DTOs;

public sealed record AuthLoginTwoFactorEmailEvent(
    string Email,
    string Code,
    DateTime? ExpiresAt = null,
    string? UserName = null
);
