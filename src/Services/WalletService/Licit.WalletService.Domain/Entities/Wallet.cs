using Licit.WalletService.Domain.Common;

namespace Licit.WalletService.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal FrozenBalance { get; private set; }
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<WalletTransaction> Transactions { get; private set; } = new List<WalletTransaction>();

    private Wallet() { }

    public Wallet(Guid userId)
    {
        UserId = userId;
        Balance = 0;
        FrozenBalance = 0;
    }

    public WalletTransaction Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("INVALID_DEPOSIT_AMOUNT");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Deposit, amount,
            "Para yatırma", null, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("INVALID_WITHDRAW_AMOUNT");
        if (Balance < amount)
            throw new InvalidOperationException("INSUFFICIENT_BALANCE");

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Withdraw, amount,
            "Para çekme", null, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Freeze(decimal amount, Guid referenceId, string? description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("INVALID_FREEZE_AMOUNT");
        if (Balance < amount)
            throw new InvalidOperationException("INSUFFICIENT_BALANCE_FOR_FREEZE");

        Balance -= amount;
        FrozenBalance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Freeze, amount,
            description ?? "Teklif için bakiye bloke edildi", referenceId, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Unfreeze(decimal amount, Guid referenceId, string? description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("INVALID_UNFREEZE_AMOUNT");
        if (FrozenBalance < amount)
            throw new InvalidOperationException("INSUFFICIENT_FROZEN_BALANCE");

        FrozenBalance -= amount;
        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Unfreeze, amount,
            description ?? "İhale kaybedildi, bloke çözüldü", referenceId, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Deduct(decimal amount, Guid referenceId, string? description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("INVALID_DEDUCT_AMOUNT");
        if (FrozenBalance < amount)
            throw new InvalidOperationException("INSUFFICIENT_FROZEN_BALANCE_FOR_DEDUCT");

        FrozenBalance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Deduct, amount,
            description ?? "İhale kazanıldı, tutar kesildi", referenceId, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }
}
