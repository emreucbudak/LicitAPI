using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;

public record WithdrawFundsCommandRequest(
    Guid UserId,
    decimal Amount
) : IRequest<WithdrawFundsCommandResponse>;
