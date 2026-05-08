using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;

public record DepositFundsCommandRequest(
    Guid UserId,
    decimal Amount,
    string IdempotencyKey,
    Guid? ReferenceId = null,
    string? Description = null
) : IRequest<DepositFundsCommandResponse>;
