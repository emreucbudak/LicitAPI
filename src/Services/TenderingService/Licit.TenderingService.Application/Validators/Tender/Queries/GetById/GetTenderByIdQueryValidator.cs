using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

namespace Licit.TenderingService.Application.Validators.Tender.Queries.GetById;

public class GetTenderByIdQueryValidator : AbstractValidator<GetTenderByIdQueryRequest>
{
    public GetTenderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");
    }
}
