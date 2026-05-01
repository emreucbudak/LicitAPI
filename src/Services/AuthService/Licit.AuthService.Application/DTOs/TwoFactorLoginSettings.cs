using System.ComponentModel.DataAnnotations;

namespace Licit.AuthService.Application.DTOs;

public record TwoFactorLoginSettings
{
    [Range(1, 60)]
    public int VerificationCodeExpirationMinutes { get; init; } = 5;
}
