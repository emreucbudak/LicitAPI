namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;

public record FreezeFundsCommandResponse(
    Guid TransactionId,
    decimal AvailableBalance,
    decimal FrozenBalance,
    DateTime CreatedAt,
    bool IdempotentReplay = false
);
