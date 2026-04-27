using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Interfaces;

public interface IRegisterVerificationStore
{
    Task StoreAsync(
        string email,
        PendingRegistrationVerification verification,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default);

    Task<PendingRegistrationVerification?> GetAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string email, CancellationToken cancellationToken = default);
}
