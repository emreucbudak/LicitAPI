using Licit.WalletService.Domain.Common;

namespace Licit.WalletService.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid? ReferenceId { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public decimal FrozenBalanceAfter { get; private set; }

    public Wallet Wallet { get; set; } = null!;

    private WalletTransaction() { }

    public WalletTransaction(Guid walletId, TransactionType type, decimal amount,
        string description, Guid? referenceId, decimal balanceAfter, decimal frozenBalanceAfter)
    {
        WalletId = walletId;
        Type = type;
        Amount = amount;
        Description = description;
        ReferenceId = referenceId;
        BalanceAfter = balanceAfter;
        FrozenBalanceAfter = frozenBalanceAfter;
    }
}
