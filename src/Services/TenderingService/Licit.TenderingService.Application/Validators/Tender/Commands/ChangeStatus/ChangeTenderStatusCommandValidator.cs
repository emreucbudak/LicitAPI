using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;

namespace Licit.TenderingService.Application.Validators.Tender.Commands.ChangeStatus;

public class ChangeTenderStatusCommandValidator : AbstractValidator<ChangeTenderStatusCommandRequest>
{
    public ChangeTenderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Yeni durum belirtilmelidir.");
    }
}
