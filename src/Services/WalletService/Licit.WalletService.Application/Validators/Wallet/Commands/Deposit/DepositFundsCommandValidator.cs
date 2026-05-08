using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;

namespace Licit.WalletService.Application.Validators.Wallet.Commands.Deposit;

public class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommandRequest>
{
    public DepositFundsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanici kimligi belirtilmelidir.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Yatirilacak tutar sifirdan buyuk olmalidir.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency-Key header'i belirtilmelidir.")
            .MaximumLength(128).WithMessage("Idempotency-Key en fazla 128 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Aciklama en fazla 500 karakter olabilir.");
    }
}
