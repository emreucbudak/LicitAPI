using Licit.BiddingService.API.Hubs;
using Licit.BiddingService.Application.Notifier;
using Microsoft.AspNetCore.SignalR;

namespace Licit.BiddingService.API.Realtime
{
    public class SignalRBiddingNotifier(IHubContext<BiddingHub> hubContext) : IBiddingNotifier
    {
        private const string AuctionFeedGroup = "auctions:feed";

        public async Task NotifyBidPlacedAsync(
            Guid auctionId,
            Guid bidId,
            Guid bidderUserId,
            int amount,
            DateTime placedAt,
            CancellationToken cancellationToken = default)
        {
            var message = new
            {
                auctionId,
                bidId,
                bidderUserId,
                amount,
                placedAt
            };

            await hubContext.Clients
                .Group($"auction:{auctionId}")
                .SendAsync("BidPlaced", message, cancellationToken);

            await hubContext.Clients
                .Group(AuctionFeedGroup)
                .SendAsync("AuctionLatestBidChanged", message, cancellationToken);
        }
    }
}
