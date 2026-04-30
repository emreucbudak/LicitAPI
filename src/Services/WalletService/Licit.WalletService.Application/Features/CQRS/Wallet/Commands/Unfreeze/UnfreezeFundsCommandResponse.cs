namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;

public record UnfreezeFundsCommandResponse(
    Guid TransactionId,
    decimal AvailableBalance,
    decimal FrozenBalance,
    DateTime CreatedAt,
    bool IdempotentReplay = false
);
