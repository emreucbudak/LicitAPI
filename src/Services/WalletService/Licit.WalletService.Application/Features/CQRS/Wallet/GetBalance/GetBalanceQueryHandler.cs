using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance.Exceptions;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;

public class GetBalanceQueryHandler(
    IWalletRepository walletRepository,
    IValidator<GetBalanceQueryRequest> validator) : IRequestHandler<GetBalanceQueryRequest, GetBalanceQueryResponse>
{
    public async Task<GetBalanceQueryResponse> Handle(GetBalanceQueryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundForBalanceException(request.UserId);

        return new GetBalanceQueryResponse(
            wallet.Id,
            wallet.Balance,
            wallet.FrozenBalance,
            wallet.Balance + wallet.FrozenBalance,
            wallet.UpdatedAt
        );
    }
}
