using FluentValidation;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;

public class GetAllTendersQueryValidator : AbstractValidator<GetAllTendersQueryRequest>
{
    public GetAllTendersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => x.Search is not null);
    }
}
