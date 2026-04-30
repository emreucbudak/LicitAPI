using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;

namespace Licit.WalletService.Application.Validators.Wallet.Commands.Freeze;

public class FreezeFundsCommandValidator : AbstractValidator<FreezeFundsCommandRequest>
{
    public FreezeFundsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Bloke edilecek tutar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Referans kimliği belirtilmelidir.");
    }
}
