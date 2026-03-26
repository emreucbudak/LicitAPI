using FluentValidation;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;

public class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQueryRequest>
{
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Sayfa numarası 1'den büyük olmalıdır.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
    }
}
