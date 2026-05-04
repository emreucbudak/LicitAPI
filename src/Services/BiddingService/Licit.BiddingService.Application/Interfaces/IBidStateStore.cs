using Licit.BiddingService.Application.DTOs;

namespace Licit.BiddingService.Application.Interfaces
{
    public interface IBidStateStore
    {
        Task<BidStateCheckResult> CheckBidCanEnterAsync(
            Guid auctionId,
            int amount,
            CancellationToken cancellationToken);

        Task<BidStateUpdateResult> TrySetHighestBidAsync(
            Guid auctionId,
            Guid bidId,
            Guid bidderUserId,
            int amount,
            DateTime placedAt,
            CancellationToken cancellationToken);

        Task<BidStateRollbackResult> TryRollbackHighestBidAsync(
            Guid auctionId,
            Guid bidId,
            CancellationToken cancellationToken);
    }
}
