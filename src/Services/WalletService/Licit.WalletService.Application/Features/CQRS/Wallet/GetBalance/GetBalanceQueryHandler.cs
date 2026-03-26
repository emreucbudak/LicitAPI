using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;

public class GetBalanceQueryHandler(
    IWalletRepository walletRepository) : IRequestHandler<GetBalanceQueryRequest, GetBalanceQueryResponse>
{
    public async Task<GetBalanceQueryResponse> Handle(GetBalanceQueryRequest request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Cüzdan bulunamadı.");

        return new GetBalanceQueryResponse(
            wallet.Id,
            wallet.Balance,
            wallet.FrozenBalance,
            wallet.Balance + wallet.FrozenBalance,
            wallet.UpdatedAt
        );
    }
}
