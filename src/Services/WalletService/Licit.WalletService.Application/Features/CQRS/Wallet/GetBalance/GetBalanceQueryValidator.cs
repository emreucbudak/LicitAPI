using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;

public class GetBalanceQueryValidator : AbstractValidator<GetBalanceQueryRequest>
{
    public GetBalanceQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
    }
}
