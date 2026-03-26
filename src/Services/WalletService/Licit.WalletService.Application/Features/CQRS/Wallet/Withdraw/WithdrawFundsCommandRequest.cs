using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;

public record WithdrawFundsCommandRequest(
    Guid UserId,
    decimal Amount
) : IRequest<WithdrawFundsCommandResponse>;
