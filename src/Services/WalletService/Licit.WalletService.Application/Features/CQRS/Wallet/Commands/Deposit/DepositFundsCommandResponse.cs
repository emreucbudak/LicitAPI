namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;

public record DepositFundsCommandResponse(
    Guid TransactionId,
    decimal NewBalance,
    decimal FrozenBalance,
    DateTime CreatedAt
);
