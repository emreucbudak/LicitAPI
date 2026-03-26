namespace Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;

public record UnfreezeFundsCommandResponse(
    Guid TransactionId,
    decimal AvailableBalance,
    decimal FrozenBalance,
    DateTime CreatedAt
);
