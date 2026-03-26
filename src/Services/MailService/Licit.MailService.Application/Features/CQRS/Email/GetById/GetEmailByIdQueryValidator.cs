using FluentValidation;

namespace Licit.MailService.Application.Features.CQRS.Email.GetById;

public class GetEmailByIdQueryValidator : AbstractValidator<GetEmailByIdQueryRequest>
{
    public GetEmailByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("E-posta kimliği belirtilmelidir.");
    }
}
