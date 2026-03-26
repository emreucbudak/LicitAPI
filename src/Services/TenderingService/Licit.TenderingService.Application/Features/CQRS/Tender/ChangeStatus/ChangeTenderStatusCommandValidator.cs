using FluentValidation;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;

public class ChangeTenderStatusCommandValidator : AbstractValidator<ChangeTenderStatusCommandRequest>
{
    public ChangeTenderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");

        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("Yeni durum belirtilmelidir.");
    }
}
