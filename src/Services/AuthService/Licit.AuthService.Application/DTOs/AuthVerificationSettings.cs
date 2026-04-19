using System.ComponentModel.DataAnnotations;

namespace Licit.AuthService.Application.DTOs;

public class AuthVerificationSettings
{
    [Range(1, 60)]
    public int RegisterVerificationCodeExpirationMinutes { get; set; } = 10;

    [Range(1, 60)]
    public int PasswordResetCodeExpirationMinutes { get; set; } = 10;

    [Range(1, 10)]
    public int MaxVerificationAttempts { get; set; } = 5;
}
