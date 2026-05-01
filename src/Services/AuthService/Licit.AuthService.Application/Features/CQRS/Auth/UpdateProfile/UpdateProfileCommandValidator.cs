using FluentValidation;

namespace Licit.AuthService.Application.Features.CQRS.Auth.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommandRequest>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must be 100 characters or fewer.");

        RuleFor(x => x.LastName)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must be 100 characters or fewer.");
    }
}
