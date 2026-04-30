namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

public record DeductFundsCommandResponse(
    Guid TransactionId,
    decimal AvailableBalance,
    decimal FrozenBalance,
    DateTime CreatedAt,
    bool IdempotentReplay = false
);
