using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;

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
    }
}
