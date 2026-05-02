
using Licit.AuctionService.Domain.Common;

namespace Licit.AuctionService.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public string AuctionName { get; set; }
        public int StartPrice { get; set; }
        public int IncreaseAmount { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string[] Rules { get; set; }
        public string Description { get; set; }
        public Guid? WinnerBidId { get; set; }
        public AuctionStatus Status { get; set; }
        public Guid CreatedByUserId {  get; set; }
        public string[] ImgUrls { get; set; }

    }
}
