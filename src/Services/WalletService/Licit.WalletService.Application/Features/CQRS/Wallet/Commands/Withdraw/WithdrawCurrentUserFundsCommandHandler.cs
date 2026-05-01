using FlashMediator;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;

public class WithdrawCurrentUserFundsCommandHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<WithdrawCurrentUserFundsCommandRequest, WithdrawFundsCommandResponse>
{
    public async Task<WithdrawFundsCommandResponse> Handle(WithdrawCurrentUserFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new WithdrawFundsCommandRequest(userId, request.Amount), cancellationToken);
    }
}
