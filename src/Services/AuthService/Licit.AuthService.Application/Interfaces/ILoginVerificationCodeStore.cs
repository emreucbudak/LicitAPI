using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Interfaces;

public interface ILoginVerificationCodeStore
{
    Task StoreAsync(string email, LoginVerificationChallenge challenge, TimeSpan lifetime, CancellationToken cancellationToken = default);
    Task<LoginVerificationChallenge?> GetAsync(string email, CancellationToken cancellationToken = default);
    Task RemoveAsync(string email, CancellationToken cancellationToken = default);
}
