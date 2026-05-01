using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;

public class FreezeCurrentUserFundsCommandHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<FreezeCurrentUserFundsCommandRequest, FreezeFundsCommandResponse>
{
    public async Task<FreezeFundsCommandResponse> Handle(FreezeCurrentUserFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new FreezeFundsCommandRequest(
            userId,
            request.Amount,
            request.ReferenceId,
            request.Description), cancellationToken);
    }
}
