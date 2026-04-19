namespace Licit.AuthService.Application.DTOs;

public sealed record TemporaryTokenPayload(
    string Email,
    string TokenId,
    string TokenType
);
