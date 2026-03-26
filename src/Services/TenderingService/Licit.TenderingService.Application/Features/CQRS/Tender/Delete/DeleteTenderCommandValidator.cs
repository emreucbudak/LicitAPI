using FluentValidation;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Delete;

public class DeleteTenderCommandValidator : AbstractValidator<DeleteTenderCommandRequest>
{
    public DeleteTenderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");
    }
}
