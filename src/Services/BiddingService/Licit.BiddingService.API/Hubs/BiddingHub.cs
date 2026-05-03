using Microsoft.AspNetCore.SignalR;

namespace Licit.BiddingService.API.Hubs
{
    public class BiddingHub : Hub
    {
        private const string AuctionFeedGroup = "auctions:feed";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AuctionFeedGroup);
            await base.OnConnectedAsync();
        }

        public Task JoinAuction(Guid auctionId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"auction:{auctionId}");

        public Task LeaveAuction(Guid auctionId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction:{auctionId}");
    }
}
