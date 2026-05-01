using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;

public record DepositCurrentUserFundsCommandRequest(
    decimal Amount,
    string? IdempotencyKey = null
) : IRequest<DepositFundsCommandResponse>;
