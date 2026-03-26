namespace Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;

public record FreezeFundsCommandResponse(
    Guid TransactionId,
    decimal AvailableBalance,
    decimal FrozenBalance,
    DateTime CreatedAt
);
