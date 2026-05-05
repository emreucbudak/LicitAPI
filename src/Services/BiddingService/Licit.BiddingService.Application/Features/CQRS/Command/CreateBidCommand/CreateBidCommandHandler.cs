using FlashMediator;
using Licit.BiddingService.Application.Exceptions;
using Licit.BiddingService.Application.Interfaces;
using Licit.BiddingService.Application.Notifier;
using Licit.BiddingService.Application.Repository;
using Licit.BiddingService.Domain.Entities;

namespace Licit.BiddingService.Application.Features.CQRS.Command.CreateBidCommand
{
    public class CreateBidCommandHandler(
        IBidStateStore bidStateStore,
        IWalletClient walletClient,
        IBiddingRepository biddingRepository,
        IUnitOfWork unitOfWork,
        IBiddingNotifier biddingNotifier,
        IBidEmailNotificationPublisher bidEmailNotificationPublisher) : IRequestHandler<CreateBidCommandRequest, CreateBidCommandResponse>
    {
        public async Task<CreateBidCommandResponse> Handle(
            CreateBidCommandRequest request,
            CancellationToken cancellationToken)
        {
            var preCheck = await bidStateStore.CheckBidCanEnterAsync(
                request.AuctionId,
                request.Amount,
                cancellationToken);

            if (!preCheck.Success)
                throw new BidPlacementException(preCheck.ErrorCode ?? "BID_PRE_CHECK_FAILED");

            var bid = new Bid(
                request.AuctionId,
                request.BidderUserId,
                request.Amount,
                request.IdempotencyKey);

            var walletHold = await walletClient.TryHoldBalanceAsync(
                request.BidderUserId,
                bid.Id,
                request.Amount,
                request.IdempotencyKey,
                cancellationToken);

            if (!walletHold.Success)
                throw new BidPlacementException(walletHold.ErrorCode ?? "WALLET_HOLD_FAILED");

            bid.AttachWalletHold(walletHold.HoldId);

            var updateResult = await bidStateStore.TrySetHighestBidAsync(
                request.AuctionId,
                bid.Id,
                request.BidderUserId,
                request.Amount,
                bid.PlacedAt,
                cancellationToken);

            if (!updateResult.Success)
            {
                await walletClient.ReleaseHoldAsync(
                    request.BidderUserId,
                    bid.Id,
                    request.Amount,
                    cancellationToken);

                throw new BidPlacementException(updateResult.ErrorCode ?? "BID_STATE_UPDATE_FAILED");
            }

            var previousBidderUserIds = await biddingRepository.GetDistinctBidderUserIdsForAuctionAsync(
                request.AuctionId,
                request.BidderUserId,
                cancellationToken);

            try
            {
                await biddingRepository.CreateBid(bid, cancellationToken);

                if (previousBidderUserIds.Count > 0)
                {
                    await unitOfWork.SaveChangesWithOutboxAsync(
                        async outboxCancellationToken =>
                        {
                            await bidEmailNotificationPublisher.PublishOutbidEmailRequestedAsync(
                                request.AuctionId,
                                bid.Id,
                                request.BidderUserId,
                                request.Amount,
                                bid.PlacedAt,
                                previousBidderUserIds,
                                outboxCancellationToken);
                        },
                        cancellationToken);
                }
                else
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch
            {
                await bidStateStore.TryRollbackHighestBidAsync(
                    request.AuctionId,
                    bid.Id,
                    cancellationToken);

                await walletClient.ReleaseHoldAsync(
                    request.BidderUserId,
                    bid.Id,
                    request.Amount,
                    cancellationToken);

                throw;
            }

            await biddingNotifier.NotifyBidPlacedAsync(
                request.AuctionId,
                bid.Id,
                request.BidderUserId,
                request.Amount,
                bid.PlacedAt,
                cancellationToken);

            return new CreateBidCommandResponse(
                bid.Id,
                bid.AuctionId,
                bid.BidderUserId,
                bid.Amount,
                bid.PlacedAt,
                updateResult.CurrentHighestBid,
                updateResult.Version,
                walletHold.HoldId);
        }
    }
}
