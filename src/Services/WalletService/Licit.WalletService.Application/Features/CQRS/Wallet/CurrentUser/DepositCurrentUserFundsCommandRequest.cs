using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record DepositCurrentUserFundsCommandRequest(
    decimal Amount,
    string? IdempotencyKey = null
) : IRequest<DepositFundsCommandResponse>;
