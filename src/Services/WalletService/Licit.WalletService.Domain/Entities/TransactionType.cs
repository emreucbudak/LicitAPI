namespace Licit.WalletService.Domain.Entities;

public enum TransactionType
{
    Deposit = 0,
    Withdraw = 1,
    Freeze = 2,
    Unfreeze = 3,
    Deduct = 4
}
