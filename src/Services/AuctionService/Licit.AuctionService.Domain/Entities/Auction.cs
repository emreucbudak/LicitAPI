
using Licit.AuctionService.Domain.Common;

namespace Licit.AuctionService.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public Auction()
        {
        }

        public Auction(string auctionName, int startPrice, int increaseAmount, DateTime startDate, DateTime endDate, string[] rules, string description, AuctionStatus status, Guid createdByUserId, string[] imgUrls)
        {
            Validate(auctionName, startPrice, increaseAmount, startDate, endDate, imgUrls, description);
            AuctionName = auctionName;
            StartPrice = startPrice;
            IncreaseAmount = increaseAmount;
            StartDate = startDate;
            EndDate = endDate;
            Rules = rules;
            Description = description;
            Status = status;
            CreatedByUserId = createdByUserId;
            ImgUrls = imgUrls;
        }

        public string AuctionName { get; set; }
        public int StartPrice { get; set; }
        public int IncreaseAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] Rules { get; set; }
        public string Description { get; set; }
        public Guid? WinnerBidId { get; set; } = Guid.Empty;
        public AuctionStatus Status { get; set; }
        public Guid CreatedByUserId {  get; set; }
        public string[] ImgUrls { get; set; }
        public void UpdateWinnerBid(Guid winnerBidId)
        {
            this.WinnerBidId = winnerBidId;
        }
        private static void Validate(string auctionName, int startPrice, int increaseAmount, DateTime startDate, DateTime endDate, string[] imgUrls, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(auctionName);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startPrice);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(increaseAmount);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            ArgumentOutOfRangeException.ThrowIfLessThan(imgUrls.Length, 3);
            ArgumentOutOfRangeException.ThrowIfNegative((DateTime.Now - startDate).TotalDays);
            ArgumentOutOfRangeException.ThrowIfNegative((DateTime.Now - endDate).TotalDays);
        }
    }
}
