namespace Licit.MailService.Application.DTOs;

public sealed record AuthPasswordResetEmailEvent(
    string Email,
    string Code,
    DateTime? ExpiresAt = null,
    string? UserName = null
);
