using FluentValidation;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

public class GetTenderByIdQueryValidator : AbstractValidator<GetTenderByIdQueryRequest>
{
    public GetTenderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");
    }
}
