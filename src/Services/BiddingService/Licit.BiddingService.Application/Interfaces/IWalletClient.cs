using Licit.BiddingService.Application.DTOs;

namespace Licit.BiddingService.Application.Interfaces
{
    public interface IWalletClient
    {
        Task<WalletHoldResult> TryHoldBalanceAsync(
            Guid userId,
            Guid bidId,
            int amount,
            string idempotencyKey,
            CancellationToken cancellationToken);

        Task ReleaseHoldAsync(
            Guid userId,
            Guid bidId,
            int amount,
            CancellationToken cancellationToken);
    }
}
