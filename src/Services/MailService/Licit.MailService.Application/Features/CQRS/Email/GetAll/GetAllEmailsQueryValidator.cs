using FluentValidation;

namespace Licit.MailService.Application.Features.CQRS.Email.GetAll;

public class GetAllEmailsQueryValidator : AbstractValidator<GetAllEmailsQueryRequest>
{
    public GetAllEmailsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Sayfa numarası 1'den büyük olmalıdır.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
    }
}
