namespace Licit.AuthService.Application.Interfaces;

public interface IPasswordFingerprintService
{
    string CreateFingerprint(string password);
}
