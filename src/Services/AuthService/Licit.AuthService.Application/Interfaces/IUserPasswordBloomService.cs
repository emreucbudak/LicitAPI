namespace Licit.AuthService.Application.Interfaces;

public interface IUserPasswordBloomService
{
    Task<bool> MayContainAsync(Guid userId, string fingerprint, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetFingerprintsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SetFingerprintsAsync(
        Guid userId,
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default);
}
