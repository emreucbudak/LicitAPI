using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Interfaces;

public interface IPasswordResetVerificationStore
{
    Task StoreAsync(
        string temporaryToken,
        PasswordResetVerificationChallenge challenge,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default);

    Task<PasswordResetVerificationChallenge?> GetAsync(
        string temporaryToken,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string temporaryToken, CancellationToken cancellationToken = default);
}
