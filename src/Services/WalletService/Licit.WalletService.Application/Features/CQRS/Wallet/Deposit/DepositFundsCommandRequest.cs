using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;

public record DepositFundsCommandRequest(
    Guid UserId,
    decimal Amount,
    string IdempotencyKey
) : IRequest<DepositFundsCommandResponse>;
