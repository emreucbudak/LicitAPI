using FluentValidation;

namespace Licit.AuctionService.Application.Validators
{
    public class UpdateAuctionStatusValidator : AbstractValidator<Feature.CQRS.Auction.Command.UpdateAuctionStatus.UpdateAuctionStatusCommandRequest>
    {
        public UpdateAuctionStatusValidator()
        {
            RuleFor(x => x.AuctionId).NotEmpty().WithMessage("Auction ID is required.");
            RuleFor(x => x.Status).IsInEnum().WithMessage("Status must be a valid enum value.");
        }
    }
}
