using FluentValidation;

namespace Licit.AuctionService.Application.Validators
{
    public class GetActiveAuctionsQueriesRequestValidator : AbstractValidator<Feature.CQRS.Auction.Queries.GetActiveAuctions.GetActiveAuctionsQueriesRequest>
    {
        public GetActiveAuctionsQueriesRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than zero.");
            RuleFor(x => x.PageSize).GreaterThan(0).LessThan(20).WithMessage("Page size must be between 1 and 20.");
        }
    }
}
