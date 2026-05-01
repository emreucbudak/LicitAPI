using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetTransactions;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record GetCurrentUserTransactionsQueryRequest(
    int Page,
    int PageSize
) : IRequest<GetTransactionsQueryResponse>;
