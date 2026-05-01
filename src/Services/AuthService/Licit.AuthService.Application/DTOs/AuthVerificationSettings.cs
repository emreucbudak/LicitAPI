using System.ComponentModel.DataAnnotations;

namespace Licit.AuthService.Application.DTOs;

public record AuthVerificationSettings
{
    [Range(1, 60)]
    public int RegisterVerificationCodeExpirationMinutes { get; init; } = 10;

    [Range(1, 60)]
    public int PasswordResetCodeExpirationMinutes { get; init; } = 10;

    [Range(1, 10)]
    public int MaxVerificationAttempts { get; init; } = 5;
}
