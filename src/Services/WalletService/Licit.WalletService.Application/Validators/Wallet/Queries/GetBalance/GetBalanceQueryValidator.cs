using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetBalance;

namespace Licit.WalletService.Application.Validators.Wallet.Queries.GetBalance;

public class GetBalanceQueryValidator : AbstractValidator<GetBalanceQueryRequest>
{
    public GetBalanceQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
    }
}
