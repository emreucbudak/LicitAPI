
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

        public string AuctionName { get; private set; }
        public int StartPrice { get; private set; }
        public int IncreaseAmount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public string[] Rules { get; private set; }
        public string Description { get; private set; }
        public Guid? WinnerBidId { get; private set; } = Guid.Empty;
        public AuctionStatus Status { get; private set; }
        public Guid CreatedByUserId {  get; private set; }
        public string[] ImgUrls { get; private set; }
        public void UpdateWinnerBid(Guid winnerBidId)
        {
            this.WinnerBidId = winnerBidId;
        }
        public void SetStatus(AuctionStatus status)
        {
            this.Status = status;
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
