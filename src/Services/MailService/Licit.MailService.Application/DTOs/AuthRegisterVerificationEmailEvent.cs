namespace Licit.MailService.Application.DTOs;

public sealed record AuthRegisterVerificationEmailEvent(
    string Email,
    string Code,
    DateTime? ExpiresAt = null,
    string? UserName = null
);
