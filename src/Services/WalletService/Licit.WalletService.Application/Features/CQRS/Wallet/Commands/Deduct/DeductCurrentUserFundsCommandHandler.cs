using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

public class DeductCurrentUserFundsCommandHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<DeductCurrentUserFundsCommandRequest, DeductFundsCommandResponse>
{
    public async Task<DeductFundsCommandResponse> Handle(DeductCurrentUserFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new DeductFundsCommandRequest(
            userId,
            request.Amount,
            request.ReferenceId,
            request.Description), cancellationToken);
    }
}
