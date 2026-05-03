using Licit.AuctionService.Domain.Entities;

namespace Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetAuctionById
{
    public record GetAuctionByIdQueriesResponse
    {
        public string AuctionName { get; init; }
        public int StartPrice { get; init; }
        public int IncreaseAmount { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string[] Rules { get; init; }
        public string Description { get; init; }
        public Guid? WinnerBidId { get; init; }
        public AuctionStatus Status { get; init; }
        public Guid CreatedByUserId { get; init; }
        public string[] ImgUrls { get; init; }
    }
}
