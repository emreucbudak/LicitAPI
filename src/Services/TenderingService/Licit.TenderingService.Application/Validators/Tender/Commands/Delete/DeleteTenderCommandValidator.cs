using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.Delete;

namespace Licit.TenderingService.Application.Validators.Tender.Commands.Delete;

public class DeleteTenderCommandValidator : AbstractValidator<DeleteTenderCommandRequest>
{
    public DeleteTenderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");
    }
}
