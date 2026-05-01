using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetBalance;

public class GetCurrentUserBalanceQueryHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<GetCurrentUserBalanceQueryRequest, GetBalanceQueryResponse>
{
    public async Task<GetBalanceQueryResponse> Handle(GetCurrentUserBalanceQueryRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new GetBalanceQueryRequest(userId), cancellationToken);
    }
}
