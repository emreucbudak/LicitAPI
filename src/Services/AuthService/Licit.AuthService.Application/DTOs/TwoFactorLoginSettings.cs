using System.ComponentModel.DataAnnotations;

namespace Licit.AuthService.Application.DTOs;

public class TwoFactorLoginSettings
{
    [Range(1, 60)]
    public int VerificationCodeExpirationMinutes { get; set; } = 5;
}
