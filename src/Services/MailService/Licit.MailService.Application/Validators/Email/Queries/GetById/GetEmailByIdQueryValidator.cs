using FluentValidation;
using Licit.MailService.Application.Features.CQRS.Email.GetById;

namespace Licit.MailService.Application.Validators.Email.Queries.GetById;

public class GetEmailByIdQueryValidator : AbstractValidator<GetEmailByIdQueryRequest>
{
    public GetEmailByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("E-posta kimliği belirtilmelidir.");
    }
}
