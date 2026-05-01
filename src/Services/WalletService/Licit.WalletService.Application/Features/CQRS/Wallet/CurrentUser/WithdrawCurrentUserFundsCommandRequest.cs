using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record WithdrawCurrentUserFundsCommandRequest(
    decimal Amount
) : IRequest<WithdrawFundsCommandResponse>;
