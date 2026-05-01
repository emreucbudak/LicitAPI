using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public class DepositCurrentUserFundsCommandHandler(
    IMediator mediator,
    ICurrentUserService currentUserService) : IRequestHandler<DepositCurrentUserFundsCommandRequest, DepositFundsCommandResponse>
{
    public async Task<DepositFundsCommandResponse> Handle(DepositCurrentUserFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        return await mediator.Send(new DepositFundsCommandRequest(
            userId,
            request.Amount,
            request.IdempotencyKey ?? string.Empty), cancellationToken);
    }
}
