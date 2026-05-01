using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;

public record WithdrawCurrentUserFundsCommandRequest(
    decimal Amount
) : IRequest<WithdrawFundsCommandResponse>;
