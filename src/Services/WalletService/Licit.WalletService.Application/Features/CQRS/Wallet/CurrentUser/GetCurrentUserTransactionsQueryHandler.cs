using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetTransactions;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public class GetCurrentUserTransactionsQueryHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<GetCurrentUserTransactionsQueryRequest, GetTransactionsQueryResponse>
{
    public async Task<GetTransactionsQueryResponse> Handle(GetCurrentUserTransactionsQueryRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new GetTransactionsQueryRequest(userId, request.Page, request.PageSize), cancellationToken);
    }
}
