namespace Licit.WalletService.Application.Interfaces;

public interface IDepositIdempotencyStore
{
    Task<bool> TryReserveAsync(
        Guid userId,
        string idempotencyKey,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
