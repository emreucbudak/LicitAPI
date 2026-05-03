using FluentValidation;

namespace Licit.AuctionService.Application.Validators
{
    public class UpdateAuctionWinnerBidRequestValidator : AbstractValidator<Feature.CQRS.Auction.Command.UpdateAuctionWinnerBid.UpdateAuctionWinnerBidCommandRequest>
    {
        public UpdateAuctionWinnerBidRequestValidator()
        {
            RuleFor(x => x.AuctionId).NotEmpty().WithMessage("Auction ID is required.");
            RuleFor(x => x.WinnerBidId).NotEmpty().WithMessage("Winner Bid ID is required.");
        }
    }
}
