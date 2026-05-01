using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetTransactions;

public record GetCurrentUserTransactionsQueryRequest(
    int Page,
    int PageSize
) : IRequest<GetTransactionsQueryResponse>;
