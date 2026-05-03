using FluentValidation;

namespace Licit.AuctionService.Application.Validators
{
    public class GetAuctionByIdQueriesRequestValidator : AbstractValidator<Feature.CQRS.Auction.Queries.GetAuctionById.GetAuctionByIdQueriesRequest>
    {
        public GetAuctionByIdQueriesRequestValidator()
        {
            RuleFor(x => x.AuctionId).NotEmpty().WithMessage("Auction ID is required.");
        }
    }
}
