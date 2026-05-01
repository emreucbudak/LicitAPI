using System.ComponentModel.DataAnnotations;

namespace Licit.AuthService.Application.DTOs;

public record JwtSettings
{
    [Required, MinLength(32)]
    public string Secret { get; init; } = null!;

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    [Range(1, 365)]
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
