using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;

public record GetTransactionsQueryRequest(
    Guid UserId,
    int Page,
    int PageSize
) : IRequest<GetTransactionsQueryResponse>;
