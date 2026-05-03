using FluentValidation;

namespace Licit.AuctionService.Application.Validators
{
    public class AuctionValidator : AbstractValidator<Domain.Entities.Auction>
    {
        public AuctionValidator() {
            RuleFor(a=> a.AuctionName).NotEmpty()
                .WithMessage("Auction name cannot be empty.")
                .NotNull()
                .WithMessage("Auction name cannot be null.");
            RuleFor(a=> a.StartPrice).GreaterThanOrEqualTo(0)
                .WithMessage("Start price must be greater than or equal to 0.");
            RuleFor(a=> a.IncreaseAmount).GreaterThanOrEqualTo(0)
                .WithMessage("Increase amount must be greater than or equal to 0.");
            RuleFor(a=> a.StartDate).LessThan(a=> a.EndDate)
                .WithMessage("Start date must be before end date.");
            RuleFor(a=> a.Rules).NotEmpty()
                .WithMessage("Rules cannot be empty.")
                .NotNull()
                .WithMessage("Rules cannot be null.");
             RuleFor(a=> a.Description).NotEmpty()
                .WithMessage("Description cannot be empty.")
                .NotNull()
                .WithMessage("Description cannot be null.");
             RuleFor(a=> a.CreatedByUserId).NotEmpty()
                .WithMessage("Created by user ID cannot be empty.")
                .NotNull()
                .WithMessage("Created by user ID cannot be null.");
        }
    }
}
