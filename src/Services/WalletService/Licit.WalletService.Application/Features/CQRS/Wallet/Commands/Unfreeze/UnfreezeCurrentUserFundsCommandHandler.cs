using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;

public class UnfreezeCurrentUserFundsCommandHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<UnfreezeCurrentUserFundsCommandRequest, UnfreezeFundsCommandResponse>
{
    public async Task<UnfreezeFundsCommandResponse> Handle(UnfreezeCurrentUserFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new UnfreezeFundsCommandRequest(
            userId,
            request.Amount,
            request.ReferenceId,
            request.Description), cancellationToken);
    }
}
