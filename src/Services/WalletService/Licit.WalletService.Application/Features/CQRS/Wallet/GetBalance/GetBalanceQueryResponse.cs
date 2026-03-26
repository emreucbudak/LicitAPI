namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;

public record GetBalanceQueryResponse(
    Guid WalletId,
    decimal Balance,
    decimal FrozenBalance,
    decimal TotalBalance,
    DateTime? UpdatedAt
);
