using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Interfaces;

public interface IRegisterVerificationStore
{
    Task StoreAsync(
        string temporaryToken,
        PendingRegistrationVerification verification,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default);

    Task<PendingRegistrationVerification?> GetAsync(
        string temporaryToken,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string temporaryToken, CancellationToken cancellationToken = default);
}
