namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;

public record DeductFundsCommandResponse(
    Guid TransactionId,
    decimal AvailableBalance,
    decimal FrozenBalance,
    DateTime CreatedAt
);
