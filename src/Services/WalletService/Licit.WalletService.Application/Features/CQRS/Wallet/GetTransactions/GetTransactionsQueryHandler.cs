using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;

public class GetTransactionsQueryHandler(
    IWalletRepository walletRepository) : IRequestHandler<GetTransactionsQueryRequest, GetTransactionsQueryResponse>
{
    public async Task<GetTransactionsQueryResponse> Handle(GetTransactionsQueryRequest request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Cüzdan bulunamadı.");

        var transactions = await walletRepository.GetTransactionsByWalletIdAsync(wallet.Id, request.Page, request.PageSize);

        var dtos = transactions.Select(t => new TransactionDto(
            t.Id,
            t.Type.ToString(),
            t.Amount,
            t.Description,
            t.ReferenceId,
            t.BalanceAfter,
            t.FrozenBalanceAfter,
            t.CreatedAt
        )).ToList();

        return new GetTransactionsQueryResponse(dtos);
    }
}
