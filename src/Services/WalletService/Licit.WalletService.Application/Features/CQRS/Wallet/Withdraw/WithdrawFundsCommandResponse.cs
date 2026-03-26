namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;

public record WithdrawFundsCommandResponse(
    Guid TransactionId,
    decimal NewBalance,
    decimal FrozenBalance,
    DateTime CreatedAt
);
