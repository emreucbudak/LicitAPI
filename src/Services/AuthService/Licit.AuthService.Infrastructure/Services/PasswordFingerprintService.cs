using System.Security.Cryptography;
using System.Text;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;

namespace Licit.AuthService.Infrastructure.Services;

public class PasswordFingerprintService(AuthBloomFilterSettings settings) : IPasswordFingerprintService
{
    private readonly byte[] _secret = Encoding.UTF8.GetBytes(settings.PasswordFingerprintSecret);

    public string CreateFingerprint(string password)
    {
        using var hmac = new HMACSHA256(_secret);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash);
    }
}
