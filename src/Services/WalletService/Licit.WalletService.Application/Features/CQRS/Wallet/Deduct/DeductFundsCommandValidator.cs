using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;

public class DeductFundsCommandValidator : AbstractValidator<DeductFundsCommandRequest>
{
    public DeductFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Kesilecek tutar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Referans kimliği belirtilmelidir.");
    }
}
